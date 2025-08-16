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
        internal int Level { get; set; } = 0; // Default level is 0, can be set to indicate nesting
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
                    continue;
                var textElement = navPoint.GetElementsByTagName("text")[0] as XmlElement;
                var contentElement = navPoint.GetElementsByTagName("content")[0] as XmlElement;
                NavItem item = new()
                {
                    Id = navPoint.GetAttribute("id"),
                    Text = textElement.InnerText,
                    Href = contentElement.GetAttribute("src"),
                    Order = int.Parse(navPoint.GetAttribute("playOrder")),
                    Level = level
                };
                navItems.Add(item);
                // Recursively parse child navPoints
                RecursiveParseNavPoints(navPoint, ref navItems, level + 1);
            }
        }
    }
}
