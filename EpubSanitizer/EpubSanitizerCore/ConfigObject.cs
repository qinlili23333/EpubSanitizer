namespace EpubSanitizerCore
{
    /// <summary>
    /// An abstract class of object with configs
    /// </summary>
    public abstract class ConfigObject
    {
        /// <summary>
        /// Dictionary of default configs
        /// </summary>
        static readonly Dictionary<string, object> ConfigList;
        /// <summary>
        /// Config object, add to default config list on first load
        /// </summary>
        static ConfigObject()
        {
            ConfigManager.AddDefaultConfig(ConfigList);
        }

    }
}
