namespace EpubSanitizerCore.Plugins.XhtmlFixPlugin
{
    internal class PluginEntry : PluginInterface
    {
        internal override void OnLoad(Version CoreVersion)
        {
            if (CoreVersion < Version.Parse(MinimumCoreVersion))
            {
                throw new Exception($"Plugin XhtmlFixPlugin requires minimum core version {MinimumCoreVersion}, current core version is {CoreVersion}");
            }
            FS.FileSystem.XhtmlFixPluginLoaded = true;
            Console.WriteLine("Xhtml fix plugin loaded, corrupted xhtml will be fixed if possible.");
        }

        private readonly string MinimumCoreVersion = "1.6.0";
    }
}
