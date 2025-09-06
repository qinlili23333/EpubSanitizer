using EpubSanitizerCore.Filters;

namespace EpubSanitizerCore.Plugins.DemoPlugin
{
    internal class DemoPluginFilter(EpubSanitizer CoreInstance) : MultiThreadFilter(CoreInstance)
    {
        internal override string[] GetProcessList()
        {
            return Utils.PathUtil.GetAllXHTMLFiles(Instance);
        }

        internal override void Process(string file)
        {
            Console.WriteLine($"[DemoPlugin] Processing file: {file}");
        }
    }
}
