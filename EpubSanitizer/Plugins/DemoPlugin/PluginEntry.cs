namespace EpubSanitizerCore.Plugins.DemoPlugin
{
    internal class PluginEntry : PluginInterface
    {
        internal override void OnLoad()
        {
            Console.WriteLine("DemoPlugin loaded!");
            Console.WriteLine("Supported plugins: " + string.Join(", ", PluginManager.Plugins));
        }
    }
}
