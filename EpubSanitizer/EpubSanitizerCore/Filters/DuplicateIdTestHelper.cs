using System.Xml;

namespace EpubSanitizerCore.Filters
{
    /// <summary>
    /// Test helper class for validating duplicate ID fixes
    /// </summary>
    internal static class DuplicateIdTestHelper
    {
        /// <summary>
        /// Test the duplicate ID fixing functionality with sample data
        /// </summary>
        /// <param name="logger">Logger function</param>
        /// <returns>True if test passes, false otherwise</returns>
        internal static bool TestDuplicateIdFix(Action<string> logger)
        {
            logger("Testing duplicate ID fix functionality...");
            
            // Create test HTML with duplicate IDs (similar to Duokan issue)
            string testHtml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:epub=""http://www.idpf.org/2007/ops"">
<head><title>Test</title></head>
<body>
    <p>123<a class=""duokan-footnote"" epub:type=""noteref"" href=""#note1"" id=""note_ref1"">Note</a>456</p>
    <aside epub:type=""footnote"" id=""note1"">
        <a href=""#note_ref1""></a>
        <ol class=""duokan-footnote-content"">
            <li class=""duokan-footnote-item"" id=""note1"" value=""1"">789</li>
        </ol>
    </aside>
    <div id=""test2"">First div</div>
    <div id=""test2"">Second div</div>
    <a href=""#note1"">Link to note1</a>
    <a href=""#test2"">Link to test2</a>
</body>
</html>";

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(testHtml);
            
            // Count initial duplicate IDs
            int initialDuplicates = CountDuplicateIds(doc);
            logger($"Initial duplicate ID count: {initialDuplicates}");
            
            if (initialDuplicates == 0)
            {
                logger("ERROR: Test data should contain duplicate IDs");
                return false;
            }
            
            // Apply the duplicate ID fix (simulate the General filter logic)
            FixDuplicateIdsTestVersion(doc, logger);
            
            // Verify no duplicates remain
            int finalDuplicates = CountDuplicateIds(doc);
            logger($"Final duplicate ID count: {finalDuplicates}");
            
            if (finalDuplicates == 0)
            {
                logger("✅ Test PASSED: All duplicate IDs were fixed");
                return true;
            }
            else
            {
                logger("❌ Test FAILED: Duplicate IDs still exist");
                return false;
            }
        }
        
        private static int CountDuplicateIds(XmlDocument doc)
        {
            Dictionary<string, int> idCount = new Dictionary<string, int>();
            
            foreach (XmlElement element in doc.GetElementsByTagName("*").Cast<XmlElement>())
            {
                string id = element.GetAttribute("id");
                if (!string.IsNullOrEmpty(id))
                {
                    if (idCount.ContainsKey(id))
                        idCount[id]++;
                    else
                        idCount[id] = 1;
                }
            }
            
            return idCount.Values.Count(count => count > 1);
        }
        
        private static void FixDuplicateIdsTestVersion(XmlDocument doc, Action<string> logger)
        {
            // This is a simplified version of the fix logic for testing
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
                
                logger($"Found {duplicateElements.Count} elements with duplicate ID '{originalId}', fixing...");

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
                    UpdateIdReferencesTestVersion(doc, oldId, newId, logger);
                    
                    logger($"Renamed duplicate ID '{originalId}' to '{newId}'");
                    
                    // Add new ID to tracking to prevent future conflicts
                    idUsage[newId] = new List<XmlElement> { duplicateElements[i] };
                }
            }
        }
        
        private static void UpdateIdReferencesTestVersion(XmlDocument doc, string oldId, string newId, Action<string> logger)
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
                        logger($"Updated reference from #{oldId} to #{newId}");
                    }
                    // Check for references within the same document
                    else if (href.Contains($"#{oldId}"))
                    {
                        string newHref = href.Replace($"#{oldId}", $"#{newId}");
                        element.SetAttribute("href", newHref);
                        logger($"Updated reference from {href} to {newHref}");
                    }
                }
            }
        }
    }
}