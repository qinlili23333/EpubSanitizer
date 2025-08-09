namespace EpubSanitizerCore.Filters
{
    internal class General(EpubSanitizer CoreInstance) : SingleThreadFilter(CoreInstance)
    {
        internal override string[] GetProcessList()
        {
            throw new NotImplementedException();
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
