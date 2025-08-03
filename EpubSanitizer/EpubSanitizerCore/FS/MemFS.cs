using System.IO.Compression;

namespace EpubSanitizerCore.FS
{
    internal class MemFS : FileSystem
    {
        private Dictionary<string, byte[]> Files = [];

        public MemFS(EpubSanitizer CoreInstance) : base(CoreInstance)
        {
        }

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
                ZipArchiveEntry entry = EpubFile.CreateEntry(file.Key, (CompressionLevel)Instance.Config.GetInt("compress"));
                using Stream entryStream = entry.Open();
                using MemoryStream ms = new(file.Value);
                ms.CopyTo(entryStream);
            }
        }

        /// <inheritdoc/>
        internal override void Import(ZipArchive EpubFile)
        {
            long totalsize = 0;
            foreach (ZipArchiveEntry entry in EpubFile.Entries)
            {
                if (entry.FullName.EndsWith('/'))
                {
                    continue;
                }
                using Stream entryStream = entry.Open();
                using MemoryStream ms = new();
                entryStream.CopyTo(ms);
                totalsize += ms.Length;
                Files.Add(entry.FullName, ms.ToArray());
            }
            Instance.Logger($"MemoryFS uses about {totalsize / 1024 / 1024} MB memory. Watch out your memory pressure.");
        }

        /// <inheritdoc/>
        internal override string ReadString(string path)
        {
            return Files.TryGetValue(path, out byte[] content)
                ? System.Text.Encoding.UTF8.GetString(content)
                : throw new FileNotFoundException($"File '{path}' not found in memory file system.");
        }

        /// <inheritdoc/>
        internal override void WriteString(string path, string content)
        {
            Files[path] = System.Text.Encoding.UTF8.GetBytes(content);
        }

        /// <inheritdoc/>
        internal override void Dispose()
        {
            Files.Clear();
        }
    }
}
