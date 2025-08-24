using System.Xml;

namespace EpubSanitizerCore.Filters
{
    internal class VitalSource(EpubSanitizer CoreInstance) : MultiThreadFilter(CoreInstance)
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
            // Remove scripts
            List<XmlNode> scriptNodes = [.. xhtmlDoc.GetElementsByTagName("script").Cast<XmlNode>()];
            foreach (XmlNode scriptNode in scriptNodes)
            {
                if (scriptNode is XmlElement scriptElement)
                {
                    if (scriptElement.GetAttribute("src").Contains("VSTEPUBClientAPI.js") || scriptElement.InnerText.Contains("VST."))
                    {
                        scriptNode.ParentNode?.RemoveChild(scriptNode);
                    }
                }
            }
            // Write back the processed content
            Instance.FileStorage.WriteXml(file, xhtmlDoc);
        }


        public static new void PrintHelp()
        {
            Console.WriteLine("VitalSource filter will remove useless injected content by Bookshelf.");
            Console.WriteLine("No options available for this filter");
        }
    }
}
