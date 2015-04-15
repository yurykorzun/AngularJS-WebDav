//  ============================================================================
//  AUTHOR		 : Simon
//  CREATE DATE	 : 30 Apr 2007
//  PURPOSE		 : Class to build a XML representation of a File record in the WebDav Namespace.
//  SPECIAL NOTES: 
//  (
//  ===========================================================================

using System.Collections;
using System.Globalization;
using System.Xml;
using DocumentManagement.Service;
using DocumentManagement.Service.Models;
using DocumentManagement.WebDav.Helpers;
using DocumentManagment.Common.Enums;

namespace DocumentManagement.WebDav.XMLDBObjects
{
   
	/// <summary>
	/// Class to build a XML representation of a File record in the WebDav Namespace.
	/// </summary>
	public static class XMLWebDavFile
	{
		#region WebDav Response Example
		/* Example of a WEBDAV PropFind Response for a File Resource
	 
	  <D:response>
	   <D:href>File.doc</D:href>
	   <D:propstat>
		   <D:status>HTTP/1.1 200 OK</D:status>
		   <D:prop>
			   <D:ContentLanguage>shendry</D:ContentLanguage>
			   <D:CreationDate b:dt="dateTime.tz">2007-04-20T15:01:37</D:CreationDate>
			   <D:lockdiscovery />
			   <D:ContentLength>34816</D:ContentLength>
			   <D:FilePath>Adoption.doc</D:FilePath>
			   <D:supportedlock>0<D:lockentry><D:lockscope><D:exclusive /></D:lockscope><D:locktype><D:write /></D:locktype></D:lockentry><D:lockentry><D:lockscope><D:shared /></D:lockscope><D:locktype><D:write /></D:locktype></D:lockentry></D:supportedlock>
			   <D:isHidden>0</D:isHidden>
			   <D:LastModified b:dt="dateTime.rfc1123">Tue, 24 Apr 2007 15:54:32 GMT</D:LastModified>
			   <D:ContentType>application/msword</D:ContentType>
			   <D:DisplayName>File.doc</D:DisplayName>
				
		   </D:prop>
	   </D:propstat>
	   <D:propstat>
		   <D:status>HTTP/1.1 404 Not Found</D:status>
		   <D:prop>
			   <D:LastAccessed />
		   </D:prop>
	   </D:propstat>
   </D:response>
		* 
		*/
		#endregion

		#region XML Generation

		/// <summary>
		/// Generates the D:Response for a given backend File Row
		/// <param name="file">A FileDS.FileRow of a file record in the File Table</param>
		/// <param name="xmlWriter">The xmlWriter to add the xml to</param>
		/// <param name="reqProps">The list of request properties in the PROPFIND request if any</param>
		/// </summary>

