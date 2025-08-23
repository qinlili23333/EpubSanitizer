using EpubSanitizerCore.Utils;
using System.Runtime.CompilerServices;
using System.Text;
using System.Xml;

namespace EpubSanitizerCore.Filters
{
    internal class Epub3(EpubSanitizer CoreInstance) : MultiThreadFilter(CoreInstance)
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
            UpgradeDcMetaAttributes();
        }

        // 将 Instance.Indexer.OpfDoc 替换为 Instance.Indexer.opfDoc
        private void UpgradeDcMetaAttributes()
        {
            List<XmlNode> metadataNodes = [.. Instance.Indexer.opfDoc.GetElementsByTagName("metadata")[0].ChildNodes.Cast<XmlNode>()];
            foreach (XmlNode node in metadataNodes)
            {
                if (node is XmlElement element && element.Prefix == "dc")
                {
                    foreach (XmlAttribute attr in element.Attributes.Cast<XmlAttribute>().ToArray())
                    {
                        if (!XmlUtil.ExpectedAttribute(element.Name, attr.Name))
                        {
                            // Create meta element if attribute not empty
                            if(!string.IsNullOrEmpty(attr.Value))
                            {
                                XmlElement metaElement = Instance.Indexer.opfDoc.CreateElement("meta", "http://www.idpf.org/2007/opf");
                                metaElement.SetAttribute("property", XmlUtil.GetMetaPropertyFromAttribute(attr.Name));
                                metaElement.InnerText = attr.Value;
                                if (!element.HasAttribute("id"))
                                {
                                    // If the original element does not have an id, generate a new one
                                    string newId = $"meta{Guid.NewGuid()}";
                                    element.SetAttribute("id", newId);
                                }
                                metaElement.SetAttribute("refines", $"#{element.GetAttribute("id")}"); // Copy id if exists
                                EnhanceMetaElement(metaElement);
                                // Add after original element
                                element.ParentNode.InsertAfter(metaElement, element);
                            }
                            // Remove unexpected attributes
                            element.Attributes.Remove(attr);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Enhance the meta element based on content inference
        /// </summary>
        /// <param name="metaElement">new created meta element</param>
        private void EnhanceMetaElement(XmlElement metaElement)
        {
            switch (metaElement.GetAttribute("property"))
            {
                case "role":
                    {
                        // Use MARC Code List for Relators is possible
                        string[] allowRoles = ["exp", "grt", "abr", "act", "adp", "rcp", "anl", "anm", "ann", "anc", "apl", "ape", "app", "arc", "arr", "acp", "adi", "art", "ill", "ard", "asg", "asn", "fmo", "att", "auc", "aue", "aup", "aut", "aqt", "aud", "ato", "ant", "bnd", "bdd", "blw", "bka", "bkd", "bkp", "bjd", "bpd", "bsl", "brl", "brd", "cll", "cop", "ctg", "cas", "cad", "cns", "chr", "cng", "cli", "cor", "col", "clt", "clr", "cmm", "cwt", "com", "cpl", "cpt", "cpe", "cmp", "cmt", "ccp", "cnd", "con", "csl", "csp", "cos", "cot", "coe", "cts", "ctt", "cte", "ctr", "ctb", "cpc", "cph", "crr", "crp", "cst", "cou", "crt", "cov", "cre", "cur", "dnc", "dtc", "dtm", "dte", "dto", "dfd", "dft", "dfe", "dgc", "dgg", "dgs", "dln", "dpc", "dpt", "dsr", "drt", "dis", "dbp", "dst", "djo", "dnr", "drm", "dbd", "dub", "edt", "edc", "edm", "edd", "elg", "elt", "enj", "eng", "egr", "etr", "evp", "exp", "fac", "fld", "fmd", "fds", "flm", "fmp", "fmk", "fpy", "frg", "fmo", "fon", "fnd", "gdv", "gis", "hnr", "hst", "his", "ilu", "ill", "ink", "ins", "itr", "ive", "ivr", "inv", "isb", "jud", "jug", "lbr", "ldr", "lsa", "led", "len", "ltr", "lil", "lit", "lie", "lel", "let", "lee", "lbt", "lse", "lso", "lgd", "ltg", "lyr", "mka", "mfp", "mfr", "mrb", "mrk", "med", "mdc", "mte", "mtk", "mxe", "mod", "mon", "mcp", "mup", "msd", "mus", "nrt", "nan", "onp", "osp", "opn", "orm", "org", "oth", "own", "pan", "ppm", "pta", "pth", "pat", "pnc", "prf", "prf", "pma", "pht", "pad", "ptf", "ptt", "pte", "plt", "pra", "pre", "prt", "pop", "prm", "prc", "pro", "prn", "prs", "pmn", "prd", "prp", "prg", "pdr", "pfr", "crr", "prv", "mfr", "pbl", "pup", "pbl", "pbd", "ppt", "rdd", "rpc", "rap", "rce", "rcd", "red", "rxa", "ren", "rpt", "rps", "rth", "rtm", "res", "rsp", "rst", "rse", "rpy", "rsg", "rsr", "rev", "rbr", "sce", "sad", "aus", "scr", "fac", "scl", "spy", "sec", "sll", "std", "stg", "sgn", "ins", "sng", "swd", "sds", "sde", "spk", "sfx", "spn", "sgd", "stm", "stn", "str", "stl", "sht", "srv", "tch", "tad", "tcd", "tld", "tlg", "tlh", "tlp", "tau", "ths", "trc", "arr", "fac", "trl", "tyd", "tyg", "bkd", "uvp", "vdg", "vfx", "vac", "wit", "wde", "wdc", "wam", "wac", "wal", "wat", "waw", "wfs", "wfw", "wft", "win", "wpr", "wst", "wts"];
                        if (allowRoles.Contains(metaElement.InnerText)) {
                            metaElement.SetAttribute("scheme", "marc:relators");
                        }
                        break;
                    }
            }
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
            StringBuilder cssStyles = new();
            foreach (var padding in PaddingRecord.Keys)
            {
                string style = $@".cellpadding{padding} td,
.cellpadding{padding} th {{
    padding: {padding}px;
}}";
                cssStyles.AppendLine(style);
                // Apply the class to all tables with this cellpadding
                foreach (var table in PaddingRecord[padding])
                {
                    XmlUtil.AddCssClass(table, $"cellpadding{padding}");
                    table.RemoveAttribute("cellpadding");
                }
            }
            foreach (var spacing in SpacingRecord.Keys)
            {
                string style = $@".cellspacing{spacing} {{
    border-spacing: {spacing}px;
    border-collapse: separate;
}}";
                cssStyles.AppendLine(style);
                // Apply the class to all tables with this cellspacing
                foreach (var table in SpacingRecord[spacing])
                {
                    XmlUtil.AddCssClass(table, $"cellspacing{spacing}");
                    table.RemoveAttribute("cellspacing");
                }
            }
            if (cssStyles.Length == 0)
            {
                return;
            }
            // If there are any styles, add them to the head of the document
            XmlElement head = doc.GetElementsByTagName("head")[0] as XmlElement;
            XmlElement styleElement = doc.CreateElement("style", "http://www.w3.org/1999/xhtml");
            styleElement.SetAttribute("type", "text/css");
            styleElement.InnerText = cssStyles.ToString();
            head.AppendChild(styleElement);
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
