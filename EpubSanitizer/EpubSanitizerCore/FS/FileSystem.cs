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

        // TODO: support non-text processing

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
        /// <param name="fs"></param>
        /// <returns></returns>
        internal static FileSystem CreateFS(FS fs)
        {
            return (FileSystem)Activator.CreateInstance(Pairs[fs]);
        }
    }
}
