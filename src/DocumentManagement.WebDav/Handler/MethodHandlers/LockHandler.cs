using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using DocumentManagement.Service;
using DocumentManagement.Service.Models;
using DocumentManagement.WebDav.Helpers;
using DocumentManagment.Common.Enums;

namespace DocumentManagement.WebDav.Handler.MethodHandlers
{
	/// <summary>
	/// http://www.restpatterns.org/HTTP_Methods/LOCK
	/// Handles a WebDav LOCK request for a resource.
	///  SPECIAL NOTES: 
	///  I have only implemented LOCK for resources not for collections. This is because this is meant as a 
	///  server for windows explorer and Office, and neither of these clients support collection locking.
	///  This also makes LOCK Management far more easy, as there is no LOCK hierarchy to manage.
	///  
	///  Currently there is no support for Shared Locks a Office doesn't request them. In the future I will
	///  test the read-only functionality to see what can be incorporated.
	/// 
	/// Request
	///  LOCK /workspace/webdav/proposal.doc HTTP/1.1 
	///  Host: example.com 
	///  Timeout: Infinite, Second-4100000000 
	///  Content-Type: application/xml; charset="utf-8" 
	///  Content-Length: xxxx 
	///  Authorization: Digest username="ejw", 
	///    realm="ejw@example.com", nonce="...", 
	///    uri="/workspace/webdav/proposal.doc", 
	///    response="...", opaque="..." 
	///    
	///  <?xml version="1.0" encoding="utf-8" ?> 
	///  <D:lockinfo xmlns:D='DAV:'> 
	///    <D:lockscope><D:exclusive/></D:lockscope> 
	///    <D:locktype><D:write/></D:locktype> 
	///    <D:owner> 
	///      <D:href>http://example.org/~ejw/contact.html</D:href> 
	///    </D:owner> 
	///  </D:lockinfo> 
	/// 
	/// Response
	///  HTTP/1.1 200 OK 
	///  Lock-Token: <urn:uuid:e71d4fae-5dec-22d6-fea5-00a0c91e6be4> 
	///  Content-Type: application/xml; charset="utf-8" 
	///  Content-Length: xxxx 
	///  
	///  <?xml version="1.0" encoding="utf-8" ?> 
	///  <D:prop xmlns:D="DAV:"> 
	///    <D:lockdiscovery> 
	///      <D:activelock> 
	///        <D:locktype><D:write/></D:locktype> 
	///        <D:lockscope><D:exclusive/></D:lockscope> 
	///        <D:depth>infinity</D:depth> 
	///        <D:owner> 
	///          <D:href>http://example.org/~ejw/contact.html</D:href> 
	///        </D:owner> 
	///        <D:timeout>Second-604800</D:timeout> 
	///        <D:locktoken> 
	///          <D:href
	///          >urn:uuid:e71d4fae-5dec-22d6-fea5-00a0c91e6be4</D:href>
	///        </D:locktoken> 
	///        <D:lockroot> 
	///          <D:href>http://example.com/workspace/webdav/proposal.doc</D:href>
	///        </D:lockroot> 
	///      </D:activelock> 
	///    </D:lockdiscovery> 
	///  </D:prop> 
	//// </summary>
	public class LockHandler : IMethodHandler
	{
		#region LockInfo class

		public class LockInfo
		{
			public LockInfo()
			{
				LockOwner = string.Empty;
				LockToken = string.Empty;
				LockTimeOut = 180;
			}

			public LockType LockType { get; set; }
			public LockScope LockScope { get; set; }
			public LockOwnerType LockOwnerType { get; set; }
			public DepthType LockDepth { get; set; }
			public string LockOwner { get; set; }
			public string LockToken { get; set; }
			public int LockTimeOut { get; set; }
		}

		#endregion


		private readonly HttpApplication _httpApplication;
		private string _requestPath;

		//Default Lock grace period.. Allows for bottlenecking of processing of token refresh
		private const int GraceLockTimeOut = 10; //Seconds
		private readonly XPathNavigator _requestXmlNavigator;

