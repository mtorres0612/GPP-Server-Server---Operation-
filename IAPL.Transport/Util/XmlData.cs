using System;
using System.Collections.Generic;
using System.Text;

namespace IAPL.Transport.Util
{
    class XmlData
    {
        private string xmlData = "";

        public XmlData() {
            xmlData = "<?xml version=\"1.0\" encoding=\"utf-8\" ?>";
        }

        public string GetXmlData {
            get {
                return this.xmlData;
            }
        }

        public void CreateMainTag(string mainTag) {
            xmlData = string.Concat(xmlData, "<", mainTag ,">", "</", mainTag, ">");
        }

        //public void AddElement(string mainTag, string element, string elementValue) {
        //    string tmpXml = "";

        //    tmpXml = string.Concat("<", element, ">", elementValue, "</", element, ">", "</" + mainTag + ">");

        //    xmlData = xmlData.Replace("</" + mainTag + ">", tmpXml);
        //}

        public void AddElement(string mainTag, string element, string elementValue)
        {
            string XmlString = string.Concat("<", element, ">", elementValue, "</", element, ">", "</" + mainTag + ">");
            string MTag =  "</" + mainTag + ">";
            string ExcessString = xmlData.Substring(xmlData.LastIndexOf(MTag) + MTag.Length);

            xmlData = xmlData.Substring(0, xmlData.LastIndexOf(MTag)) + XmlString + ExcessString;
        }

    }
}
