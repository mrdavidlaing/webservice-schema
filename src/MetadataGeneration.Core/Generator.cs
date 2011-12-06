using MetadataGeneration.Core.JsonSchemaDTO;
using MetadataGeneration.Core.WcfSMD;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MetadataGeneration.Core
{
    public class Generator
    {
        public MetadataGenerationResult GenerateJsonSchema(XmlDocSource xmlDocSource)
        {
            var results = new MetadataGenerationResult();
            

            //Checks that each DTO type can be documented
            // merge canemittype to emitter
            //results.AddValidationResults(new Auditor().AuditTypes(xmlDocSource));

            //Creates Jschema for all DTO types where it can find XML docs
            try
            {
                results.JsonSchema = new JsonSchemaDtoEmitter().EmitDtoJson(xmlDocSource);
            }
            catch (MetadataValidationException e)
            {
                results.MetadataGenerationErrors.Add(new MetadataGenerationError(MetadataType.JsonSchema, typeof(object), e));
            }

            try
            {

                
                //Checks that DTOs all have valid XML comments
                XmlDocUtils.EnsureXmlDocsAreValid(xmlDocSource);
            }
            catch (MetadataValidationException ex)
            {

                results.MetadataGenerationErrors.Add(new MetadataGenerationError(MetadataType.SMD, typeof(object), new MetadataValidationException(typeof(object), "", ex.Message, "", ex)));
            }

            return results;
        }

        public MetadataGenerationResult GenerateSmd(XmlDocSource xmlDocSource, JObject jsonSchema)
        {
            MetadataGenerationResult result = new Emitter().EmitSmdJson(xmlDocSource, true, jsonSchema);
            try
            {

                //Checks that services all have valid XML comments
                xmlDocSource.RouteAssembly.AssemblyXML.EnsureXmlDocsValid();
            }
            catch (MetadataValidationException ex)
            {

                result.MetadataGenerationErrors.Add(new MetadataGenerationError(MetadataType.SMD, typeof(object), new MetadataValidationException(typeof(object), "", ex.Message, "", ex)));
            }
            return result;
        }

        public void AddStreamingSMD(JObject smd, string streamingJsonPatch)
        {
            smd["services"]["streaming"] = (JObject)JsonConvert.DeserializeObject(streamingJsonPatch);
        }
    }
}
