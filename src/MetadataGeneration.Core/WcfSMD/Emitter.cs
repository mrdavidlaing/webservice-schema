using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using Newtonsoft.Json.Linq;

namespace MetadataGeneration.Core.WcfSMD
{
    public class Emitter
    {
        

        public MetadataGenerationResult EmitSmdJson(XmlDocSource xmlDocSource, bool includeDemoValue, JObject schema)
        {
            var result = new MetadataGenerationResult();
            JObject smd = new JObject
                              {
                                  {"SMDVersion","2.6"},
                                  {"version","1"},
                                  {"description","CIAPI SMD"},
                                  {"services", new JObject()}
                              };

            var rpc = new JObject();
            smd["services"]["rpc"] = rpc;
            rpc["target"] = "";
            JObject rpcServices = new JObject();
            rpc["services"] = rpcServices;

            var seenTypes = new List<Type>(); // just to keep track of types so we don't map twice

            foreach (RouteElement route in xmlDocSource.Routes)
            {
                var serviceResult = BuildServiceMapping(xmlDocSource, route, seenTypes, rpcServices, includeDemoValue, schema);
                result.AddValidationResults(serviceResult);
            }

            result.SMD = smd;
            return result;
        }

        private MetadataValidationResult BuildServiceMapping(XmlDocSource xmlDocSource, RouteElement route, List<Type> seenTypes, JObject smdBase, bool includeDemoValue, JObject schema)
        {
            var result = new MetadataValidationResult();

            Type type = xmlDocSource.RouteAssembly.Assembly.GetType(route.Type.Substring(0, route.Type.IndexOf(",")));
            if (seenTypes.Contains(type))
            {
                return result;
            }

            seenTypes.Add(type);

            var typeElement = type.GetXmlDocTypeNodeWithSMD();

            if (typeElement == null)
            {
                return result;
            }

            var methodTarget = route.Endpoint.Trim('/');

            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var methodElement = type.GetXmlDocMemberNodeWithSMD(type.FullName + "." + method.Name);
                if (methodElement == null)
                {
                    continue;
                }

                // get smd xml, if present
                var methodSmdElement = methodElement.XPathSelectElement("smd");
                if (methodSmdElement == null)
                {
                    result.AddMetadataGenerationError(new MetadataGenerationError(MetadataType.SMD, type, "should not have gotten a method element without smd", "All services that have XML comments must have a <smd> tag.  See https://github.com/cityindex/RESTful-Webservice-Schema/wiki/Howto-write-XML-comments-for-SMD for details"));
                    continue; //advance to next service method
                }

                var smdXmlComment = SmdXmlComment.CreateFromXml(methodSmdElement);

                //Don't document methods that are marked exclude
                if (smdXmlComment.Exclude)
                    continue; //advance to next service method

                JObject service = null;
                var opContract = ReflectionUtils.GetAttribute<OperationContractAttribute>(method);

                if (opContract != null)
                {
                    var webGet = ReflectionUtils.GetAttribute<WebGetAttribute>(method);
                    var methodName = method.Name;
                    if (!string.IsNullOrEmpty(smdXmlComment.MethodName))
                        methodName = smdXmlComment.MethodName;

                    string methodTransport = null;
                    string methodEnvelope = null;
                    string methodUriTemplate = null;

                    if (webGet != null)
                    {
                        service = new JObject();
                        methodUriTemplate = FixUriTemplate(webGet.UriTemplate);
                        methodTransport = "GET";
                        methodEnvelope = "URL";
                    }
                    else
                    {
                        var webInvoke = ReflectionUtils.GetAttribute<WebInvokeAttribute>(method);
                        if (webInvoke != null)
                        {
                            service = new JObject();
                            methodUriTemplate = FixUriTemplate(webInvoke.UriTemplate);

                            switch (webInvoke.Method.ToUpper())
                            {
                                case "POST":
                                    methodTransport = "POST";
                                    methodEnvelope = "JSON";
                                    break;
                                case "GET":
                                    methodTransport = "GET";
                                    methodEnvelope = "URL";
                                    break;
                                default:
                                    result.AddMetadataGenerationError(new MetadataGenerationError(MetadataType.SMD, type,
                                        string.Format("The {0} service has transport method of type {1} that is not supported", methodName, webInvoke.Method), "Service transports like DELETE or PUT are poorly supported by client http clients, so you advised to only use GET or POST"));
                                    continue; //advance to next service method
                            }

                        }
                    }

                    if (service != null)
                    {
                        JsonSchemaUtilities.ApplyDescription(service, methodElement);
                        string methodTargetTrimEnd = methodTarget.TrimEnd('/');
                        
                        service.Add("target", methodTargetTrimEnd);

                        if (!string.IsNullOrWhiteSpace(methodUriTemplate))
                        {
                            service.Add("uriTemplate", methodUriTemplate);
                        }
                        service.Add("contentType", "application/json");// TODO: declare this in meta or get from WebGet/WebInvoke
                        service.Add("responseContentType", "application/json");// TODO: declare this in meta or get from WebGet/WebInvoke
                        service.Add("transport", methodTransport);

                        try
                        {
                            smdBase.Add(methodName, service);
                        }
                        catch (ArgumentException e)
                        {
                            result.AddMetadataGenerationError(new MetadataGenerationError(MetadataType.SMD, type, string.Format("A service with the method name {0} already exists", methodName), "Ensure that methods names are unique across services"));
                            return result;
                        }

                        // this is not accurate/valid SMD for GET but dojox.io.services is not, yet, a very good 
                        // implementation of the SMD spec, which is funny as they were both written by the same person.
                        service.Add("envelope", methodEnvelope);

                        // determine if return type is object or primitive
                        JObject returnType = null;
                        if (Type.GetTypeCode(method.ReturnType) == TypeCode.Object)
                        {
                            if (method.ReturnType.Name != "Void")
                            {
                                string methodReturnTypeName = method.ReturnType.Name;
                                if (schema["properties"][methodReturnTypeName] == null)
                                {
                                    result.AddMetadataGenerationError(new MetadataGenerationError(MetadataType.SMD, type, "Schema missing referenced return type " + methodReturnTypeName + " for method " + method.Name, "All types used by services must be decorated with the <jschema> tag.  See https://github.com/cityindex/RESTful-Webservice-Schema/wiki/Howto-write-XML-comments-for-JSchema"));
                                }
                                returnType = new JObject(new JProperty("$ref", JsonSchemaUtilities.RootDelimiter + methodReturnTypeName));
                            }
                            else
                            {
                                returnType = null;
                            }


                        }
                        else if (Type.GetTypeCode(method.ReturnType) == TypeCode.Empty)
                        {
                            returnType = null;

                        }
                        else
                        {
                            returnType = new JObject(new JProperty("type",  method.ReturnType.GetSchemaType()["type"].Value<string>()));
                        }
                        if (returnType != null)
                        {
                            service.Add("returns", returnType);
                        }

                        SetStringAttribute(methodSmdElement, service, "group");
                        SetIntAttribute(methodSmdElement, service, "cacheDuration");
                        SetStringAttribute(methodSmdElement, service, "throttleScope");

                        var paramResult = AddParameters(type, method, methodElement, service, includeDemoValue);
                        if (paramResult.HasErrors)
                            result.AddMetadataGenerationErrors(paramResult.MetadataGenerationErrors);
                    }

                }
                if (!result.HasErrors)
                    result.AddMetadataGenerationSuccess(new MetadataGenerationSuccess(MetadataType.SMD, type));
            }

