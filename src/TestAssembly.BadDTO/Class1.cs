using System.Runtime.Serialization;

namespace TestAssembly.BadDTO
{
  
    
    [DataContract]
    public class ClassWithNoDocs
    {
        /// <summary>
        /// A string property
        /// </summary>
        public string AProperty { get; set; }
    }


    /// <summary>
    /// </summary>
    [DataContract]
    public class ClassWithNoJschema
    {
        /// <summary>
        /// A string property
        /// </summary>
        public string AProperty { get; set; }
    }


    /// <summary>
    /// </summary>
    /// <jschema />
    [DataContract]
    public class ClassWithMissingPropertyJschema
    {
        /// <summary>
        /// A string property
        /// </summary>
        public string AProperty { get; set; }
    }


    /// <summary>
    /// </summary>
    /// <jschema />
    [DataContract]
    public class ClassWithPublicField
    {
        /// <summary>
        /// A string field - not allowed
        /// </summary>
        /// <jschema></jschema>
        public string AProperty;
    }
}
