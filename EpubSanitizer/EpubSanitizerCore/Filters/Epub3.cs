using System.Xml;

namespace EpubSanitizerCore.Filters
{
    internal class Epub3(EpubSanitizer CoreInstance) : SingleThreadFilter(CoreInstance)
    {
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
                Instance.Logger("No nav detected in OPF manifest, creating nav.xhtml based on toc.ncx...");
                XmlDocument nav = Utils.TocGenerator.Generate(Instance.Indexer.NcxDoc);
                string navPath = Utils.PathUtil.ComposeOpfPath(Instance.Indexer.OpfPath,"nav.xhtml");
                Instance.FileStorage.WriteBytes(navPath, Utils.XmlUtil.ToXmlBytes(nav, false));

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
            ProcessDeprecatedAttributes(xhtmlDoc);
            // Write back the processed content
            Instance.FileStorage.WriteBytes(file, Utils.XmlUtil.ToXmlBytes(xhtmlDoc, false));
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

        /// <summary>
        /// Check whether there is a xhtml file with nav in properties in OPF manifest
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
    }
}