            return result;

        }

        private static MetadataValidationResult AddParameters(Type type, MethodInfo method, XElement methodElement, JObject service, bool includeDemoValue)
        {
            var result = new MetadataValidationResult();
            var parameters = new JArray();
            service.Add("parameters", parameters);

            foreach (var parameter in method.GetParameters())
            {
                var metaElement =
                    methodElement.Descendants("param").Where(p => p.Attribute("name").Value == parameter.Name).
                        FirstOrDefault();
                if (metaElement == null)
                {
                    string message = string.Format("param element not found for {0}.{1} - {2}", type.Name, method.Name, parameter.Name);
                    result.AddMetadataGenerationError(new MetadataGenerationError(MetadataType.SMD, type, message, "Every service parameter must have an associated <param> tag in the XML comments. See https://github.com/cityindex/RESTful-Webservice-Schema/wiki/Howto-write-XML-comments-for-SMD"));
                }
                else
                {
                    BuildParameterSchema(methodElement, metaElement, includeDemoValue, parameters, parameter.Name,
                                         parameter.ParameterType, type.FullName);
                }

            }
            return result;
        }

        public static JObject BuildParameterSchema(XElement docElement, XElement metaElement, bool includeDemoValue, JArray metaContainer, string propertyName, Type propertyType, string parentName)
        {
            // no jschema, no process

            var underlyingType = metaElement.Attributes("underlyingType").FirstOrDefault();

            if (underlyingType != null)
            {
                propertyType = Type.GetType(underlyingType.Value, true, false);
            }


            JObject propBase = JsonSchemaUtilities.BuildPropertyBase(propertyType);
            propBase.Add("name", propertyName);

            metaContainer.Add(propBase);

            AddPropertyDescription(propBase, metaElement, docElement);

            if (includeDemoValue)
            {
                var demoValueAttribute = metaElement.Attributes("demoValue").FirstOrDefault();

                // if type is class and not System.* then the demoValue indicates embedded JSON. 
                // typically this is the case when the shape of the type is problematic due to recursion
                // or circular references

                bool isComplexType = JsonSchemaUtilities.IsComplexType(propertyType);


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
                    JsonSchemaUtilities.ApplyTypedValue(propBase, demoValueAttribute, isComplexType);
                }
            }


