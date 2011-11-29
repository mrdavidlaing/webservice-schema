using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MetadataGeneration.Core.JsonSchemaDTO
{
    public class JsonSchemaDtoEmitter
    {
        public JObject EmitDtoJson(XmlDocSource xmlDocSource)
        {
            var schemaObj = new JObject();
            var assemblies = xmlDocSource.Dtos.Select(a => a.Assembly).ToArray();

            var schemaProperties = new JObject();
            schemaObj["properties"] = schemaProperties;

            var types = UtilityExtensions.GetSchemaTypes(assemblies);
            var exception = new MetadataValidationException(typeof (object), "", "Errors generating meta for types", "");
            foreach (Type type in types)
            {
                try
                {

                    var typeNode = type.GetXmlDocTypeNodeWithJSchema();
                    var jschemaXml =
                        JschemaXmlComment.CreateFromXml(typeNode.XPathSelectElement("jschema"));

                    if (jschemaXml.Exclude)
                        continue; //Skip to next type

                    var typeObj = new JObject();
                    typeObj["id"] = type.Name;

                    if (type.IsEnum)
                    {
                        RenderEnum(type, typeObj);
                    }
                    else if (type.IsClass)
                    {
                        RenderType(type, typeObj);
                    }
                    else
                    {
                        throw new NotSupportedException(type.Name + " is not supported ");
                    }

                    ApplyDescription(typeObj, typeNode);

                    if (jschemaXml.DemoValue != null)
                    {
                        typeObj["demoValue"] = jschemaXml.DemoValue;
                    }

                    schemaProperties.Add(type.Name, typeObj);
                }
                catch (MetadataValidationException ex)
                {
                    exception.AggregatedExceptions.Add(ex);
                }

            }

            if (exception.AggregatedExceptions.Count>0)
            {
                throw exception;
            }
            return schemaObj;

        }

        public static void RenderEnum(Type type, JObject typeObj)
        {
            typeObj["type"] = "integer";

            var enumArray = new JArray();
            typeObj["enum"] = enumArray;
            var optionsArray = new JArray();
            typeObj["options"] = optionsArray;
            // OK we have a quandry - there are service endpoint implementations
            // that will accept the enum 'name' as a string rather than it's numeric value
            // so need to enable that capacity via a new proprietary jschema attribute
            // but by default, render the enum values as numeric

            var enumValues = Enum.GetValues(type);
            foreach (int enumValue in enumValues)
            {
                enumArray.Add(enumValue);
                var option = new JObject();
                option["value"] = enumValue;
                string fieldName = Enum.GetName(type, enumValue);
                option["label"] = fieldName;

                string description = "";

                var fieldNode = type.GetXmlDocFieldNode(fieldName);
                if (fieldNode != null)
                {
                    // TODO: may have to do some re-encoding
                    description = fieldNode.Value;
                }

                option["description"] = description;
                optionsArray.Add(option);
            }
        }
        private static void SetAttribute(JObject pobj, XElement jsNode, string attributeName)
        {
            var node = jsNode.Attribute(attributeName);
            if (node != null)
            {
                var nodeValue = node.Value;
                pobj[attributeName] = nodeValue;
            }
        }
        private static string MungeAttributeName(string name)
        {
            if (name == "minimum")
            {
                name = "minValue";
            }
            if (name == "maximum")
            {
                name = "maxValue";
            }
            return name;
        }
        public static void RenderType(Type type, JObject typeObj)
        {
            var typeException = new MetadataValidationException(type,"", "Errors occured generating type meta.","See aggregated exceptions.");
            string typeName = type.Name;

            typeObj["type"] = "object";

            if (type.BaseType != null && type.BaseType != typeof(object))
            {
                typeObj["extends"] = "#/" + type.BaseType.Name;
            }


            var properties = new JObject();
            typeObj["properties"] = properties;

            // TODO: should we render fields if they are jschema decorated?
            // seems like some fields are getting scattered about in the DTO classes

            foreach (var propertyInfo in type.GetProperties())
            {
                string memberName = propertyInfo.Name;
                var pnode = type.GetXmlDocPropertyNode(memberName);
                if (pnode != null)
                {
                    var pobj = new JObject();
                    properties[memberName] = pobj;
                    var jschemaXml = JschemaXmlComment.CreateFromXml(pnode.XPathSelectElement("jschema"));

                    if (jschemaXml != null)
                    {

                        if (!jschemaXml.Exclude)
                        {
                            RenderTypeMeta(pobj, propertyInfo.PropertyType);

                            foreach (var item in jschemaXml.Element.Attributes())
                            {
                                string name = item.Name.ToString();
                                string value = item.Value;
                                ApplyPropertyAttribute(pobj, value, typeName, name);
                            }
                            ApplyDescription(pobj, pnode);                        
                            
                        }
                    }
                    else
                    {
                        typeException.AggregatedExceptions.Add(new MetadataValidationException(type, memberName, "Member does not have <jschema> element. All DTO properties must have a jschema element", "All DTO properties must have <jschema> element. If you wish to exclude the property use the exclude='true' in jschema"));
                    }
                }
                else
                {
                    typeException.AggregatedExceptions.Add(new MetadataValidationException(type, memberName, "Member is not documented with XML docs. All DTO properties must be documented with XML docs", "Document method with xml documentation as well as jschema element"));
                }
            }

            // #TODO - throw if public fields are present
            foreach (var  field in type.GetFields())
            {
                typeException.AggregatedExceptions.Add(new MetadataValidationException(type, field.Name, "A DTO type must not implement public fields", "Change field to property by adding {get;set;}"));
                
            }
            
            if(typeException.AggregatedExceptions.Count>0)
            {
                throw typeException;
            }

        }
        

        public static void ApplyPropertyAttribute(JObject propBase, string attributeValue, string parentName, string name)
        {
            // quick munge to clean up xml docs
            name = MungeAttributeName(name);

            switch (name)
            {
                // handle type agnostic attributes first

                // boolean
                case "optional":
                case "required":
                case "additionalProperties":
                case "uniqueItems":
                case "minimumCanEqual":
                case "maximumCanEqual":
                    propBase[name] = Convert.ToBoolean(attributeValue);
                    break;
                // string
                case "pattern":
                case "title":
                case "description":
                case "format":
                case "contentEncoding":
                case "extends":
                    propBase[name] = attributeValue;
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
                        propBase[name] = Convert.ToDecimal(attributeValue);
                    }
                    else if (propTypeString == "integer")
                    {
                        propBase[name] = Convert.ToInt64(attributeValue);
                    }
                    else
                    {
                        throw new InvalidOperationException("invalid property type for attribute min/max\n" + string.Format("Type:{0}, Member:{1}, Attribute:{2}", parentName, name, name));
                    }
                    break;
                case "minItems":
                case "maxItems":
                case "maxLength":
                case "minLength":
                    propBase[name] = Convert.ToInt64(attributeValue);
                    break;
                case "default":
                    // TODO:
                    // ApplyTypedValue(propBase, attribute, false);
                    break;
                case "enum":
                    throw new NotImplementedException("enum is constructed by use of underlying type attributeName\n" + string.Format("Type:{0}, Member:{1}, Attribute:{2}", parentName, name, name));


                case "requires":
                case "divisibleBy":
                case "disallow":
                    throw new NotImplementedException(name + " is not supported\n" + string.Format("Type:{0}, Member:{1}, Attribute:{2}", parentName, name, name));
                default:
                    // ignore all of the rest
                    break;
            }
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
                throw new Exception(errorMessage, e);

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
                propBase["description"] = summary.Trim();
            }
        }

        /// <summary>
        /// CAVEATS:
        ///   Does not handle Arrays/Lists of Arrays/Lists
        /// </summary>
        /// <param name="jobj"></param>
        /// <param name="type"></param>
        private static void RenderTypeMeta(JObject jobj, Type type)
        {
            var isArray = false;

            if (type.IsArray)
            {
                isArray = true;
                type = type.GetElementType();
            }
            else if (type.IsArrayType())
            {
                isArray = true;
                type = type.GetGenericArguments()[0];
            }



            var isNullable = type.IsNullableType();

            if (isNullable)
            {
                type = type.GetGenericArguments()[0];
            }



            // at this point type variable should contain the underlying type


            JObject typeObj;

            typeObj = type.GetSchemaType();

            if (isNullable)
            {
                if (isArray)
                {
                    jobj["type"] = "array";
                    jobj["items"] = new JArray("null", typeObj["type"]);
                }
                else
                {
                    jobj["type"] = new JArray("null", typeObj["type"]); ;
                }
            }
            else
            {
                if (isArray)
                {
                    jobj["type"] = "array";
                    jobj["items"] = new JArray(typeObj["type"]);
                }
                else
                {
                    jobj["type"] = typeObj["type"];
                }
            }

            // type is the complicated aspect
            // get it out of the way so we can apply
            // the other constraints that apply to 
            // the underlying type 
            typeObj.Remove("type");

            foreach (var item in typeObj)
            {
                jobj[item.Key] = item.Value;
            }


        }

    }
}
