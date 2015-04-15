using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Common.Logging;
using DocumentManagement.Service;
using DocumentManagement.Service.Models;
using DocumentManagment.Common.Enums;

namespace DocumentManagement.WebDav.Helpers
{
    public class WebDavHelper
    {
        private static ILog _log = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Private constructor
        /// </summary>
        private WebDavHelper() { }

        /// <summary>
        /// Retrieves the parent path from a resources URL
        /// </summary>
        /// <param name="urlPath">Absolute or relative URL path</param>
        /// <param name="path"></param>
        /// <param name="removeEndTokenCount"></param>
        /// <returns></returns>
        public static string GetParentResourcePath(string path, int removeEndTokenCount)
        {
            if (removeEndTokenCount == 0) return path;
            if (!path.Contains("/")) return string.Empty;

            var relativePhysicalPath = new StringBuilder();
            var splitPath = path.Split('/');

            for (int i = 0; i < splitPath.Length - removeEndTokenCount; i++)
            {
                relativePhysicalPath.Append(splitPath[i] + @"/");
            }
            var relativePath = relativePhysicalPath.ToString();

            return relativePath.EndsWith("/") ? relativePath.Substring(0, relativePath.Length - 1) : relativePath;
        }

        /// <summary>
        /// Retrieves the resource name from an enitre URL
        /// </summary>
        /// <param name="urlPath">Absolute or relative URL path</param>
        /// <returns></returns>
        public static string GetResourceName(string urlPath)
        {
            string tpath = urlPath.Trim();
            if (tpath.EndsWith("/"))
            {
                tpath = tpath.Substring(0, tpath.Length - 1);
            }
            if (!(tpath.Contains("/")))
            {
                return tpath;
            }

            string[] path = tpath.Split('/');

            return path[path.Length - 1];

        }


        /// <summary>
        /// Verifies the requested resource is valid
        /// </summary>
        /// <param name="urlPath">Absolute or relative URL path</param>
        /// <returns></returns>
        public static bool ValidResourceByPath(string urlPath)
        {
            var folder = GetFolder(urlPath);
            if (folder != null)
            {
                return true;
            }

            var file = GetFileAttribsOnly(urlPath);
            return file != null;
        }

        /// <summary>
        /// Retrieves a directory
        /// </summary>
        /// <param name="urlPath">Absolute or relative URL path</param>
        /// <returns>Null if the directory does not exist</returns>
        public static FolderModel GetFolder(string urlPath)
        {
            //Get the root folder
            var folder = FolderService.GetRootFolder();

            if (urlPath.Contains("/"))
            {
                int? parentId = null;
                var path = urlPath.Split("/".ToCharArray());

                for (var pathIndex = 0; pathIndex < path.Length; pathIndex++)
                {
                    var nextFolder = FolderService.GetFolder(parentId, path[pathIndex]);
                    if (nextFolder == null)
                    {
                        pathIndex = path.Length + 1;
                    }
                    else
                    {
                        parentId = folder.Id;
                        folder = nextFolder;
                    }
                }

            }
            else
            {
                folder = urlPath == string.Empty ? FolderService.GetRootFolder() : FolderService.GetFolder(null, urlPath);
            }

            return folder;
        }

        /// <summary>
        /// Retrieves a directory / file's parent directory
        /// </summary>
        /// <param name="urlPath">Absolute or relative URL path</param>
        /// <returns>Null if the parent directory does not exist</returns>
        public static FolderModel GetParentFolder(string urlPath)
        {
            string physicalPath = GetParentResourcePath(urlPath, 1);
            return GetFolder(physicalPath);
        }

        /// <summary>
        /// Retrieves a file
        /// </summary>
        /// <param name="urlPath">Absolute or relative URL path</param>
        /// <returns>Null if the file does not exist</returns>
        public static FileModel GetFile(string urlPath)
        {
            var parentFolder = GetFolder(GetParentResourcePath(urlPath, 1));

            return parentFolder != null ? FileService.GetFile(parentFolder.Id, GetResourceName(urlPath)).FirstOrDefault() : null;
        }


        /// <summary>
        /// Retrieves the attributes of a file
        /// </summary>
        /// <param name="urlPath">Absolute or relative URL path</param>
        /// <returns>Null if the file does not exist</returns>
        public static FileModel GetFileAttribsOnly(string urlPath)
        {
            var parentFolder = GetFolder(GetParentResourcePath(urlPath, 1));
            return parentFolder != null ? FileService.GetFile(parentFolder.Id, GetResourceName(urlPath)).FirstOrDefault() : null;
        }


