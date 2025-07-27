namespace EpubSanitizerCore
{
    /// <summary>
    /// An abstract class of object with configs
    /// </summary>
    public abstract class ConfigObject
    {
        static readonly Dictionary<string, object> ConfigList;

        static ConfigObject()
        {
            ConfigManager.AddDefaultConfig(ConfigList);
        }

    }
}
