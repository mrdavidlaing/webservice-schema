using System;
using System.Xml.Linq;

namespace MetadataGeneration.Core.JsonSchemaDTO
{
    public class jschemaXmlComment : XmlCommentBase
    {
        public bool Exclude { get; set; }
        public string DemoValue { get; set; }

        public static jschemaXmlComment CreateFromXml(XElement xmlElement)
        {
            var smdXmlComment = new jschemaXmlComment
                                    {
                                        DemoValue = GetAttributeValue(xmlElement, "demoValue", null),
                                        Exclude = bool.Parse(GetAttributeValue(xmlElement, "exclude", "false"))
                                    };

            return smdXmlComment;
        }
    }
}