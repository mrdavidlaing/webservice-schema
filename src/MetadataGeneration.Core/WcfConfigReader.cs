using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Xml.XPath;

namespace MetadataGeneration.Core
{
    public class WcfConfigReader
    {
        public XmlDocSource Read(string configPath, string assemblySearchPath)
        {
            var xmlDocSource = new XmlDocSource();
            var config = XDocument.Load(configPath);
            var apiNode = config.XPathSelectElement("configuration/tradingApi");
            var profile = apiNode.XPathSelectElement("profiles").Descendants("profile").First();
            
            foreach(var dtoAssemblyName in profile.Descendants("dtoAssemblies").Descendants("add").Select(n => n.Attribute("assembly").Value).ToArray())
            {
                xmlDocSource.Dtos.Add(AssemblyWithXmlDocs.CreateFromName(dtoAssemblyName, assemblySearchPath));
            }

            var routeNodes = profile.XPathSelectElement("routes").XPathSelectElements("add").ToList();
            xmlDocSource.Routes = new List<RouteElement>();
            foreach (var item in routeNodes)
            {
                var map = new RouteElement()
                {
                    Endpoint = item.Attribute("endpoint").Value /*+ item.Attribute("pathInfo").Value*/,
                    Name = item.Attribute("name").Value,
                    Type = item.Attribute("type").Value
                };
                xmlDocSource.Routes.Add(map);
            }
            xmlDocSource.RouteAssembly =
                 AssemblyWithXmlDocs.CreateFromName(xmlDocSource.Routes[0].Type.Substring(xmlDocSource.Routes[0].Type.IndexOf(",") + 1).Trim(), assemblySearchPath);

            return xmlDocSource;
        }

       
    }
}
