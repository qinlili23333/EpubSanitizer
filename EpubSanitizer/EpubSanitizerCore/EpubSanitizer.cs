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
        internal FS.FileSystem FileStorage;
        /// <summary>
        /// FileIndexer instance to index files, used by many filters
        /// </summary>
        internal FileIndexer Indexer;
        /// <summary>
        /// Logger function, can be override by user to log messages
        /// </summary>
        public Action<string> Logger = (message) => { };
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
            Indexer = new FileIndexer(this);
            Indexer.IndexFiles();
        }

        /// <summary>
        /// Process the Epub with all selected filters
        /// </summary>
        public void Process()
        {
            Config.GetString("filter").Split(',')
                .Select(f => f.Trim())
                .Where(f => !string.IsNullOrEmpty(f))
                .ToList()
                .ForEach(filterName =>
                {
                    if (Filters.Filter.Filters.TryGetValue(filterName, out Type? filterType))
                    {
                        Logger($"Applying filter: {filterName}");
                        var filterInstance = (Filters.Filter)Activator.CreateInstance(filterType, this);
                        filterInstance.ProcessFiles();
                    }
                    else
                    {
                        Logger($"Filter '{filterName}' not found, skipping.");
                    }
                });
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

        /// <summary>
        /// Static method to get all available filters
        /// </summary>
        /// <returns></returns>
        public static string[] GetFilters()
        {
            return [.. Filters.Filter.Filters.Keys];
        }

        /// <summary>
        /// Print filter help to console
        /// </summary>
        /// <param name="filter">filter name</param>
        public static void PrintFilterHelp(string filter)
        {
            Filters.Filter.Filters.TryGetValue(filter, out Type? filterType);
            if (filterType == null)
            {
                Console.WriteLine($"Filter '{filter}' not found.");
                return;
            }
            filterType.GetMethod("PrintHelp", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                ?.Invoke(null, null);
        }
    }
}
