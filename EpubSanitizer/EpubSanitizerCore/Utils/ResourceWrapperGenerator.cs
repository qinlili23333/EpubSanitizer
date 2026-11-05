using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace EpubSanitizerCore.Utils
{
    internal static class ResourceWrapperGenerator
    {
        /// <summary>
        /// Empty template, loaded at static constructor.
        /// </summary>
        private static readonly XmlDocument emptyImgDoc = new();
        /// <summary>
        /// Parse string template for nav.xhtml document.
        /// Only do once and cached in static field to accelerate for batch processing.
        /// </summary>
        static ResourceWrapperGenerator()
        {
            emptyImgDoc.LoadXml(Res.ImageXhtmlTemplate);
        }
        internal static XmlDocument GenerateImageWrapper(string path)
        {
            XmlDocument doc = emptyImgDoc.Clone() as XmlDocument;
           (doc.GetElementsByTagName("img")[0] as XmlElement).SetAttribute("src", path);
            return doc;
        }
    }
}
