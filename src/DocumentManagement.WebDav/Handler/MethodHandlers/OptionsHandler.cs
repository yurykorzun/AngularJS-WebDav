using System.Web;
using DocumentManagment.Common.Enums;

namespace DocumentManagement.WebDav.Handler.MethodHandlers
{
    /// <summary>
    /// http://www.restpatterns.org/HTTP_Methods/OPTIONS
    /// Request 
    /// OPTIONS /somecollection/ HTTP/1.1
    /// Host: example.org
    /// 
    /// Response
    /// HTTP/1.1 200 OK
    /// Allow: OPTIONS, GET, HEAD, POST, PUT, DELETE, TRACE, COPY, MOVE
    /// Allow: MKCOL, PROPFIND, PROPPATCH, LOCK, UNLOCK, ORDERPATCH
    /// DAV: 1, 2, ordered-collections
    /// </summary>
    public class OptionsHandler : IMethodHandler
    {
        private readonly HttpApplication _httpApplication;

        public OptionsHandler(HttpApplication httpApplication)
        {

            _httpApplication = httpApplication;

        }

        #region IMethodHandler Interface
        public HandlerResult Handle()
        {
            if (_httpApplication==null)
                return new HandlerResult { StatusCode = (int)ServerResponseCode.BadRequest };

            _httpApplication.Response.AppendHeader("DAV", "1,2");
            _httpApplication.Response.AppendHeader("MS-Author-Via", "DAV");
            _httpApplication.Response.AppendHeader("Versioning-Support", "DAV:basicversioning");
            _httpApplication.Response.AppendHeader("DASL", "<DAV:sql>");
            _httpApplication.Response.AppendHeader("Public", "COPY, DELETE, GET, HEAD, LOCK, MKCOL, MOVE, OPTIONS, PROPFIND, PROPPATCH, PUT, UNLOCK, REPORT, VERSION-CONTROL, CHECKOUT, CHECKIN, UNCHECKOUT");
            _httpApplication.Response.AppendHeader("Allow", "COPY, DELETE, GET, HEAD, LOCK, MKCOL, MOVE, OPTIONS, PROPFIND, PROPPATCH, PUT, UNLOCK, REPORT,  VERSION-CONTROL, CHECKOUT, CHECKIN, UNCHECKOUT");

            return new HandlerResult { StatusCode = (int)ServerResponseCode.Ok };
        }
        #endregion
    }


}
