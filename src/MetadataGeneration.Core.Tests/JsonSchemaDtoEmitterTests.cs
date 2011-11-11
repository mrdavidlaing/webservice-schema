using System;
using MetadataGeneration.Core.JsonSchemaDTO;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MetadataGeneration.Core.Tests
{
    [TestFixture]
    public class JsonSchemaDtoEmitterTests: TestBase 
    {
        [Test]
        public void AllArrayTypesShouldBeDescribedAsArray()
        {
            var jsonSchema = GenerateJsonSchemaForTestAssemblyDTO();

            Console.WriteLine(jsonSchema["properties"]["ArrayTypes"]);
            Assert.That(jsonSchema["properties"]["ArrayTypes"]["properties"]["IEnumerableOfInt"]["type"].ToString(), Is.EqualTo("array"));
            Assert.That(jsonSchema["properties"]["ArrayTypes"]["properties"]["ArrayOfInt"]["type"].ToString(), Is.EqualTo("array"));
            Assert.That(jsonSchema["properties"]["ArrayTypes"]["properties"]["ListOfInt"]["type"].ToString(), Is.EqualTo("array"));
            Assert.That(jsonSchema["properties"]["ArrayTypes"]["properties"]["IListOfInt"]["type"].ToString(), Is.EqualTo("array"));
        }

        [Test]
        public void DtosMarkedWithExludeShouldBeOmitted()
        {
            var jsonSchema = GenerateJsonSchemaForTestAssemblyDTO();

            Assert.That(jsonSchema["properties"]["ExcludedDto"], Is.Null, "ExcludedDto should not appear in jschema");
        }

        private JObject GenerateJsonSchemaForTestAssemblyDTO()
        {
            var xmlDocSource = new XmlDocSource();
            xmlDocSource.Dtos.Add(AssemblyWithXmlDocs.CreateFromName("TestAssembly.DTO, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", @"TestData\valid"));

            return new JsonSchemaDtoEmitter().EmitDtoJson(xmlDocSource);
        }
    }
}