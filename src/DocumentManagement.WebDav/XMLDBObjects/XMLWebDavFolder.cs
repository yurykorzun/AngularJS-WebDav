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
using DocumentManagement.Service.Models;
using DocumentManagement.WebDav.Helpers;
using DocumentManagment.Common.Enums;

namespace DocumentManagement.WebDav.XMLDBObjects
{

	/// <summary>
	/// Class to build a XML representation of a File record in the WebDav Namespace.
	/// </summary>
	public static class XMLWebDavFolder
	{
		#region WebDav Response Example
		/* Example of a WEBDAV PropFind Response for a Folder Resource
	 
	  <D:response>
		<D:href>New Folder</D:href>
		<D:propstat>
			<D:status>HTTP/1.1 200 OK</D:status>
			<D:prop>
				<D:ishidden>0</D:ishidden>
				<D:getcontenttype>application/webdav-collection</D:getcontenttype>
				<D:getcontentlanguage>en-us</D:getcontentlanguage>
				<D:creationdate b:dt="dateTime.tz">2007-04-17T11:56:04</D:creationdate>
				<D:getlastmodified b:dt="dateTime.rfc1123">Tue, 17 Apr 2007 11:57:28 GMT</D:getlastmodified>
				<D:getcontentlength>2</D:getcontentlength>
				<D:displayname>New Folder</D:displayname>
				<D:resourcetype>
					<D:collection />
				</D:resourcetype>
			</D:prop>
		</D:propstat>
		<D:propstat>
			<D:status>HTTP/1.1 404 Not Found</D:status>
			<D:prop>
				<D:name />
				<D:parentname />
				<D:href />
				<D:isreadonly />
				<D:contentclass />
				<D:lastaccessed />
				<D:iscollection />
				<D:isstructureddocument />
				<D:defaultdocument />
				<D:isroot />
			</D:prop>
		</D:propstat>
	</D:response>
		* 
		*/
		#endregion

		#region XML Generation

		/// <summary>
		/// Generates the D:Response for a given backend Folder Row
		/// <param name="folder">A FolderDS.FolderRow of a file record in the File Table</param>
		/// <param name="xmlWriter">The xmlWriter to add the xml to</param>
		/// <param name="reqProps">The list of request properties in the PROPFIND request if any</param>
		/// <param name="isPropOnly">If this is a PROPFIND request for properties only. If true then ReqProps must be null</param>
		/// </summary>

		public static void GetXML(FolderModel folder,  XmlTextWriter xmlWriter, RequestedPropertyCollection reqProps, PropertyRequestType requestType)
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
				"resourcetype",
				"supportedlock"
			};

			var inValidProps = new RequestedPropertyCollection();

			if (requestType == PropertyRequestType.PropertyNames && reqProps != null) return;

			if (folder == null) return;

			//Open the response element
			xmlWriter.WriteStartElement("response", "DAV:");

			//Load the valid items HTTP/1.1 200 OK
			xmlWriter.WriteElementString("href", "DAV:", folder.FolderName);

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
					reqProps.Add(new RequestedProperty((string)prop, "DAV:"));
				}
			}

			foreach (RequestedProperty reqProp in reqProps)
			{
				string propertyName = reqProp.LocalName;
				if (propertyName.ToLower(CultureInfo.InvariantCulture).StartsWith("get"))
					propertyName = propertyName.Substring(3);

				if ((reqProp.NS != "DAV:") || (validProps.IndexOf(propertyName.ToLower()) == -1))
				{
					inValidProps.Add(reqProp);
				}
				else
				{

					if (requestType == PropertyRequestType.PropertyNames)
					{
						//if this is a request for property names only then just return the named elements:

						xmlWriter.WriteElementString(propertyName, reqProp.NS, "");

					}
					else
					{
						//Map the property to the Row Data and return the PropStat XML.

						switch (propertyName.ToLower())
						{

							case "contentlanguage":
								xmlWriter.WriteElementString(reqProp.LocalName, "DAV:", "en-us");
								break;
							case "contentlength":
								//To do, calculate Contenlength
								xmlWriter.WriteElementString(reqProp.LocalName, "DAV:", "0");
								break;
							case "contenttype":
								xmlWriter.WriteElementString(reqProp.LocalName, "DAV:", "application/webdav-collection");
								break;
							case "displayname":
								xmlWriter.WriteElementString(reqProp.LocalName, "DAV:", folder.FolderName);
								break;
							case "filepath":
								xmlWriter.WriteElementString(reqProp.LocalName, "DAV:", folder.FolderName);
								break;
							case "ishidden":
								//May adjust later to allow hidden files
								xmlWriter.WriteElementString(reqProp.LocalName, "DAV:", "0");
								break;
							case "resourcetype":
								//May adjust later to allow hidden files
								xmlWriter.WriteStartElement(reqProp.LocalName, "DAV:");
								xmlWriter.WriteElementString("collection", "DAV:", "");
								xmlWriter.WriteEndElement();
								break;
							case "lastaccessed":
								xmlWriter.WriteStartElement(reqProp.LocalName, "DAV:");
								xmlWriter.WriteAttributeString("b:dt", "dateTime.rfc1123");

								//This change is outside the spec for MS. If you set this date is set to the rfc1123 compliant dateformat
								//the Windows Explorer errors out.

								// xmlWriter.WriteString(_fdr.UpdateDate.ToUniversalTime().ToString("r", CultureInfo.InvariantCulture));

								xmlWriter.WriteString(folder.UpdatedDate.ToUniversalTime().ToString("dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture));
								xmlWriter.WriteEndElement();
								break;

							case "lastmodified":
								xmlWriter.WriteStartElement(reqProp.LocalName, "DAV:");
								xmlWriter.WriteAttributeString("b:dt", "dateTime.rfc1123");
								xmlWriter.WriteString(folder.UpdatedDate.ToUniversalTime().ToString("r", CultureInfo.InvariantCulture));
								xmlWriter.WriteEndElement();
								break;

							case "creationdate":
								xmlWriter.WriteStartElement(reqProp.LocalName, "DAV:");
								xmlWriter.WriteAttributeString("b:dt", "dateTime.tz");
								xmlWriter.WriteString(folder.CreatedDate.ToUniversalTime().ToString("s", CultureInfo.InvariantCulture));
								xmlWriter.WriteEndElement();
								break;
							
							case "supportedlock":

								xmlWriter.WriteStartElement("D", reqProp.LocalName, "DAV:");

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
				foreach (RequestedProperty _InValidProp in inValidProps)
					xmlWriter.WriteElementString(_InValidProp.LocalName, _InValidProp.NS, "");

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