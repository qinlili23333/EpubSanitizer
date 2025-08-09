namespace EpubSanitizerCore.Filters
{
    internal class General(EpubSanitizer CoreInstance) : SingleThreadFilter(CoreInstance)
    {
        /// <summary>
        /// General filter only processes XHTML files.
        /// </summary>
        /// <returns>list of XHTML files</returns>
        internal override string[] GetProcessList()
        {
            string[] files = [];
            foreach (var file in Instance.Indexer.ManifestFiles)
            {
                if (file.mimetype == "application/xhtml+xml" || file.mimetype == "application/xml")
                {
                    files =[..files,file.path];
                }
            }
            return files;
        }

        internal override void Process(string file)
        {
            throw new NotImplementedException();
        }

        public static new void PrintHelp()
        {
            Console.WriteLine("General filter is a default filter that does basic processing for standard fixing.");
            Console.WriteLine("Options:");
            Console.WriteLine("    --general.deprecateFix=true    Fix deprecated attributes if possible.");
            Console.WriteLine("    --general.minify=false         Minify XHTML files, may accelerate some reader.");
        }
    }
}
