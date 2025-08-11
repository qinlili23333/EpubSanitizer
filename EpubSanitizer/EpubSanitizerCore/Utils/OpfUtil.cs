using System.Xml;

namespace EpubSanitizerCore.Utils
{
    internal static class OpfUtil
    {
        /// <summary>
        /// Get unique identifier from OPF document
        /// </summary>
        /// <param name="OpfDoc">OPF XmlDocument object</param>
        /// <returns>unique identifier if exist (required in standard), or random GUID generated</returns>
        internal static string GetUniqueIdentifier(XmlDocument OpfDoc)
        {
            string identifierId = OpfDoc.DocumentElement.GetAttribute("unique-identifier");
            if (identifierId != string.Empty)
            {
                foreach (var item in OpfDoc.GetElementsByTagName("dc:identifier"))
                {
                    if (item is XmlElement itemEle)
                    {
                        if (itemEle.GetAttribute("id") == identifierId)
                        {
                            return itemEle.InnerText;
                        }
                    }
                }
            }
            return Guid.NewGuid().ToString();
        }
    }
}
