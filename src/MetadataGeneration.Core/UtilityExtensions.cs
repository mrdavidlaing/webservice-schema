using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using MetadataGeneration.Core.JsonSchemaDTO;

namespace MetadataGeneration.Core
{
    public static class UtilityExtensions
    {
        /// <summary>
        /// TODO: this validation should be in the Auditor, no?
        /// </summary>
        /// <param name="assemblies"></param>
        /// <returns></returns>
        public static IEnumerable<Type> GetSchemaTypes(params Assembly[] assemblies)
        {
            var schemaTypes = new List<Type>();
            var noXmlDocTypes = new List<Type>();
            var noJschemaTypes = new List<Type>();
            foreach (Assembly assembly in assemblies)
            {
                Type[] assemblyTypes = assembly.GetTypes();
                foreach (Type assemblyType in assemblyTypes)
                {
                    
                    // if it has DataContract, include it
                    var dataContract = assemblyType.GetCustomAttributes(typeof(DataContractAttribute), true).FirstOrDefault();
                    if (dataContract != null)
                    {
                        var xdoc = assemblyType.GetXmlDocTypeNode();
                        if (xdoc == null)
                        {
                            noXmlDocTypes.Add(assemblyType);
                        }
                        else
                        {
                            xdoc = assemblyType.GetXmlDocTypeNodeWithJSchema();
                            if (xdoc == null)
                            {
                                noJschemaTypes.Add(assemblyType);
                            }
                            else
                            {
                                schemaTypes.Add(assemblyType);
                            }
                        }
                    }
                    
                }


            }
            var errorMessage = "";
            if (noXmlDocTypes.Count > 0)
            {
                errorMessage = "Types have no XML Documentation: " + string.Join(",", noXmlDocTypes.Select(t => t.FullName).ToArray());
            }
            if (noJschemaTypes.Count > 0)
            {
                errorMessage = errorMessage + "\nTypes have no jschema element. Use exclude='true' if necessary: " + string.Join(",", noJschemaTypes.Select(t => t.FullName).ToArray());
            }
            if (!string.IsNullOrEmpty(errorMessage))
            {
                throw new MetadataValidationException(errorMessage, "add XmlDocs to types and include a jschema element. If type must be excluded from meta, use exclude='true'");
            }
            return schemaTypes;
        }

        public static bool IsNullableType(this Type type)
        {
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public static bool IsArrayType(this Type type)
        {
            return type.IsGenericType
                && (type.GetGenericTypeDefinition() == typeof(List<>)
                   || type.GetGenericTypeDefinition() == typeof(IList<>)
                   || type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                   );
        }
    }
}