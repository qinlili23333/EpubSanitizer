using System.Xml;

namespace EpubSanitizerCore.Utils
{
    internal static class TocGenerator
    {
        /// <summary>
        /// Empty template, loaded at static constructor.
        /// </summary>
        private readonly static XmlDocument emptyNavDoc = new();
        /// <summary>
        /// Parse string template for nav.xhtml document.
        /// Only do once and cached in static field to accelerate for batch processing.
        /// </summary>
        static TocGenerator()
        {
            emptyNavDoc.LoadXml(Res.TocXhtmlTemplate);
        }
        /// <summary>
        /// Generate a new nav.xhtml document based on the NCX file.
        /// </summary>
        /// <param name="ncxDoc">NCX XmlDocument</param>
        /// <returns>xhtml XmlDocument object</returns>
        internal static XmlDocument Generate(XmlDocument ncxDoc)
        {
            XmlDocument navDoc = emptyNavDoc.Clone() as XmlDocument;
            return null;
        }
    }
}
