using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MetadataGeneration.Core.JsonSchemaDTO;
using Newtonsoft.Json.Linq;

namespace MetadataGeneration.Core.Streaming
{
    public class StreamingValidator
    {
        public void ValidateStreamingFragment(JObject ssmd, JObject schema)
        {
            var ex = new MetadataValidationException(typeof(object), "streaming","streaming validation failed","see AggregatedExceptions");


            IEnumerable<JProperty> services = ((JObject)ssmd["services"]).Properties();
            foreach (JProperty service in services)
            {
                JToken svc = service.Value;
                var returnValue = svc["returns"]["$ref"].Value<string>();
                if (returnValue.StartsWith("#."))
                {
                    returnValue = returnValue.Substring(2);
                }
                if (schema["properties"][returnValue] == null)
                {
                    var method = svc["target"].Value<string>()+"."+ svc["channel"].Value<string>();
                    ex.AggregatedExceptions.Add(new MetadataValidationException(typeof(Object), method, "Every service return type must be represented in the json-schema.", "ensure that the return type is represented in the json schema"));
                  
                }
            }

            if(ex.AggregatedExceptions.Count>0)
            {
                throw ex;
            }

        }
    }
}
