namespace EpubSanitizerCore.Plugins.DemoPlugin
{
    internal class PluginEntry : PluginInterface
    {
        internal override void OnLoad()
        {
            Console.WriteLine("DemoPlugin loaded!");
            Console.WriteLine("Supported plugins: " + string.Join(", ", PluginManager.Plugins));
            Filters.Filter.Filters.Add("demoplugin", typeof(DemoPluginFilter));
            Console.WriteLine("DemoPlugin filter registered as 'demoplugin'");
        }
    }
}
