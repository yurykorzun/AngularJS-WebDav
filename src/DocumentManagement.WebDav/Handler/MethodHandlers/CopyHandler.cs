using System;
using System.Web;
using DocumentManagement.Service;
using DocumentManagement.Service.Models;
using DocumentManagement.WebDav.Helpers;
using DocumentManagement.WebDav.XMLDBObjects;
using DocumentManagment.Common.Enums;
using Microsoft.Ajax.Utilities;

namespace DocumentManagement.WebDav.Handler.MethodHandlers
{
	/// <summary>
	/// http://www.webdav.org/specs/rfc4918.html#METHOD_COPY
	/// http://www.restpatterns.org/HTTP_Methods/COPY
	/// Not implemented the keepAliveURis for a copy, all properties of a resource are copies by default, even if the client 
	//  specifies otherwise.This is because this is meant as a server for windows explorer and Office, neither of these clients support keepAliveURis 
	//  In fact neither of these client send any form of request XML.
	/// Request
	/// COPY /~fielding/index.html HTTP/1.1 
	/// Host: www.example.com 
	/// Destination: http://www.example.com/users/f/fielding/index.html 
	/// 
	/// Response
	///  HTTP/1.1 204 No Content 
	/// </summary>
	public class CopyHandler : IMethodHandler
	{
		private readonly HttpApplication _httpApplication;
		private string _requestPath;
		private string _destinationPath;
	   
		private bool _overwriteResource;


		public CopyHandler(HttpApplication httpApplication)
		{

			_httpApplication = httpApplication;
		}

		#region IMethodHandler Interface

		public HandlerResult Handle()
		{
			var errors = new ProcessingErrorCollection();
			if (_httpApplication == null)
				return new HandlerResult { StatusCode = (int)ServerResponseCode.BadRequest };

			var isValidRequest = ValidateRequest();
			if (!isValidRequest)
				return new HandlerResult { StatusCode = (int) ServerResponseCode.BadRequest };

			_requestPath = WebDavHelper.GetRelativePath(_httpApplication, _httpApplication.Request.FilePath);
			_destinationPath = GetRequestDestination();

			//Check to make sure the resource is valid
			if (_requestPath == _destinationPath)
				return new HandlerResult { StatusCode = (int)DavCopyResponseCode.Conflict };

			if (!WebDavHelper.ValidResourceByPath(_requestPath))
				return new HandlerResult { StatusCode = (int)ServerResponseCode.NotFound};

			//Make sure the destination directory exists
			var destFolder = WebDavHelper.GetParentFolder(_destinationPath);
			if (destFolder == null)
				return new HandlerResult { StatusCode = (int)DavCopyResponseCode.Conflict };

			var sourceFile = WebDavHelper.GetFile(_requestPath);
			var sourceDir = WebDavHelper.GetFolder(_requestPath);
			if (sourceDir != null)
			{
				//How much do we want to copy?
				switch (WebDavHelper.GetRequestDepth(_httpApplication))
				{
					case DepthType.ResourceOnly:
						try
						{
							CreateDirectory(_destinationPath);
						}
						catch (Exception)
						{
							errors.Add(new ProcessingError(WebDavHelper.GetResourceName(_destinationPath), WebDavHelper.GetEnumHttpResponse(DavCopyResponseCode.InsufficientStorage)));
						}
						break;

					case DepthType.Infinity:
						try
						{
							var lockedFileName = CheckforDestinationLocks(sourceDir, _destinationPath);
							if (!lockedFileName.IsNullOrWhiteSpace())
							{
								errors.Add(new ProcessingError(lockedFileName, WebDavHelper.GetEnumHttpResponse(DavCopyResponseCode.Locked)));
							}
						}
						catch (Exception)
						{
							errors.Add(new ProcessingError(sourceDir.FolderName, WebDavHelper.GetEnumHttpResponse(DavCopyResponseCode.Forbidden)));
						}

						if (errors.Count == 0)
						{
							try
							{
								var cloneErrors = CloneDirectory(sourceDir, _destinationPath);
								if (cloneErrors.Count > 0)
								{
									foreach (ProcessingError copyError in cloneErrors)
									{
										errors.Add(copyError);
									}
								}

							}
							catch (Exception)
							{
								errors.Add(new ProcessingError(sourceDir.FolderName, WebDavHelper.GetEnumHttpResponse(DavCopyResponseCode.Forbidden)));
							}
						}	
						break;
				}
				return new HandlerResult { StatusCode = (int)DavCopyResponseCode.Created, ErrorXml = XMLWebDavError.ProcessErrorCollection(errors) };
			}

			if (sourceFile == null)
				return new HandlerResult { StatusCode = (int)DavCopyResponseCode.BadGateway, ErrorXml = XMLWebDavError.ProcessErrorCollection(errors) };

			try
			{
				if (CheckforDestinationLocks(sourceFile.FileName, _destinationPath))
				{
					errors.Add(new ProcessingError(sourceFile.FileName, WebDavHelper.GetEnumHttpResponse(DavCopyResponseCode.Locked)));
				}
			}
			catch (Exception)
			{
				errors.Add(new ProcessingError(sourceFile.FileName, WebDavHelper.GetEnumHttpResponse(DavCopyResponseCode.Forbidden)));
			}


			if (errors.Count == 0)
			{
				try
				{
					var copyErrors = CopyFile(sourceFile, _destinationPath);
					if (copyErrors.Count > 0)
					{
						foreach (ProcessingError copyError in copyErrors)
						{
							errors.Add(copyError);
						}
					}
				}
				catch (Exception)
				{
					errors.Add(new ProcessingError(sourceFile.FileName, WebDavHelper.GetEnumHttpResponse(DavCopyResponseCode.Forbidden)));
				}
			}


			return new HandlerResult { StatusCode = (int)DavCopyResponseCode.Created, ErrorXml = XMLWebDavError.ProcessErrorCollection(errors) };
		}

