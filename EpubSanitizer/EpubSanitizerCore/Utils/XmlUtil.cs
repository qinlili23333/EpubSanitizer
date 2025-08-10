using System.Text;
using System.Xml;

namespace EpubSanitizerCore.Utils
{
    internal static class XmlUtil
    {
        /// <summary>
        /// Return parent node in XmlElement if possible
        /// </summary>
        /// <param name="element">XmlElement</param>
        /// <returns>Parent XmlElement if exists</returns>
        internal static XmlElement? GetParent(XmlElement element)
        {
            XmlNode? parent = element.ParentNode;
            if (parent != null && parent is XmlElement parentElement)
            {
                return parentElement;
            }
            return null;
        }

        /// <summary>
        /// Serializes an XmlDocument to a string with optional indentation (minification).
        /// </summary>
        /// <param name="doc">The XmlDocument to serialize.</param>
        /// <param name="minify">If true, the output is minified (no indentation). If false, it's indented for readability.</param>
        /// <returns>The XML content as a string.</returns>
        public static string ToXmlString(XmlDocument doc, bool minify)
        {
            // We will use a StringWriter as the target for our XmlWriter
            using var stringWriter = new StringWriter();
            var settings = new XmlWriterSettings
            {
                Indent = !minify,
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = false
            };
            using (var xmlWriter = XmlWriter.Create(stringWriter, settings))
            {
                doc.Save(xmlWriter);
            }
            return stringWriter.ToString();
        }
    }
}
