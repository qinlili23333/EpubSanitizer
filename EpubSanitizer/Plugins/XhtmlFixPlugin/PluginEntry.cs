using HtmlAgilityPack;
using System.Xml;

namespace EpubSanitizerCore.Plugins.XhtmlFixPlugin
{
    internal class PluginEntry : PluginInterface
    {
        internal override void OnLoad(Version CoreVersion)
        {
            if (CoreVersion < Version.Parse(MinimumCoreVersion))
            {
                throw new Exception($"Plugin XhtmlFixPlugin requires minimum core version {MinimumCoreVersion}, current core version is {CoreVersion}");
            }
            FS.FileSystem.XhtmlFixPluginLoaded = true;
            FS.FileSystem.FixXhtml = FixXhtml;
            Console.WriteLine("Xhtml fix plugin loaded, corrupted xhtml will be fixed if possible.");
        }

        internal static string FixXhtml(string input)
        {
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(input);
            htmlDoc.OptionOutputAsXml = true;
            using var sw = new StringWriter();
            using var xw = XmlWriter.Create(sw);
            htmlDoc.Save(xw);
            xw.Flush();
            return sw.ToString();
        }


        private readonly string MinimumCoreVersion = "1.6.0";
    }
}
