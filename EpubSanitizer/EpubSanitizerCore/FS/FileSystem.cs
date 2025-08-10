using System.IO.Compression;

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
    internal abstract class FileSystem
    {
        /// <summary>
        /// The instance, mainly used for getting config
        /// </summary>
        internal readonly EpubSanitizer Instance;

        internal FileSystem(EpubSanitizer CoreInstance)
        {
            Instance = CoreInstance;
        }

        /// <summary>
        /// Write string content to target path
        /// </summary>
        /// <param name="path">relative path</param>
        /// <param name="content">string content</param>
        internal abstract void WriteString(string path, string content);
        /// <summary>
        /// Read string content
        /// </summary>
        /// <param name="path">relative path</param>
        /// <returns>string content</returns>
        internal abstract string ReadString(string path);
        /// <summary>
        /// Write byte array content to target path
        /// </summary>
        /// <param name="path">relative path</param>
        /// <param name="content">byte array content</param>
        internal abstract void WriteBytes(string path, byte[] content);
        /// <summary>
        /// Read byte array content
        /// </summary>
        /// <param name="path">relative path</param>
        /// <returns>byte array content</returns>
        internal abstract byte[] ReadBytes(string path);
        /// <summary>
        /// Remove file permanently from file system
        /// </summary>
        /// <param name="path">relative path</param>
        internal abstract void DeleteFile(string path);
        /// <summary>
        /// Calculate SHA256 hash of a file
        /// </summary>
        /// <param name="path">relative path</param>
        /// <returns>SHA256 string</returns>
        internal abstract string GetSHA256(string path);
        /// <summary>
        /// Check if a file exists in the file system
        /// </summary>
        /// <param name="path">relative path</param>
        /// <returns>bool indicates whether file exists</returns>
        internal abstract bool FileExists(string path);
        /// <summary>
        /// Get all files in the file system
        /// </summary>
        /// <returns>string array of all files</returns>
        internal abstract string[] GetAllFiles();

        /// <summary>
        /// Load content from Epub file
        /// </summary>
        /// <param name="EpubFile">The original Epub file</param>
        internal abstract void Import(ZipArchive EpubFile);
        /// <summary>
        /// Export content to a new Epub file
        /// </summary>
        /// <param name="EpubFile"></param>
        internal abstract void Export(ZipArchive EpubFile);

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
