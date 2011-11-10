using System;
using System.Linq;
using System.Xml.Linq;

namespace MetadataGeneration.Core.WcfSMD
{
    public class SmdXmlComment
    {
        public bool Excluded { get; set; }
        public string MethodName { get; set; }

        public static SmdXmlComment CreateFromXml(XElement methodSmdElement)
        {
            var smdXmlComment = new SmdXmlComment
                                    {
                                        MethodName = GetAttributeValue(methodSmdElement, "method", ""),
                                        Excluded = bool.Parse(GetAttributeValue(methodSmdElement, "excluded", "false"))
                                    };

            return smdXmlComment;
        }

        private static string GetAttributeValue(XElement el, string attributeName, string theDefault)
        {
            if (!el.HasAttributes) return theDefault;
            
            var attribute = el.Attributes(attributeName).FirstOrDefault();
            return attribute == null ? theDefault : attribute.Value;
        }
    }
}