            JObject attributeTarget = propBase;
            if (propBase["type"] != null && propBase["type"].Value<string>() == "array")
            {
                attributeTarget = propBase["items"].Value<JObject>();
            }

            foreach (var attribute in metaElement.Attributes())
            {
                JsonSchemaUtilities.ApplyPropertyAttribute(attributeTarget, attribute, parentName, propertyName);
            }

            //if (propBase["required"] == null)
            //{
            //    propBase.Add("optional", false);
            //}
            return propBase;
        }

        private static void SetStringAttribute(XElement methodSmdElement, JObject service, string attributeName)
        {
            XAttribute throttleScopeAttribute = methodSmdElement.Attributes(attributeName).FirstOrDefault();
            if (throttleScopeAttribute != null)
            {
                service.Add(attributeName, throttleScopeAttribute.Value);
            }
        }

        private static void SetIntAttribute(XElement methodSmdElement, JObject service, string attributeName)
        {
            XAttribute throttleScopeAttribute = methodSmdElement.Attributes(attributeName).FirstOrDefault();
            if (throttleScopeAttribute != null)
            {
                service.Add(attributeName, Convert.ToInt64(throttleScopeAttribute.Value));
            }
        }
        private static string FixUriTemplate(string methodUriTemplate)
        {
            if (!string.IsNullOrWhiteSpace(methodUriTemplate))
            {
                methodUriTemplate = methodUriTemplate.Replace("/?", "?");

                //if (methodUriTemplate == "/")
                //{
                //    methodUriTemplate = null;
                //}
            }
            return methodUriTemplate;
        }

        internal static void AddPropertyDescription(JObject propBase, XElement metaElement, XElement docElement)
        {
            if (TryAddPropertyDescription(metaElement.Value, propBase)) return;

            var summary = docElement.Elements()
                    .Where(e => e.Name.LocalName.ToLower() == "summary")
                    .Select(e => e.Value).FirstOrDefault();
            TryAddPropertyDescription(summary, propBase);
        }

        private static bool TryAddPropertyDescription(string description, JObject propBase)
        {
            if (string.IsNullOrWhiteSpace(description)) return false;

            propBase.Add("description", description.Trim());
            return true;
        }
    }
}