		public LockHandler(HttpApplication httpApplication)
		{

			_httpApplication = httpApplication;
			if (_httpApplication.Request.InputStream.Length != 0)
			{
				_requestXmlNavigator = new XPathDocument(_httpApplication.Request.InputStream).CreateNavigator();
			}
		}

		#region IMethodHandler Interface

		public HandlerResult Handle()
		{
			string responseXml;
			var lockResponseInfo = new LockInfo {LockDepth = WebDavHelper.GetRequestDepth(_httpApplication)};

			//No support for ResourceChildren LOCKing
			if (lockResponseInfo.LockDepth == DepthType.ResourceChildren)
				return new HandlerResult {StatusCode = (int) DavLockResponseCode.BadRequest};

			_requestPath = WebDavHelper.GetRelativePath(_httpApplication, _httpApplication.Request.FilePath);

			//Check to see if this is a lock refresh request.
			if (_httpApplication.Request.Headers["If"] != null)
			{
				lockResponseInfo.LockToken = WebDavHelper.ParseOpaqueLockToken(_httpApplication.Request.Headers["If"]);
				lockResponseInfo.LockTimeOut = WebDavHelper.ParseTimeoutHeader(_httpApplication, lockResponseInfo.LockTimeOut);
					
				//Check to see that the lock exists on the requested resource.
				var lockItem = LockService.GetLockByToken(lockResponseInfo.LockToken);
				if (lockItem == null)
					return new HandlerResult { StatusCode = (int)DavLockResponseCode.PreconditionFailed };

				//Check if the lock is expired , include token grace timeout in calculation
				var span = DateTime.Now.Subtract(lockItem.UpdatedDate);
				if (span.TotalSeconds > lockItem.Timeout + GraceLockTimeOut)
				{
					//the Lock has expired so delete it an return as if it did not exist
					LockService.DeleteLock(lockItem.Id);
					return new HandlerResult { StatusCode = (int)DavLockResponseCode.PreconditionFailed };
				}

				//the lock exists and is not expired so update the timeout and UpdateDate
				lockItem.Timeout = lockResponseInfo.LockTimeOut;
				LockService.SaveLock(lockItem);

				//Send timeout header back to client
				_httpApplication.Response.AppendHeader("Timeout", "Second-" + lockResponseInfo.LockTimeOut);

				//Deserialize the lock token in the DB to get the rest of the data
				DeserializeLock(lockResponseInfo, lockItem);
				responseXml = BuildResponse(lockResponseInfo);
				
				return new HandlerResult { StatusCode = (int)DavLockResponseCode.Ok, ResponseXml = responseXml };
			}

			//This is not a refresh it is a new LOCK request. So check that it is valid
					
			//Check to see if the resource exists
					
			var fileInfo = WebDavHelper.GetFileAttribsOnly(_requestPath);
			if (fileInfo == null)
				return new HandlerResult { StatusCode = (int)DavLockResponseCode.BadRequest };
					  
			//Need to workout how to resolve this problem where office attempts to lock a resource
			//it knows does not exist.
						   

			//Check that it is not already locked
			var fileLock = LockService.GetLockByFile(fileInfo.Id);
			if (fileLock != null)
			{
				//Check if the lock is expired , include token grace timeout in calculation
				var span = DateTime.Now.Subtract(fileLock.UpdatedDate);
				if (span.TotalSeconds > fileLock.Timeout + GraceLockTimeOut)
				{
					//the Lock has expired so delete it an return as if it did not exist
					LockService.DeleteLock(fileLock.Id);
				}

				//File is locked, can open only in readonly
				return new HandlerResult { StatusCode = (int)DavLockResponseCode.Locked };
			}

			//Check that the request XML is valid for the LOCK request
			if (_requestXmlNavigator == null)
				return new HandlerResult { StatusCode = (int)ServerResponseCode.BadRequest };

			//Load the valid properties
			var lockInfoNodeIterator =_requestXmlNavigator.SelectDescendants("lockinfo", "DAV:", false);
			if (!lockInfoNodeIterator.MoveNext())
				return new HandlerResult { StatusCode = (int)ServerResponseCode.BadRequest }; 

			//Create a new Lock
			lockResponseInfo.LockToken   = Guid.NewGuid().ToString("D");
			lockResponseInfo.LockTimeOut = WebDavHelper.ParseTimeoutHeader(_httpApplication, lockResponseInfo.LockTimeOut);     
	 
			//Get the lock type
			var lockType = ReadRequestNodeType("locktype", lockInfoNodeIterator);
			switch (lockType)
			{
				case "read":
					lockResponseInfo.LockType = LockType.Read;
					break;

				default:
					lockResponseInfo.LockType = LockType.Write;
					break;
			}


			//Get the lock scope
			var lockScope = ReadRequestNodeType("lockscope", lockInfoNodeIterator);
			switch (lockScope)
			{
				case "shared":
					lockResponseInfo.LockScope = LockScope.Shared;
					break;

				default:
					lockResponseInfo.LockScope = LockScope.Exclusive;
					break;
			}


			//Get the lock owner

			var lockOwner = ReadRequestNodeType("owner", lockInfoNodeIterator);
			switch (lockOwner)
			{
				case "href":
					lockResponseInfo.LockOwnerType = LockOwnerType.Href;
					break;
				default:
					lockResponseInfo.LockOwnerType = LockOwnerType.User;
					break;;
			}
			lockResponseInfo.LockOwner = ReadRequestNodeValue("owner", lockInfoNodeIterator);

			//Now save the Lock to the DB;
			SaveLock(fileInfo.Id, lockResponseInfo);
			responseXml = BuildResponse(lockResponseInfo);

			return new HandlerResult { StatusCode = (int)DavLockResponseCode.Ok, ResponseXml = responseXml };
		}
		 

