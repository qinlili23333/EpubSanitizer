using System.Linq;

namespace EpubSanitizerCore
{
    /// <summary>
    /// An exception raised when config not found
    /// </summary>
    public class ConfigNotFoundException : Exception
    {
        internal ConfigNotFoundException(string message) : base($"'{message}' does not exist in config")
        {
        }
    }
    /// <summary>
    /// Config manager for current EpubSanitizer instance
    /// </summary>
    public class ConfigManager
    {
        /// <summary>
        /// Static dictionary to store all default value added in static constructor
        /// </summary>
        private static Dictionary<string, object> DefaultConfigList = [];
        /// <summary>
        /// Add default value to dictionary, only call by static constructor
        /// </summary>
        /// <param name="List"></param>
        internal static void AddDefaultConfig(Dictionary<string, object> List)
        {
            DefaultConfigList = DefaultConfigList.Concat(List).ToDictionary();
        }

        /// <summary>
        /// Added configs in string format for lazy parse
        /// </summary>
        private Dictionary<string, string> ConfigString;
        /// <summary>
        /// Added configs already parsed to target type
        /// </summary>
        private Dictionary<string, object> ConfigObj;
        /// <summary>
        /// Create a new instance of config manager
        /// </summary>
        internal ConfigManager()
        {
            ConfigString = [];
            ConfigObj = [];
        }
        /// <summary>
        /// Add configs in string format
        /// </summary>
        /// <param name="Config"></param>
        public void LoadConfigString(Dictionary<string, string> Config)
        {
            ConfigString = Config.Concat(ConfigString).GroupBy(kvp => kvp.Key).ToDictionary(g => g.Key, g => g.First().Value);
        }
        /// <summary>
        /// Add parsed object configs
        /// </summary>
        /// <param name="Config"></param>
        public void LoadConfigObj(Dictionary<string, object> Config)
        {
            ConfigObj = Config.Concat(ConfigObj).GroupBy(kvp => kvp.Key).ToDictionary(g => g.Key, g => g.First().Value);
        }
        /// <summary>
        /// Get config by type, private method for universal process
        /// </summary>
        /// <param name="key">name of key</param>
        /// <param name="type">type of the key</param>
        /// <returns>config object if exist</returns>
        /// <exception cref="ConfigNotFoundException">config not found</exception>
        private dynamic GetByType(string key, Type type)
        {
            if (ConfigString.TryGetValue(key, out string? str))
            {
                if (type == typeof(int))
                {
                    ConfigObj[key] = int.Parse(str);
                }
                else if (type == typeof(string))
                {
                    ConfigObj[key] = str;
                }else if(type == typeof(bool))
                {
                    ConfigObj[key] = bool.Parse(str);
                }
                ConfigString.Remove(key);
                return Convert.ChangeType(ConfigObj[key], type);
            }
            if (ConfigObj.TryGetValue(key, out object? value))
            {
                return Convert.ChangeType(value, type);
            }
            if (DefaultConfigList.TryGetValue(key, out object? defval))
            {
                return Convert.ChangeType(defval, type);
            }
            throw new ConfigNotFoundException(key);
        }
        /// <summary>
        /// Get config in int
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public int GetInt(string key)
        {
            return GetByType(key, typeof(int));
        }
        /// <summary>
        /// Get config in string
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetString(string key)
        {
            return GetByType(key,typeof(string));
        }
        /// <summary>
        /// Get config in bool
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool GetBool(string key)
        {
            return GetByType(key, typeof(bool));
        }


    }
}
