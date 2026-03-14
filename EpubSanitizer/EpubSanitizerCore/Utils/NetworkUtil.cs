using System;
using System.Collections.Generic;
using System.Text;

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
    }
}
