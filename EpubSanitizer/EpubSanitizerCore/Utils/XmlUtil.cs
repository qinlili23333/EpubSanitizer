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
        /// Serializes an XmlDocument to a byte array with optional indentation (minification).
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

        /// <summary>
        /// Add class name to XmlElement if it does not already exist.
        /// </summary>
        /// <param name="element">element need to add class name</param>
        /// <param name="className">a dedicated class name</param>
        public static void AddCssClass(XmlElement element, string className)
        {
            if (element == null || string.IsNullOrEmpty(className))
            {
                return;
            }
            string existingClass = element.GetAttribute("class");
            if (string.IsNullOrEmpty(existingClass))
            {
                element.SetAttribute("class", className);
            }
            else if (!existingClass.Split(' ').Contains(className))
            {
                element.SetAttribute("class", $"{existingClass} {className}");
            }
        }

        /// <summary>
        /// Normalize xmlns of target namespace URI to no prefix ones
        /// </summary>
        /// <param name="doc">XmlDocument object</param>
        /// <param name="targetNamespaceUri">the namespace require normalize</param>
        public static void NormalizeXmlns(XmlDocument doc, string targetNamespaceUri)
        {
            List<XmlElement> elementsToReplace = [];
            foreach (XmlElement element in doc.GetElementsByTagName("*"))
            {
                if (element.NamespaceURI == targetNamespaceUri)
                {
                    elementsToReplace.Add(element);
                }
            }
            foreach (XmlElement oldElement in elementsToReplace)
            {
                oldElement.Prefix = string.Empty;
            }
            XmlElement root = doc.DocumentElement;
            foreach (XmlAttribute attr in root.Attributes)
            {
                if (attr.Prefix == "xmlns" && attr.Value == targetNamespaceUri)
                {
                    root.Attributes.Remove(attr);
                    break;
                }
            }
            root.SetAttribute("xmlns", targetNamespaceUri);
        }
    }
}