        /// <summary>
        /// Retrieves the full path of a Folder given an ID
        /// </summary>
        /// <param name="folderId">The ID of the file in the Database</param>
        /// <returns></returns>   
        public static string GetFolderFullPath(int folderId)
        {
            string retval = string.Empty;
            var folder = FolderService.GetFolder(folderId);
            if (folder != null)
            {
                while (folder.ParentFolderId.HasValue)
                {

                    retval += folder.FolderName + "/";
                    folder = FolderService.GetFolder(folder.ParentFolderId.Value);
                }
            }
            return retval;
        }

        /// <summary>
        /// HttpRequest Length
        /// </summary>
        /// <param name="httpApplication"></param>
        public static long GetRequestLength(HttpApplication httpApplication)
        {
            if (httpApplication == null)
                throw new ArgumentNullException("httpApplication", "Cannot find handle to HTTP Application");

            return httpApplication.Request.InputStream.Length;
        }

        /// <summary>
        /// WebDav Requested Depth
        /// </summary>
        /// <param name="httpApplication"></param>
        public static DepthType GetRequestDepth(HttpApplication httpApplication)
        {
            var depth = DepthType.Infinity;

            if (httpApplication == null)
                throw new ArgumentNullException("httpApplication", "Cannot find handle to HTTP Application");

            //No depth provided
            if (!httpApplication.Request.Headers.AllKeys.Contains("Depth"))
            {
                return depth;
            }

            try
            {
                depth = (DepthType) Enum.Parse(typeof (DepthType), httpApplication.Request.Headers["Depth"], true);
            }
            catch (Exception ex)
            {
                _log.ErrorFormat("Following error occurred while parsing Depth header: {0}", ex);
            }

            return depth;
        }

        /// <summary>
        /// WebDav Parse Lock token If Header
        /// </summary>
        /// <param name="inputString">This should be the HttpApplication.Request.Headers["If"]</param>
        /// <returns>The token string if it exists</returns>

        public static string ParseOpaqueLockToken(string inputString)
        {
            string opaqueLockToken = string.Empty;

            if (inputString != null)
            {
                //const string prefixTag = "<opaquelocktoken:";
                const string prefixTag = "<urn:uuid:";
                var prefixIndex = inputString.IndexOf(prefixTag);
                if (prefixIndex != -1)
                {
                    int endIndex = inputString.IndexOf('>', prefixIndex);
                    if (endIndex > prefixIndex)
                        opaqueLockToken = inputString.Substring(prefixIndex + prefixTag.Length, endIndex - (prefixIndex + prefixTag.Length));
                }
            }

            return opaqueLockToken;
        }

        /// <summary>
        /// Gets the NonPathPart of a URI/URL
        /// </summary>
        /// <param name="httpApplication"></param>
        public static string GetNonPathPart(HttpApplication httpApplication)
        {
            if (httpApplication == null)
                throw new ArgumentNullException("httpApplication", "Cannot find handle to HTTP Application");

            var completePath = httpApplication.Request.Url.AbsoluteUri;
            var relativePath = httpApplication.Request.Url.AbsolutePath;

            return completePath.Substring(0, completePath.Length - relativePath.Length);
        }


        /// <summary>
        /// Function for obtaining a URIPath's relative path (Removes the NonPathPart)
        /// </summary>
        /// <param name="httpApplication"></param>
        /// <param name="uriPath"></param>
        /// <returns></returns>
        public static string GetRelativePath(HttpApplication httpApplication, string uriPath)
        {
            if (httpApplication == null)
                throw new ArgumentNullException("httpApplication", "Cannot find handle to HTTP Application");

            var nonPathPart = GetNonPathPart(httpApplication);

            string retValue = uriPath.ToLower().StartsWith(nonPathPart.ToLower()) ? uriPath.Remove(0, nonPathPart.Length) : uriPath;

            //Remove the application path
            var appPath = httpApplication.Request.ApplicationPath;
            if (retValue.ToLower().StartsWith(appPath.ToLower()))
                retValue = retValue.Remove(0, appPath.Length);

            return HttpUtility.UrlDecode(retValue.Trim('/'));
        }



