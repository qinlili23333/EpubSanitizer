namespace EpubSanitizerCore.Plugins.DemoPlugin
{
    internal class PluginEntry : PluginInterface
    {
        internal override void OnLoad(Version CoreVersion)
        {
            if(CoreVersion < Version.Parse(MinimumCoreVersion))
            {
                throw new Exception($"Plugin DemoPlugin requires minimum core version {MinimumCoreVersion}, current core version is {CoreVersion}");
            }
            Console.WriteLine("DemoPlugin loaded!");
            Console.WriteLine("Supported plugins: " + string.Join(", ", PluginManager.Plugins));
            Filters.Filter.Filters.Add("demoplugin", typeof(DemoPluginFilter));
            Console.WriteLine("DemoPlugin filter registered as 'demoplugin'");
        }

        private string MinimumCoreVersion = "1.3.0";
    }
}
