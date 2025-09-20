namespace EpubSanitizerCore.Utils
{
    /// <summary>
    /// A class for all path related methods
    /// </summary>
    internal static class PathUtil
    {
        /// <summary>
        /// Calculate the full path of a file in OPF, given the OPF path and the file path.
        /// </summary>
        /// <param name="basePath">base file path</param>
        /// <param name="filePath">File path in OPF manifest</param>
        /// <returns></returns>
        internal static string ComposeFromRelativePath(string basePath, string filePath)
        {
            if (filePath.StartsWith('/'))
            {
                return filePath[1..];
            }
            if (filePath.StartsWith('.'))
            {
                // Handle ./ and ../
                var baseParts = basePath.Split('/').ToList();
                var fileParts = filePath.Split('/').ToList();
                baseParts.RemoveAt(baseParts.Count - 1); // Remove the file part
                foreach (var part in fileParts)
                {
                    if (part == ".")
                    {
                        continue;
                    }
                    else if (part == "..")
                    {
                        if (baseParts.Count > 0)
                        {
                            baseParts.RemoveAt(baseParts.Count - 1);
                        }
                    }
                    else
                    {
                        baseParts.Add(part);
                    }
                }
                return string.Join('/', baseParts);
            }
            else
            {
                string normalizedOpfPath = basePath[..(basePath.LastIndexOf('/') + 1)];
                return normalizedOpfPath + filePath;
            }
        }

        /// <summary>
        /// Calculate the relative path of a file which should be used in OPF
        /// </summary>
        /// <param name="basePath">base file path</param>
        /// <param name="filePath">absolute file path</param>
        /// <returns></returns>
        internal static string ComposeRelativePath(string basePath, string filePath)
        {
            string normalizedOpfPath = basePath[..(basePath.LastIndexOf('/') + 1)];
            if (filePath.StartsWith(normalizedOpfPath))
            {
                return filePath[normalizedOpfPath.Length..];
            }
            else
            {
                throw new ArgumentException($"File path '{filePath}' is not under base path '{basePath}'.");
            }
        }

        /// <summary>
        /// A static method to get all XHTML files from the EpubSanitizer instance.
        /// </summary>
        /// <param name="instance">EpubSanitizer Instance</param>
        /// <returns>array of xhtml files</returns>
        internal static string[] GetAllXHTMLFiles(EpubSanitizer instance)
        {
            List<string> files = [];
            foreach (var file in instance.Indexer.ManifestFiles)
            {
                if (file.mimetype == "application/xhtml+xml" || file.mimetype == "application/xml")
                {
                    files.Add(file.path);
                }
            }
            return files.ToArray();
        }
    }
}
