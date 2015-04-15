using System;
using System.Web;
using DocumentManagement.Service;
using DocumentManagement.Service.Models;
using DocumentManagement.WebDav.Helpers;
using DocumentManagement.WebDav.XMLDBObjects;
using DocumentManagment.Common.Enums;

namespace DocumentManagement.WebDav.Handler.MethodHandlers
{
    /// <summary>
    /// http://www.restpatterns.org/HTTP_Methods/MOVE
    /// http://www.webdav.org/specs/rfc4918.html#METHOD_PROPFIND
    /// 
    /// MOVE /~fielding/index.html HTTP/1.1 
    ///  Host: www.example.com 
    ///  Destination: http://www.example/users/f/fielding/index.html 
    ///
    /// HTTP/1.1 201 Created 
    ///  Location: http://www.example.com/users/f/fielding/index.html 
    /// </summary>
    public class MoveHandler : IMethodHandler
    {
        private readonly HttpApplication _httpApplication;
        private string _requestPath;
 
        private string _destinationPath;
        private bool _isrename = false;
      
        private bool _overwriteResource;

        private readonly ProcessingErrorCollection _errors = new ProcessingErrorCollection();

        public MoveHandler(HttpApplication httpApplication)
        {

            _httpApplication = httpApplication;

        }

        #region IMethodHandler Interface
        public HandlerResult Handle()
        {

            _requestPath = WebDavHelper.GetRelativePath(_httpApplication, _httpApplication.Request.FilePath);
            _destinationPath = GetRequestDestination();
            _overwriteResource = true;

            if (_httpApplication.Request.Headers["Overwrite"] != null)
                _overwriteResource = _httpApplication.Request.Headers["Overwrite"] != "f";

            //Check to make sure the resource is valid
             if (!WebDavHelper.ValidResourceByPath(_requestPath))
                 return new HandlerResult { StatusCode = (int)ServerResponseCode.NotFound, ErrorXml = XMLWebDavError.ProcessErrorCollection(_errors) };

            if (WebDavHelper.GetParentResourcePath(_destinationPath,1) == WebDavHelper.GetParentResourcePath(_requestPath, 1))
                _isrename = true;


            if (_isrename)
            {
                if (WebDavHelper.ValidResourceByPath(_destinationPath))
                {
                    return new HandlerResult { StatusCode = (int)DavMoveResponseCode.Conflict, ErrorXml = XMLWebDavError.ProcessErrorCollection(_errors) };
                }

                var sourceFile = WebDavHelper.GetFile(_requestPath);
                var sourceDir  = WebDavHelper.GetFolder(_requestPath);
                if (sourceDir != null)
                {
                    sourceDir.FolderName = WebDavHelper.GetResourceName(_destinationPath);
                    FolderService.SaveFolder(sourceDir);
                }
                else if (sourceFile != null)
                {
                    sourceFile.FileName = WebDavHelper.GetResourceName(_destinationPath);
                    FileService.SaveFile(sourceFile);
                }
                else
                {
                    return new HandlerResult { StatusCode = (int)DavMoveResponseCode.BadGateway, ErrorXml = XMLWebDavError.ProcessErrorCollection(_errors) };
                }
            }
            else
            {

                //Make sure the destination directory exists
                var parentFolder = WebDavHelper.GetParentFolder(_destinationPath);
                if (parentFolder == null)
                    return new HandlerResult { StatusCode = (int)DavMoveResponseCode.Conflict, ErrorXml = XMLWebDavError.ProcessErrorCollection(_errors) };

                var sourceFile = WebDavHelper.GetFile(_requestPath);
                var sourceDir  = WebDavHelper.GetFolder(_requestPath);
                if (sourceDir != null)
                {
                    CheckforDestinationLocks(sourceDir, _destinationPath);
                    if (_errors.Count == 0)
                    {
                        var folder = WebDavHelper.GetFolder(_destinationPath);
                        if (folder != null)
                        {
                            DeleteFolder(folder, sourceDir.Id);
                            FolderService.DeleteFolder(folder.Id);
                        }
                        sourceDir.ParentFolderId = parentFolder.Id;
                        sourceDir.FolderName = WebDavHelper.GetResourceName(_destinationPath);
                        FolderService.SaveFolder(sourceDir);
                    }

                }
                else if (sourceFile != null)
                {
                    var destFile =WebDavHelper.GetFileAttribsOnly(_destinationPath);

                    if ((!_overwriteResource) && (destFile != null))
                    {
                        return new HandlerResult { StatusCode = (int)DavMoveResponseCode.BadGateway, ErrorXml = XMLWebDavError.ProcessErrorCollection(_errors) };
                    }
                    CheckforDestinationLocks(sourceFile.FileName, _destinationPath);
                    if (_errors.Count == 0)
                    {
                        if (destFile != null)
                        {
                            FileService.DeleteFile(destFile.Id);
                        }

                        sourceFile.ParentFolderId = parentFolder.Id;
                        sourceFile.FileName = WebDavHelper.GetResourceName(_destinationPath);
                        FileService.SaveFile(sourceFile);
                    }
                }
            }
            return new HandlerResult { StatusCode = (int)DavMoveResponseCode.Created, ErrorXml = XMLWebDavError.ProcessErrorCollection(_errors) };
        }
        
