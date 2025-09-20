namespace EpubSanitizerCore.Plugins.CaesiumPlugin
{
    internal class PluginEntry : PluginInterface
    {
        internal override void OnLoad(Version CoreVersion)
        {
            if (CoreVersion < Version.Parse(MinimumCoreVersion))
            {
                throw new Exception($"Plugin CaesiumPlugin requires minimum core version {MinimumCoreVersion}, current core version is {CoreVersion}");
            }
            Filters.Filter.Filters.Add("caesium", typeof(Caesium));
            Console.WriteLine("CaesiumPlugin filter registered as 'caesium'");
        }

        private readonly string MinimumCoreVersion = "1.4.0";
    }
}
