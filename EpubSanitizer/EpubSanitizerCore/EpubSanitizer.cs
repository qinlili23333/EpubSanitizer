using System.Security.Cryptography.X509Certificates;

namespace EpubSanitizerCore
{
    public class EpubSanitizer : ConfigObject
    {
        static readonly Dictionary<string, object> ConfigList = new() {
            {"filter", "default"},
            {"compress", 0 }
        };

        public ConfigManager Config;

        public EpubSanitizer()
        {
            Config = new();
        }
    }
}
