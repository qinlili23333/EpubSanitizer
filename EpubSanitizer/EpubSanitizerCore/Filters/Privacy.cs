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
            RemoveThumbsDB();
            RemoveProQuestWatermark();
            RemoveHyreadWatermark();
        }

        private void RemoveHyreadWatermark()
        {
            foreach (XmlElement ele in Instance.Indexer.OpfDoc.GetElementsByTagName("meta").Cast<XmlElement>().ToArray())
            {
                if (ele.GetAttribute("property") == "hdf")
                {
                    Instance.Logger("Found Hyread watermark meta tag, removing it.");
                    ele.ParentNode.RemoveChild(ele);
                }
            }
        }

        private void RemoveProQuestWatermark()
        {
            if (Instance.FileStorage.FileExists("text.xhtml") && Instance.FileStorage.ReadString("text.xhtml").Contains("Watermark Page") && Instance.FileStorage.ReadString("text.xhtml").Contains("ProQuest Ebook Central"))
            {
                Instance.Logger("Found ProQuest watermark file, removing it.");
                Instance.FileStorage.DeleteFile("text.xhtml");
                Instance.Indexer.DeleteFileRecord("text.xhtml");
            }
        }

        /// <summary>
        /// Windows is shit, Thumbs.db should be removed
        /// </summary>
        private void RemoveThumbsDB()
        {
            foreach (var file in Instance.FileStorage.GetAllFiles())
            {
                if (file.Split('/').Last() == "Thumbs.db")
                {
                    Instance.FileStorage.DeleteFile(file);
                    Instance.Indexer.DeleteFileRecord(file);
                }
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
