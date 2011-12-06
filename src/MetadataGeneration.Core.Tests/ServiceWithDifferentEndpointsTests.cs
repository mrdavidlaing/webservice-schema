using System;
using System.Collections.Generic;
using System.Linq;
using MetadataGeneration.Core.JsonSchemaDTO;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MetadataGeneration.Core.Tests
{
    [TestFixture]
    public class ServiceWithDifferentEndpointsTests: TestBase 
    {
        private XmlDocSource _xmlDocSource;
        private JObject _jsonSchema;
        private MetadataGenerationResult _smdSchema;
        private const string ENDPOINT1 = "simpleservice/pluraltarget";
        private const string ENDPOINT2 = "simpleservice/singluartarget";

        /// <summary>
        /// Some services have methods with different endpoints.  A common case is when the 
        /// GetAll method has a plural endpoint and a GetDetail method has a singular endpoint.
        /// For example:
        ///    /watchlists  - Get all watchlists - endpoint is         /watchlists
        ///    /watchlist/save - save a single watchlist - endpoint is /watchlist
        /// </summary>
        [SetUp]
        public void Context()
        {
            _xmlDocSource = CreateXmlDocSourceForTestAssembly(
               new List<RouteElement>{
                new RouteElement {
                                    Endpoint = ENDPOINT1,
                                    Name = "GetAll",
                                    Type = "TestAssembly.Service.ISimpleService, TestAssembly.Service, Version=1.0.0.0"},
                new RouteElement {
                                    Endpoint = ENDPOINT2,
                                    Name = "GetDetail",
                                    Type = "TestAssembly.Service.ISimpleService, TestAssembly.Service, Version=1.0.0.0"},
            });

            _jsonSchema = new JsonSchemaDtoEmitter().EmitDtoJson(_xmlDocSource);
            _smdSchema = new WcfSMD.Emitter().EmitSmdJson(_xmlDocSource, true, _jsonSchema);
        }

        [Test, Ignore("WIP")]
        public void ServicesWithDifferentEndpointsShouldHaveDifferentTargets()
        {
            Console.WriteLine(_smdSchema.SMD);
            Assert.That(_smdSchema.SMD["services"]["rpc"]["services"]["GetAll"]["target"].ToString(), Is.EqualTo(ENDPOINT1));
            Assert.That(_smdSchema.SMD["services"]["rpc"]["services"]["GetDetail"]["target"].ToString(), Is.EqualTo(ENDPOINT2));
        }
    }
}
