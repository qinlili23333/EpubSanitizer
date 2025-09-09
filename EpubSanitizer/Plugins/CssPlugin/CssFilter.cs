using AngleSharp;
using AngleSharp.Css.Dom;
using EpubSanitizerCore.Filters;

namespace EpubSanitizerCore.Plugins.CssPlugin
{
    internal class CssFilter(EpubSanitizer CoreInstance) : MultiThreadFilter(CoreInstance)
    {
        internal override string[] GetProcessList()
        {
            return GetAllCssFiles(Instance);
        }

        internal override void Process(string file)
        {
            string cssString = Instance.FileStorage.ReadString(file);
            var config = Configuration.Default.WithCss();
            var context = BrowsingContext.New(config);
            var document = context.OpenAsync(req => req.Content(cssString)).GetAwaiter().GetResult();
            var stylesheet = document.StyleSheets.OfType<ICssStyleSheet>().FirstOrDefault();
            if (stylesheet == null)
            {
                Instance.Logger($"No valid CSS found in file {file}, skipping.");
                return;
            }
            RemoveInvalidUrlInCss(stylesheet);

        }

        private void RemoveInvalidUrlInCss(ICssStyleSheet sheet)
        {
            foreach (var rule in sheet.Rules.OfType<ICssStyleRule>())
            {
                foreach (var decl in rule.Style)
                {
                    if (decl.Name.Contains("url", StringComparison.OrdinalIgnoreCase))
                    {
                        // Simple check for invalid URL, can be improved
                        if (!Uri.IsWellFormedUriString(decl.Value, UriKind.RelativeOrAbsolute))
                        {
                            rule.Style.RemoveProperty(decl.Name);
                            Instance.Logger($"Removed invalid URL in CSS property {decl.Name}");
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
            string[] files = [];
            foreach (var file in instance.Indexer.ManifestFiles)
            {
                if (file.mimetype == "text/css")
                {
                    files = [.. files, file.path];
                }
            }
            return files;
        }
    }
}