        /// <summary>
        /// Parse the Timeout header of a lock request to determine if a timeout was specified.
        /// </summary>
        /// <param name="httpApplication"></param>
        /// <returns></returns>
        public static int ParseTimeoutHeader(HttpApplication httpApplication, int defaulttimeout)
        {
            var lockTimeout = defaulttimeout;

            if (httpApplication.Request.Headers["Timeout"] != null)
            {
                //Parse the Timeout lock request
                var timeoutHeader = httpApplication.Request.Headers["Timeout"];
                var timeoutInfo = timeoutHeader.Split('-');

                //There should only be 2 segments
                if (timeoutInfo.Length == 2)
                {
                    try
                    {
                        lockTimeout = Convert.ToInt32(timeoutInfo[1], CultureInfo.InvariantCulture);
                    }
                    catch (InvalidCastException ex)
                    {
                        _log.ErrorFormat("Following error occurred casting lock timeout to int: {0}", ex);
                        return defaulttimeout;
                    }
                }
            }
            //Do not allow the client to override the maximum timeout
            //if (lockTimeout > defaulttimeout)
            //    lockTimeout = defaulttimeout;

            return lockTimeout;
        }

        public static string GetCurrentUserName()
        {
            if (HttpContext.Current != null && HttpContext.Current.User != null)
            {
                return HttpContext.Current.User.Identity.Name;
            }

            return @"PBHC\Yury.Korzun";
        }
        
        /// <summary>
        /// Returns the string version of a http status response code
        /// </summary>
        /// <param name="statusCode"></param>
        /// <returns></returns>
        public static string GetEnumHttpResponse(Enum statusCode)
        {
            string httpResponse = string.Empty;

            switch (GetEnumValue(statusCode))
            {
                case 200:
                    httpResponse = "HTTP/1.1 200 OK";
                    break;

                case 404:
                    httpResponse = "HTTP/1.1 404 Not Found";
                    break;

                case 423:
                    httpResponse = "HTTP/1.1 423 Locked";
                    break;

                case 424:
                    httpResponse = "HTTP/1.1 424 Failed Dependency";
                    break;

                case 507:
                    httpResponse = "HTTP/1.1 507 Insufficient Storage";
                    break;
            }

            return httpResponse;
        }

        public static string GetHtmlErrorMsg(Enum statusCode)
        {
            return GetHtmlErrorMsg(GetEnumHttpResponse(statusCode));
        }

        public static string GetHtmlErrorMsg(string msg)
        {

            string retval = "<HTML><HEAD>";
            retval += "<TITLE>Resource Error</TITLE>\r\n";
            retval += "\r\n<STYLE>.Error{font-size:9pt;font-family:'trebuchet ms',helvetica,sans-serif;}</STYLE>\r\n";
            retval += "</HEAD>\r\n<BODY>\r\n<H3 class=\"Error\">" + msg + "</h3>\r\n";
            retval += "</BODY></HTML>";
            return retval;
        }

        public static string GetHtmlFolderMsg()
        {
            const string msg = "This is a WEBDAV server, please connect using a WEBDAV client, such as Windows Explorer, that understand how to issue a http PROPFIND method<BR/>Directory browsing via http GET is not allowed";
            string retval = "<HTML><HEAD>";
            retval += "<TITLE>WebDav Server</TITLE>\r\n";
            retval += "\r\n<STYLE>.Error{font-size:9pt;font-family:'trebuchet ms',helvetica,sans-serif;}</STYLE>\r\n";
            retval += "</HEAD>\r\n<BODY>\r\n<H3 class=\"Error\">" + msg + "</h3>\r\n";
            retval += "</BODY></HTML>";
            return retval;

        }

        public static bool ValidateEnumType(Enum enumToValidate)
        {
            if (enumToValidate.GetTypeCode() != TypeCode.Int32)
                throw new Exception("Invalid Enum Type");

            return true;
        }

        public static int GetEnumValue(Enum statusCode)
        {
            int enumValue = 0;
            if (ValidateEnumType(statusCode))
                enumValue = (int)Enum.Parse(statusCode.GetType(), statusCode.ToString(), true);

            return enumValue;
        }

        public static string StreamtoString(Stream stream)
        {
            string retval;
            using (var streamReader = new StreamReader(stream, Encoding.UTF8))
            {
                //Go to the beginning of the stream
                streamReader.BaseStream.Position = 0;
                retval = streamReader.ReadToEnd();
            }
            return retval;
        }


    }
}
