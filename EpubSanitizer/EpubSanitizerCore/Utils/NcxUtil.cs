using System.Xml;

namespace EpubSanitizerCore.Utils
{
    internal static class NcxUtil
    {
        /// <summary>
        /// Get UID element from NCX file if exist
        /// </summary>
        /// <param name="Doc">NCX XmlDocument object</param>
        /// <returns>meta XmlElement if exist, or null</returns>
        internal static XmlElement? GetUidElement(XmlDocument Doc)
        {
            XmlNodeList metaElement = Doc.GetElementsByTagName("meta");
            if (metaElement.Count > 0)
            {
                foreach (XmlElement meta in metaElement)
                {
                    if (meta.GetAttribute("name") == "dtb:uid")
                    {
                        return meta;
                    }
                }
            }
            return null;
        }
    }
}