		#endregion

		#region private handler methods
		private bool ValidateRequest()
		{
			if ((WebDavHelper.GetRequestDepth(_httpApplication) != DepthType.ResourceOnly) && (WebDavHelper.GetRequestDepth(_httpApplication) != DepthType.Infinity))
				return false;
			if (_httpApplication.Request.Headers["Destination"] == null)
				return false;

			_overwriteResource = true;

			if (_httpApplication.Request.Headers["Overwrite"] != null)
				_overwriteResource = _httpApplication.Request.Headers["Overwrite"] != "f";

			return true;
		}

		private string GetRequestDestination()
		{
			string destination =_httpApplication.Request.Headers["Destination"];
			destination = destination == null ? string.Empty : HttpUtility.UrlDecode(destination.Trim('/'));
			return WebDavHelper.GetRelativePath(_httpApplication, destination);
		}

		private string CheckforDestinationLocks(FolderModel sourceDirectory, string destination)
		{
			//loop through the destination and determine if there are any filelocks
			// if they exist then add the error to the errorlist
			if (sourceDirectory != null)
			{
				var destFolder = WebDavHelper.GetFolder(destination);

				if (destFolder == null) return null;

				var filelist = FileService.GetFileByParentFolder(sourceDirectory.Id);

				foreach (var sfr in filelist)
				{
					var destfile = WebDavHelper.GetFileAttribsOnly(destination + "/" + sfr.FileName);
					if (destfile == null) continue;

					if (LockService.GetLockByFile(destfile.Id) != null)
					{
						//We already have an error so leave now
						return sfr.FileName;
					}
				}

				var folders = FolderService.GetFolderByParentId(sourceDirectory.Id);

				foreach (var dir in folders)
					CheckforDestinationLocks(dir, destination + "/" + dir.FolderName);
			}

			return null;
		}

		private bool CheckforDestinationLocks(string sourceName, string destination)
		{
			//Check for a lock at a destination string(used for single filecopy only)
			//if they exist then add the error to the errorlist
			var destFile = WebDavHelper.GetFileAttribsOnly(destination);

			if (destFile == null) return false;

			return LockService.GetLockByFile(destFile.Id) != null;
		}

		private ProcessingErrorCollection CloneDirectory(FolderModel sourceDirectory, string destination)
		{
			var errors = new ProcessingErrorCollection();
			if (sourceDirectory != null)
			{
				var destFolder = WebDavHelper.GetFolder(destination);

				if (!_overwriteResource && destFolder != null)
				{
					errors.Add(new ProcessingError(sourceDirectory.FolderName, WebDavHelper.GetEnumHttpResponse(DavCopyResponseCode.PreconditionFailed)));
				}
				else
				{
					//Create the destination directory
					if (destFolder == null)
					{
						CreateDirectory(destination);
					}
					//Move over the directory files
					var filelist = FileService.GetFileByParentFolder(sourceDirectory.Id);

					foreach (var sfr in filelist)
					{
						var file = FileService.GetFile(sfr.Id);
						CopyFile(file, destination + "/" + file.FileName);
					}

					var dirlist = FolderService.GetFolderByParentId(sourceDirectory.Id);

					foreach (var dir in dirlist)
						CloneDirectory(dir, destination + "/" + dir.FolderName);
				}
			}

			return errors;
		}

		private ProcessingErrorCollection CopyFile(FileModel sourceFile, string destination)
		{
			var errors = new ProcessingErrorCollection();
			if (sourceFile != null)
			{
				var destFile = WebDavHelper.GetFile(destination);

				if (!_overwriteResource && destFile != null)
				{
					errors.Add(new ProcessingError(sourceFile.FileName, WebDavHelper.GetEnumHttpResponse(DavCopyResponseCode.PreconditionFailed)));
				}
				else
				{
					var destdir = WebDavHelper.GetParentFolder(destination);
					if (destdir == null)
					{
						errors.Add(new ProcessingError(sourceFile.FileName, WebDavHelper.GetEnumHttpResponse(DavCopyResponseCode.BadGateway)));
					}
					else
					{
						if (destFile == null)
						{
							destFile = new FileModel();     
						}

						destFile.ContentType       = sourceFile.ContentType;
						destFile.FileData          = sourceFile.FileData;
						destFile.FileDataSize      = sourceFile.FileDataSize;
						destFile.FileName          = sourceFile.FileName;
						destFile.ParentFolderId    = destdir.Id;
						destFile.CreatedUser       = WebDavHelper.GetCurrentUserName();
						destFile.UpdatedUser       = WebDavHelper.GetCurrentUserName();

						FileService.SaveFile(destFile);
					}
				}

			}
			return errors;
		}

		private static void CreateDirectory(string path)
		{
			var parentFolder = WebDavHelper.GetParentFolder(path);

			var folder = new FolderModel
			{
				ParentFolderId    = parentFolder.Id,
				FolderName        = WebDavHelper.GetResourceName(path),
				CreatedUser       = WebDavHelper.GetCurrentUserName(),
				UpdatedUser       = WebDavHelper.GetCurrentUserName()
			};

			FolderService.SaveFolder(folder);
		}

		#endregion
	}
}