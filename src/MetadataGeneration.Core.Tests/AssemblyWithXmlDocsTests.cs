using NUnit.Framework;

namespace MetadataGeneration.Core.Tests
{
    [TestFixture]
    public class AssemblyWithXmlDocsTests
    {
        [Test]
        public void ShouldLoadAssembliesFromReferencedPath()
        {   
            var assembly = AssemblyWithXmlDocs.CreateFromName("TestAssembly.DTO, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", @"TestData\valid");

            Assert.That(assembly.Assembly.FullName, Is.StringStarting("TestAssembly"));
            Assert.That(assembly.AssemblyXML, Is.Not.Null);
        }
    }
}
