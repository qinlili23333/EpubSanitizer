using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace EpubSanitizerCore.Utils
{
    internal static class XmlUtil
    {
        internal static XmlElement? GetParent(XmlElement element)
        {
            XmlNode? parent = element.ParentNode;
            if(parent!=null && parent is XmlElement parentElement)
            {
                return parentElement;
            }
            return null;
        }
    }
}