        #endregion

        #region private Method functions

        private string GetRequestDestination()
        {
            var destination = _httpApplication.Request.Headers["Destination"];
            destination = destination == null ? string.Empty : HttpUtility.UrlDecode(destination.Trim('/'));

            return WebDavHelper.GetRelativePath(_httpApplication, destination);
        }

        private void CheckforDestinationLocks(FolderModel sourceDirectory, string destination)
        {
            //loop through the destination and determine if there are any filelocks
            // if they exist then add the error to the errorlist
            if (sourceDirectory != null)
            {
                try
                {
                    var destFolder = WebDavHelper.GetFolder(destination);
                    if (destFolder == null) return;

                    var filelist = FileService.GetFileByParentFolder(sourceDirectory.Id);
                    foreach (var sfr in filelist)
                    {
                        var destfile = WebDavHelper.GetFileAttribsOnly(destination + "/" + sfr.FileName);
                        if (destfile != null)
                        {
                            if (LockService.GetLockByFile(destfile.Id) != null)
                            {
                                _errors.Add(new ProcessingError(sfr.FileName, WebDavHelper.GetEnumHttpResponse(DavMoveResponseCode.Locked)));
                                //We already have an error so leave now
                                return;
                            }
                        }
                    }

                    var dirlist = FolderService.GetFolderByParentId(sourceDirectory.Id);

                    foreach (var dir in dirlist)
                        CheckforDestinationLocks(dir, destination + "/" + dir.FolderName);

                }
                catch (Exception)
                {
                    _errors.Add(new ProcessingError(sourceDirectory.FolderName, WebDavHelper.GetEnumHttpResponse(DavMoveResponseCode.Forbidden)));
                }
            }
        }
        private void CheckforDestinationLocks(string SourceName, string destination)
        {
            //Check for a lock at a destination string(used for single filecopy only)
            //if they exist then add the error to the errorlist
            try
            {
                var destFile = WebDavHelper.GetFileAttribsOnly(destination);

                if (destFile == null) return;

                if (LockService.GetLockByFile(destFile.Id) != null)
                    _errors.Add(new ProcessingError(SourceName, WebDavHelper.GetEnumHttpResponse(DavMoveResponseCode.Locked)));
            }
            catch (Exception)
            {
                _errors.Add(new ProcessingError(SourceName, WebDavHelper.GetEnumHttpResponse(DavMoveResponseCode.Forbidden)));
            }
        }
        private void DeleteFolder(FolderModel sourceDirectory, int sourceId)
        {
            if (sourceDirectory != null)
            {
                //Check that the directory we are about to delete and transverse is not in fact the
                //orginal source of the move. This can occur if the source folder already lies under the destination
                //and a folder of the same name exists in the destination folder.
                if (sourceDirectory.Id != sourceId)
                {
                    //Move over the directory files
                    var filelist = FileService.GetFileByParentFolder(sourceDirectory.Id);

                    foreach (var file in filelist)
                        FileService.DeleteFile(file.Id);

                    var folders = FolderService.GetFolderByParentId(sourceDirectory.Id);

                    foreach (var dir in folders)
                    {
                        DeleteFolder(dir,sourceId);

                        if (sourceId !=dir.Id)
                            FolderService.DeleteFolder(dir.Id);
                    }
                }
            }
        }
        #endregion
    }
}
