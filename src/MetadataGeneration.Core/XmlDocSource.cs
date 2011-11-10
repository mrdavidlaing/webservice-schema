using System.Collections.Generic;

namespace MetadataGeneration.Core
{
    public class XmlDocSource
    {
        public List<RouteElement> Routes;
        public AssemblyWithXmlDocs RouteAssembly { get; set; }
        public List<AssemblyWithXmlDocs> Dtos = new List<AssemblyWithXmlDocs>();
    }
}
