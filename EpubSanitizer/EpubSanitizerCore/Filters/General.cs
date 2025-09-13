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

            // Fix duplicate IDs (e.g., from Duokan and other sources)
            FixDuplicateIds(xhtmlDoc);

            // Write back the processed content
            Instance.FileStorage.WriteXml(file, xhtmlDoc);
        }

        /// <summary>
        /// Fix duplicate ID attributes in the document by renaming duplicates
        /// </summary>
        /// <param name="doc">XML document to process</param>
        private void FixDuplicateIds(XmlDocument doc)
        {
            // Dictionary to track ID usage: id -> (element, count)
            Dictionary<string, List<XmlElement>> idUsage = new();
            
            // Find all elements with ID attributes
            foreach (XmlElement element in doc.GetElementsByTagName("*").Cast<XmlElement>())
            {
                string? id = element.GetAttribute("id");
                if (!string.IsNullOrEmpty(id))
                {
                    if (!idUsage.ContainsKey(id))
                    {
                        idUsage[id] = new List<XmlElement>();
                    }
                    idUsage[id].Add(element);
                }
            }

            // Process duplicates (create a list first to avoid modification during enumeration)
            var duplicateIds = idUsage.Where(x => x.Value.Count > 1).ToList();
            foreach (var kvp in duplicateIds)
            {
                string originalId = kvp.Key;
                List<XmlElement> duplicateElements = kvp.Value;
                
                Instance.Logger($"Found {duplicateElements.Count} elements with duplicate ID '{originalId}', fixing...");

                // Keep the first element with original ID, rename others
                for (int i = 1; i < duplicateElements.Count; i++)
                {
                    string newId = $"{originalId}_{i}";
                    
                    // Ensure the new ID is unique
                    int suffix = i;
                    while (idUsage.ContainsKey(newId))
                    {
                        suffix++;
                        newId = $"{originalId}_{suffix}";
                    }
                    
                    string oldId = duplicateElements[i].GetAttribute("id");
                    duplicateElements[i].SetAttribute("id", newId);
                    
                    // Update references to the old ID
                    UpdateIdReferences(doc, oldId, newId);
                    
                    Instance.Logger($"Renamed duplicate ID '{originalId}' to '{newId}'");
                    
                    // Add new ID to tracking to prevent future conflicts
                    idUsage[newId] = new List<XmlElement> { duplicateElements[i] };
                }
            }
        }

        /// <summary>
        /// Update references to an ID throughout the document
        /// </summary>
        /// <param name="doc">XML document</param>
        /// <param name="oldId">Old ID value</param>
        /// <param name="newId">New ID value</param>
        private void UpdateIdReferences(XmlDocument doc, string oldId, string newId)
        {
            // Update href attributes that reference the old ID
            foreach (XmlElement element in doc.GetElementsByTagName("*").Cast<XmlElement>())
            {
                string? href = element.GetAttribute("href");
                if (!string.IsNullOrEmpty(href))
                {
                    // Check for fragment identifier references (#oldId)
                    if (href == $"#{oldId}")
                    {
                        element.SetAttribute("href", $"#{newId}");
                    }
                    // Check for references within the same document
                    else if (href.Contains($"#{oldId}"))
                    {
                        element.SetAttribute("href", href.Replace($"#{oldId}", $"#{newId}"));
                    }
                }
            }
        }

        public static new void PrintHelp()
        {
            Console.WriteLine("General filter does basic processing for standard compliance:");
            Console.WriteLine("- Fixes duplicate element IDs (e.g., from Duokan and other sources)");
            Console.WriteLine("- Updates references to renamed IDs automatically");
        }

        /// <summary>
        /// Test the duplicate ID fixing functionality
        /// </summary>
        /// <returns>True if test passes</returns>
        internal static bool RunTests()
        {
            return DuplicateIdTestHelper.TestDuplicateIdFix(message => Console.WriteLine($"[TEST] {message}"));
        }
    }
}
