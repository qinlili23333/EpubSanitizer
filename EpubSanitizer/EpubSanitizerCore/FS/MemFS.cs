using System.Collections.Concurrent;
using System.IO.Compression;

namespace EpubSanitizerCore.FS
{
    internal class MemFS(EpubSanitizer CoreInstance) : FileSystem(CoreInstance)
    {
        private ConcurrentDictionary<string, byte[]> Files = [];

        /// <inheritdoc/>
        internal override void Export(ZipArchive EpubFile)
        {
            base.Export(EpubFile);
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
                Files.TryAdd(entry.FullName, ms.ToArray());
            }
            Instance.Logger($"MemoryFS uses about {totalsize / 1024 / 1024} MB memory. Watch out your memory pressure.");
        }

        /// <inheritdoc/>
        public override string ReadString(string path)
        {
            return Files.TryGetValue(path, out byte[] content)
                ? System.Text.Encoding.UTF8.GetString((content.Length >= 3 && content[0] == 0xEF && content[1] == 0xBB && content[2] == 0xBF) ? [.. content.Skip(3)] : content)
                : throw new FileNotFoundException($"File '{path}' not found in memory file system.");
        }

        /// <inheritdoc/>
        public override void WriteString(string path, string content)
        {
            Files[path] = System.Text.Encoding.UTF8.GetBytes(content);
        }
        /// <inheritdoc/>
        public override void WriteBytes(string path, byte[] content)
        {
            Files[path] = content;
        }
        /// <inheritdoc/>
        public override byte[] ReadBytes(string path)
        {
            return Files.TryGetValue(path, out byte[] content)
                ? content
                : throw new FileNotFoundException($"File '{path}' not found in memory file system.");
        }
        /// <inheritdoc/>
        internal override void Dispose()
        {
            Files.Clear();
        }

        /// <inheritdoc/>
        public override void DeleteFile(string path)
        {
            Files.TryRemove(path, out _);
        }

        /// <inheritdoc/>
        public override string GetSHA256(string path)
        {
            if (Files.TryGetValue(path, out byte[] content))
            {
                byte[] hashBytes = System.Security.Cryptography.SHA256.HashData(content);
                return Convert.ToHexStringLower(hashBytes);
            }
            else
            {
                throw new FileNotFoundException($"File '{path}' not found in memory file system.");
            }
        }

        public override bool FileExists(string path)
        {
            return Files.ContainsKey(path);
        }

        public override string[] GetAllFiles()
        {
            return Files.Keys.ToArray();
        }
    }
}
