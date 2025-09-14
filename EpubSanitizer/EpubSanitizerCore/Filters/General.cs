using System.Xml;

namespace EpubSanitizerCore.Filters
{
    internal class General(EpubSanitizer CoreInstance) : MultiThreadFilter(CoreInstance)
    {
        /// <summary>
        /// General filter only processes XHTML files.
        /// </summary>
        /// <returns>list of XHTML files</returns>
        internal override string[] GetProcessList()
        {
            return Utils.PathUtil.GetAllXHTMLFiles(Instance);
        }



        internal override void Process(string file)
        {
            XmlDocument xhtmlDoc = Instance.FileStorage.ReadXml(file);
            if (xhtmlDoc == null)
            {
                Instance.Logger($"Error loading XHTML file {file}, skipping...");
                return;
            }
            FixDuokanNoteID(xhtmlDoc);
            FixExternalLink(xhtmlDoc);
            // Write back the processed content
            Instance.FileStorage.WriteXml(file, xhtmlDoc);
        }

        /// <summary>
        /// Add http:// to all external links that missing scheme
        /// </summary>
        /// <param name="doc"></param>
        private void FixExternalLink(XmlDocument doc)
        {
            foreach (XmlElement element in (doc.GetElementsByTagName("body")[0] as XmlElement).GetElementsByTagName("a"))
            {
                string link = element.GetAttribute("href");
                if (link != string.Empty && link[0] != '/' && link[0] != '.' && !Instance.FileStorage.FileExists("link") && link.Contains('.'))
                {
                    element.SetAttribute("href", "http://" + link);
                }
            }
        }

        /// <summary>
        /// Fix duplicate note ID created by Duokan
        /// </summary>
        /// <param name="doc">xhtml XmlDocument object</param>
        private static void FixDuokanNoteID(XmlDocument doc)
        {
            foreach (XmlElement element in (doc.GetElementsByTagName("body")[0] as XmlElement).GetElementsByTagName("aside"))
            {
                string id = element.GetAttribute("id");
                if (id != string.Empty)
                {
                    foreach (XmlElement child in element.GetElementsByTagName("*"))
                    {
                        if (child.GetAttribute("id") == id)
                        {
                            child.RemoveAttribute("id");
                        }
                    }
                }
            }
        }

        public static new void PrintHelp()
        {
            Console.WriteLine("General filter is a default filter that does basic processing for standard fixing.");
        }
    }
}
