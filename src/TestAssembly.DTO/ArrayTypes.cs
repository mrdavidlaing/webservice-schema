using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MetadataProcessor.DTOTestAssembly
{
    /// <summary>
    /// All these types become arrays
    /// </summary>
    /// <jschema/>
    [DataContract]
    public class ArrayTypes
    {
        ///<summary>
        ///</summary>
        /// <jschema/>
        public IEnumerable<int> IEnumerableOfInt { get; set; }

        ///<summary>
        ///</summary>
        /// <jschema/>
        public int[] ArrayOfInt { get; set; }

        ///<summary>
        ///</summary>
        /// <jschema/>
        public List<int> ListOfInt { get; set; }

        ///<summary>
        ///</summary>
        /// <jschema/>
        public IList<int> IListOfInt { get; set; }
    }
}