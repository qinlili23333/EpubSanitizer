using System.Reflection;


namespace EpubSanitizerCore
{
    /// <summary>
    /// Basic interface for plugin to implement
    /// </summary>
    internal abstract class PluginInterface
    {
        /// <summary>
        /// OnLoad will be called when plugin is loaded
        /// </summary>
        internal abstract void OnLoad(Version CoreVersion);
    }

    public static class PluginManager
    {
        /// <summary>
        /// Supported plugins, keep it same as project InternalsVisibleTo in EpubSanitizerCore.csproj
        /// </summary>
        public static readonly string[] Plugins = [
            "EpubSanitizerCore.Plugins.DemoPlugin",
        ];

        /// <summary>
        /// Enable all plugins
        /// </summary>
        public static void EnablePlugins()
        {
            LoadPlugins(Plugins);
        }

        /// <summary>
        /// Enable specific plugin
        /// </summary>
        /// <param name="pluginName">plugin assembly name</param>
        public static void EnablePlugin(string pluginName)
        {
            if(!Plugins.Contains(pluginName))
            {
                throw new ArgumentException("Plugin not supported: " + pluginName);
            }
            LoadPlugins([pluginName]);
        }

        /// <summary>
        /// Load and initialize plugins from list
        /// </summary>
        /// <param name="pluginList">string array of plugin assembly name</param>
        private static void LoadPlugins(string[] pluginList)
        {
            //Scan and load plugins
            string Folder = Path.GetDirectoryName(typeof(PluginManager).Assembly.Location) ?? Path.GetDirectoryName(Environment.ProcessPath);
            foreach (string plugin in pluginList)
            {
                if (File.Exists(Path.Combine(Folder, plugin + ".dll")))
                {
                    var plug = Assembly.LoadFrom(Path.Combine(Folder, plugin + ".dll"));
                    var pluginEntryType = plug.GetType(plugin + ".PluginEntry");
                    if (pluginEntryType != null && typeof(PluginInterface).IsAssignableFrom(pluginEntryType))
                    {
                        var pluginInstance = (PluginInterface?)Activator.CreateInstance(pluginEntryType) ?? throw new Exception("Failed to create instance of plugin: " + plugin);
                        pluginInstance.OnLoad(typeof(PluginManager).Assembly.GetName().Version);
                    }
                }
            }
        }
    }
}
