using System.IO;
using System.Web;
using DocumentManagement.WebDav.Helpers;
using DocumentManagment.Common.Enums;

namespace DocumentManagement.WebDav.Handler.MethodHandlers
{
	/// <summary>
	/// http://www.webdav.org/specs/rfc4918.html#rfc.section.9.4
	/// http://www.restpatterns.org/HTTP_Methods/GET
	/// 
	/// The GET method means retrieve whatever information (in the form of an entity) is identified by the Request-URI. 
	/// </summary>
	public class GetHandler : IMethodHandler
	{
		private readonly HttpApplication _httpApplication;
		private string _requestPath;

		private const HttpCacheability ResponseCache = HttpCacheability.NoCache;

		public GetHandler(HttpApplication httpApplication)
		{

			_httpApplication = httpApplication;

		}

		#region IMethodHandler Interface

		public HandlerResult Handle()
		{
			var errorxml = string.Empty;

			if (_httpApplication==null)
				return new HandlerResult { StatusCode = (int)ServerResponseCode.BadRequest };

			_requestPath = WebDavHelper.GetRelativePath(_httpApplication, _httpApplication.Request.FilePath);

			var folderInfo = WebDavHelper.GetFolder(_requestPath);
			var fileInfo   = WebDavHelper.GetFile(_requestPath);

			if (fileInfo == null){

				if (folderInfo == null)
				{
					_httpApplication.Response.ContentType = "text/html";
					_httpApplication.Response.Write(WebDavHelper.GetHtmlErrorMsg(ServerResponseCode.NotFound));
				}
				else
				{
					//redirect the Get request to the pass-through or this folder. The _MWDRes will make sure the 
					//redirected page will be handled by the ASP engine not this handler.
					errorxml = _httpApplication.Request.ApplicationPath + "/Folder.aspx?FPath=" + HttpUtility.UrlEncode(_requestPath) + "&_MWDRes=1";
				}

				return new HandlerResult
				{
					StatusCode = (int)ServerResponseCode.NotFound,
					ErrorXml = errorxml
				}; 
			}

			_httpApplication.Response.Cache.SetCacheability(ResponseCache);
			_httpApplication.Response.ContentType = fileInfo.ContentType;
			_httpApplication.Response.AddHeader("Content-Disposition", "attachment; filename=\"" + HttpUtility.HtmlEncode(fileInfo.FileName) + "\"");
			_httpApplication.Response.AddHeader("Content-Length", fileInfo.FileDataSize.ToString());
			 
			
			using (var outputStream = new BinaryWriter(_httpApplication.Response.OutputStream))
			{
				outputStream.Write(fileInfo.FileData);
				outputStream.Close();
			}

			return new HandlerResult { StatusCode = (int)ServerResponseCode.Ok }; 
		}
		
		#endregion
	}
}
