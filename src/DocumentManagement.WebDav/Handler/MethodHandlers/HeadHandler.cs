using System.Web;
using DocumentManagement.WebDav.Helpers;
using DocumentManagment.Common.Enums;

namespace DocumentManagement.WebDav.Handler.MethodHandlers
{
    /// <summary>
    /// http://www.restpatterns.org/HTTP_Methods/HEAD
    /// The HEAD method is identical to GET except that the server MUST NOT return a message-body in the response. 
    /// </summary>
    public class HeadHandler : IMethodHandler
    {
        private readonly HttpApplication _httpApplication;
        private string _requestPath;

       
        public HeadHandler(HttpApplication httpApplication)
        {

            _httpApplication = httpApplication;
        }

        #region IMethodHandler Interface

        public HandlerResult Handle()
        {
            if (_httpApplication == null)
                return new HandlerResult { StatusCode = (int)ServerResponseCode.BadRequest };

            _requestPath = WebDavHelper.GetRelativePath(_httpApplication, _httpApplication.Request.FilePath);

            return !(WebDavHelper.ValidResourceByPath(_requestPath)) ? 
                new HandlerResult { StatusCode = (int)ServerResponseCode.NotFound } : 
                new HandlerResult { StatusCode = (int)ServerResponseCode.Ok };
        }

        #endregion
    }
}

