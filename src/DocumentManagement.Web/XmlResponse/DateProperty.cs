using System;

namespace DocumentManagement.Web.XmlResponse
{
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true, Namespace = "DAV:")]
    public class DateProperty
    {
        
        [System.Xml.Serialization.XmlAttributeAttribute(Form = System.Xml.Schema.XmlSchemaForm.Qualified, Namespace = "urn:uuid:c2f41010-65b3-11d1-a29f-00aa00c14882/")]
        public string dt { get; set; }

        
        [System.Xml.Serialization.XmlTextAttribute]
        public DateTime Value { get; set; }
    }
}