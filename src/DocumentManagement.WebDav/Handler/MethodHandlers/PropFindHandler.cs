using System.Globalization;
using System.IO;
using System.Text;
using System.Web;
using System.Xml;
using System.Xml.XPath;
using DocumentManagement.Service;
using DocumentManagement.WebDav.Helpers;
using DocumentManagement.WebDav.XMLDBObjects;
using DocumentManagment.Common.Enums;

namespace DocumentManagement.WebDav.Handler.MethodHandlers
{
    /// <summary>
    /// http://www.restpatterns.org/HTTP_Methods/PROPFIND
    /// http://www.webdav.org/specs/rfc4918.html#METHOD_PROPFIND
    /// The PROPFIND Method retrieves properties for a resource identified by the request Uniform Resource Identifier (URI).
    /// 
    /// Request
    /// 
    /// PROPFIND /file HTTP/1.1 
    /// Host: www.example.com 
    /// Content-type: application/xml; charset="utf-8" 
    ///  Content-Length: xxxx 
    ///    
    ///  <?xml version="1.0" encoding="utf-8" ?> 
    ///  <D:propfind xmlns:D="DAV:"> 
    ///    <D:prop xmlns:R="http://ns.example.com/boxschema/"> 
    ///      <R:bigbox/> 
    ///      <R:author/> 
    ///      <R:DingALing/> 
    ///      <R:Random/> 
    ///    </D:prop> 
    ///  </D:propfind> 
    /// 
    /// 
    /// Response
    ///  HTTP/1.1 207 Multi-Status 
    ///  Content-Type: application/xml; charset="utf-8" 
    ///  Content-Length: xxxx 
    ///    
    ///  <?xml version="1.0" encoding="utf-8" ?> 
    ///  <D:multistatus xmlns:D="DAV:"> 
    ///    <D:response xmlns:R="http:///ns.example.com/boxschema/"> 
    ///      <D:href>http://www.example.com/file</D:href> 
    ///      <D:propstat> 
    ///        <D:prop> 
    ///          <R:bigbox> 
    ///            <R:BoxType>Box type A</R:BoxType> 
    ///          </R:bigbox> 
    ///          <R:author> 
    ///            <R:Name>J.J. Johnson</R:Name> 
    ///          </R:author> 
    ///        </D:prop> 
    ///        <D:status>HTTP/1.1 200 OK</D:status> 
    ///      </D:propstat> 
    ///      <D:propstat> 
    ///        <D:prop><R:DingALing/><R:Random/></D:prop> 
    ///        <D:status>HTTP/1.1 403 Forbidden</D:status> 
    ///        <D:responsedescription> The user does not have access to the 
    ///   DingALing property. 
    ///        </D:responsedescription> 
    ///      </D:propstat> 
    ///    </D:response> 
    ///    <D:responsedescription> There has been an access violation error.
    ///    </D:responsedescription> 
    ///  </D:multistatus> 
    /// </summary>
    public class PropFindHandler : IMethodHandler
    {

        private readonly HttpApplication _httpApplication;
        private PropertyRequestType _requestPropertyType;
        private RequestedPropertyCollection _requestedProperties;

        private const HttpCacheability ResponseCache = HttpCacheability.NoCache;

        private string _requestPath;
        private readonly XPathNavigator _requestXmlNavigator;
           
        public PropFindHandler(HttpApplication httpApplication) {

            _httpApplication = httpApplication;
            if (_httpApplication.Request.InputStream.Length != 0)
            {
                _requestXmlNavigator = new XPathDocument(_httpApplication.Request.InputStream).CreateNavigator();
            }
        }

        #region IMethodHandler Interface
      
        public HandlerResult Handle()
        {
            var httpResponseCode = GetRequestType();
            if (httpResponseCode == (int) ServerResponseCode.Ok)
            {
                if (_requestPropertyType == PropertyRequestType.NamedProperties)
                           _requestedProperties = GetRequestedProps();
                                
                _requestPath = WebDavHelper.GetRelativePath(_httpApplication, _httpApplication.Request.FilePath);
                _httpApplication.Response.Cache.SetCacheability(ResponseCache);               
                var requestXml = BuildResponse();

                return new HandlerResult { StatusCode = (int) ServerResponseCode.MultiStatus, ResponseXml = requestXml };
            }

            return new HandlerResult { StatusCode = httpResponseCode };
        }
       
