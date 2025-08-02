using System.IO.Compression;
using System.Security.Cryptography;

namespace EpubSanitizerCore.FS
{
    internal class DiskFS : FileSystem
    {
        /// <summary>
        /// Create 8 character random string from current timestamp
        /// </summary>
        /// <returns></returns>
        private static string GenerateRandomStringFromTimestamp()
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            byte[] timestampBytes = BitConverter.GetBytes(timestamp);
            using SHA256 sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(timestampBytes);
            System.Text.StringBuilder stringBuilder = new();
            for (int i = 0; i < 4; i++)
            {
                stringBuilder.Append(hashBytes[i].ToString("x2"));
            }
            return stringBuilder.ToString();
        }
        /// <summary>
        /// Folder to hold files
        /// </summary>
        private readonly string Folder = Path.GetTempPath() + "\\" + GenerateRandomStringFromTimestamp();

        public DiskFS(EpubSanitizer CoreInstance) : base(CoreInstance)
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

            foreach (string filePath in Directory.GetFiles(Folder, "*", SearchOption.AllDirectories))
            {
                // skip mimetype file
                if (filePath.EndsWith("mimetype"))
                {
                    continue;
                }
                string relativePath = Path.GetRelativePath(Folder, filePath);
                string entryName = Path.Combine("EpubSanitizerExport", relativePath).Replace('\\', '/');
                ZipArchiveEntry entry = EpubFile.CreateEntry(entryName, (CompressionLevel)Instance.Config.GetInt("compress"));
                using FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read);
                using Stream entryStream = entry.Open();
                fileStream.CopyTo(entryStream);
            }
        }

        /// <inheritdoc/>
        internal override void Import(ZipArchive EpubFile)
        {
            Directory.CreateDirectory(Folder);
            EpubFile.ExtractToDirectory(Folder, true);
        }

        /// <inheritdoc/>
        internal override string ReadString(string path)
        {
            if(!File.Exists(Path.Combine(Folder, path)))
            {
                throw new FileNotFoundException($"File '{path}' does not exist in the file system.");
            }
            return File.ReadAllText(Path.Combine(Folder, path));
        }

        /// <inheritdoc/>
        internal override void WriteString(string path, string content)
        {
            File.WriteAllText(Path.Combine(Folder, path), content);
        }

        /// <inheritdoc/>
        internal override void Dispose()
        {
            Directory.Delete(Folder, true);
        }
    }
}
