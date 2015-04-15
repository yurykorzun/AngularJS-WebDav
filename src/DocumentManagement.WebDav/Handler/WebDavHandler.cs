using System;
using System.Web;
using Common.Logging;
using DocumentManagement.WebDav.Handler.MethodHandlers;
using DocumentManagement.WebDav.Helpers;
using DocumentManagment.Common.Enums;
using Microsoft.Ajax.Utilities;

namespace DocumentManagement.WebDav.Handler
{
    /// <summary>
    /// PURPOSE
    //  Handles first call from IHttpModule, checks the METHOD and hands of to the relevant method handler
    //  SPECIAL NOTES: 
    //  Version control and PROPPATCH are not implemented in this version as neither Windows Explorer or office support
    //  these under WEBDAV. Please see summary messages for each method handler for information on how each method is handled.
    /// </summary>
    public class WebDavHandler
    {
        public static void HandleRequest(HttpApplication httpApplication)
        {
            var log = LogManager.GetCurrentClassLogger();

            //Set the status code to NotImplemented by default
            HandlerResult handlerResult = null;
            var httpMethod =string.Empty;

            IMethodHandler methodHandler = null;

            if (httpApplication == null)
                throw new ArgumentNullException("httpApplication", "No Handle to HttpApplication Found");

            try
            {
                //Don't handle anything with the _MWDRes query string, this means that this is a request
                //for a local resource and should not be handled by this Handler. It should fall through
                //to the asp .net engine to handle.
                if (httpApplication.Request.QueryString["_MWDRes"] != null)
                    return;

                //Make sure we don't handle anything from the IDE.
                if (httpApplication.Request.Headers["User-Agent"] != null && !httpApplication.Request.Headers["User-Agent"].StartsWith("Microsoft-Visual-Studio.NET"))
                {

                    httpMethod = httpApplication.Request.HttpMethod.ToUpper();

                    switch (httpMethod)
                    {

                        case "OPTIONS":
                            methodHandler = new OptionsHandler(httpApplication);
                            break;
                        case "MKCOL":
                            methodHandler = new MkcolHandler(httpApplication);
                            break;
                        case "PROPFIND":
                            methodHandler = new PropFindHandler(httpApplication);
                            break;
                        case "HEAD":
                            methodHandler = new HeadHandler(httpApplication);
                            break;
                        case "DELETE":
                            methodHandler = new DeleteHandler(httpApplication);
                            break;
                        case "MOVE":
                            methodHandler = new MoveHandler(httpApplication);
                             break;
                        case "COPY":
                            methodHandler = new CopyHandler(httpApplication);
                             break;
                        case "PUT":
                            methodHandler = new PutHandler(httpApplication);
                            break;
                        case "GET":
                            methodHandler = new GetHandler(httpApplication);
                            break;
                        case "LOCK":
                            methodHandler = new LockHandler(httpApplication);
                            break;
                        case "UNLOCK":
                            methodHandler = new UnlockHandler(httpApplication);
                            break;
                        case "PROPPATCH":
                            break;
                        default:
                            handlerResult = new HandlerResult { StatusCode = (int)ServerResponseCode.MethodNotImplemented };
                            break;

                    }
                    if (methodHandler != null)
                    {
                        handlerResult = methodHandler.Handle();
                    }
                }
            }
            catch (Exception ex)
            {
                httpApplication.Response.StatusCode = (int)ServerResponseCode.BadRequest;
                httpApplication.Response.Write(WebDavHelper.GetHtmlErrorMsg(ex.ToString()));
                httpApplication.Response.ContentType = "text/html";
                httpApplication.Response.End();

                log.Error("Error--------------------");
                log.Error(httpMethod);
                log.Error(ex.ToString());

                return;

            }

            if (handlerResult == null)
            {
                handlerResult = new HandlerResult {StatusCode = (int) ServerResponseCode.BadRequest};
            }

            if (httpMethod == "GET" && !handlerResult.ErrorXml.IsNullOrWhiteSpace())
            {
                //this is a request for a folder that has been redirected through pass-through so handle the redirection
                httpApplication.Response.Redirect(handlerResult.ErrorXml, true);
                return;
            }

            httpApplication.Response.StatusCode = handlerResult.StatusCode;

            if (!handlerResult.ErrorXml.IsNullOrWhiteSpace())
            {
                httpApplication.Response.StatusCode = (int)ServerResponseCode.MultiStatus;
                httpApplication.Response.ContentEncoding = System.Text.Encoding.UTF8;
                httpApplication.Response.ContentType = "text/xml";
                httpApplication.Response.Write(handlerResult.ErrorXml);
            }
            else
            {
                if (!handlerResult.ResponseXml.IsNullOrWhiteSpace())
                {
                    httpApplication.Response.ContentEncoding = System.Text.Encoding.UTF8;
                    httpApplication.Response.ContentType = "text/xml";
                    httpApplication.Response.Write(handlerResult.ResponseXml);
                }
            }

            httpApplication.Response.End();

        }
    }
}
