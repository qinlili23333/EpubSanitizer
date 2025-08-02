using System.IO.Compression;

namespace EpubSanitizerCore.FS
{
    internal class MemFS : FileSystem
    {
        private Dictionary<string, byte[]> Files = [];

        /// <inheritdoc/>
        internal override void Export(ZipArchive EpubFile)
        {
            // write mimetype first
            ZipArchiveEntry mimetypeEntry = EpubFile.CreateEntry("mimetype", CompressionLevel.NoCompression);
            using (Stream mimetypeStream = mimetypeEntry.Open())
            {
                using StreamWriter writer = new(mimetypeStream);
                writer.Write("application/epub+zip");
            }
            foreach (var file in Files)
            {
                // skip mimetype file
                if (file.Key == "mimetype")
                {
                    continue;
                }
                ZipArchiveEntry entry = EpubFile.CreateEntry(file.Key, CompressionLevel.Optimal);
                using Stream entryStream = entry.Open();
                using MemoryStream ms = new(file.Value);
                ms.CopyTo(entryStream);
            }
        }

        /// <inheritdoc/>
        internal override void Import(ZipArchive EpubFile)
        {
            foreach (ZipArchiveEntry entry in EpubFile.Entries)
            {
                if (entry.FullName.EndsWith('/'))
                {
                    continue;
                }
                using Stream entryStream = entry.Open();
                using MemoryStream ms = new();
                entryStream.CopyTo(ms);
                Files.Add(entry.FullName, ms.ToArray());
            }
        }

        /// <inheritdoc/>
        internal override string Read(string path)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        internal override void Write(string path, string content)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        internal override void Dispose()
        {
            Files.Clear();
        }
    }
}
