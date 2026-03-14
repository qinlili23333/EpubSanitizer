using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

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
    }
}
