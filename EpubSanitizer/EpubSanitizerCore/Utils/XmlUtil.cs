using System.Text;
using System.Xml;

namespace EpubSanitizerCore.Utils
{
    internal static class XmlUtil
    {
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
            foreach (XmlElement element in doc.GetElementsByTagName("*").Cast<XmlElement>().ToArray())
            {
                if (element.NamespaceURI == targetNamespaceUri)
                {
                    element.Prefix = string.Empty;
                }

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
            // Recursive mode
            bool needAnotherPass = true;
            while (needAnotherPass)
            {
                needAnotherPass = false;
                foreach (XmlElement element in doc.GetElementsByTagName("*").Cast<XmlElement>().ToArray())
                {
                    if (element.NamespaceURI == string.Empty)
                    {
                        // Recreate with proper namespace URI
                        XmlElement newElement = doc.CreateElement(element.LocalName, targetNamespaceUri);
                        CopyTo(element, newElement);
                        // Replace the old element with the new one
                        element.ParentNode.ReplaceChild(newElement, element);
                        needAnotherPass = true;
                    }
                    foreach (XmlAttribute attr in element.Attributes.Cast<XmlAttribute>().ToArray())
                    {
                        if (attr.Prefix != string.Empty && attr.NamespaceURI == targetNamespaceUri)
                        {
                            // create a new attribute without prefix
                            XmlAttribute newAttr = doc.CreateAttribute(attr.LocalName);
                            newAttr.Value = attr.Value;
                            element.Attributes.Append(newAttr);
                            // Remove the prefix and namespace URI from the attribute
                            element.Attributes.Remove(attr);
                        }
                        if (attr.Name == "xmlns" && (attr.Value == targetNamespaceUri || attr.Value == string.Empty))
                        {
                            // Remove the xmlns attribute if it is not needed
                            element.Attributes.Remove(attr);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Dictionary of allowed attributes for specific tags.
        /// </summary>
        private static readonly Dictionary<string, string[]> AllowAttributes = new()
        {
            { "dc:identifier", [ "id" ] },
            { "dc:title", [ "dir", "id", "xml:lang" ] },
            { "dc:language", [ "id" ] },
            { "dc:contributor", [ "dir", "id", "xml:lang" ] },
            { "dc:coverage", [ "dir", "id", "xml:lang" ] },
            { "dc:creator", [ "dir", "id", "xml:lang" ] },
            { "dc:date", [ "dir", "id", "xml:lang" ] },
            { "dc:description", [ "dir", "id", "xml:lang" ] },
            { "dc:format", [ "dir", "id", "xml:lang" ] },
            { "dc:publisher", [ "dir", "id", "xml:lang" ] },
            { "dc:relation", [ "dir", "id", "xml:lang" ] },
            { "dc:rights", [ "dir", "id", "xml:lang" ] },
            { "dc:source", [ "dir", "id", "xml:lang" ] },
            { "dc:subject", [ "dir", "id", "xml:lang" ] },
            { "dc:type", [ "dir", "id", "xml:lang" ] },
        };
        /// <summary>
        /// Check if the attribute is expected for the given tag name.
        /// </summary>
        /// <param name="tagName">tag name</param>
        /// <param name="attributeName">attribute name</param>
        /// <returns></returns>
        public static bool ExpectedAttribute(string tagName, string attributeName)
        {
            if (AllowAttributes.TryGetValue(tagName, out string[] allowedAttributes))
            {
                return allowedAttributes.Contains(attributeName);
            }
            return true;
        }

        /// <summary>
        /// A dictionary mapping Epub 2 attribute names to Epub 3 meta property names.
        /// Does not contain any attributes that are not changed for property name.
        /// </summary>
        private static readonly Dictionary<string, string> MetaPropertyMap = new()
        {
            { "scheme", "identifier-type" },

        };

        /// <summary>
        /// Get the property used in meta element for Epub 3 from the attribute name in Epub 2
        /// Why they change these names? Totally shit change. BTW I also perfer Epub 2 style attributes to be honest
        /// </summary>
        /// <param name="attributeName">attribute name in Epub 2</param>
        /// <returns></returns>
        public static string GetMetaPropertyFromAttribute(string attributeName)
        {
            if (MetaPropertyMap.TryGetValue(attributeName, out string value))
            {
                return value;
            }
            return attributeName;
        }

        /// <summary>
        /// Provides a set of HTML tag names that are considered inline elements according to HTML specifications.
        /// </summary>
        private static readonly HashSet<string> InlineElements =
        [
            "a", "abbr", "b", "bdi", "bdo", "br", "button", "cite", "code", "data", "datalist", "dfn", "em", "i", "iframe",
            "img", "input", "kbd", "label", "link", "map", "mark", "meter", "output", "progress", "q", "s", "samp", "script",
            "select", "slot", "small", "span", "strong", "sub", "sup", "template", "textarea", "time", "u", "var", "video", "wbr"
        ];

        /// <summary>
        /// Determines whether the specified HTML tag name represents an inline element.
        /// </summary>
        /// <param name="tagName">The name of the HTML tag to evaluate. The comparison is case-insensitive.</param>
        /// <returns>true if the tag name corresponds to a recognized inline HTML element; otherwise, false.</returns>
        public static bool IsInline(string tagName)
        {
            return InlineElements.Contains(tagName.ToLowerInvariant());
        }

        /// <summary>
        /// Copy all attributes and child nodes from source XmlElement to target XmlElement, two elements must belong to the same XmlDocument.
        /// </summary>
        /// <param name="source">source element</param>
        /// <param name="target">target element</param>
        internal static void CopyTo(XmlElement source, XmlElement target)
        {
            foreach (XmlAttribute attr in source.Attributes)
            {
                target.SetAttribute(attr.LocalName, attr.NamespaceURI, attr.Value);
            }
            while (source.HasChildNodes)
            {
                target.AppendChild(source.FirstChild);
            }
        }

        /// <summary>
        /// Copy all attributes and child nodes from source XmlElement to target XmlElement, two elements can belong to different XmlDocument, but slower than CopyTo.
        /// </summary>
        /// <param name="source">source element</param>
        /// <param name="target">target element</param>
        internal static void CopyToAcross(XmlElement source, XmlElement target)
        {
            foreach (XmlAttribute attr in source.Attributes)
            {
                target.SetAttribute(attr.LocalName, attr.NamespaceURI, attr.Value);
            }
            foreach (XmlNode child in source.ChildNodes)
            {
                XmlNode importedChild = target.OwnerDocument.ImportNode(child, true);
                target.AppendChild(importedChild);
            }
        }

        /// <summary>
        /// Copy all attributes and child nodes from source XmlElement to target XmlElement with override empty namespace, two elements can belong to different XmlDocument, but slower than CopyTo.
        /// </summary>
        /// <param name="source">source element</param>
        /// <param name="target">target element</param>
        /// <param name="namespaceUri">target namespace</param>
        internal static void CopyToAcrossOverrideEmptyNamespace(XmlElement source, XmlElement target, string namespaceUri)
        {
            foreach (XmlAttribute attr in source.Attributes)
            {
                if (target.NamespaceURI == namespaceUri && attr.NamespaceURI == string.Empty)
                {
                    target.SetAttribute(attr.LocalName, attr.Value);
                } else
                {
                    target.SetAttribute(attr.LocalName, attr.NamespaceURI == string.Empty ? namespaceUri : attr.NamespaceURI, attr.Value);
                }
            }
            foreach (XmlNode child in source.ChildNodes)
            {
                if (child is XmlElement childElement && childElement.NamespaceURI == string.Empty)
                {
                    XmlElement newChildElement = target.OwnerDocument.CreateElement(childElement.LocalName, namespaceUri);
                    CopyToAcrossOverrideEmptyNamespace(childElement, newChildElement, namespaceUri);
                    target.AppendChild(newChildElement);
                }
                else
                {
                    XmlNode importedChild = target.OwnerDocument.ImportNode(child, true);
                    target.AppendChild(importedChild);
                }
            }
        }
    }
}
