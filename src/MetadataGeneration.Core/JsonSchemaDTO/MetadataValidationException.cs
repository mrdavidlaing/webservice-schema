using System;

namespace MetadataGeneration.Core.JsonSchemaDTO
{
    public class MetadataValidationException : Exception
    {
        public MetadataValidationException(string message, string suggestedSolution) : base(message)
        {
            SuggestedSolution = suggestedSolution;
        }

        public string SuggestedSolution { get; private set; }
    }
}