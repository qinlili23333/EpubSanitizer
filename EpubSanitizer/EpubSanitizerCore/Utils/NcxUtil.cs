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


        /// <summary>
        /// Try get title from ncx file based on file path
        /// </summary>
        /// <param name="relativepath">file relative path in opf</param>
        /// <param name="NcxDoc">NCX XmlDocument object</param>
        /// <returns>title string if exist, or empty string</returns>
        internal static string GetTitleFromNcx(XmlDocument NcxDoc, string relativepath)
        {
            if (NcxDoc != null)
            {
                XmlNodeList contents = NcxDoc.GetElementsByTagName("content");
                foreach (XmlElement content in contents)
                {
                    // use StartsWith to ignore #
                    if (content.GetAttribute("src").StartsWith(relativepath))
                    {
                        XmlElement parent = content.ParentNode as XmlElement;
                        XmlNodeList texts = parent.GetElementsByTagName("text");
                        if (texts.Count > 0)
                        {
                            return texts[0].InnerText;
                        }
                    }
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Reorder playOrder in Ncx
        /// </summary>
        /// <param name="NcxDoc">NCX XmlDocument object</param>
        internal static void ReorderNcx(XmlDocument NcxDoc)
        {
            HashSet<string> ids = [];
            // Sanitize id and playOrder
            int order = 0;
            string lastTarget = string.Empty;
            foreach (XmlElement navPoint in NcxDoc.GetElementsByTagName("navPoint"))
            {
                if (int.TryParse(navPoint.GetAttribute("id").AsSpan(0, 1), out _))
                {
                    navPoint.SetAttribute("id", "navPoint-" + navPoint.GetAttribute("id"));
                }
                string id = navPoint.GetAttribute("id");
                if (ids.Contains(id))
                {
                    // Generate new id if empty or duplicate
                    int i = 1;
                    while (ids.Contains("navPoint-" + i.ToString() + "-" + id))
                    {
                        i++;
                    }
                    id = "navPoint-" + i.ToString() + "-" + id;
                    navPoint.SetAttribute("id", id);
                }
                ids.Add(id);
                string target = (navPoint.GetElementsByTagName("content")[0] as XmlElement).GetAttribute("src");
                if (target != lastTarget)
                {
                    lastTarget = target;
                    order++;
                }
                navPoint.SetAttribute("playOrder", order.ToString());
            }
        }

    }
}
