using System;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace MetadataGeneration.Core
{
    public class AssemblyWithXmlDocs
    {
        public Assembly Assembly { get; set; }
        public XDocument AssemblyXML { get; set; }
        private static string _extraAssemblySearchPath = "";

        public static AssemblyWithXmlDocs CreateFromName(string dtoAssemblyName, string extraAssemblySearchPath)
        {
            AddCustomAssemblyResolver(extraAssemblySearchPath);
            try
            {
                var assembly = Assembly.Load(dtoAssemblyName);
                var assemblyXml = LoadXml(assembly);
                return new AssemblyWithXmlDocs
                           {
                               Assembly = assembly, 
                               AssemblyXML = assemblyXml
                           };
            }
            finally
            {
                RemoveCustomAssemblyResolver();
            }
        }

        private static XDocument LoadXml(Assembly assembly)
        {
            var fileName = Path.GetFileNameWithoutExtension(assembly.CodeBase) + ".xml";
            var filePath = Path.Combine(Path.GetDirectoryName(assembly.CodeBase), fileName);

            var doc = XDocument.Load(filePath);
            return doc;
        }
        /// <summary>
        /// Ensure we also look for assemblies referenced in the Web.Config in the specified dtoAssemblyBasePath
        /// </summary>
        /// <param name="extraAssemblySearchPath"></param>
        private static void AddCustomAssemblyResolver(string extraAssemblySearchPath)
        {
            _extraAssemblySearchPath = Path.GetFullPath(extraAssemblySearchPath);
            AppDomain.CurrentDomain.AssemblyResolve += CustomAssemblyResolver;
        }

        private static Assembly CustomAssemblyResolver(object o, ResolveEventArgs args)
        {
            //System.Diagnostics.Debugger.Launch();
            var assemblyname = args.Name.Split(',')[0];
            var assemblyFilePath = Path.Combine(_extraAssemblySearchPath, assemblyname + ".dll");
            var assembly = Assembly.LoadFile(assemblyFilePath);
            return assembly;
        }

        public static void RemoveCustomAssemblyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve -= CustomAssemblyResolver;
        }
    }
}