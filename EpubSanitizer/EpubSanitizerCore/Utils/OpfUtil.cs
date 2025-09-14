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
    }
}
