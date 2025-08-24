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
            string content = Instance.FileStorage.ReadString(file);
            XmlDocument xhtmlDoc = new();
            try
            {
                xhtmlDoc.LoadXml(content);
            }
            catch (XmlException ex)
            {
                Instance.Logger($"Error loading XHTML file {file}: {ex.Message}");
                return;
            }
            RemoveAdept(xhtmlDoc);
            Instance.FileStorage.WriteBytes(file, Utils.XmlUtil.ToXmlBytes(xhtmlDoc, false));
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
                    return element.Name == "meta" && element.GetAttribute("name") == "Adept.expected.resource";
                }
                return false;
            }).ToList().ForEach(node => node.ParentNode.RemoveChild(node));
        }
    }
}
