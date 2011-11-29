using System;
using System.Collections.Generic;
using System.Linq;

namespace MetadataGeneration.Core.JsonSchemaDTO
{
 
    public class MetadataValidationException : Exception
    {
        public MetadataValidationException(Type type, string method,string message, string suggestedSolution)
            : base(message)
        {
            SuggestedSolution = suggestedSolution;
            AggregatedExceptions = new List<MetadataValidationException>();
            Method = method;
            Type = type;

        }

        public MetadataValidationException(Type type, string method, string message, string suggestedSolution, Exception innerException)
            : base(message, innerException)
        {
            SuggestedSolution = suggestedSolution;
            AggregatedExceptions = new List<MetadataValidationException>();
            Method = method;
            Type = type;
        }

        public Type Type { get; set; }
        public string Method { get; set; }
        public string SuggestedSolution { get; private set; }
        

        public List<MetadataValidationException> AggregatedExceptions { get; private set; }


        public override string ToString()
        {
            string aggregated = string.Join(Environment.NewLine, AggregatedExceptions.Select(e => "\t" + e.ToString()));
            string message = Type.FullName + "." + Method + " : " + Message + Environment.NewLine + aggregated;
            return message;
        }
    }
}