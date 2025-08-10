using System.Xml;

namespace EpubSanitizerCore.Filters
{
    internal class General(EpubSanitizer CoreInstance) : MultiThreadFilter(CoreInstance)
    {
        static readonly Dictionary<string, object> ConfigList = new() {
            {"general.deprecateFix", true}
        };
        static General()
        {
            ConfigManager.AddDefaultConfig(ConfigList);
        }
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
            string content = Instance.FileStorage.ReadString(file);
            XmlDocument xhtmlDoc = new();
            try
            {
                xhtmlDoc.LoadXml(content);
            }
            catch (XmlException ex)
            {
                Instance.Logger($"Error loading XHTML file {file}: {ex.Message}");
                // TODO: try fix invalid XHTML if possible
                return;
            }
            // Process deprecated attributes
            if (Instance.Config.GetBool("general.deprecateFix"))
            {
                ProcessDeprecatedAttributes(xhtmlDoc);
            }

            // Write back the processed content
            Instance.FileStorage.WriteString(file, Utils.XmlUtil.ToXmlString(xhtmlDoc,false));
        }

        /// <summary>
        /// Remove deprecated attributes from XHTML files.
        /// </summary>
        /// <param name="doc">XmlDocument object</param>
        private static void ProcessDeprecatedAttributes(XmlDocument doc)
        {
            // Process all nodes
            foreach (XmlElement element in doc.SelectNodes("//*"))
            {
                string[] deprecatedAttributes = { "doc-biblioentry", "doc-endnote" };
                if (deprecatedAttributes.Contains(element.GetAttribute("role")))
                {
                    element.RemoveAttribute("role");
                }
            }
        }



        public static new void PrintHelp()
        {
            Console.WriteLine("General filter is a default filter that does basic processing for standard fixing.");
            Console.WriteLine("Options:");
            Console.WriteLine("    --general.deprecateFix=true    Fix deprecated attributes if possible.");
        }
    }
}
