using EpubSanitizerCore;

namespace EpubSanitizerCore.Extends.EpubWebDownloader
{
    public static class EpubWebDownloader
    {
        public static void SetupProxyFS(EpubSanitizer sanitizer, string baseURL)
        {
            if(!baseURL.EndsWith("/"))
            {
                throw new ArgumentException("Base URL is not a folder URL.");
            }
            if (sanitizer.FileStorage != null)
            {
                throw new Exception("FileSystem object is already set up. Cannot set up ProxyFS. You should not call LoadFile or InitializeEmptyFS if you need ProxyFS.");
            }
            sanitizer.InitializeEmptyFS();
            sanitizer.FileStorage = new ProxyFS(sanitizer, baseURL, sanitizer.FileStorage);
            sanitizer.Indexer.IndexFiles();
        }
    }
}
