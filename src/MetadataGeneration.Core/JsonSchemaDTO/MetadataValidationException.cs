using System;
using System.Collections.Generic;
using System.Linq;
namespace MetadataGeneration.Core.JsonSchemaDTO
{
    public class MethodMetadataValidationException : TypeMetadataValidationException
    {
        public string Method { get; set; }
        public MethodMetadataValidationException(string method, Type type, string message, string suggestedSolution)
            : base(type, message, suggestedSolution)
        {
            Method = method;
        }

        public MethodMetadataValidationException(string method, Type type, string message, string suggestedSolution, Exception innerException)
            : base(type, message, suggestedSolution, innerException)
        {
            Method = method;
        }

        public override string ToString()
        {
            return Type.FullName + "." + Method + " : " + Message + Environment.NewLine + string.Join(Environment.NewLine, AggregatedExceptions.Select(e => e.ToString())); 
        }
    }
    public class TypeMetadataValidationException : MetadataValidationException
    {
        public TypeMetadataValidationException(Type type, string message, string suggestedSolution)
            : base(message, suggestedSolution)
        {
            Type = type;
        }


        public TypeMetadataValidationException(Type type, string message, string suggestedSolution, Exception innerException)
            : base(message, suggestedSolution, innerException)
        {
            Type = type;
        }

        public Type Type { get; set; }

        public override string ToString()
        {
            return Type.FullName + " : " + Message + Environment.NewLine + string.Join(Environment.NewLine, AggregatedExceptions.Select(e => e.ToString() )); 
        }
    }
    public class MetadataValidationException : Exception
    {
        public MetadataValidationException(string message, string suggestedSolution)
            : base(message)
        {
            SuggestedSolution = suggestedSolution;
            AggregatedExceptions = new List<MetadataValidationException>();
        }
        public MetadataValidationException(string message, string suggestedSolution, Exception innerException)
            : base(message, innerException)
        {
            SuggestedSolution = suggestedSolution;
            AggregatedExceptions = new List<MetadataValidationException>();
        }
        public string SuggestedSolution { get; private set; }

        public List<MetadataValidationException> AggregatedExceptions { get; private set; }


        public override string ToString()
        {
            return Message + Environment.NewLine + string.Join(Environment.NewLine, AggregatedExceptions.Select(e => e.ToString())); 
        }
    }
}