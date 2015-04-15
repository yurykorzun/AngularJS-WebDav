using System.Web;
using DocumentManagement.Service;
using DocumentManagement.WebDav.Helpers;
using DocumentManagment.Common.Enums;
using Microsoft.Ajax.Utilities;

namespace DocumentManagement.WebDav.Handler.MethodHandlers
{
    /// <summary>
    /// http://www.restpatterns.org/HTTP_Methods/UNLOCK
    /// http://www.webdav.org/specs/rfc4918.html#METHOD_UNLOCK
    /// Handles a WebDav UNLOCK request for a resource.
    ///  SPECIAL NOTES: 
    ///  Please refer to the summary of the LOCKHandler for special notes on supported locksFileService.GetByToken()
    /// 
    /// Request
    /// UNLOCK /workspace/webdav/info.doc HTTP/1.1 
    /// Host: example.com 
    /// Lock-Token: <urn:uuid:a515cfa4-5da4-22e1-f5b5-00a0451e6bf7> 
    /// Authorization: Digest username="ejw" 
    /// realm="ejw@example.com", nonce="...", 
    /// uri="/workspace/webdav/proposal.doc", 
    /// response="...", opaque="..." 
    /// 
    /// Response
    /// HTTP/1.1 204 No Content 
    /// </summary>
    public class UnlockHandler : IMethodHandler
    {
        private readonly HttpApplication _httpApplication;
       
        public UnlockHandler(HttpApplication httpApplication)
        {

            _httpApplication = httpApplication;
           
        }

        #region IMethodHandler Interface

        public HandlerResult Handle()
        {
            if (WebDavHelper.GetRequestLength(_httpApplication) != 0)
                return new HandlerResult { StatusCode = (int)ServerResponseCode.BadRequest };

            if (_httpApplication.Request.Headers["Lock-Token"] == null)
                return new HandlerResult { StatusCode = (int)DavUnlockResponseCode.BadRequest };

            var lockTokenId = WebDavHelper.ParseOpaqueLockToken(_httpApplication.Request.Headers["Lock-Token"]);

            if (lockTokenId.IsNullOrWhiteSpace())
            {
                return new HandlerResult { StatusCode = (int)DavUnlockResponseCode.PreconditionFailed };
            }

            var lockToken = LockService.GetLockToken(lockTokenId);

            if (lockToken == null)
            {
                return new HandlerResult { StatusCode = (int)DavUnlockResponseCode.PreconditionFailed };
            }

            var fileLock = LockService.GetLock(lockToken.LockId);
            if (fileLock == null)
            {
                return new HandlerResult { StatusCode = (int)DavUnlockResponseCode.PreconditionFailed };
            }

            //DeleteFile the locked files
            LockService.DeleteLockToken(lockToken.Id);
            LockService.DeleteLock(fileLock.Id);

            return new HandlerResult { StatusCode = (int)DavUnlockResponseCode.NoContent };
        }

        #endregion

    }
}
