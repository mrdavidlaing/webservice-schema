using System.Collections.Generic;
using System.Runtime.Serialization;

namespace MetadataProcessor.DTOTestAssembly
{
    /// <summary>
    /// All these types become arrays
    /// </summary>
    /// <jschema/>
    [DataContract]
    public class DTOClass
    {
        /// <summary>
        /// 
        /// </summary>
        /// <jschema/>
        public string Foo { get; set; }
    }
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


        ///<summary>
        ///</summary>
        /// <jschema/>
        public IEnumerable<DTOClass> IEnumerableOfClass { get; set; }

        ///<summary>
        ///</summary>
        /// <jschema/>
        public DTOClass[] ArrayOfClass { get; set; }

        ///<summary>
        ///</summary>
        /// <jschema/>
        public List<DTOClass> ListOfClass { get; set; }

        ///<summary>
        ///</summary>
        /// <jschema/>
        public IList<DTOClass> IListOfClass { get; set; }


        ///<summary>
        ///</summary>
        /// <jschema/>
        public IEnumerable<int?> IEnumerableOfNullableInt { get; set; }

        ///<summary>
        ///</summary>
        /// <jschema/>
        public int?[] ArrayOfNullableInt { get; set; }

        ///<summary>
        ///</summary>
        /// <jschema/>
        public List<int?> ListOfNullableInt { get; set; }

        ///<summary>
        ///</summary>
        /// <jschema/>
        public IList<int?> IListOfNullableInt { get; set; }
    }
}