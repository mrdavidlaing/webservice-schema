using System.Runtime.Serialization;

namespace TestAssembly.DTO
{
    /// <summary>
    /// This DTO should be excluded
    /// </summary>
    /// <jschema exclude="true"/>
    [DataContract]
    public class ExcludedDto
    {
        public string Property1 { get; set; }
        public string Property2 { get; set; }
    }

  
}
