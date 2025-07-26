using System.Linq;

namespace EpubSanitizerCore
{
    public class ConfigManager
    {
        private static Dictionary<string, object> DefaultConfigList = [];

        internal static void AddDefaultConfig(Dictionary<string, object> List)
        {
            DefaultConfigList = DefaultConfigList.Concat(List).ToDictionary();
        }

        private Dictionary<string, string> ConfigString;

        private Dictionary<string, object> ConfigObj;

        internal ConfigManager()
        {
            ConfigString = [];
            ConfigObj = [];
        }

        public void LoadConfigString(Dictionary<string, string> Config)
        {
            ConfigString = Config.Concat(ConfigString).GroupBy(kvp => kvp.Key).ToDictionary(g => g.Key, g => g.First().Value);
        }

        public void LoadConfigObj(Dictionary<string, object> Config)
        {
            ConfigObj = Config.Concat(ConfigObj).GroupBy(kvp => kvp.Key).ToDictionary(g => g.Key, g => g.First().Value);
        }
    }
}
