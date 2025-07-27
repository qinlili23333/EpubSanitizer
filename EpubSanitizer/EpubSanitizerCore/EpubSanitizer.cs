namespace EpubSanitizerCore
{
    public class EpubSanitizer : ConfigObject
    {
        /// <inheritdoc />
        static readonly Dictionary<string, object> ConfigList = new() {
            {"filter", "default"},
            {"compress", 0 }
        };
        /// <summary>
        /// Instance of config manager
        /// </summary>
        public ConfigManager Config;
        /// <summary>
        /// Create a new instance of EpubSanitizer
        /// </summary>
        public EpubSanitizer()
        {
            Config = new();
        }

    }
}
