using System;
using MetadataGeneration.Core.JsonSchemaDTO;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace MetadataGeneration.Core.Tests
{
    [TestFixture]
    public class JsonSchemaDtoEmitterTests : TestBase
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

                Assert.AreEqual("TestAssembly.BadDTO.ClassWithPublicField. : Errors occured generating type meta.\r\n\tTestAssembly.BadDTO.ClassWithPublicField.AProperty : A DTO type must not implement public fields\r\n", ex.ToString());
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
                Assert.AreEqual(1, ex.AggregatedExceptions.Count, "should have one exception");
                Assert.AreEqual("TestAssembly.BadDTO.ClassWithMissingPropertyJschema. : Errors occured generating type meta.\r\n\tTestAssembly.BadDTO.ClassWithMissingPropertyJschema.AProperty : Member does not have <jschema> element. All DTO properties must have a jschema element\r\n", ex.ToString());


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

            Assert.That(jsonSchema["properties"]["SampleDto"]["description"].ToString(), Is.EqualTo("This is a sample Dto"));
            Assert.That(jsonSchema["properties"]["SampleDto"]["properties"]["SampleName"]["description"].ToString(), Is.EqualTo("The name of the Sample"));
        }

        [Test]
        public void EnumDescriptionShouldHaveWritespaceTrimmedOff()
        {
            var jsonSchema = GenerateJsonSchemaForTestAssemblyDTO();

            Assert.That(jsonSchema["properties"]["SampleEnum"]["description"].ToString(), Is.EqualTo("This is a sample enum"));
            Assert.That(jsonSchema["properties"]["SampleEnum"]["options"][0]["description"].ToString(), Is.EqualTo("The first value in the enum"));
            Assert.That(jsonSchema["properties"]["SampleEnum"]["options"][1]["description"].ToString(), Is.EqualTo("The second value in the enum"));
            Assert.That(jsonSchema["properties"]["SampleEnum"]["options"][2]["description"].ToString(), Is.EqualTo("The third value in the enum"));
        }

        private JObject GenerateJsonSchemaForTestAssemblyDTO()
        {
            var xmlDocSource = new XmlDocSource();
            xmlDocSource.Dtos.Add(AssemblyWithXmlDocs.CreateFromName("TestAssembly.DTO, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", @"TestData\valid"));
            xmlDocSource.RouteAssembly = new AssemblyWithXmlDocs() { Version = "1.0.0.0" };
            return new JsonSchemaDtoEmitter().EmitDtoJson(xmlDocSource);
        }
    }
}