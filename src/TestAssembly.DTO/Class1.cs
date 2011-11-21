using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

// need an isolated assembly for testing

namespace MetadataProcessor.DTOTestAssembly
{
    /// <summary>
    /// 
    /// </summary>
    /// <jschema
    /// />
    [DataContract]
    public class Class1
    {
        ///<summary>
        ///</summary>
        /// <jschema/>
        public string StringProp { get; set; }

        ///<summary>
        ///</summary>
        /// <jschema/>
        public int? NullableIntProp { get; set; }

        ///<summary>
        ///</summary>
        /// <jschema/>
        public Class2 Class2Prop { get; set; }

        ///<summary>
        ///</summary>
        /// <jschema/>
        public DerivedClass2 DerivedClass2Prop { get; set; }

        ///<summary>
        ///</summary>
        /// <jschema/>
        public IList<DerivedClass2> IListDerivedClass2Prop { get; set; }
    }

    ///<summary>
    ///</summary>
    /// <jschema/>
    [DataContract]
    public class Class2
    {
        ///<summary>
        ///</summary>
        /// <jschema/>
        public EnumType EnumTypeProp { get; set; }
    }

    ///<summary>
    ///</summary>
    /// <jschema/>
    [DataContract]
    public class DerivedClass2:Class2
    {
        
    }

    /// <summary>
    /// 
    /// </summary>
    /// <jschema/>
    [DataContract]
    public enum EnumType
    {
        ///<summary>
        ///</summary>
        /// <jschema/>
        Field1
    }
}
