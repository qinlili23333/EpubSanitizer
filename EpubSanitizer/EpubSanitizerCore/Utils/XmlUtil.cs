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
            foreach (XmlElement element in doc.GetElementsByTagName("*").Cast<XmlElement>().ToArray())
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
            foreach (XmlElement element in doc.GetElementsByTagName("*").Cast<XmlElement>().ToArray())
            {
                if (element.NamespaceURI == string.Empty)
                {
                    // Recreate with proper namespace URI
                    XmlElement newElement = doc.CreateElement(element.LocalName, targetNamespaceUri);
                    // Copy attributes and children
                    foreach (XmlAttribute attr in element.Attributes)
                    {
                        newElement.SetAttribute(attr.Name, attr.Value);
                    }
                    foreach (XmlNode child in element.ChildNodes)
                    {
                        XmlNode importedChild = doc.ImportNode(child, true);
                        newElement.AppendChild(importedChild);
                    }
                    // Replace the old element with the new one
                    element.ParentNode.ReplaceChild(newElement, element);
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
                    if (attr.Name == "xmlns" && attr.Value == targetNamespaceUri)
                    {
                        // Remove the xmlns attribute if it is not needed
                        element.Attributes.Remove(attr);
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
    }
}
