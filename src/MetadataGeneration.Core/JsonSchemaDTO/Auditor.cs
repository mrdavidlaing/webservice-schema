using System;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using System.Xml.XPath;

namespace MetadataGeneration.Core.JsonSchemaDTO
{
    /// <summary>
    /// Use auditor on input assemblies to ensure that
    /// the generation infrastructure fully recognizes and
    /// can represent the contained types/services
    /// </summary>
    public class Auditor
    {

        public void AuditType(Type type, bool failIfMetaMissing)
        {
            XDocument doc = type.GetXmlDocs();
            var typeNode = doc.XPathSelectElement("/doc/members/member[@name = 'T:" + type.FullName + "']");
            if (typeNode == null)
            {
                if (failIfMetaMissing)
                {
                    throw new MetadataValidationException("Missing xmldocs for " + type.FullName, "Ensure you have decorated every DTO property with the appropriate <jschema> XML comment - https://github.com/cityindex/RESTful-Webservice-Schema/wiki/Howto-write-XML-comments-for-JSchema");
                }

                return;
            }

            var jschemaNode = typeNode.XPathSelectElement("jschema");

            if (jschemaNode != null)
            {

                // get base type, if any and audit recursively before continuing here
                // if any of the base types are missing jschema then fail

                if (type.BaseType != typeof(object) && type.BaseType != typeof(Enum))
                {
                    AuditType(type.BaseType, true);
                }

            }
            else if (failIfMetaMissing)
            {
                throw new Exception("missing jschema for " + type.FullName);
            }

            foreach (PropertyInfo pinfo in type.GetProperties())
            {

                var propertyNode = doc.XPathSelectElement("/doc/members/member[@name = 'P:" + type.FullName + "." + pinfo.Name + "']");
                if (propertyNode == null)
                {
                    // emit warning?
                }
                else
                {
                    var propJschemaNode = propertyNode.XPathSelectElement("jschema");
                    if (propJschemaNode == null)
                    {
                        // emit warning?
                    }
                    else
                    {
                        // ok, type and prop have jschema attribute. 
                        // need to verify that we have the capability of emitting
                        // a representation of the property typeNode
                        VerifyCanEmitType(pinfo.PropertyType);
                    }
                }


            }

        }

        private static bool VerifyCanEmitType(Type propertyType)
        {
            // check for lists and arrays and just set the property type to the element type
            // because we know we can emit arrays
            if (propertyType.IsArray)
            {
                propertyType = propertyType.GetElementType();
            }
            else if (UtilityExtensions.IsArrayType(propertyType))
            {
                propertyType = propertyType.GetGenericArguments()[0];
            }
            
            // if it is nullable set the property type to the type argument because
            // we know we can emit nullable e.g. type: [null, "integer"]
            if (UtilityExtensions.IsNullableType(propertyType))
            {
                propertyType = propertyType.GetGenericArguments()[0];
            }

            var typecode = Type.GetTypeCode(propertyType);

            switch (typecode)
            {
                case TypeCode.Boolean:
                    // type: "boolean"
                    return true;
                case TypeCode.Byte:
                    // type: "integer" minValue:0,maxValue:255
                    return true;
                case TypeCode.Char:
                    // ? quandry - represent char as a string of length 1 or as an uint16?
                    // type: "string" maxLength: 1
                    return true;
                case TypeCode.DateTime:
                    // type: "string" format: "wcf-date" <-- negotiable depending on service serialization 
                    return true;
                    break;
                case TypeCode.Decimal:
                    // type: "number" format: "decimal" minValue:-79228162514264337593543950335,maxValue:79228162514264337593543950335, divisibleBy:0.01 <-- defines precision of 2
                    return true;

                case TypeCode.Double:
                    // type: "number", minValue: -1.79769313486231e308 (one more than .net  else inifinty),maxValue: 1.79769313486231e308 (one less than .net else inifinty)
                    return true;
                case TypeCode.Int16:
                    // type: "integer" minValue: -32768 ,maxValue: 32767
                    return true;
                case TypeCode.Int32:
                    // type: "integer" minValue:-2147483648,maxValue: 2147483647
                    return true;
                case TypeCode.Int64:
                    // type: "integer" minValue:-9223372036854775808 ,maxValue: 9223372036854775807
                    return true;
                case TypeCode.SByte:
                    // type: "integer" minValue:-128,maxValue:127
                    return true;

                case TypeCode.Single:
                    // type: "number", minValue: -3.402823e38,maxValue: 3.402823e38
                    return true;
                    break;
                case TypeCode.String:
                    // type: "string" 
                    return true;
                case TypeCode.UInt16:
                    // type: "integer" minValue:0 ,maxValue: 65535
                    return true;
                case TypeCode.UInt32:
                    // type: "integer" minValue:0 ,maxValue: 4294967295
                    return true;
                case TypeCode.UInt64:
                    // type: "integer" minValue:0 ,maxValue: 18446744073709551615
                    return true;

                case TypeCode.Object:

                    Type baseType = propertyType.BaseType;

                    if (baseType == null)
                    {
                        throw new MetadataValidationException(propertyType.Name + "on " + propertyType.Name + " has no accessible base type", "Ensure that the base type also has XML comments");
                    }
                    return true;

                case TypeCode.DBNull:
                case TypeCode.Empty:
                    throw new MetadataValidationException("unsupported type " + typecode, "Jschema metadata cannot be generated this type.  Please use another type");

                default:
                    throw new DefectException("typecode from outerspace");
            }

        }

        public MetadataValidationResult AuditTypes(XmlDocSource xmlDocSource)
        {
            var results = new MetadataValidationResult();
            foreach (var assembly in xmlDocSource.Dtos.Select(a => a.Assembly))
            {
                foreach (Type type in assembly.GetTypes())
                {
                    // we don't emit interfaces
                    if (type.IsInterface)
                    {
                        continue;
                    }

                    try
                    {
                        AuditType(type, false);
                        results.AddMetadataGenerationSuccess(new MetadataGenerationSuccess(MetadataType.JsonSchema, type));
                    }
                    catch (MetadataValidationException e)
                    {
                        results.AddMetadataGenerationError(new MetadataGenerationError(MetadataType.JsonSchema, type, e));
                    }
                }
            }
            return results;
        }
    }
}