		public static void GetXML(FileModel file,  XmlTextWriter xmlWriter, RequestedPropertyCollection reqProps, PropertyRequestType requestType)
		{
						
			//Load the Valid Properties for the file resource
			var validProps = new ArrayList
			{
				"contentlanguage",
				"contentlength",
				"contenttype",
				"creationdate",
				"displayname",
				"filepath",
				"ishidden",
				"lastaccessed",
				"lastmodified",
				"lockdiscovery",
				"resourcetype",
				"supportedlock",
				"name",
				"parentname",
				"isreadonly",
				"contentclass",
				"iscollection",
				"isstructureddocument",
				"defaultdocument",
				"isroot",
				"href"
			};

			var inValidProps = new RequestedPropertyCollection();

			if (requestType == PropertyRequestType.PropertyNames && reqProps != null) return;

			if (file ==null) return;

			//Open the response element
			xmlWriter.WriteStartElement("response", "DAV:");
			xmlWriter.WriteStartElement("R", "response", "http://ns.example.com/boxschema/");

			//Load the valid items HTTP/1.1 200 OK
			xmlWriter.WriteElementString("href", "DAV:", file.FileName);

			//Open the propstat element section
			xmlWriter.WriteStartElement("propstat", "DAV:");
			xmlWriter.WriteElementString("status", "DAV:", WebDavHelper.GetEnumHttpResponse(ServerResponseCode.Ok));

			//Open the prop element section
			xmlWriter.WriteStartElement("prop", "DAV:");
			
			//If there are no requested Properties then return all props for File.
			if (reqProps == null)
			{
				reqProps = new RequestedPropertyCollection();
				foreach (object prop in validProps)
				{
					reqProps.Add(new RequestedProperty((string)prop,"DAV:"));
				}
			}

			foreach (RequestedProperty _ReqProp in reqProps)
			{
				string propertyName = _ReqProp.LocalName;
				if (propertyName.ToLower(CultureInfo.InvariantCulture).StartsWith("get"))
					propertyName = propertyName.Substring(3);

				if ((_ReqProp.NS != "DAV:") || (validProps.IndexOf(propertyName.ToLower()) == -1))
				{
					inValidProps.Add(_ReqProp);
				}else{

					if (requestType == PropertyRequestType.PropertyNames)
					{
						//if this is a request for property names only then just return the named elements:

						xmlWriter.WriteElementString(propertyName, _ReqProp.NS, "");
					   
					}
					else
					{
						//Map the property to the Row Data and return the PropStat XML.

						switch (propertyName.ToLower())
						{

							case "contentlanguage":
								xmlWriter.WriteElementString(_ReqProp.LocalName, "DAV:", "en-us");
								break;
							case "contentlength":
								xmlWriter.WriteElementString(_ReqProp.LocalName, "DAV:", file.FileDataSize.ToString());
								break;
							case "contenttype":
								xmlWriter.WriteElementString(_ReqProp.LocalName, "DAV:", file.ContentType);
								break;
							case "displayname":
								xmlWriter.WriteElementString(_ReqProp.LocalName, "DAV:", file.FileName);
								break;
							case "filepath":
								xmlWriter.WriteElementString(_ReqProp.LocalName, "DAV:", file.FileName);
								break;
							case "ishidden":
								//May adjust later to allow hidden files
								xmlWriter.WriteElementString(_ReqProp.LocalName, "DAV:", "false");
								break;
							case "isreadonly":
								//May adjust later to allow read-only access later
								xmlWriter.WriteElementString(_ReqProp.LocalName, "DAV:", "false");
								break;
							case "resourcetype":
								xmlWriter.WriteElementString(_ReqProp.LocalName, "DAV:", "");
								break;
							case "lastaccessed":
								xmlWriter.WriteStartElement(_ReqProp.LocalName, "DAV:");
								xmlWriter.WriteAttributeString("b:dt", "dateTime.rfc1123");

								//This change is outside the spec for MS. If you set this date is set to the rfc1123 compliant dateformat
								//the Windows Explorer errors out.
																 
								// xmlWriter.WriteString(_fdr.UpdateDate.ToUniversalTime().ToString("r", CultureInfo.InvariantCulture));

								xmlWriter.WriteString(file.UpdatedDate.ToUniversalTime().ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture));
								xmlWriter.WriteEndElement();
								break;

							case "lastmodified":
								xmlWriter.WriteStartElement(_ReqProp.LocalName, "DAV:");
								xmlWriter.WriteAttributeString("b:dt", "dateTime.rfc1123");
								xmlWriter.WriteString(file.UpdatedDate.ToUniversalTime().ToString("r", CultureInfo.InvariantCulture));
								xmlWriter.WriteEndElement();
								break;
						   
							case "creationdate":
								xmlWriter.WriteStartElement(_ReqProp.LocalName, "DAV:");
								xmlWriter.WriteAttributeString("b:dt", "dateTime.tz");
								xmlWriter.WriteString(file.CreatedDate.ToUniversalTime().ToString("s", CultureInfo.InvariantCulture));
								xmlWriter.WriteEndElement();
								break;
							case "lockdiscovery":
								xmlWriter.WriteStartElement("D", "lockdiscovery", "DAV:");
								XMLWebDavLock.GetXML(LockService.GetLockByFile(file.Id), xmlWriter);
								xmlWriter.WriteEndElement();
								break;
							case "supportedlock":

								xmlWriter.WriteStartElement("D", _ReqProp.LocalName, "DAV:");

								xmlWriter.WriteStartElement("lockentry", "DAV:");
								xmlWriter.WriteStartElement("lockscope", "DAV:");
								xmlWriter.WriteElementString("exclusive", "DAV:", "");
								xmlWriter.WriteEndElement();
								xmlWriter.WriteStartElement("locktype", "DAV:");
								xmlWriter.WriteElementString("write", "DAV:", "");
								xmlWriter.WriteEndElement();
								xmlWriter.WriteEndElement();

								xmlWriter.WriteStartElement("lockentry", "DAV:");
								xmlWriter.WriteStartElement("lockscope", "DAV:");
								xmlWriter.WriteElementString("shared", "DAV:", "");
								xmlWriter.WriteEndElement();
								xmlWriter.WriteStartElement("locktype", "DAV:");
								xmlWriter.WriteElementString("write", "DAV:", "");
								xmlWriter.WriteEndElement();
								xmlWriter.WriteEndElement();
								xmlWriter.WriteEndElement();

								break;
							default:
								xmlWriter.WriteElementString(_ReqProp.LocalName, "DAV:","");
								break;
						}
					}
				}
					
			}   
			
			//Close the prop element section
			xmlWriter.WriteEndElement();

			//Close the propstat element section
			xmlWriter.WriteEndElement();
			//END Load the valid items HTTP/1.1 200 OK
			
			//Load the invalid items HTTP/1.1 404 Not Found
			if (inValidProps.Count > 0)
			{
				xmlWriter.WriteStartElement("propstat", "DAV:");
				xmlWriter.WriteElementString("status", "DAV:", WebDavHelper.GetEnumHttpResponse(ServerResponseCode.NotFound));

				//Open the prop element section
				xmlWriter.WriteStartElement("prop", "DAV:");

				//Load all the invalid properties
				foreach (RequestedProperty inValidProp in inValidProps)
				   xmlWriter.WriteElementString(inValidProp.LocalName, inValidProp.NS,"");
				  
				//Close the prop element section
				xmlWriter.WriteEndElement();
				//Close the propstat element section
				xmlWriter.WriteEndElement();
			}
			//END Load the invalid items HTTP/1.1 404 Not Found

			//Close the response element
			xmlWriter.WriteEndElement();

		}
	   
	  
	   #endregion
			
	}
	
}

