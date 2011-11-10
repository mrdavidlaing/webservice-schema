using System;
using MetadataGeneration.Core.JsonSchemaDTO;
using NUnit.Framework;

namespace MetadataGeneration.Core.Tests
{
    [TestFixture]
    public class AuditorTests : TestBase
    {
      [Test]
        public void AllArrayTypesShouldValidate()
        {
            var xmlDocSource = new XmlDocSource();
            xmlDocSource.Dtos.Add(AssemblyWithXmlDocs.CreateFromName("TestAssembly.DTO, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", @"TestData\valid"));

            var result = new Auditor().AuditTypes(xmlDocSource);

            result.MetadataGenerationErrors.ForEach(e => Console.WriteLine(e.ToString()));
            Assert.AreEqual(0, result.MetadataGenerationErrors.Count, "No errors should have been reported");
        }
    }
}
