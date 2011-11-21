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
        public void APropertyWithPublicFieldShouldThrow()
        {
            try
            {
                var type = typeof(TestAssembly.BadDTO.ClassWithPublicField);
                JsonSchemaDtoEmitter.RenderType(type, new JObject());
            }
            catch (MetadataValidationException ex)
            {

                Assert.AreEqual("A DTO type must not implement public fields", ex.Message);
            }
        }
        [Test]
        public void APropertyMissingJschemaShouldThrow()
        {
            try
            {
                var type = typeof(TestAssembly.BadDTO.ClassWithMissingPropertyJschema);
                JsonSchemaDtoEmitter.RenderType(type, new JObject());
            }
            catch (MetadataValidationException ex)
            {

                Assert.AreEqual("TestAssembly.BadDTO.ClassWithMissingPropertyJschema.AProperty does not have <jschema> element. All DTO properties must have a jschema element", ex.Message);
            }
        }
        [Test]
        public void TypesMissingDocsAndJschemaShouldThrow()
        {
            
            try
            {
                UtilityExtensions.GetSchemaTypes(typeof(TestAssembly.BadDTO.ClassWithNoDocs).Assembly);    
            }
            catch (MetadataValidationException ex)
            {

                Assert.AreEqual("Types have no XML Documentation: TestAssembly.BadDTO.ClassWithNoDocs\nTypes have no jschema element. Use exclude='true' if necessary: TestAssembly.BadDTO.ClassWithNoJschema", ex.Message);
            }

            
            
        }
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