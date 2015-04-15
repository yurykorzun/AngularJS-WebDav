namespace DocumentManagement.Web.XmlResponse
{
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "DAV:")]
    public class PROPFINDItem
    {
        
        public string href { get; set; }

        
        public multistatusResponsePropstat propstat { get; set; }
    }
}