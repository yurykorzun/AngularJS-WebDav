using System;
using System.Web;
using DocumentManagement.Service;
using DocumentManagement.Service.Models;
using DocumentManagement.WebDav.Helpers;
using DocumentManagment.Common.Enums;

namespace DocumentManagement.WebDav.Handler.MethodHandlers
{
	/// <summary>
	/// http://www.restpatterns.org/HTTP_Methods/MKCOL
	/// http://www.webdav.org/specs/rfc4918.html#METHOD_MKCOL
	/// 
	/// MKCOL creates a new collection resource (folder) at the location specified by the Request-URI. 
	/// 
	/// Request
	///  MKCOL /webdisc/xfiles/ HTTP/1.1 
	/// Host: www.example.com 
	/// 
	/// Response
	/// HTTP/1.1 201 Created 
	/// </summary>
	public class MkcolHandler: IMethodHandler
	{

		private readonly HttpApplication _httpApplication;
		private string _requestPath;
		
		public MkcolHandler(HttpApplication httpApplication) {

			_httpApplication = httpApplication;

		}

		#region IMethodHandler Interface

		public HandlerResult Handle()
		{
			if (_httpApplication==null)
				return new HandlerResult { StatusCode = (int)ServerResponseCode.BadRequest };

			_requestPath = WebDavHelper.GetRelativePath(_httpApplication, _httpApplication.Request.FilePath);
			
			//Check to see if the RequestPath is already a resource
			if (WebDavHelper.ValidResourceByPath(_requestPath))
				return new HandlerResult { StatusCode = (int)DavMKColResponseCode.MethodNotAllowed };

			//Check to see if the we can create a new folder
			var parentFolder = WebDavHelper.GetParentFolder(_requestPath);

			//The parent folder does not exist
			if (parentFolder == null)
				return new HandlerResult { StatusCode = (int)DavMKColResponseCode.Conflict };

			var requestedFolder = WebDavHelper.GetResourceName(_requestPath);

			try {
											 
				var folder = new FolderModel
				{
					ParentFolderId = parentFolder.Id,
					FolderName = requestedFolder
				};

				if (FolderService.SaveFolder(folder).Id == 0){
					return new HandlerResult { StatusCode = (int)DavMKColResponseCode.InsufficientStorage };
				}
						
			}
			catch (Exception) {
				return new HandlerResult { StatusCode = (int)DavMKColResponseCode.InsufficientStorage };
			}

			return new HandlerResult { StatusCode = (int)DavMKColResponseCode.Created };
		}
			
				
		#endregion
	}
}
