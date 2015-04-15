using System.IO;
using System.Web;
using DocumentManagement.Service;
using DocumentManagement.Service.Models;
using DocumentManagement.WebDav.Helpers;
using DocumentManagment.Common.Enums;
using Microsoft.Win32;

namespace DocumentManagement.WebDav.Handler.MethodHandlers
{
	/// <summary>
	/// http://www.restpatterns.org/HTTP_Methods/PUT
	/// http://www.webdav.org/specs/rfc4918.html#METHOD_PUT
	/// A PUT performed on an existing resource replaces the GET response entity of the resource. 
	/// </summary>
	public class PutHandler : IMethodHandler
	{
		private readonly HttpApplication _httpApplication;

		private string _requestPath;

		public PutHandler(HttpApplication httpApplication)
		{

			_httpApplication = httpApplication;

		}

		#region IMethodHandler Interface

		public HandlerResult Handle()
		{
			if (_httpApplication == null)
				return new HandlerResult { StatusCode = (int)ServerResponseCode.BadRequest };

			if (WebDavHelper.GetRequestLength(_httpApplication) == 0)
				return new HandlerResult { StatusCode = (int)DavPutResponseCode.Created };

			_requestPath = WebDavHelper.GetRelativePath(_httpApplication, _httpApplication.Request.FilePath);

		   var parentFolder = WebDavHelper.GetParentFolder(_requestPath);

		   //The parent folder does not exist
			if (parentFolder == null)
			{
				return new HandlerResult { StatusCode = (int)ServerResponseCode.NotFound };
			}

			if (OverwriteExistingResource())
			{
				if (SaveFile(parentFolder.Id))
					return new HandlerResult { StatusCode = (int)DavPutResponseCode.Created };
			}

			//Check to see if the resource already exists
			return WebDavHelper.GetFileAttribsOnly(_requestPath) != null ? 
				new HandlerResult { StatusCode = (int)DavPutResponseCode.Conflict } : 
				new HandlerResult { StatusCode = (int)DavPutResponseCode.InsufficientStorage };
		}

	   #endregion

		#region private handler methods
		private bool OverwriteExistingResource()
		{
			return _httpApplication.Request.Headers["If-None-Match"] == null;
		}

		private byte[] GetRequestInput()
		{
			var inputStream = new StreamReader(_httpApplication.Request.InputStream);
			var inputSize   = inputStream.BaseStream.Length;
			var inputBytes  = new byte[inputSize];

			inputStream.BaseStream.Read(inputBytes, 0, (int)inputSize);
			return inputBytes;
		}

		private bool SaveFile(int parentId)
		{
			var requestInput = GetRequestInput();

			var newFile = WebDavHelper.GetFile(_requestPath);

			if (newFile == null)
			{
				newFile = new FileModel
				{
					FileData       = requestInput,
					FileDataSize   = requestInput.LongLength,
					ParentFolderId = parentId,
					FileName       = WebDavHelper.GetResourceName(_requestPath),
					CreatedUser    = WebDavHelper.GetCurrentUserName(),
					UpdatedUser    = WebDavHelper.GetCurrentUserName()
				};
				newFile.ContentType = GetMIMEType(newFile.FileName);
				newFile.UpdatedUser = WebDavHelper.GetCurrentUserName();

				return FileService.SaveFile(newFile).Id != 0;
			}

			newFile.FileData     = requestInput;
			newFile.FileDataSize = requestInput.LongLength;

			return FileService.SaveFile(newFile).Id != 0;
		}

		private string GetMIMEType(string fileName)
		{
			if (fileName.Contains("."))
			{
				string[] dot = fileName.Split(@".".ToCharArray());

				if (dot[dot.Length - 1] == "exe") return "application/octet-stream";

				var rkContentTypes = Registry.ClassesRoot.OpenSubKey("." + dot[dot.Length - 1]);
				if (rkContentTypes != null)
				{
					object key = rkContentTypes.GetValue("Content Type", "binary/octet-stream");
					return key.ToString();
				}
				return "binary/octet-stream";
			}
			return "binary/octet-stream";
		}

		#endregion

	}
	}