using System.Collections.Concurrent;
using System.IO.Compression;
using System.Xml;

namespace EpubSanitizerCore.FS
{
    internal enum FS
    {
        /// <summary>
        /// Memory based file system
        /// </summary>
        Ram,
        /// <summary>
        /// Disk based file system
        /// </summary>
        Disk
    }

    /// <summary>
    /// Abstract class of file system used for processing
    /// </summary>
    public abstract class FileSystem
    {
        /// <summary>
        /// The instance, mainly used for getting config
        /// </summary>
        internal readonly EpubSanitizer Instance;

        /// <summary>
        /// Controls whether the XHTML fix plugin is loaded
        /// </summary>
        internal static bool XhtmlFixPluginLoaded = false;
        /// <summary>
        /// The function to fix xhtml, set by XhtmlFixPlugin if loaded
        /// </summary>
        internal static Func<string, string>? FixXhtml = null;

        internal FileSystem(EpubSanitizer CoreInstance)
        {
            Instance = CoreInstance;
        }

        /// <summary>
        /// Dictionary to store cached XmlDocument, key is the relative path in epub
        /// </summary>
        private ConcurrentDictionary<string, XmlDocument> XmlCache = [];

        /// <summary>
        /// Get XmlDocument from path, will use cache if enabled
        /// </summary>
        /// <param name="path">relative path in epub</param>
        /// <returns>XmlDocument object if exist, null if file not found or cannot parse</returns>
        internal XmlDocument? ReadXml(string path)
        {
            if (XmlCache.TryGetValue(path, out XmlDocument? value))
            {
                return value;
            }
            else
            {
                XmlDocument doc = new();
                try
                {
                    doc.LoadXml(ReadString(path).Replace("&nbsp;", "\u00A0"));
                    foreach (XmlNode node in doc.SelectNodes("//comment()").Cast<XmlNode>().ToArray())
                    {
                        node.ParentNode.RemoveChild(node);
                    }
                }
                catch (XmlException ex)
                {
                    if (XhtmlFixPluginLoaded && FixXhtml != null)
                    {
                        Instance.Logger($"XHTML file {path} is malformed, trying to fix it...");
                        try
                        {
                            doc.LoadXml(FixXhtml(ReadString(path).Replace("&nbsp;", "\u00A0")));
                        }
                        catch (Exception fixEx)
                        {
                            Instance.Logger($"Error loading XHTML file {path} after fix attempt: {fixEx.Message}");
                            return null;
                        }
                    }
                    else
                    {
                        Instance.Logger($"Error loading XHTML file {path}: {ex.Message}");
                        return null;
                    }
                }
                catch (FileNotFoundException)
                {
                    Instance.Logger($"XHTML file {path} not exist.");
                    return null;
                }
                if (Instance.Config.GetBool("xmlCache"))
                {
                    XmlCache[path] = doc;
                }
                return doc;
            }
        }

        /// <summary>
        /// Write XmlDocument to path, will use cache if enabled
        /// </summary>
        /// <param name="path">relative path</param>
        /// <param name="doc">XmlDocument object</param>
        internal void WriteXml(string path, XmlDocument doc)
        {
            if (!Instance.Config.GetBool("xmlCache"))
            {
                WriteBytes(path, Utils.XmlUtil.ToXmlBytes(doc, false));
                XmlCache.TryRemove(path, out _);
            }
            else
            {
                XmlCache[path] = doc;
            }
        }

        /// <summary>
        /// Remove xml from cache, use it if you modified the file without using Read/WriteXml functions
        /// </summary>
        /// <param name="path">relative path</param>
        internal void FlushXmlCache(string path)
        {
            XmlCache.Remove(path, out _);
        }



        /// <summary>
        /// Write string content to target path
        /// </summary>
        /// <param name="path">relative path</param>
        /// <param name="content">string content</param>
        public abstract void WriteString(string path, string content);
        /// <summary>
        /// Read string content
        /// </summary>
        /// <param name="path">relative path</param>
        /// <returns>string content</returns>
        public abstract string ReadString(string path);
        /// <summary>
        /// Write byte array content to target path
        /// </summary>
        /// <param name="path">relative path</param>
        /// <param name="content">byte array content</param>
        public abstract void WriteBytes(string path, byte[] content);
        /// <summary>
        /// Read byte array content
        /// </summary>
        /// <param name="path">relative path</param>
        /// <returns>byte array content</returns>
        public abstract byte[] ReadBytes(string path);
        /// <summary>
        /// Remove file permanently from file system
        /// </summary>
        /// <param name="path">relative path</param>
        public abstract void DeleteFile(string path);
        /// <summary>
        /// Calculate SHA256 hash of a file
        /// </summary>
        /// <param name="path">relative path</param>
        /// <returns>SHA256 string</returns>
        public abstract string GetSHA256(string path);
        /// <summary>
        /// Check if a file exists in the file system
        /// </summary>
        /// <param name="path">relative path</param>
        /// <returns>bool indicates whether file exists</returns>
        public abstract bool FileExists(string path);
        /// <summary>
        /// Get all files in the file system
        /// </summary>
        /// <returns>string array of all files</returns>
        public abstract string[] GetAllFiles();

        /// <summary>
        /// Load content from Epub file
        /// </summary>
        /// <param name="EpubFile">The original Epub file</param>
        internal abstract void Import(ZipArchive EpubFile);
        /// <summary>
        /// Export content to a new Epub file
        /// </summary>
        /// <param name="EpubFile"></param>
        internal virtual void Export(ZipArchive EpubFile)
        {
            if (Instance.Config.GetBool("xmlCache"))
            {
                // write cached xml files first
                Instance.Logger($"Writing {XmlCache.Count} cached XML files to file system.");
                Parallel.ForEach(XmlCache, pair =>
                {
                    WriteBytes(pair.Key, Utils.XmlUtil.ToXmlBytes(pair.Value, false));
                });
            }
            // write mimetype first
            ZipArchiveEntry mimetypeEntry = EpubFile.CreateEntry("mimetype", CompressionLevel.NoCompression);
            using (Stream mimetypeStream = mimetypeEntry.Open())
            {
                using StreamWriter writer = new(mimetypeStream);
                writer.Write("application/epub+zip");
            }
        }

        /// <summary>
        /// Dispose all resources used by this file system
        /// </summary>
        internal abstract void Dispose();

        /// <summary>
        /// Internal relationship of enum and class
        /// </summary>
        private static readonly Dictionary<FS, Type> Pairs = new(){
            {FS.Ram,typeof(MemFS)},
            {FS.Disk,typeof(DiskFS)},
        };
        /// <summary>
        /// Create file system instance
        /// </summary>
        /// <param name="Instance">Instance of EpubSanitizer</param>
        /// <param name="fs">Target file system</param>
        /// <returns></returns>
        internal static FileSystem CreateFS(EpubSanitizer Instance, FS fs)
        {
            return (FileSystem)Activator.CreateInstance(Pairs[fs], [Instance]);
        }
    }
}
