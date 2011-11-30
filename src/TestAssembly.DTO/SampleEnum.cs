using System.Runtime.Serialization;

namespace TestAssembly.DTO
{
    /// <summary>
    /// 
    ///    This is a sample enum
    /// 
    /// </summary>
    /// <jschema />
    [DataContract]
    public enum SampleEnum
    {
        /// <summary>
        /// 
        ///    The first value in the enum
        /// 
        /// </summary>
        [EnumMember]
        First = 1, 

        /// <summary>
        /// 
        /// The second value in the enum
        /// </summary>
        [EnumMember]
        Second = 2, 

        /// <summary>
        /// The third value in the enum
        /// </summary>
        [EnumMember]
        Third = 3
    }
}
