using System;
using System.Collections.Generic;
using System.Linq;
using MetadataGeneration.Core.JsonSchemaDTO;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MetadataGeneration.Core.Tests
{
    [TestFixture]
    public class ServiceOverridingTests : TestBase
    {
        private XmlDocSource _xmlDocSource;
        private JObject _jsonSchema;
        private MetadataGenerationResult _smdSchema;
        private const string ENDPOINT1 = "endpoint/from/config";

        [SetUp]
        public void Context()
        {
            _xmlDocSource = CreateXmlDocSourceForTestAssembly(
               new List<RouteElement>{
                  new RouteElement {
                                    Endpoint = ENDPOINT1,
                                    Name = "GetDetail",
                                    Type = "TestAssembly.Service.ISimpleService, TestAssembly.Service, Version=1.0.0.0"}
                  ,
            });

            _jsonSchema = new JsonSchemaDtoEmitter().EmitDtoJson(_xmlDocSource);
            _smdSchema = new WcfSMD.Emitter().EmitSmdJson(_xmlDocSource, true, _jsonSchema);
        }

        [Test]
        public void TargetIsScrapedFromTheRouteElement()
        {
            Console.WriteLine(_smdSchema.SMD);
            Assert.That(_smdSchema.SMD["services"]["rpc"]["services"]["GetDetail"]["target"].ToString(), Is.EqualTo(ENDPOINT1));
        }

        [Test]
        public void UriTemplateIsScrapedFromTheWebInvokeElement()
        {
            Console.WriteLine(_smdSchema.SMD);
            Assert.That(_smdSchema.SMD["services"]["rpc"]["services"]["GetDetail"]["uriTemplate"].ToString(), Is.EqualTo("/detail"));
        }

        [Test]
        public void TargetCanBeOverriddenBySmdEndpointAttribute()
        {
            Console.WriteLine(_smdSchema.SMD);
            Assert.That(_smdSchema.SMD["services"]["rpc"]["services"]["GetOverriddenExample"]["target"].ToString(), Is.Not.EqualTo(ENDPOINT1));
            Assert.That(_smdSchema.SMD["services"]["rpc"]["services"]["GetOverriddenExample"]["target"].ToString(), Is.EqualTo("/endpoint/from/smd"));
        }

        [Test]
        public void UriTemplateCanBeOverriddenBySmdUriTemplateAttribute()
        {
            Console.WriteLine(_smdSchema.SMD);
            Assert.That(_smdSchema.SMD["services"]["rpc"]["services"]["GetOverriddenExample"]["uriTemplate"].ToString(), Is.Not.EqualTo("/uriTemplate/{from}/WebInvoke"));
            Assert.That(_smdSchema.SMD["services"]["rpc"]["services"]["GetOverriddenExample"]["uriTemplate"].ToString(), Is.EqualTo("/uriTemplate/{from}/smd"));
        }
    }
}
