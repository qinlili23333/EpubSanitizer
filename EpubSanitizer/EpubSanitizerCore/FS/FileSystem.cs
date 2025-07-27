using System.IO.Compression;

namespace EpubSanitizerCore.FS
{
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
        internal abstract void Write(string path, string content);
        /// <summary>
        /// Read string content
        /// </summary>
        /// <param name="path">relative path</param>
        /// <returns>string content</returns>
        internal abstract string Read(string path);
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
    }
}
