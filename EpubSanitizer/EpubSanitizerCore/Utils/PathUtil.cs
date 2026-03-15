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
            string[] baseDirParts = basePath.Contains('/') ? basePath.Split('/')[..^1] : [];
            string[] fileParts = filePath.Split('/');

            int commonIndex = 0;
            while (commonIndex < baseDirParts.Length &&
                   commonIndex < fileParts.Length - 1 &&
                   baseDirParts[commonIndex] == fileParts[commonIndex])
            {
                commonIndex++;
            }

            var relativeParts = new List<string>();

            for (int i = commonIndex; i < baseDirParts.Length; i++)
            {
                relativeParts.Add("..");
            }

            for (int i = commonIndex; i < fileParts.Length; i++)
            {
                relativeParts.Add(fileParts[i]);
            }

            return string.Join('/', relativeParts);
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
                if (file.mimetype == "application/xhtml+xml")
                {
                    files.Add(file.path);
                }
            }
            return [.. files];
        }
        /// <summary>
        /// Test whether a given URL string is an HTTP or HTTPS URL.
        /// </summary>
        /// <param name="urlString">url</param>
        /// <returns>whether it's an HTTP or HTTPS URL</returns>
        internal static bool IsHttpOrHttpsUrl(string urlString)
        {
            if (string.IsNullOrWhiteSpace(urlString))
            {
                return false;
            }

            if (Uri.TryCreate(urlString, UriKind.Absolute, out Uri uriResult))
            {
                return uriResult.Scheme == Uri.UriSchemeHttp ||
                       uriResult.Scheme == Uri.UriSchemeHttps;
            }
            return false;
        }

        /// <summary>
        /// Get the base path of a file path, which is the path without the file name. For example, the base path of "OEBPS/Text/chapter1.xhtml" is "OEBPS/Text/".
        /// </summary>
        /// <param name="path">The file path</param>
        /// <returns>The base path of the file</returns>
        internal static string GetBasePath(string path)
        {
            int lastSlashIndex = path.LastIndexOf('/');
            if (lastSlashIndex == -1)
            {
                return string.Empty;
            }
            return path[..(lastSlashIndex + 1)];
        }
    }
}
