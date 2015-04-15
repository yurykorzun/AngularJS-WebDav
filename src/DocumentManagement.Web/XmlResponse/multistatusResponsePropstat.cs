namespace DocumentManagement.Web.XmlResponse
{
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "DAV:")]
    public class multistatusResponsePropstat
    {
        [System.Xml.Serialization.XmlElementAttribute("status")]
        public string Status { get; set; }

        [System.Xml.Serialization.XmlElementAttribute("prop")]
        public multistatusResponsePropstatProp Prop { get; set; }
    }
}