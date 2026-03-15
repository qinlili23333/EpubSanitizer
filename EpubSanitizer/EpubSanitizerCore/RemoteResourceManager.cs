using System.Collections.Concurrent;

namespace EpubSanitizerCore
{
    internal class RemoteFile
    {
        internal required string Url;
        internal string mimetype = "application/octet-stream";
        internal string SHA256 = string.Empty;
        internal byte[] BinaryData = [];

    }
    internal class RemoteResourceManager
    {
        /// <summary>
        /// The instance, mainly used for getting config
        /// </summary>
        internal readonly EpubSanitizer Instance;
        /// <summary>
        /// Dictionary of remote files, the key is the URL, and the value is the RemoteFile object.
        /// </summary>
        internal ConcurrentDictionary<string, RemoteFile> RemoteFiles = new();
        internal RemoteResourceManager(EpubSanitizer CoreInstance)
        {
            Instance = CoreInstance;
        }

        /// <summary>
        /// Get file from the URL, if the file is already downloaded, return it directly, otherwise download it and return it.
        /// </summary>
        /// <param name="url">target url</param>
        /// <returns>RemoteFile object</returns>
        internal RemoteFile GetFulfilledFile(string url)
        {
            if (RemoteFiles.TryGetValue(url, out RemoteFile file))
            {
                return file;
            }
            else
            {
                // If not exist, create a new one and download it
                file = new RemoteFile() { Url = url };
                try
                {
                    file.mimetype = Utils.NetworkUtil.GetRemoteMimeType(url);
                    file.BinaryData = Utils.NetworkUtil.GetRemoteUrl(url);
                    file.SHA256 = Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(file.BinaryData));
                    RemoteFiles.TryAdd(url, file);
                    Instance.Logger("Successfully downloaded remote resource: " + url);
                }
                catch (Exception ex)
                {
                    Instance.Logger($"Failed to download remote resource: {url}. Error: {ex.Message}");
                    file.BinaryData = [];
                    file.mimetype = "application/octet-stream";
                    file.SHA256 = string.Empty;
                }
                return file;
            }
        }

        /// <summary>
        /// Get data URI from the RemoteFile object, the format is "data:[mimetype];base64,[base64Data]", if the file is empty, return an empty string.
        /// </summary>
        /// <param name="file">RemoteFile object</param>
        /// <returns>Data URI string</returns>
        internal static string ConvertDataUri(RemoteFile file)
        {
            string base64Data = Convert.ToBase64String(file.BinaryData);
            return $"data:{file.mimetype};base64,{base64Data}";
        }

        /// <summary>
        /// Returns a data URI string representing the contents of the remote file at the specified URL.
        /// </summary>
        /// <param name="url">The URL of the remote file to retrieve and convert to a data URI. Cannot be null or empty.</param>
        /// <returns>A data URI string containing the file's binary data if available; otherwise, an empty string.</returns>
        internal string GetDataUriFromUrl(string url)
        {
            RemoteFile file = GetFulfilledFile(url);
            if (file.BinaryData.Length > 0)
            {
                return ConvertDataUri(file);
            }
            else
            {
                return string.Empty;
            }
        }




        /// <summary>
        /// Dispose cached network resources
        /// </summary>
        internal void Dispose()
        {
            RemoteFiles.Clear();
        }
    }
}
