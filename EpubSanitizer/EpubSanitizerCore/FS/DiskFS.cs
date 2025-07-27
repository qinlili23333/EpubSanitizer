using System.IO.Compression;

namespace EpubSanitizerCore.FS
{
    internal class DiskFS : FileSystem
    {
        internal override void Export(ZipArchive EpubFile)
        {
            throw new NotImplementedException();
        }

        internal override void Import(ZipArchive EpubFile)
        {
            throw new NotImplementedException();
        }

        internal override string Read(string path)
        {
            throw new NotImplementedException();
        }

        internal override void Write(string path, string content)
        {
            throw new NotImplementedException();
        }
    }
}
