using System;
using System.Xml.Linq;

namespace MetadataGeneration.Core.JsonSchemaDTO
{
    /// <summary>
    /// TODO: expand accessors to include all possible attributes and explanations
    /// </summary>
    public class JschemaXmlComment : XmlCommentBase
    {
        public bool Exclude { get; set; }
        public string DemoValue { get; set; }
        public XElement Element { get; set; }

        public static JschemaXmlComment CreateFromXml(XElement xmlElement)
        {
            if(xmlElement==null)
            {
                return null;
            }
            var smdXmlComment = new JschemaXmlComment
                                    {
                                        DemoValue = GetAttributeValue(xmlElement, "demoValue", null),
                                        Exclude = bool.Parse(GetAttributeValue(xmlElement, "exclude", "false")),
                                        Element = xmlElement
                                    };

            return smdXmlComment;
        }
    }
}