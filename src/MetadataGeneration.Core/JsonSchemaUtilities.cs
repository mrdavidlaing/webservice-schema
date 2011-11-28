using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MetadataGeneration.Core
{
    public static class JsonSchemaUtilities
    {
        public const string RootDelimiter = "#.";

        private const bool SwallowAttributeErrors = false;

        public static Type GetNullableTypeIfAny(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                type = type.GetGenericArguments()[0];
            }
            return type;
        }


        /// <summary>
        /// NOTE: arrays of arrays (or lists of lists etc) are not supported by this check
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsCollectionType(Type type)
        {
            return GetCollectionType(type) != null;
        }

        /// <summary>
        /// NOTE: arrays of arrays (or lists of lists etc) are not supported by this check
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static Type GetCollectionType(Type type)
        {
            // check all possible array, list and collection types
            Type result = null;
            if (type.IsArray)
            {
                throw new Exception("array types are not supported. Use IList");
                //return type.GetElementType();
            }

            if (!type.IsGenericType)
            {
                return null;
            }

            if (!type.Name.StartsWith("IList"))
            {
                throw new Exception(type.Name + " are not supported. Use IList");
            }
            return type.GetGenericArguments()[0];
        }

        public static JObject BuildEnumSchema(XDocument doc, Type targetType, bool includeDemoValue)
        {
            var result = BuildPropertyBase(Enum.GetUnderlyingType(targetType));
            var typeElement = GetMemberNode(doc, "T:" + targetType.FullName);

            if (includeDemoValue)
            {
                var jschemaElement = typeElement.Descendants("jschema").First();
                XAttribute demoValueAttribute = jschemaElement.Attribute("demoValue");


                if (demoValueAttribute == null)
                {
                    // FIXME: reinstate when complex type demov value strategy determined
                    //throw new Exception(string.Format("includeDemoValue is true but {0} has no demoValue attribute", targetType.FullName));
                }
                else
                {
                    result.Add("demoValue", Convert.ToInt32(demoValueAttribute.Value));
                }

            }

            ApplyDescription(result, typeElement);

            var values = new JArray();

            result.Add("enum", values);

            foreach (var value in Enum.GetValues(targetType))
            {
                values.Add(value);
            }



            var options = new JArray();
            foreach (var name in Enum.GetNames(targetType))
            {

                object value = Enum.Parse(targetType, name);


                JObject option = new JObject(new JProperty("value", value), new JProperty("label", name));
                var enumLabel = GetMemberNode(doc, "F:" + targetType.FullName + "." + name);
                if (enumLabel != null)
                {
                    var enumSummary = enumLabel.Descendants("summary").FirstOrDefault();
                    if (enumSummary != null)
                    {
                        // NOTE: this is additional to the schema 
                        option.Add(new JProperty("description", enumSummary.Value.Trim()));
                    }

                }
                options.Add(option);
            }



            result.Add("options", options);


            return result;
        }

        /// <summary>
        /// This method builds a property base, building array if necessary.
        /// If the type is not an intrinsic type, the full typename is applied. 
        /// Post-process to taste.
        /// </summary>
        /// <param name="targetType"></param>
        /// <returns></returns>
        public static JObject BuildPropertyBase(Type targetType)
        {
            var result = new JObject();
            var jsPropertyValue = result;

            // determine if targetType is a collection type 

            // string type name comparison is a hack but the alternative is a lot more code with no added value
            if (targetType.Name.EndsWith("[]") || targetType.Name.StartsWith("List")
                || targetType.Name.StartsWith("IList") || targetType.Name.StartsWith("ICollection")
                || targetType.Name.StartsWith("IEnumerable"))
            {

                if (targetType.IsGenericType)
                {
                    targetType = targetType.GetGenericArguments()[0];
                }
                else if (targetType.IsArray)
                {
                    targetType = targetType.GetElementType();
                }

                result.Add("type", "array");

                jsPropertyValue = new JObject();
                result.Add("items", jsPropertyValue);
            }

            // get schema type

            // need to determine if the type is a system type
            if (targetType.FullName.StartsWith("System"))
            {
                var schemaType = MiscellaneousUtils.GetJsonSchemaType(targetType, Required.Always);

                jsPropertyValue.Add("type", MiscellaneousUtils.JsonSchemaReverseTypeMapping[schemaType]);



            }
            else
            {
                jsPropertyValue.Add("$ref", JsonSchemaUtilities.RootDelimiter + targetType.Name);
            }


            return result;
        }

        public static XElement GetMemberNode(XDocument doc, string memberName)
        {
            return doc.XPathSelectElement("/doc/members/member[@name='" + memberName + "']");
        }

        public static IEnumerable<XElement> GetMemberNodes(XDocument doc, string memberNamePartial)
        {
            return doc.XPathSelectElements("/doc/members/member[starts-with(@name,'" + memberNamePartial + "')]");
        }
        public static IEnumerable<XElement> GetMemberNodesExplicit(XDocument doc, string memberName)
        {
            return doc.XPathSelectElements("/doc/members/member[@name='" + memberName + "']");
        }

        /// <summary>
        /// This will require a bit of special casing. 
        /// demoValue needs to be applied to the body of the property, including
        /// collection types. 
        /// 
        /// Will do string comparison on the type to allow for future handling
        /// of nullable union types
        /// </summary>
        public static void ApplyTypedValue(JObject propBase, XAttribute attribute, bool treatAsLiteral)
        {
            try
            {

                if (attribute == null)
                {
                    throw new ArgumentException("attribute is null");
                }

                JToken propTypeAttribute = propBase["type"];
                var propType = propTypeAttribute != null ? propTypeAttribute.Value<string>() : "string";
                var propertyName = attribute.Name.ToString();
                var propertyValue = attribute.Value;

                if (treatAsLiteral)
                {
                    var literal = (JToken)JsonConvert.DeserializeObject(propertyValue);
                    propBase.Add(propertyName, literal);
                }
                else
                {



                    if (propType.IndexOf("string") > -1)
                    {


                        propBase.Add(propertyName, propertyValue);


                    }
                    if (propType.IndexOf("integer") > -1)
                    {
                        propBase.Add(propertyName, Convert.ToInt64(propertyValue));

                    }
                    if (propType.IndexOf("number") > -1)
                    {
                        // this seems right in the context of demoing the API 
                        propBase.Add(propertyName, Convert.ToDecimal(propertyValue));
                    }
                    if (propType.IndexOf("boolean") > -1)
                    {
                        propBase.Add(propertyName, Convert.ToBoolean(propertyValue));
                    }

                    if (propType.IndexOf("object") > -1)
                    {
                        propBase.Add(propertyName, propertyValue);
                    }

                    // handles only simple types - arrays of complex types are handled when the reference is resolved
                    if (propType.IndexOf("array") > -1)
                    {

                        var elementType = propBase["items"]["type"].Value<string>();

                        // have to deconstruct the array and build it to emit
                        // the proper JSON



                        var elements = propertyValue.Trim('[', ']').Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        var arrayProp = new JArray();

                        foreach (string element in elements)
                        {

                            if (elementType.IndexOf("string") > -1)
                            {
                                // strip the quotes
                                arrayProp.Add(element.Trim('"', '\''));
                            }
                            if (elementType.IndexOf("integer") > -1)
                            {
                                arrayProp.Add(Convert.ToInt64(element));

                            }
                            if (elementType.IndexOf("number") > -1)
                            {
                                arrayProp.Add(Convert.ToDecimal(element));

                            }
                            if (elementType.IndexOf("boolean") > -1)
                            {
                                arrayProp.Add(Convert.ToBoolean(element));
                            }

                        }

                        propBase.Add(propertyName, arrayProp);

                    }
                    if (propType.IndexOf("any") > -1)
                    {
                        propBase.Add(propertyName, propertyValue);
                    }
                }

            }
            catch (Exception e)
            {
                string errorMessage = string.Format("An exception occured when applying attribute\n\n{1}\n\nto\n\n{0}", propBase, attribute);
                if (!SwallowAttributeErrors)
                {

                    throw new ApplicationException(errorMessage, e);
                }
                else
                {
                    Console.WriteLine(errorMessage);
                }

            }
        }

        public static void ApplyDescription(JObject propBase, XElement propElement)
        {
            var summaryElement = propElement.Descendants("summary").FirstOrDefault();
            if (summaryElement != null)
            {
                string summary = summaryElement.Value;
                summary = summary.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ").Replace("\t", " ");
                summary = Regex.Replace(summary, "\\s+", " ", RegexOptions.Singleline);
                propBase.Add("description", summary.Trim());
            }
        }

        public static void ApplyPropertyAttribute(JObject propBase, XAttribute attribute, string parentName, string name)
        {





            string attributeName = attribute.Name.ToString();
            switch (attributeName)
            {
                // handle type agnostic attributes first

                // boolean
                case "optional":
                case "required":
                case "additionalProperties":
                case "uniqueItems":
                case "minimumCanEqual":
                case "maximumCanEqual":
                    propBase.Add(attributeName, Convert.ToBoolean(attribute.Value));
                    break;
                // string
                case "pattern":
                case "title":
                case "description":
                case "format":
                case "contentEncoding":
                case "extends":
                    propBase.Add(attributeName, attribute.Value);
                    break;

                // numeric values
                case "minimum":
                case "maximum":

                    var propType = propBase["type"];
                    string propTypeString = null;
                    if (propType != null)
                    {
                        propTypeString = propType.Value<string>();
                    }

                    if (propTypeString == "array")
                    {
                        propTypeString = propBase["items"]["type"].Value<string>();

                        // if the type is not an object, make it an object so that we can add the extra properties
                    }


                    // will be either number or integer
                    if (propTypeString == "number")
                    {
                        propBase.Add(attributeName, Convert.ToDecimal(attribute.Value));
                    }
                    else if (propTypeString == "integer")
                    {
                        propBase.Add(attributeName, Convert.ToInt64(attribute.Value));
                    }
                    else
                    {
                        throw new InvalidOperationException("invalid property type for attribute min/max\n" + string.Format("Type:{0}, Member:{1}, Attribute:{2}", parentName, name, attributeName));
                    }
                    break;
                case "minItems":
                case "maxItems":
                case "maxLength":
                case "minLength":
                    propBase.Add(attributeName, Convert.ToInt64(attribute.Value));
                    break;
                case "default":
                    ApplyTypedValue(propBase, attribute, false);
                    break;
                case "enum":
                    throw new NotImplementedException("enum is constructed by use of underlying type attributeName\n" + string.Format("Type:{0}, Member:{1}, Attribute:{2}", parentName, name, attributeName));


                case "requires":
                case "divisibleBy":
                case "disallow":
                    throw new NotImplementedException(attributeName + " is not supported\n" + string.Format("Type:{0}, Member:{1}, Attribute:{2}", parentName, name, attributeName));
                default:
                    // ignore all of the rest
                    break;
            }
        }


        public static JObject BuildTypeSchema(Type type, XDocument doc, bool includeDemoValue)
        {

            var typeElement = GetMemberNode(doc, "T:" + type.FullName);
            if (typeElement != null)
            {
                var typeJschemaElement = typeElement.Descendants("jschema").FirstOrDefault();
                // TODO: flatten nesting
                if (typeJschemaElement != null)
                {

                    var outputAttribute = typeJschemaElement.Attributes("output").FirstOrDefault();
                    if (outputAttribute == null || outputAttribute.Value == "true")
                    {
                        JObject jsob;
                        if (type.IsEnum)
                        {
                            jsob = BuildEnumSchema(doc, type, includeDemoValue);
                        }
                        else
                        {
                            jsob = new JObject
                                       {
                                           {
                                               "id", type.Name
                                           }, 
                                           {
                                               "type", "object"
                                            }
                                       };

                            ApplyDescription(jsob, typeElement);


                            // check for inheritance
                            if (type.BaseType != null && type.BaseType.IsClass && type.BaseType != typeof(object))
                            {
                                // must be derived from one of our classes
                                var propObj = new JObject(new JProperty("$ref", JsonSchemaUtilities.RootDelimiter + type.BaseType.Name));
                                jsob.Add(new JProperty("extends", propObj));
                            }


                            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.DeclaredOnly | BindingFlags.Instance);
                            var schemaProperties = new JObject();
                            jsob.Add("properties", schemaProperties);

                            foreach (PropertyInfo property in properties)
                            {

                                XElement propElement = GetMemberNode(doc, "P:" + type.FullName + "." + property.Name);
                                if (propElement != null)
                                {
                                    XElement metaElement = propElement.Descendants("jschema").FirstOrDefault();
                                    if (metaElement != null)
                                    {
                                        BuildPropertySchema(propElement, metaElement, includeDemoValue, schemaProperties, property.Name, property.PropertyType, type.FullName);
                                    }

                                }


                            }

                        }



                        return jsob;
                    }


                }
            }
            return null;
        }

        public static JObject GetSchemaType(this Type type)
        {
            TypeCode typecode = Type.GetTypeCode(type);
            var typeObj = new JObject();
            switch (typecode)
            {
                case TypeCode.Boolean:
                    typeObj["type"] = "boolean";
                    break;
                case TypeCode.Byte:
                    typeObj["type"] = "integer";
                    typeObj["format"] = "byte";
                    typeObj["minValue"] = 0;
                    typeObj["maxValue"] = 255;
                    break;
                case TypeCode.Char:
                    // ? quandry - represent char as a string of length 1 or as an uint16?
                    // type: "string" maxLength: 1
                    typeObj["type"] = "string";
                    typeObj["minLength"] = 1;
                    typeObj["maxLength"] = 1;
                    break;
                case TypeCode.DateTime:
                    // type: "string" format: "wcf-date" <-- negotiable depending on service serialization 
                    typeObj["type"] = "string";
                    typeObj["format"] = "wcf-date";
                    break;
                case TypeCode.Decimal:
                    // type: "number" format: "decimal" minValue:-79228162514264337593543950335,maxValue:79228162514264337593543950335, divisibleBy:0.01 <-- defines precision of 2

                    typeObj["type"] = "number";
                    typeObj["format"] = "decimal";
                    typeObj["minValue"] = -79228162514264337593543950335m;
                    typeObj["maxValue"] = 79228162514264337593543950335m;
                    break;

                case TypeCode.Double:
                    // type: "number", minValue: -1.79769313486231e308 (one more than .net  else inifinty),maxValue: 1.79769313486231e308 (one less than .net else inifinty)
                    typeObj["type"] = "number";
                    typeObj["format"] = "decimal";
                    typeObj["minValue"] = double.MinValue + 1; // minvalue is -infinity in IE js vm
                    typeObj["maxValue"] = double.MaxValue - 1;// maxValue is infinity in IE js vm
                    break;
                    break;
                case TypeCode.Int16:
                    typeObj["type"] = "integer";
                    typeObj["format"] = "short";
                    typeObj["minValue"] = -32768;
                    typeObj["maxValue"] = 32767;
                    break;
                case TypeCode.Int32:
                    typeObj["type"] = "integer";
                    typeObj["minValue"] = -2147483648;
                    typeObj["maxValue"] = 2147483647;
                    break;
                case TypeCode.Int64:
                    typeObj["type"] = "integer";
                    typeObj["format"] = "long";
                    typeObj["minValue"] = -9223372036854775808;
                    typeObj["maxValue"] = 9223372036854775807;

                    break;
                case TypeCode.SByte:
                    typeObj["type"] = "integer";
                    typeObj["format"] = "sbyte";
                    typeObj["minValue"] = -128;
                    typeObj["maxValue"] = 127;
                    break;

                case TypeCode.Single:
                    typeObj["type"] = "number";
                    typeObj["format"] = "single";
                    typeObj["minValue"] = -3.402823e38;
                    typeObj["maxValue"] = 3.402823e38;
                    break;

                case TypeCode.String:
                    typeObj["type"] = "string";
                    break;
                case TypeCode.UInt16:
                    typeObj["type"] = "integer";
                    typeObj["format"] = "ushort";
                    typeObj["minValue"] = 0;
                    typeObj["maxValue"] = 65535;
                    break;
                case TypeCode.UInt32:
                    typeObj["type"] = "integer";
                    typeObj["format"] = "uint";
                    typeObj["minValue"] = 0;
                    typeObj["maxValue"] = 4294967295;
                    break;
                case TypeCode.UInt64:
                    typeObj["type"] = "integer";
                    typeObj["format"] = "ulong";
                    typeObj["minValue"] = 0;
                    typeObj["maxValue"] = 18446744073709551615;
                    break;

                case TypeCode.Object:
                    JObject obj = new JObject();
                    obj["$ref"] = RootDelimiter + type.Name;
                    typeObj["type"] = obj;
                    break;
                case TypeCode.DBNull:
                case TypeCode.Empty:
                    throw new NotSupportedException("unsupported type " + typecode);

                default:
                    throw new DefectException("typecode from outerspace");
            }

            return typeObj;
        }
        public static bool IsComplexType(Type propertyType)
        {
            return (
                // is a custom type
                (propertyType.IsClass || propertyType.IsEnum) && !propertyType.FullName.StartsWith("System") ||
                // is a list of custom types
                (propertyType.Name.StartsWith("IList") && !propertyType.GetGenericArguments()[0].FullName.StartsWith("System"))

            );
        }
        public static JObject BuildPropertySchema(XElement docElement, XElement metaElement, bool includeDemoValue, JObject metaContainer, string propertyName, Type propertyType, string parentName)
        {
            try
            {
                // no jschema, no process 
                if (propertyName == "ListJSchemaDTOProperty")
                {

                }



                var underlyingType = metaElement.Attributes("underlyingType").FirstOrDefault();

                if (underlyingType != null)
                {
                    propertyType = Type.GetType(underlyingType.Value, true, false);
                }


                var propBase = BuildPropertyBase(propertyType);


                metaContainer.Add(propertyName, propBase);

                ApplyDescription(propBase, docElement);

                if (includeDemoValue)
                {
                    var demoValueAttribute = metaElement.Attributes("demoValue").FirstOrDefault();

                    // if type is class and not System.* then the demoValue indicates embedded JSON. 
                    // typically this is the case when the shape of the type is problematic due to recursion
                    // or circular references



                    bool isComplexType = IsComplexType(propertyType);


                    // we do not force demo values on complex types. if one is not present, the js will try to compose one.
                    if (demoValueAttribute == null)
                    {
                        //if (!isComplexType)
                        //{
                        //    throw new Exception(
                        //        string.Format("includeDemoValue is true but {0}.{1} has no demoValue attribute", parentName,
                        //                      propertyName));
                        //}

                    }
                    else
                    {
                        ApplyTypedValue(propBase, demoValueAttribute, isComplexType);
                    }

                }

                JObject attributeTarget = propBase;

                if (propBase["type"] != null && propBase["type"].Value<string>() == "array")
                {
                    attributeTarget = propBase["items"].Value<JObject>();
                }

                foreach (var attribute in metaElement.Attributes())
                {
                    ApplyPropertyAttribute(attributeTarget, attribute, parentName, propertyName);
                }


                return propBase;
            }
            catch (Exception e)
            {
                throw new Exception(String.Format("Exception with\n\n{0}", docElement), e);
            }
        }





    }
}