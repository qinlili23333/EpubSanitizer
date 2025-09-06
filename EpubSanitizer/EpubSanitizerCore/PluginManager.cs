using System.Reflection;


namespace EpubSanitizerCore
{

    internal abstract class PluginInterface
    {
        internal abstract void OnLoad();
    }

    public static class PluginManager
    {
        public static readonly string[] Plugins = [
            "EpubSanitizerCore.Plugins.DemoPlugin",
        ];

        static PluginManager()
        {

        }

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
            LoadPlugins([pluginName]);
        }

        private static void LoadPlugins(string[] pluginList)
        {
            //Scan and load plugins
            string Folder = Path.GetDirectoryName(typeof(PluginManager).Assembly.Location);
            foreach (string plugin in pluginList)
            {
                if (File.Exists(Folder + "\\" + plugin + ".dll"))
                {
                    var plug = Assembly.LoadFrom(Folder + "\\" + plugin + ".dll");
                    var pluginEntryType = plug.GetType(plugin + ".PluginEntry");
                    if (pluginEntryType != null && typeof(PluginInterface).IsAssignableFrom(pluginEntryType))
                    {
                        var pluginInstance = (PluginInterface?)Activator.CreateInstance(pluginEntryType);
                        pluginInstance?.OnLoad();
                    }



                }
            }
        }
    }
}
