using System;
using System.Collections.Generic;
using MetadataGeneration.Core.JsonSchemaDTO;
using NUnit.Framework;

namespace MetadataGeneration.Core.Tests
{
    [TestFixture]
    public class SMDEmitterTests: TestBase 
    {
        [Test]
        public void ExcludeAttributeOnMethodShouldOmitServiceMethod()
        {
            var xmlDocSource = new XmlDocSource();
            xmlDocSource.Dtos.Add(AssemblyWithXmlDocs.CreateFromName("TestAssembly.DTO, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "."));
            xmlDocSource.RouteAssembly = AssemblyWithXmlDocs.CreateFromName("TestAssembly.Service, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", ".");
            xmlDocSource.Routes = new List<RouteElement>{
                new RouteElement {
                    Endpoint = "/PartiallyExcludedService/included",
                    Name = "IncludedEndpoint",
                    Type = "TestAssembly.Service.IPartiallyExcludedService, TestAssembly.Service, Version=1.0.0.0"},
                new RouteElement {
                    Endpoint = "/PartiallyExcludedService/excluded",
                    Name = "ExcludedEndpoint",
                    Type = "TestAssembly.Service.IPartiallyExcludedService, TestAssembly.Service, Version=1.0.0.0"},
            };

            var jsonSchema = new JsonSchemaDtoEmitter().EmitDtoJson(xmlDocSource);
            var smdSchema = new WcfSMD.Emitter().EmitSmdJson(xmlDocSource,true,jsonSchema);

            Console.WriteLine(smdSchema.SMD);
            Assert.That(smdSchema.SMD["services"]["rpc"]["services"]["IncludedEndpoint"], Is.Not.Null, "SMD should contain definition for IncludedEndpoint");
            Assert.That(smdSchema.SMD["services"]["rpc"]["services"]["ExcludedEndpoint"], Is.Null, "SMD should not contain definition for ExcludedEndpoint");
        }

        [Test]
        public void CorrectlyReferencedServicesShouldBeIncludedInSMD()
        {
            var xmlDocSource = new XmlDocSource();
            xmlDocSource.Dtos.Add(AssemblyWithXmlDocs.CreateFromName("TestAssembly.DTO, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "."));
            xmlDocSource.RouteAssembly = AssemblyWithXmlDocs.CreateFromName("TestAssembly.Service, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", ".");
            xmlDocSource.Routes = new List<RouteElement>{ 
                new RouteElement {
                    Endpoint = "/session",
                    Name = "session",
                    Type = "TestAssembly.Service.ITestService, TestAssembly.Service, Version=1.0.0.0"},
                new RouteElement {
                    Endpoint = "/session",
                    Name = "session_logout",
                    Type = "TestAssembly.Service.ITestService, TestAssembly.Service, Version=1.0.0.0"}
            };

            var jsonSchema = new JsonSchemaDtoEmitter().EmitDtoJson(xmlDocSource);
            var smdSchema = new WcfSMD.Emitter().EmitSmdJson(xmlDocSource, true, jsonSchema);

            Console.WriteLine(smdSchema.SMD);
            Assert.That(smdSchema.SMD["services"]["rpc"]["services"]["CreateSession"], Is.Not.Null);
            Assert.That(smdSchema.SMD["services"]["rpc"]["services"]["DeleteSession"], Is.Not.Null);
        }

        [Test, Ignore("TODO")]
        public void WebInvokeMethodGetShouldBeIncludedAsGet()
        {
            var xmlDocSource = new XmlDocSource();
            xmlDocSource.Dtos.Add(AssemblyWithXmlDocs.CreateFromName("TestAssembly.DTO, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", "."));
            xmlDocSource.RouteAssembly = AssemblyWithXmlDocs.CreateFromName("TestAssembly.Service, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", ".");
            xmlDocSource.Routes = new List<RouteElement>{
                new RouteElement {
                    Endpoint = "/WebInvokeMethodGet",
                    Name = "WebInvokeMethodGet",
                    Type = "TestAssembly.Service.ITestService, TestAssembly.Service, Version=1.0.0.0"}
            };

            var jsonSchema = new JsonSchemaDtoEmitter().EmitDtoJson(xmlDocSource);
            var smdSchema = new WcfSMD.Emitter().EmitSmdJson(xmlDocSource, true, jsonSchema);

            Console.WriteLine(smdSchema.SMD);
            Assert.That(smdSchema.SMD["services"]["rpc"]["services"]["WebInvokeMethodGet"], Is.Not.Null, "SMD doesn't contain WebInvokeMethodGet");
        }
    }
}
