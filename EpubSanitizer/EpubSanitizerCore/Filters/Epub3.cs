using EpubSanitizerCore.Utils;
using System.Xml;

namespace EpubSanitizerCore.Filters
{
    internal class Epub3(EpubSanitizer CoreInstance) : SingleThreadFilter(CoreInstance)
    {
        static readonly Dictionary<string, object> ConfigList = new() {
            {"epub3.guessToc", false}
        };
        static Epub3()
        {
            ConfigManager.AddDefaultConfig(ConfigList);
        }

        internal override void PreProcess()
        {

        }


        internal override string[] GetProcessList()
        {
            return Utils.PathUtil.GetAllXHTMLFiles(Instance);
        }

        internal override void PostProcess()
        {
            Utils.OpfUtil.RemoveEmptyMetadataElements(Instance.Indexer.opfDoc);
            Utils.OpfUtil.AddDctermsModifiedIfNeed(Instance.Indexer.opfDoc);
            if (!DetectNavInOpf())
            {
                if (Instance.Config.GetBool("epub3.guessToc"))
                {
                    Instance.Logger("No nav detected in OPF manifest, trying to guess toc from OPF...");
                    if (Utils.OpfUtil.GuessTocFromOpf(ref Instance.Indexer.ManifestFiles, Instance))
                    {
                        Instance.Logger("Toc guessed from OPF manifest, nav properties added.");
                    }
                    else
                    {
                        BuildNavFromOpf();
                    }
                }
                else
                {
                    BuildNavFromOpf();
                }

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
            ProcessDeprecatedRoleAttributes(xhtmlDoc);
            ProcessTableCellAttributes(xhtmlDoc);
            // Write back the processed content
            Instance.FileStorage.WriteBytes(file, Utils.XmlUtil.ToXmlBytes(xhtmlDoc, false));
        }

        /// <summary>
        /// Remove deprecated attributes from XHTML files.
        /// </summary>
        /// <param name="doc">XmlDocument object</param>
        private static void ProcessDeprecatedRoleAttributes(XmlDocument doc)
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

        /// <summary>
        /// Convert cellpadding and cellspacing attributes to CSS styles to comply with Epub 3 standards.
        /// </summary>
        /// <param name="doc">XmlDocument object</param>
        private static void ProcessTableCellAttributes(XmlDocument doc)
        {
            // Find all table elements with cellpadding or cellspacing attributes and classify by their values
            Dictionary<string, List<XmlElement>> PaddingRecord = [];
            Dictionary<string, List<XmlElement>> SpacingRecord = [];
            foreach (XmlElement table in doc.GetElementsByTagName("table").Cast<XmlElement>().ToArray())
            {
                if (table.HasAttribute("cellpadding"))
                {
                    if (PaddingRecord.ContainsKey(table.GetAttribute("cellpadding")))
                    {
                        PaddingRecord[table.GetAttribute("cellpadding")].Add(table);
                    }
                    else
                    {
                        PaddingRecord[table.GetAttribute("cellpadding")] = [table];
                    }
                }
                if (table.HasAttribute("cellspacing"))
                {
                    if (SpacingRecord.ContainsKey(table.GetAttribute("cellspacing")))
                    {
                        SpacingRecord[table.GetAttribute("cellspacing")].Add(table);
                    }
                    else
                    {
                        SpacingRecord[table.GetAttribute("cellspacing")] = [table];
                    }
                }
            }
            // Generate CSS styles for each unique cellpadding and cellspacing value
            string cssStyles = "";
            foreach (var padding in PaddingRecord.Keys)
            {
                string style = $@"
.cellpadding{padding} td,
.cellpadding{padding} th {{
    padding: {padding}px;
}}";
                cssStyles += style;
                // Apply the class to all tables with this cellpadding
                foreach (var table in PaddingRecord[padding])
                {
                    XmlUtil.AddCssClass(table, $"cellpadding{padding}");
                    table.RemoveAttribute("cellpadding");
                }
            }
            foreach (var spacing in SpacingRecord.Keys)
            {
                string style = $@"
.cellspacing{spacing} {{
    border-spacing: {spacing}px;
    border-collapse: separate;
}}";
                cssStyles += style;
                // Apply the class to all tables with this cellspacing
                foreach (var table in SpacingRecord[spacing])
                {
                    XmlUtil.AddCssClass(table, $"cellspacing{spacing}");
                    table.RemoveAttribute("cellspacing");
                }
            }
            // If there are any styles, add them to the head of the document
            if (!string.IsNullOrEmpty(cssStyles))
            {
                XmlElement head = doc.GetElementsByTagName("head")[0] as XmlElement;
                XmlElement styleElement = doc.CreateElement("style", "http://www.w3.org/1999/xhtml");
                styleElement.SetAttribute("type", "text/css");
                styleElement.InnerText = cssStyles;
                head.AppendChild(styleElement);
            }
        }

        /// <summary>
        /// Check whether there is a xhtml file with nav in properties in OPF manifest or guess disabled
        /// </summary>
        /// <returns>true if nav is detected, false otherwise</returns>
        private bool DetectNavInOpf()
        {
            foreach (var file in Instance.Indexer.ManifestFiles)
            {
                if (file.mimetype == "application/xhtml+xml" && file.originElement.GetAttribute("properties").Split(' ').Contains("nav"))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Build nav.xhtml file from toc.ncx if no nav is detected in OPF manifest.
        /// </summary>
        private void BuildNavFromOpf()
        {
            Instance.Logger("No nav detected in OPF manifest, creating nav.xhtml based on toc.ncx...");
            XmlDocument nav = Utils.TocGenerator.Generate(Instance.Indexer.NcxDoc);
            string navPath = Utils.PathUtil.ComposeOpfPath(Instance.Indexer.OpfPath, "nav_epubsanitizer_generated.xhtml");
            Instance.FileStorage.WriteBytes(navPath, Utils.XmlUtil.ToXmlBytes(nav, false));
            OpfFile NavFile = new()
            {
                opfpath = "nav_epubsanitizer_generated.xhtml",
                path = navPath,
                id = "toc_generated",
                mimetype = "application/xhtml+xml",
                properties = "nav"
            };
            Instance.Indexer.ManifestFiles = [.. Instance.Indexer.ManifestFiles, NavFile];
        }

        public static new void PrintHelp()
        {
            Console.WriteLine("Filter applied to Epub 3 files.");
            Console.WriteLine("Options:");
            Console.WriteLine("    --epub3.guessToc=false    If true, will try to guess the toc file from OPF instead of creating new one if possible, default is false.");
        }
    }
}
