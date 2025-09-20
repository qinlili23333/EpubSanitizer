using EpubSanitizerCore.Filters;
using System;
using System.Collections.Generic;
using System.Text;

namespace EpubSanitizerCore.Plugins.CaesiumPlugin
{
    internal class Caesium(EpubSanitizer CoreInstance) : MultiThreadFilter(CoreInstance)
    {
        internal override string[] GetProcessList()
        {
            throw new NotImplementedException();
        }

        internal override void Process(string file)
        {
            throw new NotImplementedException();
        }
    }
}
