//  ============================================================================
//  AUTHOR		 : Simon
//  CREATE DATE	 : 2 May  2007
//  PURPOSE		 : Class to build a XML representation of a FileLock in the WebDav Namespace.
//  SPECIAL NOTES: 
//  (
//  ===========================================================================

using System.Xml;
using DocumentManagement.Service;
using DocumentManagement.Service.Models;
using DocumentManagment.Common.Enums;

namespace DocumentManagement.WebDav.XMLDBObjects
{
    /// <summary>
    /// Class to build a XML representation of a FileLock in the WebDav Namespace 
    /// </summary>
    public static class XMLWebDavLock
    {

        public static void GetXML(LockModel lockModel, XmlWriter xmlWriter)
        {
            if (lockModel == null) return;

            xmlWriter.WriteStartElement("activelock", "DAV:");

            xmlWriter.WriteStartElement("locktype", "DAV:");
            switch ((LockType)lockModel.LockType)
            {
                case LockType.Read:
                    xmlWriter.WriteElementString("read", "DAV:");
                    break;

                case LockType.Write:
                    xmlWriter.WriteElementString("write", "DAV:");
                    break;
            }
            xmlWriter.WriteEndElement();

            xmlWriter.WriteStartElement("lockscope", "DAV:");
            switch ((LockScope)lockModel.LockScope)
            {
                case LockScope.Exclusive:
                    xmlWriter.WriteElementString("exclusive", "DAV:");
                    break;

                case LockScope.Shared:
                    xmlWriter.WriteElementString("shared", "DAV:");
                    break;
            }
            xmlWriter.WriteEndElement();

            DepthType LockDepth = (DepthType)lockModel.LockDepth;

            if (LockDepth == DepthType.Infinity)
                xmlWriter.WriteElementString("depth", "DAV:", LockDepth.ToString());
            else
                xmlWriter.WriteElementString("depth", "DAV:", (string)System.Enum.Parse(LockDepth.GetType(), LockDepth.ToString(), true));

            //Append the owner
            xmlWriter.WriteElementString("owner", "DAV:", lockModel.LockOwner);
            xmlWriter.WriteElementString("timeout", "DAV:", "Seconds-" + lockModel.Timeout.ToString());

            //Append all the tokens
            xmlWriter.WriteStartElement("locktoken", "DAV:");

            //Get LockTokens from the DB

            var lockTokens = LockService.GetLockTokens(lockModel.Id);
            foreach (var lockToken in lockTokens)
            {
                //xmlWriter.WriteElementString("href", "DAV:", "opaquelocktoken:" + ltr.Token);
                xmlWriter.WriteElementString("href", "DAV:", "urn:uuid:" + lockToken.Token);
            }

            xmlWriter.WriteEndElement();

            //End ActiveLock
            xmlWriter.WriteEndElement();


        }
    }

}

