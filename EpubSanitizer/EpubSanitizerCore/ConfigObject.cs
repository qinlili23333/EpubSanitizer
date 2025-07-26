using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
