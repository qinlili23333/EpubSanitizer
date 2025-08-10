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
            return Encoding.UTF8.GetString(ToXmlBytes(doc, minify));
        }

        /// <summary>
        /// Serializes an XmlDocument to a string with optional indentation (minification).
        /// </summary>
        /// <param name="doc">The XmlDocument to serialize.</param>
        /// <param name="minify">If true, the output is minified (no indentation). If false, it's indented for readability.</param>
        /// <returns>The XML content as byte array.</returns>
        public static byte[] ToXmlBytes(XmlDocument doc, bool minify)
        {
            using var memoryStream = new MemoryStream();
            var settings = new XmlWriterSettings
            {
                Encoding = new UTF8Encoding(false),
                Indent = !minify,
                OmitXmlDeclaration = false
            };
            using (var xmlWriter = XmlWriter.Create(memoryStream, settings))
            {
                doc.Save(xmlWriter);
            }
            return memoryStream.ToArray();
        }
    }
}
