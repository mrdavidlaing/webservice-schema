using System.Xml.Linq;

namespace MetadataGeneration.Core.WcfSMD
{
    public class SmdXmlComment : XmlCommentBase
    {
        public bool Exclude { get; set; }
        public string MethodName { get; set; }
        public string Endpoint { get; set; }
        public static SmdXmlComment CreateFromXml(XElement methodSmdElement)
        {
            var smdXmlComment = new SmdXmlComment
                                    {
                                        MethodName = GetAttributeValue(methodSmdElement, "method", ""),
                                        Endpoint = GetAttributeValue(methodSmdElement, "endpoint", ""),
                                        Exclude = bool.Parse(GetAttributeValue(methodSmdElement, "exclude", "false"))
                                    };

            return smdXmlComment;
        }
    }
}