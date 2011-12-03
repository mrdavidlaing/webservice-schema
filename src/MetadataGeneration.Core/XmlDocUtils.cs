using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;

namespace MetadataGeneration.Core
{
    public static class XmlDocUtils
    {
        public static XElement GetXmlDocTypeNodeWithJSchema(this Type type)
        {
            var typeNode = type.GetXmlDocTypeNode();

            if (typeNode != null)
            {
                if (typeNode.XPathSelectElement("jschema") == null)
                {
                    typeNode = null;
                }
            }

            return typeNode;
        }
        public static XElement GetXmlDocTypeNodeWithSMD(this Type type)
        {
            XDocument doc = type.GetXmlDocs();
            var node = doc.XPathSelectElement("/doc/members/member[@name = 'T:" + type.FullName + "']");
            if (node != null)
            {
                if (node.XPathSelectElement("smd") == null)
                {
                    node = null;
                }
            }

            return node;
        }
        public static XElement GetXmlDocMemberNodeWithSMD(this Type type, string name)
        {
            XElement node2 = null;
            XDocument doc = type.GetXmlDocs();

            if (doc != null)
            {

                node2 = doc.XPathSelectElement("/doc/members/member[@name='M:" + name + "']") ?? doc.XPathSelectElement("/doc/members/member[starts-with(@name,'M:" + name + "(')]");

                if (node2 != null)
                {
                    if (node2.XPathSelectElement("smd") == null)
                    {
                        node2 = null;
                    }
                }
            }
            return node2;
        }
        public static XElement GetXmlDocNode(this Type type, string typeName, string memberName)
        {
            XDocument doc = type.GetXmlDocs();
            var node = doc.XPathSelectElement("/doc/members/member[@name = '" + typeName + ":" + memberName + "']");
            
            return node;
        }

        public static XElement GetXmlDocTypeNode(this Type type)
        {
            XDocument doc = type.GetXmlDocs();
            var node = doc.XPathSelectElement("/doc/members/member[@name = 'T:" + type.FullName + "']");
            return node;

            
        }


        public static XElement GetXmlDocMemberNode(this Type type, string name)
        {
            return type.GetXmlDocNode("M", type.FullName + "." + name);
        }

        public static XElement GetXmlDocFieldNode(this Type type, string name)
        {
            return type.GetXmlDocNode("F", type.FullName + "." + name);
        }


        public static XElement GetXmlDocPropertyNode(this Type type, string name)
        {
            var t = type;
            XElement element = t.GetXmlDocNode("P", t.FullName + "." + name);

            // properties are documented on the declaring type, so if not overridden
            // then we need to drill down to the base type
            while (element == null && t.BaseType != null && !t.BaseType.FullName.StartsWith("System"))
            {
                t = t.BaseType;
                element = t.GetXmlDocNode("P", t.FullName + "." + name);
            }
            return element;
        }
        public static void EnsureXmlDocsAreValid(XmlDocSource xmlDocSource)
        {
            foreach (var assembly in xmlDocSource.Dtos.Select(a => a.Assembly))
            {
                foreach (Type type in assembly.GetTypes())
                {
                    var doc = type.GetXmlDocs();
                    doc.EnsureXmlDocsValid();
                }
            }



        }

        /// <summary>
        /// Loads the xml document from referenced type build output
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static XDocument GetXmlDocs(this Type type)
        {
            var fileName = Path.GetFileNameWithoutExtension(type.Assembly.CodeBase) + ".xml";
            var filePath = Path.Combine(Path.GetDirectoryName(type.Assembly.CodeBase), fileName);

            var doc = XDocument.Load(filePath);
            return doc;
        }

        public static void EnsureXmlDocsValid(this XDocument doc)
        {
            var badCommentRX = new Regex("<!-- Badly formed XML comment ignored for member (?<bad>\".*\") -->", RegexOptions.ExplicitCapture);
            var matches = badCommentRX.Matches(doc.ToString());
            if (matches.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (Match m in matches)
                {
                    sb.Append(m.Groups["bad"].Value + ", ");
                }
                throw new Exception("Badly formed XML comments for " + sb);
            }
        }
    }
}