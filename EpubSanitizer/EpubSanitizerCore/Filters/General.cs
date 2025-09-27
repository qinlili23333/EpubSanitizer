using EpubSanitizerCore.Utils;
using HeyRed.Mime;
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
            FixDuplicateContentType(xhtmlDoc);
            FixDuokanNoteID(xhtmlDoc);
            FixExternalLink(file, xhtmlDoc);
            FixInvalidWidthHeight(xhtmlDoc);
            if (Instance.Config.GetBool("correctMime"))
            {
                FixSourceMime(xhtmlDoc);
            }
            // Write back the processed content
            Instance.FileStorage.WriteXml(file, xhtmlDoc);
        }


        /// <summary>
        /// width and height only allows integer without any dimension, if it's invalid, use css to set width and height instead
        /// </summary>
        /// <param name="doc">xhtml XmlDocument object</param>
        private static void FixInvalidWidthHeight(XmlDocument doc)
        {
            foreach (XmlElement element in doc.GetElementsByTagName("*").Cast<XmlElement>().ToArray())
            {
                if( element.HasAttribute("width") && !int.TryParse(element.GetAttribute("width"), out _))
                {
                    element.SetAttribute("style", (element.GetAttribute("style") + $";width:{element.GetAttribute("width")};").Replace(";;",";"));
                    element.RemoveAttribute("width");
                }
                if (element.HasAttribute("height") && !int.TryParse(element.GetAttribute("height"), out _))
                {
                    element.SetAttribute("style", (element.GetAttribute("style") + $";height:{element.GetAttribute("height")};").Replace(";;", ";"));
                    element.RemoveAttribute("height");
                }
            }
        }

        /// <summary>
        /// Update <source> tag mime type according to file extension
        /// </summary>
        /// <param name="doc">xhtml XmlDocument object</param>
        private static void FixSourceMime(XmlDocument doc)
        {
            foreach (XmlElement element in doc.GetElementsByTagName("source").Cast<XmlElement>().ToArray())
            {
                element.SetAttribute("type", MimeTypesMap.GetMimeType(element.GetAttribute("src").Split('/').Last()));
            }
        }

        /// <summary>
        /// Fix duplicate content type meta tag created by some editors, seems they just add new meta tag without checking existing oneF
        /// </summary>
        /// <param name="doc">xhtml XmlDocument object</param>
        private static void FixDuplicateContentType(XmlDocument doc)
        {
            foreach (XmlElement element in (doc.GetElementsByTagName("head")[0] as XmlElement).GetElementsByTagName("meta").Cast<XmlElement>().ToArray())
            {
                if (element.GetAttribute("http-equiv").Equals("content-type", StringComparison.InvariantCultureIgnoreCase) || element.HasAttribute("charset"))
                {
                    // only keep the first one
                    element.ParentNode.RemoveChild(element);
                }
            }
            // add a correct one
            XmlElement meta = doc.CreateElement("meta", doc.DocumentElement.NamespaceURI);
            meta.SetAttribute("http-equiv", "Content-Type");
            meta.SetAttribute("content", "text/html; charset=utf-8");
            (doc.GetElementsByTagName("head")[0] as XmlElement).AppendChild(meta);
        }


        /// <summary>
        /// Add http:// to all external links that missing scheme
        /// </summary>
        /// <param name="doc">xhtml XmlDocument object</param>
        /// <param name="file">file path</param>
        private void FixExternalLink(string file, XmlDocument doc)
        {
            foreach (XmlElement element in (doc.GetElementsByTagName("body")[0] as XmlElement).GetElementsByTagName("a"))
            {
                string link = element.GetAttribute("href");
                if (link.StartsWith("http"))
                {
                    continue;
                }
                if (link != string.Empty && link[0] != '/' && link[0] != '.' && !Instance.FileStorage.FileExists(PathUtil.ComposeFromRelativePath(file, link).Split('#')[0]) && link.Split('/')[0].Split('.').Length >= 3)
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