		#endregion

		#region Private Handler Methods
		private static void SaveLock(int fileId, LockInfo lockInfo){
			//ResType=0 as we aren't supporting Locked Collections
			var newLockItem = new LockModel
			{
				ResType            = 0,
				LockDepth          = (int)lockInfo.LockDepth,
				LockOwner          = lockInfo.LockOwner,
				LockOwnerType      = (int)lockInfo.LockOwnerType,
				LockScope          = (int)lockInfo.LockScope,
				LockType           = (int)lockInfo.LockType,
				FileId             = fileId,
				Timeout            = lockInfo.LockTimeOut,
				CreatedUser        = WebDavHelper.GetCurrentUserName(),
				UpdatedUser        = WebDavHelper.GetCurrentUserName()
			};

			newLockItem = LockService.SaveLock(newLockItem);

			if (newLockItem.Id == 0) return;

			var lockToken = new LockTokenModel
			{
				Token       = lockInfo.LockToken,
				LockId      = newLockItem.Id,
				CreatedUser = WebDavHelper.GetCurrentUserName(),
				UpdatedUser = WebDavHelper.GetCurrentUserName()
			};

			LockService.SaveLockToken(lockToken);
		}

		private static string ReadRequestNodeType(string node, XPathNodeIterator nodeIterator)
		{
			var iterator = nodeIterator.Current.SelectDescendants(node, "DAV:", false);
			if (iterator.MoveNext())
			{
				var currentNode = iterator.Current;

				if (currentNode.HasChildren)
				{
					if (currentNode.MoveToFirstChild())
					{
						return currentNode.LocalName.ToLower(CultureInfo.InvariantCulture);
					}
				}
			}

			return null;
		}

		private static string ReadRequestNodeValue(string node, XPathNodeIterator nodeIterator)
		{
			var iterator = nodeIterator.Current.SelectDescendants(node, "DAV:", false);
			if (iterator.MoveNext())
			{
				var currentNode = iterator.Current;

				return currentNode.Value;
			}

			return null;
		}



