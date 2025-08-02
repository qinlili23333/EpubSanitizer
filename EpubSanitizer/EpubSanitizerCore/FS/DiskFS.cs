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
            System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
            for (int i = 0; i < 4; i++)
            {
                stringBuilder.Append(hashBytes[i].ToString("x2"));
            }
            return stringBuilder.ToString();
        }
        /// <summary>
        /// Folder to hold files
        /// </summary>
        private readonly string Folder = Path.GetTempPath()+"\\"+ GenerateRandomStringFromTimestamp();

        /// <inheritdoc/>
        internal override void Export(ZipArchive EpubFile)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc/>
        internal override void Import(ZipArchive EpubFile)
        {
            Directory.CreateDirectory(Folder);
            EpubFile.ExtractToDirectory(Folder, true);
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
            throw new NotImplementedException();
        }
    }
}
