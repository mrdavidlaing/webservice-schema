using System;
using MetadataGeneration.Core.JsonSchemaDTO;

namespace MetadataGeneration.Core
{
    public enum MetadataType
    {
        JsonSchema,
        SMD
    } ;

    public class MetadataGenerationError
    {
        public MetadataGenerationError(MetadataType metadataType, Type type, string errorReason, string sugestionedSolution)
        {
            MetadataType = metadataType;
            Type = type;
            ErrorReason = errorReason;
            SuggestedSolution = sugestionedSolution;
        }

        public MetadataGenerationError(MetadataType metadataType, Type type, MetadataValidationException metadataValidationException):
            this(metadataType, type, metadataValidationException.Message, metadataValidationException.SuggestedSolution)
        {
            
        }

        public MetadataType MetadataType { get; private set; }
        public Type Type { get; private set; }
        public string ErrorReason { get; private set; }
        public string SuggestedSolution { get; private set; }
        public override string ToString()
        {
            return string.Format("MetadataGenerationError: MetadataType={0}, Type={1},\r\n\tErrorReason={2},\r\n\tSuggestedSolution={3}", MetadataType, Type, ErrorReason, SuggestedSolution);
        }
    }
}
