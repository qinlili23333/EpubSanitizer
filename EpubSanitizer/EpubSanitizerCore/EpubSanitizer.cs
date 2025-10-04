using System.IO.Compression;

namespace EpubSanitizerCore
{
    public class EpubSanitizer
    {
        static readonly Dictionary<string, object> ConfigList = new() {
            {"filter", "default,privacy"},
            {"compress", 0 },
            {"cache", FS.FS.Ram },
            {"threads", Filters.Threads.Multi },
            {"sanitizeNcx", true },
            {"epubVer", 0 },
            {"overwrite", false },
            {"correctMime", true },
            {"xmlCache", true },
            {"publisherMode", false }
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
        public FS.FileSystem FileStorage { get; internal set; }
        /// <summary>
        /// FileIndexer instance to index files, used by many filters
        /// </summary>
        internal FileIndexer Indexer;
        /// <summary>
        /// Logger function, can be override by user to log messages
        /// </summary>
        public Action<string> Logger = (message) => { };
        /// <summary>
        /// Epub version, do upgrade to Epub 3 if possible
        /// </summary>
        internal int TargetEpubVer = 3;
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
            FileStorage ??= FS.FileSystem.CreateFS(this, Config.GetEnum<FS.FS>("cache"));
            FileStorage.Import(archive);
            Logger("Build file index...");
            Indexer = new FileIndexer(this);
            Indexer.IndexFiles();
        }

        /// <summary>
        /// Initialize an empty file system, used when you want to create a new Epub from scratch
        /// </summary>
        public void InitializeEmptyFS()
        {
            FileStorage ??= FS.FileSystem.CreateFS(this, Config.GetEnum<FS.FS>("cache"));
            Indexer = new FileIndexer(this);
        }

        /// <summary>
        /// Process the Epub with all selected filters
        /// </summary>
        public void Process()
        {
            List<string> filters = [.. Config.GetString("filter").Split(',')
                .Select(f => f.Trim().ToLowerInvariant())
                .Where(f => !string.IsNullOrEmpty(f))];
            if (filters.Contains("all"))
            {
                filters = [.. Filters.Filter.Filters.Keys];
            }
            if (TargetEpubVer == 3 && !filters.Contains("epub3"))
            {
                filters.Add("epub3"); // Add epub3 filter if not specified
            }
            filters.ForEach(filterName =>
                {
                    if (Filters.Filter.Filters.TryGetValue(filterName, out Type? filterType))
                    {
                        Logger($"Applying filter: {filterName}");
                        var filterInstance = (Filters.Filter)Activator.CreateInstance(filterType, this);
                        filterInstance.PreProcess();
                        filterInstance.ProcessFiles();
                        filterInstance.PostProcess();
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
            Console.WriteLine("    --filter=xxx              The filter used for xhtml processing, default value is 'default,privacy' which only enables general and privacy filter, and 'epub3' for Epub 3 target. Pass 'all' to enable all available filters.");
            Console.WriteLine("    --compress=0              Compression level used for compressible file, value in number as CompressionLevel Enum of .NET, default value is 0. Not applicable to non-compressible files.");
            Console.WriteLine("    --cache=ram|disk          Where to store cache during sanitization, ram mode privides faster speed but may consume enormous memory, default value is 'ram'.");
            Console.WriteLine("    --threads=single|multi    Enable multithread processing or not, multithread provides faster speed on multi core devices, but may affect system responsibility on low end devices, default value is 'multi'.");
            Console.WriteLine("    --overwrite               Overwrite sanitized file to existing file. If no output file is provided, output will overwrite original file with this option on. If process crashed of power lost, you may lose your file. Use at your own risk!");
            Console.WriteLine("    --sanitizeNcx=true        Sanitize NCX file, enabled by default.");
            Console.WriteLine("    --epubVer=0               Target Epub version, default is 0 (auto, only use Epub 2 when source is Epub 2 and overwrite enabled, otehrwise use Epub 3), acceptable value: 0, 2, 3. You cannot force Epub 2 when source is Epub 3, doing such will be ignored.");
            Console.WriteLine("    --correctMime=true        Correct MIME type in content.opf, enabled by default.");
            Console.WriteLine("    --xmlCache=true           Cache XML parsing result, enabled by default, improve performance for multiple filter processing, but use more memory.");
            Console.WriteLine("    --enablePlugins           Enable plugin support, disabled by default. WARNING: Plugins may contain malicious code, only enable plugins from trusted source.");
            Console.WriteLine("    --publisherMode           Disable all processing on missing resources, helpful for publisher. Disabled by default.");
            Console.WriteLine("Special arguments:");
            Console.WriteLine("    -v                        Print version information.");
            Console.WriteLine("    -h                        Print this general help.");
            Console.WriteLine("    -f                        Print all available filters.");
            Console.WriteLine("    -h filter_name            Print help of specific filter.");
        }
    }
}
