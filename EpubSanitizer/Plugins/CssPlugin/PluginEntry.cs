namespace EpubSanitizerCore.Plugins.CssPlugin
{
    internal class PluginEntry : PluginInterface
    {
        internal override void OnLoad(Version CoreVersion)
        {
            if (CoreVersion < Version.Parse(MinimumCoreVersion))
            {
                throw new Exception($"Plugin CssPlugin requires minimum core version {MinimumCoreVersion}, current core version is {CoreVersion}");
            }
            Filters.Filter.Filters.Add("css", typeof(CssFilter));
            Console.WriteLine("CssPlugin filter registered as 'css'");
        }

        private string MinimumCoreVersion = "1.3.0";
    }
}
