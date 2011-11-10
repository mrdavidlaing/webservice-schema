using System;
using MetadataGeneration.Core.JsonSchemaDTO;
using NUnit.Framework;

namespace MetadataGeneration.Core.Tests
{
    [TestFixture]
    public class JsonSchemaDtoEmitterTests: TestBase 
    {
        [Test]
        public void AllArrayTypesShouldBeDescribedAsArray()
        {
            var xmlDocSource = new XmlDocSource();
            xmlDocSource.Dtos.Add(AssemblyWithXmlDocs.CreateFromName("TestAssembly.DTO, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", @"TestData\valid"));

            var jsonSchema = new JsonSchemaDtoEmitter().EmitDtoJson(xmlDocSource);

            Console.WriteLine(jsonSchema["properties"]["ArrayTypes"]);
            Assert.That(jsonSchema["properties"]["ArrayTypes"]["properties"]["IEnumerableOfInt"]["type"].ToString(), Is.EqualTo("array"));
            Assert.That(jsonSchema["properties"]["ArrayTypes"]["properties"]["ArrayOfInt"]["type"].ToString(), Is.EqualTo("array"));
            Assert.That(jsonSchema["properties"]["ArrayTypes"]["properties"]["ListOfInt"]["type"].ToString(), Is.EqualTo("array"));
            Assert.That(jsonSchema["properties"]["ArrayTypes"]["properties"]["IListOfInt"]["type"].ToString(), Is.EqualTo("array"));
        }
    }
}