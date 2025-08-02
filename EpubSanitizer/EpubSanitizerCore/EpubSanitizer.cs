using System.IO.Compression;

namespace EpubSanitizerCore
{
    public class EpubSanitizer
    {
        static readonly Dictionary<string, object> ConfigList = new() {
            {"filter", "default"},
            {"compress", 0 },
            {"cache", FS.FS.Ram }
        };
        static EpubSanitizer()
        {
            ConfigManager.AddDefaultConfig(ConfigList);
        }

        /// <summary>
        /// Instance of config manager
        /// </summary>
        public ConfigManager Config;
        /// <summary>
        /// FileSystem instance to hold file
        /// </summary>
        private FS.FileSystem FileStorage;
        /// <summary>
        /// Create a new instance of EpubSanitizer
        /// </summary>
        public EpubSanitizer()
        {
            Config = new();
        }

        /// <summary>
        /// Load Epub file to instance, can only call once for each instance, can safely close archive after load
        /// </summary>
        /// <param name="archive">Opened Epub file for read</param>
        public void LoadFile(ZipArchive archive)
        {
            if (FileStorage != null)
            {
                throw new InvalidOperationException("File already load to instance!");
            }
            FileStorage = FS.FileSystem.CreateFS(this, Config.GetEnum<FS.FS>("cache"));
            FileStorage.Import(archive);
        }

        /// <summary>
        /// Process the Epub with all selected filters
        /// </summary>
        public void Process()
        {

        }

        /// <summary>
        /// Save processed Epub file
        /// </summary>
        /// <param name="archive">Empty file for write, you must create archive with UTF-8 encoding to comply with Epub standard</param>
        public void SaveFile(ZipArchive archive)
        {
            FileStorage.Export(archive);
        }

        /// <summary>
        /// Clean up resources used by this instance
        /// </summary>
        public void Dispose()
        {
            FileStorage.Dispose();
        }
    }
}