        #endregion
        
        #region Private Handler Methods
        private int GetRequestType()
        {
            var returnCode = (int)ServerResponseCode.Ok;

            //NOTE: An empty PROPFIND request body MUST be treated as a request for the names 
            //	and values of all properties.

            if (_requestXmlNavigator == null)
                _requestPropertyType = PropertyRequestType.AllProperties;

            else
            {
                var propFindNodeIterator = _requestXmlNavigator.SelectDescendants("propfind", "DAV:", false);
                if (propFindNodeIterator.MoveNext())
                {
                    if (propFindNodeIterator.Current.MoveToFirstChild())
                    {
                        switch (propFindNodeIterator.Current.LocalName.ToLower(CultureInfo.InvariantCulture))
                        {
                            case "propnames":
                                _requestPropertyType = PropertyRequestType.PropertyNames;
                                break;
                            case "allprop":
                                _requestPropertyType = PropertyRequestType.AllProperties;
                                break;
                            default:
                                _requestPropertyType = PropertyRequestType.NamedProperties;
                                break;
                        }
                    }
                    else
                        returnCode = (int)ServerResponseCode.BadRequest;
                }
                else
                    returnCode = (int)ServerResponseCode.BadRequest;
            }

            return returnCode;
        }

        private RequestedPropertyCollection GetRequestedProps()
        {
            var davProperties = new RequestedPropertyCollection();

            if (_requestXmlNavigator != null)
            {
                var propNodeIterator = _requestXmlNavigator.SelectDescendants("prop", "DAV:", false);
                if (propNodeIterator.MoveNext())
                {
                    var nodeChildren = propNodeIterator.Current.SelectChildren(XPathNodeType.All);
                    while (nodeChildren.MoveNext())
                    {
                        var currentNode = nodeChildren.Current;

                        if (currentNode.NodeType == XPathNodeType.Element)

                            davProperties.Add(new RequestedProperty(currentNode.LocalName, currentNode.NamespaceURI));
                    }
                }
            }
            if (davProperties.Count==0)
                    return null;

            return davProperties;
        }

        private string BuildResponse()
        {
            string responseXml;
            using (Stream responseStream = new MemoryStream())
            {
                var xmlWriter = new XmlTextWriter(responseStream, Encoding.UTF8)
                {
                    Formatting = Formatting.Indented,
                    IndentChar = '\t',
                    Indentation = 1
                };

                xmlWriter.WriteStartDocument();

                //Set the Multistatus
                xmlWriter.WriteStartElement("D", "multistatus", "DAV:");

                var dirInfo = WebDavHelper.GetFolder(_requestPath);

                if (dirInfo == null)
                {
                    var fileInfo = WebDavHelper.GetFileAttribsOnly(_requestPath);

                    if (fileInfo != null)
                    {
                        XMLWebDavFile.GetXML(fileInfo, xmlWriter, _requestedProperties, _requestPropertyType);
                    }
                }
                else
                {
                    var subDirs = FolderService.GetFolderByParentId(dirInfo.Id);
                    foreach (var subDir in subDirs)
                    {
                        XMLWebDavFolder.GetXML(subDir, xmlWriter, _requestedProperties, _requestPropertyType);
                    }

                    var subFiles = FileService.GetFileByParentFolder(dirInfo.Id);
                    foreach (var fileInfo in subFiles)
                    {
                        XMLWebDavFile.GetXML(fileInfo, xmlWriter, _requestedProperties, _requestPropertyType);
                    }
                }

                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Flush();

                responseXml = WebDavHelper.StreamtoString(responseStream);
                xmlWriter.Close();

            }

            return responseXml;
        }
        #endregion

    }
}
