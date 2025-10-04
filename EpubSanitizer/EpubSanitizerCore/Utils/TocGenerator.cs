using System.Xml;

namespace EpubSanitizerCore.Utils
{
    /// <summary>
    /// a class representing navPoint item in NCX file.
    /// </summary>
    internal class NavItem
    {
        internal required string Id { get; set; }
        internal required string Text { get; set; }
        internal required string Href { get; set; }
        internal required int Order { get; set; }
        internal int Level { get; set; }
    }

    internal static class TocGenerator
    {
        /// <summary>
        /// Empty template, loaded at static constructor.
        /// </summary>
        private static readonly XmlDocument emptyNavDoc = new();
        /// <summary>
        /// Parse string template for nav.xhtml document.
        /// Only do once and cached in static field to accelerate for batch processing.
        /// </summary>
        static TocGenerator()
        {
            emptyNavDoc.LoadXml(Res.TocXhtmlTemplate);
        }

        /// <summary>
        /// Converts the navigation XML document to a page list format and updates the specified OPF document
        /// accordingly.
        /// </summary>
        /// <param name="nav">The navigation XML document to convert.</param>
        /// <param name="Instance">The EpubSanitizer instance containing the OPF document to update.</param>
        /// <param name="navPath">The path to the navigation document within the EPUB file.</param>
        internal static void ConvertPageMapToPageList(XmlDocument nav, EpubSanitizer Instance, string navPath)
        {
            // Find page-map file
            XmlElement spine = Instance.Indexer.OpfDoc.GetElementsByTagName("spine")[0] as XmlElement;
            string id = spine.GetAttribute("page-map");
            OpfFile mapFile = Instance.Indexer.ManifestFiles.FirstOrDefault(file => file.id == id);
            // Check whether page-list already exist
            foreach (XmlElement navElem in nav.GetElementsByTagName("nav"))
            {
                if (navElem.GetAttribute("epub:type") == "page-list" ||
                    navElem.GetAttribute("type", "http://www.idpf.org/2007/ops") == "page-list")
                {
                    goto delete;
                }
            }
            XmlDocument pageMapDoc = Instance.FileStorage.ReadXml(mapFile.path);
            nav.DocumentElement.SetAttribute("xmlns:epub", "http://www.idpf.org/2007/ops");
            XmlElement pageList = nav.CreateElement("nav", nav.DocumentElement.NamespaceURI);
            pageList.SetAttribute("type", "http://www.idpf.org/2007/ops", "page-list");
            pageList.SetAttribute("hidden", "hidden");
            XmlElement olElement = nav.CreateElement("ol", nav.DocumentElement.NamespaceURI);
            foreach (XmlElement pageTarget in pageMapDoc.GetElementsByTagName("page"))
            {
                XmlElement liElement = nav.CreateElement("li", nav.DocumentElement.NamespaceURI);
                XmlElement aElement = nav.CreateElement("a", nav.DocumentElement.NamespaceURI);
                aElement.SetAttribute("href", PathUtil.ComposeRelativePath(navPath, PathUtil.ComposeFromRelativePath(mapFile.path, pageTarget.GetAttribute("href"))));
                aElement.InnerText = pageTarget.GetAttribute("name");
                liElement.AppendChild(aElement);
                olElement.AppendChild(liElement);
            }
            pageList.AppendChild(olElement);
            nav.GetElementsByTagName("body")[0].AppendChild(pageList);
        delete:
            // Delete page-map file from manifest and file storage
            Instance.Indexer.DeleteFileRecord(mapFile.path);
            Instance.FileStorage.DeleteFile(mapFile.path);
            spine.RemoveAttribute("page-map");
            // Avoid pagemap still in file from cache
            Instance.FileStorage.FlushXmlCache(mapFile.path);
        }

        /// <summary>
        /// Generate a new nav.xhtml document based on the NCX file.
        /// </summary>
        /// <param name="ncxDoc">NCX XmlDocument</param>
        /// <returns>xhtml XmlDocument object</returns>
        internal static XmlDocument Generate(XmlDocument ncxDoc)
        {
            XmlDocument navDoc = emptyNavDoc.Clone() as XmlDocument;
            XmlElement navMap = ncxDoc.GetElementsByTagName("navMap")[0] as XmlElement;
            List<NavItem> navItems = [];
            RecursiveParseNavPoints(navMap, ref navItems, 0);
            navItems.Sort((x, y) => x.Order.CompareTo(y.Order));
            int maxLevel = navItems.Max(item => item.Level);
            XmlElement[] lastOlByLevel = new XmlElement[maxLevel + 1];
            XmlElement olElement = navDoc.GetElementsByTagName("ol")[0] as XmlElement;
            for (var item = 0; item < navItems.Count; item++)
            {
                // Create a new list item for each NavItem
                NavItem navItem = navItems[item];
                XmlElement liElement = navDoc.CreateElement("li", "http://www.w3.org/1999/xhtml");
                XmlElement aElement = navDoc.CreateElement("a", "http://www.w3.org/1999/xhtml");
                aElement.SetAttribute("href", navItem.Href);
                aElement.InnerText = navItem.Text;
                liElement.AppendChild(aElement);
                // create child ol element if needed
                if (item + 1 < navItems.Count && navItem.Level < navItems[item + 1].Level)
                {
                    XmlElement nestedOlElement = navDoc.CreateElement("ol", "http://www.w3.org/1999/xhtml");
                    liElement.AppendChild(nestedOlElement);
                    lastOlByLevel[navItem.Level] = nestedOlElement;
                }
                if (navItem.Level == 0)
                {
                    // If the item is at top level, append it to the main ol element
                    olElement.AppendChild(liElement);
                }
                else
                {
                    lastOlByLevel[navItem.Level - 1].AppendChild(liElement);
                }

            }
            return navDoc;
        }
        /// <summary>
        /// Recursively parse navPoints in NCX document and build a list of NavItem objects.
        /// </summary>
        /// <param name="parentElement">The outside element</param>
        /// <param name="navItems">reference of NavItem list</param>
        /// <param name="level">current depth level</param>
        private static void RecursiveParseNavPoints(XmlElement parentElement, ref List<NavItem> navItems, int level)
        {
            foreach (XmlElement navPoint in parentElement.ChildNodes)
            {
                if (navPoint.Name != "navPoint")
                {
                    continue;
                }
                var textElement = navPoint.GetElementsByTagName("text")[0] as XmlElement;
                var contentElement = navPoint.GetElementsByTagName("content")[0] as XmlElement;
                NavItem item = new()
                {
                    Id = navPoint.GetAttribute("id"),
                    Text = textElement.InnerText,
                    Href = contentElement.GetAttribute("src"),
                    Order = int.TryParse(navPoint.GetAttribute("playOrder"), out int playOrder) ? playOrder : 0,
                    Level = level
                };
                navItems.Add(item);
                // Recursively parse child navPoints
                RecursiveParseNavPoints(navPoint, ref navItems, level + 1);
            }
        }
    }
}
