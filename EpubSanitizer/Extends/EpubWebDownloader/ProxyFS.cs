using EpubSanitizerCore;
using EpubSanitizerCore.FS;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Compression;
using System.Text;
using System.Xml;
using static System.Net.WebRequestMethods;

namespace EpubSanitizerCore.Extends.EpubWebDownloader
{
    internal class ProxyFS(EpubSanitizer CoreInstance, string baseURL, FileSystem backendFS) : FileSystem(CoreInstance)
    {
        private readonly FileSystem BackendFS = backendFS;

        private readonly Uri BaseURL = new(baseURL);

        internal override void Export(ZipArchive EpubFile)
        {
            // Sync XmlCache
            BackendFS.XmlCache = XmlCache;
            BackendFS.Export(EpubFile);
        }
        public override void DeleteFile(string path)
        {
            throw new NotImplementedException();
        }

        public override bool FileExists(string path)
        {
            if (BackendFS.FileExists(path))
            {
                return true;
            }
            return FetchFile(path);
        }

        public override string[] GetAllFiles()
        {
            Instance.Logger("ProxyFS Warning: file list may not be complete as only downloaded files are returned.");
            return BackendFS.GetAllFiles();
        }

        public override string GetSHA256(string path)
        {
            if (!BackendFS.FileExists(path))
            {
                if (!FetchFile(path))
                {
                    throw new FileNotFoundException("File not found and failed to fetch from remote source.", path);
                }
            }
            return BackendFS.GetSHA256(path);
        }

        public override byte[] ReadBytes(string path)
        {
            if (!BackendFS.FileExists(path))
            {
                if (!FetchFile(path))
                {
                    throw new FileNotFoundException("File not found and failed to fetch from remote source.", path);
                }
            }
            return BackendFS.ReadBytes(path);
        }

        public override string ReadString(string path)
        {
            if (!BackendFS.FileExists(path))
            {
                if (!FetchFile(path))
                {
                    throw new FileNotFoundException("File not found and failed to fetch from remote source.", path);
                }
            }
            return BackendFS.ReadString(path);
        }

        public override void WriteBytes(string path, byte[] content)
        {
            BackendFS.WriteBytes(path, content);
        }

        public override void WriteString(string path, string content)
        {
            BackendFS.WriteString(path, content);
        }

        internal override void Dispose()
        {
            BackendFS.Dispose();
        }

        internal override void Import(ZipArchive EpubFile)
        {
            // I originally wanted to throw NotImplementedException here, but I thought it may be useful for hybrid books from some apps.
            // But this is not tested, use with caution.
            BackendFS.Import(EpubFile);
        }

        /// <summary>
        /// Fetch file from remote url and save to backend FileSystem
        /// </summary>
        /// <param name="path">relative path to base url, NOT to OPF</param>
        /// <returns>whether fetch is success (file written to backend)</returns>
        private bool FetchFile(string path)
        {
            // Combine base URL and path to get the full URL
            string url = new Uri(BaseURL, path).ToString();
            RemoteFile file = Instance.Indexer.RemoteManager.GetFulfilledFile(url, false);
            if (file.SHA256 == string.Empty)
            {
                Instance.Logger($"Failed to fetch file from {url}");
                return false;
            }
            else
            {
                Instance.Logger($"Fetched file from {url}");
                BackendFS.WriteBytes(path, file.BinaryData);
                return true;
            }
        }
    }
}
