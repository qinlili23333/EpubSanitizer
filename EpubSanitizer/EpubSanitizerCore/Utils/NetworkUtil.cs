using HeyRed.Mime;

namespace EpubSanitizerCore.Utils
{
    internal static class NetworkUtil
    {
        /// <summary>
        /// Download file and return binary data
        /// Why don't I use async? I just don't want async in a library. Devs using library should care about threading by themselves.
        /// </summary>
        /// <param name="url">Target url</param>
        /// <returns>Binary data of the downloaded file</returns>
        internal static byte[] GetRemoteUrl(string url)
        {
            using var client = new System.Net.Http.HttpClient();
            return client.GetByteArrayAsync(url).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Get mimetype by HEAD request, or if not provided guess from file extension
        /// </summary>
        /// <param name="url">Target url</param>
        /// <returns>mimetype string</returns>
        internal static string GetRemoteMimeType(string url)
        {
            // Only send HEAD request to get mime type, no need to download whole file
            using var client = new System.Net.Http.HttpClient();
            var request = new System.Net.Http.HttpRequestMessage(System.Net.Http.HttpMethod.Head, url);
            var response = client.SendAsync(request).GetAwaiter().GetResult();
            if (response.Content.Headers.ContentType != null && response.Content.Headers.ContentType.MediaType != "application/octet-stream")
            {
                return response.Content.Headers.ContentType.MediaType;
            }
            else
            {
                // Fallback to guess from file extension, if content type is not provided or is generic
                return MimeTypesMap.GetMimeType(url);
            }

        }
    }
}
