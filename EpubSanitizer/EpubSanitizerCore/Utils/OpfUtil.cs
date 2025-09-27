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

        /// <summary>
        /// Epub 3 forbids empty metadata elements like <dc:format />
        /// This function removes all of them.
        /// </summary>
        /// <param name="OpfDoc">OPF XmlDocument object</param>
        internal static void RemoveEmptyMetadataElements(XmlDocument OpfDoc)
        {
            List<XmlNode> metadataNodes = [.. OpfDoc.GetElementsByTagName("metadata")[0].ChildNodes.Cast<XmlNode>()];
            foreach (XmlNode node in metadataNodes)
            {
                // Remove empty elements
                // Empty elements has no Text child nodes
                if (node is XmlElement element && element.Prefix == "dc" && !element.HasChildNodes)
                {
                    element.ParentNode?.RemoveChild(element);
                }
            }
        }

        /// <summary>
        /// Epub 3 requires dcterms:modified element to be present in OPF metadata, this function adds it if not present.
        /// </summary>
        /// <param name="OpfDoc">OPF XmlDocument object</param>
        internal static void AddDctermsModifiedIfNeed(XmlDocument OpfDoc)
        {
            // Check if dcterms:modified exists
            List<XmlNode> metadataNodes = [.. OpfDoc.GetElementsByTagName("metadata")[0].ChildNodes.Cast<XmlNode>()];
            bool hasDctermsModified = metadataNodes.Any(node => node is XmlElement element && element.Name == "meta" && element.GetAttribute("property") == "dcterms:modified");
            if (!hasDctermsModified)
            {
                // Add dcterms:modified element with current date and time
                XmlElement modifiedElement = OpfDoc.CreateElement("meta", OpfDoc.DocumentElement.NamespaceURI);
                modifiedElement.SetAttribute("property", "dcterms:modified");
                modifiedElement.InnerText = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
                OpfDoc.GetElementsByTagName("metadata")[0].AppendChild(modifiedElement);
            }
        }

        /// <summary>
        /// try to guess toc from OPF manifest files and add nav properties if found
        /// </summary>
        /// <param name="files">Manifest file list reference</param>
        /// <returns>true if found and added properties</returns>
        internal static bool GuessTocFromOpf(ref OpfFile[] files, EpubSanitizer Instance)
        {
            foreach (var file in files)
            {
                if (file.mimetype == "application/xhtml+xml")
                {
                    // file name check passed, check internal structure
                    XmlDocument doc = new();
                    try
                    {
                        doc.LoadXml(Instance.FileStorage.ReadString(file.path));
                    }
                    catch (XmlException)
                    {
                        // If the file is not a valid XHTML, skip it
                        continue;
                    }
                    // Check if it has nav element
                    XmlNodeList navElements = doc.GetElementsByTagName("nav");
                    if (navElements.Count == 1 && ((XmlElement)navElements[0]).GetAttribute("epub:type") == "toc")
                    {
                        // If nav element exists, add nav property
                        file.properties = [.. file.properties, "nav"];
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Get paths in spine order from OPF document
        /// </summary>
        /// <param name="OpfDoc">OPF XmlDocument object</param>
        /// <returns>string array of relative paths</returns>
        internal static string[] GetSpineArray(XmlDocument OpfDoc)
        {
            List<string> spineArray = [];
            XmlNodeList itemrefs = OpfDoc.GetElementsByTagName("spine")[0].ChildNodes;
            foreach (XmlElement itemref in itemrefs)
            {
                string idref = itemref.GetAttribute("idref");
                if (!string.IsNullOrEmpty(idref))
                {
                    spineArray.Add((OpfDoc.SelectSingleNode($"//*[@id='{idref}']") as XmlElement).GetAttribute("href"));
                }
            }
            return spineArray.ToArray();
        }

        /// <summary>
        /// Get OpfFile item from manifest by path
        /// </summary>
        /// <param name="files">file list in Indexer</param>
        /// <param name="path">path in epub</param>
        /// <returns>OpfFile item if exists, or null</returns>
        internal static OpfFile? GetItemFromManifest(OpfFile[] files, string path)
        {
            for (int i = 0; i < files.Length; i++)
            {
                if (files[i].path == path)
                {
                    return files[i];
                }
            }
            return null;
        }

        /// <summary>
        /// Correct spine order in OPF document based on NCX file.
        /// Items not in NCX will be kept at the beginning, followed by NCX order.
        /// </summary>
        /// <param name="opfDoc">opf Document</param>
        /// <param name="ncxDoc">ncx Document</param>
        internal static void CorrectSpineOrderFromNcx(XmlDocument opfDoc, XmlDocument ncxDoc)
        {
            List<string> ncxOrder = [];
            XmlNodeList navPoints = ncxDoc.GetElementsByTagName("navPoint");
            foreach (XmlElement navPoint in navPoints)
            {
                XmlElement? content = navPoint.GetElementsByTagName("content").Cast<XmlElement?>().FirstOrDefault();
                if (content != null)
                {
                    string src = content.GetAttribute("src").Split('#')[0];
                    if (!string.IsNullOrEmpty(src) && !ncxOrder.Contains(src))
                    {
                        ncxOrder.Add(src);
                    }
                }
            }
            XmlNodeList itemrefs = opfDoc.GetElementsByTagName("spine")[0].ChildNodes;
            List<XmlElement> itemrefElements = [.. itemrefs.Cast<XmlElement>()];
            itemrefElements.Sort((a, b) =>
            {
                string aHref = (opfDoc.SelectSingleNode($"//*[@id='{a.GetAttribute("idref")}']") as XmlElement)?.GetAttribute("href") ?? "";
                string bHref = (opfDoc.SelectSingleNode($"//*[@id='{b.GetAttribute("idref")}']") as XmlElement)?.GetAttribute("href") ?? "";
                bool aInNcx = ncxOrder.Contains(aHref);
                bool bInNcx = ncxOrder.Contains(bHref);

                if (!aInNcx && !bInNcx) return 0; // both not in NCX, keep original order
                if (!aInNcx) return -1; // a not in NCX, keep at front
                if (!bInNcx) return 1;  // b not in NCX, keep at front

                int aIndex = ncxOrder.IndexOf(aHref);
                int bIndex = ncxOrder.IndexOf(bHref);
                return aIndex.CompareTo(bIndex);
            });
            XmlElement spineElement = (XmlElement)opfDoc.GetElementsByTagName("spine")[0];
            // Keep attributes but no childs
            spineElement.ChildNodes.Cast<XmlNode>().ToList().ForEach(node => spineElement.RemoveChild(node));
            foreach (XmlElement itemref in itemrefElements)
            {
                spineElement.AppendChild(itemref);
            }
        }
    }
}
