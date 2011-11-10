using System;

namespace MetadataGeneration.Core
{
    /// <summary>
    /// http://davidlaing.com/2011/02/18/a-defect-exception/
    /// </summary>
    public class DefectException : Exception
    {
        public DefectException()
        {

        }
        public DefectException(string message)
            : base(message)
        {
            
        }
        public DefectException(string message, Exception innerException)
            : base(message, innerException)
        {
            
        }
        protected DefectException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
            : base(info, context)
        {

        }

    }
}