		private static void DeserializeLock(LockInfo input, LockModel lockrow){

			input.LockOwner     = lockrow.LockOwner;
			input.LockDepth     = (DepthType)lockrow.LockDepth;
			input.LockOwnerType = (LockOwnerType)lockrow.LockOwnerType;
			input.LockType      = (LockType)lockrow.LockType;
			input.LockScope     = (LockScope)lockrow.LockScope;
		}

		private string BuildResponse(LockInfo lockInfo)
		{
			using (Stream responseStream = new MemoryStream())
			{
				var xmlWriter = new XmlTextWriter(responseStream, Encoding.UTF8)
				{
					Formatting = Formatting.Indented,
					IndentChar = '\t',
					Indentation = 1,
					Namespaces = true
				};

				xmlWriter.WriteStartDocument();

				//Open the prop element section
				xmlWriter.WriteStartElement("prop", "DAV:");
				xmlWriter.WriteAttributeString("xmlns", "D", null, "DAV:");

				xmlWriter.WriteStartElement("lockdiscovery", "DAV:");

				xmlWriter.WriteStartElement("activelock", "DAV:");

				xmlWriter.WriteStartElement("locktype", "DAV:");

				switch (lockInfo.LockType)
				{
					case LockType.Read:
						xmlWriter.WriteElementString("read", "DAV:", "");
						break;

					case LockType.Write:
						xmlWriter.WriteElementString("write", "DAV:", "");
						break;
				}

				xmlWriter.WriteEndElement();

				xmlWriter.WriteStartElement("lockscope", "DAV:");

				switch (lockInfo.LockScope)
				{
					case LockScope.Exclusive:
						xmlWriter.WriteElementString("exclusive", "DAV:", "");

						break;

					case LockScope.Shared:
						xmlWriter.WriteElementString("shared", "DAV:", "");
						break;
				}
				xmlWriter.WriteEndElement();


				//Append the depth
				if (lockInfo.LockDepth == DepthType.Infinity)
				{
					xmlWriter.WriteElementString("depth", "DAV:", lockInfo.LockDepth.ToString());
				}
				else
				{
					xmlWriter.WriteElementString("depth", "DAV:", lockInfo.LockDepth == DepthType.ResourceOnly ? "0" : "1");
				}


				//Append the owner

				switch (lockInfo.LockOwnerType)
				{
					case LockOwnerType.User:
						xmlWriter.WriteElementString("owner", "DAV:", lockInfo.LockOwner);
						break;
					case LockOwnerType.Href:
						xmlWriter.WriteStartElement("owner", "DAV:");
						xmlWriter.WriteElementString("href", "DAV:", lockInfo.LockOwner);
						xmlWriter.WriteEndElement();
						break;
				}

				//Append the timeout
				xmlWriter.WriteElementString("timeout", "DAV:", "Second-" + lockInfo.LockTimeOut);

				//Append the lockToken
				xmlWriter.WriteStartElement("locktoken", "DAV:");
				//xmlWriter.WriteElementString("href", "DAV:", "opaquelocktoken:" + lockInfo.LockToken);
				xmlWriter.WriteElementString("href", "DAV:", "urn:uuid:" + lockInfo.LockToken);
				xmlWriter.WriteEndElement();


				xmlWriter.WriteStartElement("lockroot", "DAV:");
				xmlWriter.WriteElementString("href", "DAV:", _httpApplication.Request.Url.ToString());
				xmlWriter.WriteEndElement();

				//close activelock
				xmlWriter.WriteEndElement();
				//close lockdiscovery
				xmlWriter.WriteEndElement();

				xmlWriter.WriteEndDocument();
				xmlWriter.Flush();

				string responseXml = WebDavHelper.StreamtoString(responseStream);
				xmlWriter.Close();

				_httpApplication.Response.Headers["Lock-Token"] = string.Format("<urn:uuid:{0}>", lockInfo.LockToken);

				return responseXml;
			}
		}
		#endregion



	}
}
