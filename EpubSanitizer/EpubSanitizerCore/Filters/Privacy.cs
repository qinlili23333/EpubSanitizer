using System.Xml;

namespace EpubSanitizerCore.Filters
{
    internal class Privacy(EpubSanitizer CoreInstance) : MultiThreadFilter(CoreInstance)
    {
        internal override string[] GetProcessList()
        {
            return Utils.PathUtil.GetAllXHTMLFiles(Instance);
        }

        internal override void PreProcess()
        {
            if (Instance.FileStorage.FileExists("META-INF/calibre_bookmarks.txt"))
            {
                Instance.Logger("Found calibre bookmark file, removing it.");
                Instance.FileStorage.DeleteFile("META-INF/calibre_bookmarks.txt");
            }
        }

        internal override void Process(string file)
        {
            XmlDocument xhtmlDoc = Instance.FileStorage.ReadXml(file);
            if (xhtmlDoc == null)
            {
                Instance.Logger($"Error loading XHTML file {file}, skipping...");
                return;
            }
            RemoveAdept(xhtmlDoc);
            Instance.FileStorage.WriteXml(file, xhtmlDoc);
        }

        /// <summary>
        /// Remove Adept uuid if exists
        /// </summary>
        /// <param name="xhtmlDoc">XmlDocument object of xhtml</param>
        private static void RemoveAdept(XmlDocument xhtmlDoc)
        {
            xhtmlDoc.GetElementsByTagName("head")[0]?.ChildNodes.Cast<XmlNode>().Where(node =>
            {
                if (node is XmlElement element)
                {
                    return element.Name == "meta" && element.HasAttribute("name") && (element.GetAttribute("name") is "Adept.expected.resource" or "Adept.resource");
                }
                return false;
            }).ToList().ForEach(node => node.ParentNode.RemoveChild(node));
        }
    }
}
