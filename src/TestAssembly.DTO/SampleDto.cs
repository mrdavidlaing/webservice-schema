using System.Runtime.Serialization;

namespace TestAssembly.DTO
{
    /// <summary>
    /// 
    ///    This is a sample Dto
    /// 
    /// </summary>
    /// <jschema />
    [DataContract]
    public class SampleDto
    {
        /// <summary>
        /// 
        ///     The name of the Sample
        /// 
        /// </summary>
        /// <jschema />
        [DataMember]
        public string SampleName { get; set; }
    }
}
