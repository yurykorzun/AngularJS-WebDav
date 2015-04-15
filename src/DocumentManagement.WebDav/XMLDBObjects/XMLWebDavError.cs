using System.IO;
using System.Text;
using System.Xml;

namespace DocumentManagement.WebDav.XMLDBObjects
{
    public class XMLWebDavError
    {
        public static string ProcessErrorCollection(ProcessingErrorCollection errorResources)
        {
            if (errorResources.Count == 0)
            {
                return string.Empty;
            }

            string _errorRequest;

            //Build the response 
            using (Stream responseStream = new MemoryStream())
            {
                var xmlWriter = new XmlTextWriter(responseStream, Encoding.UTF8);

                xmlWriter.Formatting = Formatting.Indented;
                xmlWriter.IndentChar = '\t';
                xmlWriter.Indentation = 1;
                xmlWriter.WriteStartDocument();

                //Set the Multistatus
                xmlWriter.WriteStartElement("D", "multistatus", "DAV:");
                xmlWriter.WriteAttributeString("xmlns:b", "urn:uuid:c2f41010-65b3-11d1-a29f-00aa00c14882");

                foreach (ProcessingError _err in errorResources)
                {

                        //Open the response element
                        xmlWriter.WriteStartElement("response", "DAV:");
                        xmlWriter.WriteElementString("href", "DAV:", _err.ResourcePath);
                        xmlWriter.WriteElementString("status", "DAV:", _err.ErrorCode);
                        //Close the response element section
                        xmlWriter.WriteEndElement();
                   
                }

                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Flush();

                using (StreamReader _streamReader = new StreamReader(responseStream, Encoding.UTF8))
                {
                    //Go to the begining of the stream
                    _streamReader.BaseStream.Position = 0;
                    _errorRequest = _streamReader.ReadToEnd();
                }
                xmlWriter.Close();
            }
            return _errorRequest;
        }
         
    }

}
