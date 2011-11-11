using System.Linq;
using System.Xml.Linq;

namespace MetadataGeneration.Core
{
    public class XmlCommentBase
    {
        protected static string GetAttributeValue(XElement el, string attributeName, string theDefault)
        {
            if (!el.HasAttributes) return theDefault;
            
            var attribute = el.Attributes(attributeName).FirstOrDefault();
            return attribute == null ? theDefault : attribute.Value;
        }
    }
}