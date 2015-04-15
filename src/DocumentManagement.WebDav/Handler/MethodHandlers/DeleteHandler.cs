using System;
using System.Web;
using Common.Logging;
using DocumentManagement.Service;
using DocumentManagement.Service.Models;
using DocumentManagement.WebDav.Helpers;
using DocumentManagement.WebDav.XMLDBObjects;
using DocumentManagment.Common.Enums;

namespace DocumentManagement.WebDav.Handler.MethodHandlers
{
    /// <summary>
    /// http://www.restpatterns.org/HTTP_Methods/DELETE
    /// http://www.webdav.org/specs/rfc4918.html#METHOD_DELETE
    /// 
    /// Request
    /// DELETE  /container/ HTTP/1.1 
    /// Host: www.example.com 
    /// 
    /// Response
    ///  HTTP/1.1 207 Multi-Status 
    ///  Content-Type: application/xml; charset="utf-8" 
    ///  Content-Length: xxxx 
    ///      
    ///  <?xml version="1.0" encoding="utf-8" ?> 
    ///  <d:multistatus xmlns:d="DAV:"> 
    ///    <d:response> 
    ///      <d:href>http://www.example.com/container/resource3</d:href> 
    ///      <d:status>HTTP/1.1 423 Locked</d:status> 
    ///      <d:error><d:lock-token-submitted/></d:error>
    ///    </d:response> 
    ///  </d:multistatus> 
    /// </summary>
    public class DeleteHandler : IMethodHandler
    {
        private readonly ILog _log = LogManager.GetCurrentClassLogger();
        private readonly HttpApplication _httpApplication;
        private string _requestPath;
        private readonly ProcessingErrorCollection _errors = new ProcessingErrorCollection();

        public DeleteHandler(HttpApplication httpApplication)
        {

            _httpApplication = httpApplication;

        }

        #region IMethodHandler Interface
        public HandlerResult Handle()
        {
            //Check to see if the resource is a folder
            _requestPath = WebDavHelper.GetRelativePath(_httpApplication, _httpApplication.Request.FilePath);
            var dirInfo  = WebDavHelper.GetFolder(_requestPath);

            if (dirInfo != null)
            {
                try
                {
                    //Check for locks recursively.
                    DeleteFolder(dirInfo, true);
 
                    //if there are no lock errors then remove the files and folders.
                    if (_errors.Count == 0)
                    {
                        DeleteFolder(dirInfo, false);
                        
                        FolderService.DeleteFolder(dirInfo.Id);
                    }

                }
                catch (Exception ex) {
                    _log.ErrorFormat("Error occurred while deleting: {0)", ex);
                    return new HandlerResult { StatusCode = (int) DavDeleteResponseCode.Locked, ErrorXml = XMLWebDavError.ProcessErrorCollection(_errors) };
                }
            }
            else
            {
                var fileInfo = WebDavHelper.GetFileAttribsOnly(_requestPath);
                if (fileInfo != null)
                {
                    if (LockService.GetLockByFile(fileInfo.Id) == null)
                    {
                        FileService.DeleteFile(fileInfo.Id);
                    }
                    else
                    {
                        //this is for a single file so just respond in header.
                        return new HandlerResult { StatusCode = (int)DavDeleteResponseCode.Locked, ErrorXml = XMLWebDavError.ProcessErrorCollection(_errors) };
                    }
                }
            }
            return new HandlerResult { StatusCode = (int)ServerResponseCode.Ok, ErrorXml = XMLWebDavError.ProcessErrorCollection(_errors) };

        }

        #endregion

        #region private Handler Methods

        private void DeleteFolder(FolderModel sourceDirectory, bool justCheckforLocks)
        {
            if (sourceDirectory != null)
            {
                //Move over the directory files
                var filelist = FileService.GetFileByParentFolder(sourceDirectory.Id);

                foreach (var file in filelist)
                {
                    if (justCheckforLocks)
                    {
                        if (LockService.GetLockByFile(file.Id) != null)
                        {
                            _errors.Add(new ProcessingError(WebDavHelper.GetFolderFullPath(file.ParentFolderId) + file.FileName, WebDavHelper.GetEnumHttpResponse(DavDeleteResponseCode.Locked)));
                        }
                    }
                    else
                    {
                        FileService.DeleteFile(file.Id);
                    }
                }

                var folders = FolderService.GetFolderByParentId(sourceDirectory.Id);

                foreach (var dir in folders)
                {
                    DeleteFolder(dir, justCheckforLocks);
                    if (!(justCheckforLocks))
                        FolderService.DeleteFolder(dir.Id);

                }
            }
        }

        #endregion
    }
}

