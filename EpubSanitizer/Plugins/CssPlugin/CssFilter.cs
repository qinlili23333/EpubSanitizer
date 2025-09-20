using AngleSharp;
using AngleSharp.Css;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Parser;
using EpubSanitizerCore.Filters;
using System.Text.RegularExpressions;

namespace EpubSanitizerCore.Plugins.CssPlugin
{
    internal class CssFilter(EpubSanitizer CoreInstance) : MultiThreadFilter(CoreInstance)
    {
        static readonly Dictionary<string, object> ConfigList = new() {
            {"css.minify", true}
        };
        static CssFilter()
        {
            ConfigManager.AddDefaultConfig(ConfigList);
        }

        internal override string[] GetProcessList()
        {
            return GetAllCssFiles(Instance);
        }

        internal override void Process(string file)
        {
            string cssString = Instance.FileStorage.ReadString(file);
            var cssParser = new CssParser();
            var stylesheet = cssParser.ParseStyleSheet(cssString);
            RemoveInvalidUrlInCss(stylesheet, file);
            var stringWriter = new StringWriter();
            IStyleFormatter formatter = Instance.Config.GetBool("css.minify") ? new MinifyStyleFormatter() : new PrettyStyleFormatter(); // Or CssCompactFormatter for minified output
            stylesheet.ToCss(stringWriter, formatter);
            Instance.FileStorage.WriteString(file, stringWriter.ToString());
        }

        private void RemoveInvalidUrlInCss(ICssStyleSheet sheet, string file)
        {
            foreach (var rule in sheet.Rules.OfType<ICssStyleRule>())
            {
                foreach (var decl in rule.Style.ToArray())
                {
                    if (decl.Value.Contains("url"))
                    {
                        // Simple check for invalid URL, can be improved
                        string path = ConvertUrlToPath(decl.Value);
                        // Ignore data URLs and absolute URLs
                        if (path.StartsWith("data") || path.StartsWith("http"))
                        {
                            continue;
                        }
                        if (!Instance.FileStorage.FileExists(Utils.PathUtil.ComposeFromRelativePath(file, path)))
                        {
                            rule.Style.RemoveProperty(decl.Name);
                            Instance.Logger($"Removed invalid URL in CSS property {decl.Name} targeting {path}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// A static method to get all css files from the EpubSanitizer instance.
        /// </summary>
        /// <param name="instance">EpubSanitizer Instance</param>
        /// <returns>array of css files</returns>
        internal static string[] GetAllCssFiles(EpubSanitizer instance)
        {
            List<string> files = [];
            foreach (var file in instance.Indexer.ManifestFiles)
            {
                if (file.mimetype == "text/css")
                {
                    files.Add(file.path);
                }
            }
            return files.ToArray();
        }

        /// <summary>
        /// Extracts the URL path from a CSS url() property string.
        /// This method handles URLs enclosed in either single or double quotes.
        /// </summary>
        /// <param name="cssUrlString">The CSS url() string, e.g., "url('../Images/contents.jpg')".</param>
        /// <returns>The extracted path, or null if the input format is invalid.</returns>
        private static string ConvertUrlToPath(string cssUrlString)
        {
            // Define a regular expression to match the url(...) pattern.
            // It looks for "url(", then either a single or double quote,
            // then captures the content inside (non-greedily), and then
            // matches the closing quote and parenthesis.
            // The captured group is the path we want.
            string pattern = @"url\(['""]?(.*?)['""]?\)";

            // Create a Regex object.
            Regex regex = new Regex(pattern);

            // Perform the match.
            Match match = regex.Match(cssUrlString);

            // Check if the match was successful and if a group was captured.
            if (match.Success && match.Groups.Count > 1)
            {
                // Return the captured group (the content inside the quotes).
                return match.Groups[1].Value;
            }

            // Return null or an empty string if the format is not matched.
            return string.Empty;
        }

        public static new void PrintHelp()
        {
            Console.WriteLine("Filter applied to css files.");
            Console.WriteLine("Options:");
            Console.WriteLine("    --css.minify=true    Whether to minify CSS output, default is true.");
        }
    }
}
