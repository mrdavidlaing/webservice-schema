using System;
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

       
    }
}