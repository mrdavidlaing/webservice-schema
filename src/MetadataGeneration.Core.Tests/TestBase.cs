using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace MetadataGeneration.Core.Tests
{
    public class TestBase
    {
        protected string _dtoAssemblyBasePath;
        protected WcfConfigReader _wcfConfigReader = new WcfConfigReader();
        protected Generator _generator = new Generator();


        protected static XmlDocSource CreateXmlDocSourceForTestAssembly(List<RouteElement> routes)
        {
            var xmlDocSource = new XmlDocSource();
            xmlDocSource.Dtos.Add(AssemblyWithXmlDocs.CreateFromName("TestAssembly.DTO, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "."));
            xmlDocSource.RouteAssembly = AssemblyWithXmlDocs.CreateFromName("TestAssembly.Service, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", ".");
            xmlDocSource.Routes = routes;
            return xmlDocSource;
        }
    }
}