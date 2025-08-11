using System.IO.Compression;

namespace EpubSanitizerCore
{
    public class EpubSanitizer
    {
        static readonly Dictionary<string, object> ConfigList = new() {
            {"filter", "default"},
            {"compress", 0 },
            {"cache", FS.FS.Ram },
            {"threads", Filters.Threads.Multi },
            {"sanitizeNcx", true }
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
            Logger("Build file index...");
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
                    filterName = filterName.ToLowerInvariant();
                    if (Filters.Filter.Filters.TryGetValue(filterName, out Type? filterType))
                    {
                        Logger($"Applying filter: {filterName}");
                        var filterInstance = (Filters.Filter)Activator.CreateInstance(filterType, this);
                        filterInstance.ProcessFiles();
                        GC.Collect(); // Force garbage collection to clean up resources after filter processing
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
            Indexer.UpdateOpf();
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

        /// <summary>
        /// Print command line usage
        /// </summary>
        public static void PrintUsage()
        {
            Console.WriteLine("Usage: EpubSanitizerCLI <options> file <output>");
            Console.WriteLine("e.g. EpubSanitizerCLI --filter=default,vitalsource extract.epub sanitized.epub");
            Console.WriteLine();
            Console.WriteLine("Universal options:");
            Console.WriteLine("    --filter=xxx              The filter used for xhtml processing, default value is 'default' which only enables general filter");
            Console.WriteLine("    --compress=0              Compression level used for compressible file, value in number as CompressionLevel Enum of .NET, default value is 0. Not applicable to non-compressible files.");
            Console.WriteLine("    --cache=ram|disk          Where to store cache during sanitization, ram mode privides faster speed but may consume enormous memory, default value is 'ram'.");
            Console.WriteLine("    --threads=single|multi    Enable multithread processing or not, multithread provides faster speed on multi core devices, but may affect system responsibility on low end devices, default value is 'multi'.");
            Console.WriteLine("    --overwrite               Overwrite sanitized file to existing file. If no output file is provided, output will overwrite original file with this option on. If process crashed of power lost, you may lose your file. Use at your own risk!");
            Console.WriteLine("    --sanitizeNcx=true        Sanitize NCX file, enabled by default.");
            Console.WriteLine("Special arguments:");
            Console.WriteLine("    -v                        Print version information.");
            Console.WriteLine("    -h                        Print this general help.");
            Console.WriteLine("    -f                        Print all available filters.");
            Console.WriteLine("    -h filter_name            Print help of specific filter.");
        }
    }
}
