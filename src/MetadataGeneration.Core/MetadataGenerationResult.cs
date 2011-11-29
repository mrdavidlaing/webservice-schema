using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MetadataGeneration.Core
{
    public class MetadataGenerationResult
    {
        public MetadataGenerationResult()
        {
            JsonSchema = JObject.Parse("{\"properties\": {} }");
        }
        public bool HasErrors
        {
            get { return _metadataGenerationErrors.Count > 0; }
        }

        private List<MetadataGenerationError> _metadataGenerationErrors = new List<MetadataGenerationError>();
        public List<MetadataGenerationError> MetadataGenerationErrors
        {
            get { return _metadataGenerationErrors; }
            set { _metadataGenerationErrors = value; }
        }

        private List<MetadataGenerationSuccess> _metadataGenerationSuccesses = new List<MetadataGenerationSuccess>();
        public List<MetadataGenerationSuccess> MetadataGenerationSuccesses
        {
            get { return _metadataGenerationSuccesses; }
            set { _metadataGenerationSuccesses = value; }
        }

        public JObject JsonSchema { get; set; }
        public JObject SMD { get; set; }

        public void AddValidationResults(MetadataValidationResult metadataValidationResult)
        {
            _metadataGenerationErrors.AddRange(metadataValidationResult.MetadataGenerationErrors);
            _metadataGenerationSuccesses.AddRange(metadataValidationResult.MetadataGenerationSuccesses);
        }
        public override string ToString()
        {
            var str = string.Format("HasErrors={0}\n", HasErrors);
            if (HasErrors)
                str += string.Format("\n------------------\nErrors:\n{0}", string.Join(Environment.NewLine, MetadataGenerationErrors.Select(e => e.ToString())));
            str += string.Format("\n------------------\nSuccess:\n{0}", string.Join(Environment.NewLine, MetadataGenerationSuccesses.Select(e => e.ToString())));
            return str;
        }
    }
}