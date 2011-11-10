using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization.Formatters;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Schema;

namespace MetadataGeneration.Core
{
    /// <summary>
    /// Everything in this file was pulled from JSON.NET with modifications specific to our needs. 
    /// Distribute appropriately.
    /// </summary>
    internal static class MiscellaneousUtils
    {


        public static JsonSchemaType GetJsonSchemaType(Type type, Required valueRequired)
        {
            JsonSchemaType schemaType = JsonSchemaType.None;
            if (valueRequired != Required.Always && ReflectionUtils.IsNullable(type))
            {
                schemaType = JsonSchemaType.Null;
                if (ReflectionUtils.IsNullableType(type))
                    type = Nullable.GetUnderlyingType(type);
            }

            TypeCode typeCode = Type.GetTypeCode(type);

            switch (typeCode)
            {
                case TypeCode.Empty:
                case TypeCode.Object:
                    return schemaType | JsonSchemaType.String;
                case TypeCode.DBNull:
                    return schemaType | JsonSchemaType.Null;
                case TypeCode.Boolean:
                    return schemaType | JsonSchemaType.Boolean;
                case TypeCode.Char:
                    return schemaType | JsonSchemaType.String;
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                    return schemaType | JsonSchemaType.Integer;
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    return schemaType | JsonSchemaType.Float;
                // convert to string?
                case TypeCode.DateTime:
                    return schemaType | JsonSchemaType.String;
                case TypeCode.String:
                    return schemaType | JsonSchemaType.String;
                default:
                    throw new Exception(string.Format("Unexpected type code '{0}' for type '{1}'.", typeCode, type));
            }
        }
        public static readonly IDictionary<JsonSchemaType, string> JsonSchemaReverseTypeMapping = new Dictionary<JsonSchemaType, string>()
    {
      { JsonSchemaType.String,"string"},
      { JsonSchemaType.Object,"object"},
      { JsonSchemaType.Integer,"integer"},
      { JsonSchemaType.Float,"number"},
      { JsonSchemaType.Null,"null"},
      { JsonSchemaType.Boolean,"boolean"},
      { JsonSchemaType.Array,"array"},
      { JsonSchemaType.Any,"any"}
    };
        public static readonly IDictionary<string, JsonSchemaType> JsonSchemaTypeMapping = new Dictionary<string, JsonSchemaType>()
    {
      {"string", JsonSchemaType.String},
      {"object", JsonSchemaType.Object},
      {"integer", JsonSchemaType.Integer},
      {"number", JsonSchemaType.Float},
      {"null", JsonSchemaType.Null},
      {"boolean", JsonSchemaType.Boolean},
      {"array", JsonSchemaType.Array},
      {"any", JsonSchemaType.Any}
    };
        public static bool TryAction<T>(Creator<T> creator, out T output)
        {


            try
            {
                output = creator();
                return true;
            }
            catch
            {
                output = default(T);
                return false;
            }
        }

        
    }


    internal static class ReflectionUtils
    {
        public static Type GetObjectType(object v)
        {
            return (v != null) ? v.GetType() : null;
        }

        public static string GetTypeName(Type t, FormatterAssemblyStyle assemblyFormat)
        {
            switch (assemblyFormat)
            {
                case FormatterAssemblyStyle.Simple:
                    return GetSimpleTypeName(t);
                case FormatterAssemblyStyle.Full:
                    return t.AssemblyQualifiedName;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string GetSimpleTypeName(Type type)
        {
#if !SILVERLIGHT
            string fullyQualifiedTypeName = type.FullName + ", " + type.Assembly.GetName().Name;

            // for type names with no nested type names then return
            if (!type.IsGenericType || type.IsGenericTypeDefinition)
                return fullyQualifiedTypeName;
#else
    // Assembly.GetName() is marked SecurityCritical
      string fullyQualifiedTypeName = type.AssemblyQualifiedName;
#endif

            StringBuilder builder = new StringBuilder();

            // loop through the type name and filter out qualified assembly details from nested type names
            bool writingAssemblyName = false;
            bool skippingAssemblyDetails = false;
            for (int i = 0; i < fullyQualifiedTypeName.Length; i++)
            {
                char current = fullyQualifiedTypeName[i];
                switch (current)
                {
                    case '[':
                        writingAssemblyName = false;
                        skippingAssemblyDetails = false;
                        builder.Append(current);
                        break;
                    case ']':
                        writingAssemblyName = false;
                        skippingAssemblyDetails = false;
                        builder.Append(current);
                        break;
                    case ',':
                        if (!writingAssemblyName)
                        {
                            writingAssemblyName = true;
                            builder.Append(current);
                        }
                        else
                        {
                            skippingAssemblyDetails = true;
                        }
                        break;
                    default:
                        if (!skippingAssemblyDetails)
                            builder.Append(current);
                        break;
                }
            }

            return builder.ToString();
        }

        public static bool IsInstantiatableType(Type t)
        {


            if (t.IsAbstract || t.IsInterface || t.IsArray || t.IsGenericTypeDefinition || t == typeof(void))
                return false;

            if (!HasDefaultConstructor(t))
                return false;

            return true;
        }

        public static bool HasDefaultConstructor(Type t)
        {
            return HasDefaultConstructor(t, false);
        }

        public static bool HasDefaultConstructor(Type t, bool nonPublic)
        {


            if (t.IsValueType)
                return true;

            return (GetDefaultConstructor(t, nonPublic) != null);
        }

        public static ConstructorInfo GetDefaultConstructor(Type t)
        {
            return GetDefaultConstructor(t, false);
        }

        public static ConstructorInfo GetDefaultConstructor(Type t, bool nonPublic)
        {
            BindingFlags accessModifier = BindingFlags.Public;

            if (nonPublic)
                accessModifier = accessModifier | BindingFlags.NonPublic;

            return t.GetConstructor(accessModifier | BindingFlags.Instance, null, new Type[0], null);
        }

        public static bool IsNullable(Type t)
        {


            if (t.IsValueType)
                return IsNullableType(t);

            return true;
        }

        public static bool IsNullableType(Type t)
        {


            return (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        //public static bool IsValueTypeUnitializedValue(ValueType value)
        //{
        //  if (value == null)
        //    return true;

        //  return value.Equals(CreateUnitializedValue(value.GetType()));
        //}

        public static bool IsUnitializedValue(object value)
        {
            if (value == null)
            {
                return true;
            }
            else
            {
                object unitializedValue = CreateUnitializedValue(value.GetType());
                return value.Equals(unitializedValue);
            }
        }

        public static object CreateUnitializedValue(Type type)
        {


            if (type.IsGenericTypeDefinition)
                throw new ArgumentException(string.Format("Type {0} is a generic type definition and cannot be instantiated.", type), "type");

            if (type.IsClass || type.IsInterface || type == typeof(void))
                return null;
            else if (type.IsValueType)
                return Activator.CreateInstance(type);
            else
                throw new ArgumentException(string.Format("Type {0} cannot be instantiated.", type), "type");
        }

        public static bool IsPropertyIndexed(PropertyInfo property)
        {


            return !CollectionUtils.IsNullOrEmpty<ParameterInfo>(property.GetIndexParameters());
        }

        public static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition)
        {
            Type implementingType;
            return ImplementsGenericDefinition(type, genericInterfaceDefinition, out implementingType);
        }

        public static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition, out Type implementingType)
        {



            if (!genericInterfaceDefinition.IsInterface || !genericInterfaceDefinition.IsGenericTypeDefinition)
                throw new ArgumentNullException(string.Format("'{0}' is not a generic interface definition.", genericInterfaceDefinition));

            if (type.IsInterface)
            {
                if (type.IsGenericType)
                {
                    Type interfaceDefinition = type.GetGenericTypeDefinition();

                    if (genericInterfaceDefinition == interfaceDefinition)
                    {
                        implementingType = type;
                        return true;
                    }
                }
            }

            foreach (Type i in type.GetInterfaces())
            {
                if (i.IsGenericType)
                {
                    Type interfaceDefinition = i.GetGenericTypeDefinition();

                    if (genericInterfaceDefinition == interfaceDefinition)
                    {
                        implementingType = i;
                        return true;
                    }
                }
            }

            implementingType = null;
            return false;
        }

        public static bool AssignableToTypeName(this Type type, string fullTypeName, out Type match)
        {
            Type current = type;

            while (current != null)
            {
                if (string.Equals(current.FullName, fullTypeName, StringComparison.Ordinal))
                {
                    match = current;
                    return true;
                }

                current = current.BaseType;
            }

            foreach (Type i in type.GetInterfaces())
            {
                if (string.Equals(i.Name, fullTypeName, StringComparison.Ordinal))
                {
                    match = type;
                    return true;
                }
            }

            match = null;
            return false;
        }

        public static bool AssignableToTypeName(this Type type, string fullTypeName)
        {
            Type match;
            return type.AssignableToTypeName(fullTypeName, out match);
        }

        public static bool InheritsGenericDefinition(Type type, Type genericClassDefinition)
        {
            Type implementingType;
            return InheritsGenericDefinition(type, genericClassDefinition, out implementingType);
        }

        public static bool InheritsGenericDefinition(Type type, Type genericClassDefinition, out Type implementingType)
        {



            if (!genericClassDefinition.IsClass || !genericClassDefinition.IsGenericTypeDefinition)
                throw new ArgumentNullException(string.Format("'{0}' is not a generic class definition.", genericClassDefinition));

            return InheritsGenericDefinitionInternal(type, genericClassDefinition, out implementingType);
        }

        private static bool InheritsGenericDefinitionInternal(Type currentType, Type genericClassDefinition, out Type implementingType)
        {
            if (currentType.IsGenericType)
            {
                Type currentGenericClassDefinition = currentType.GetGenericTypeDefinition();

                if (genericClassDefinition == currentGenericClassDefinition)
                {
                    implementingType = currentType;
                    return true;
                }
            }

            if (currentType.BaseType == null)
            {
                implementingType = null;
                return false;
            }

            return InheritsGenericDefinitionInternal(currentType.BaseType, genericClassDefinition, out implementingType);
        }

        /// <summary>
        /// Gets the type of the typed collection's items.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>The type of the typed collection's items.</returns>
        public static Type GetCollectionItemType(Type type)
        {

            Type genericListType;

            if (type.IsArray)
            {
                return type.GetElementType();
            }
            else if (ImplementsGenericDefinition(type, typeof(IEnumerable<>), out genericListType))
            {
                if (genericListType.IsGenericTypeDefinition)
                    throw new Exception(string.Format("Type {0} is not a collection.", type));

                return genericListType.GetGenericArguments()[0];
            }
            else if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                return null;
            }
            else
            {
                throw new Exception(string.Format("Type {0} is not a collection.", type));
            }
        }

        public static void GetDictionaryKeyValueTypes(Type dictionaryType, out Type keyType, out Type valueType)
        {


            Type genericDictionaryType;
            if (ImplementsGenericDefinition(dictionaryType, typeof(IDictionary<,>), out genericDictionaryType))
            {
                if (genericDictionaryType.IsGenericTypeDefinition)
                    throw new Exception(string.Format("Type {0} is not a dictionary.", dictionaryType));

                Type[] dictionaryGenericArguments = genericDictionaryType.GetGenericArguments();

                keyType = dictionaryGenericArguments[0];
                valueType = dictionaryGenericArguments[1];
                return;
            }
            else if (typeof(IDictionary).IsAssignableFrom(dictionaryType))
            {
                keyType = null;
                valueType = null;
                return;
            }
            else
            {
                throw new Exception(string.Format("Type {0} is not a dictionary.", dictionaryType));
            }
        }

        public static Type GetDictionaryValueType(Type dictionaryType)
        {
            Type keyType;
            Type valueType;
            GetDictionaryKeyValueTypes(dictionaryType, out keyType, out valueType);

            return valueType;
        }

        public static Type GetDictionaryKeyType(Type dictionaryType)
        {
            Type keyType;
            Type valueType;
            GetDictionaryKeyValueTypes(dictionaryType, out keyType, out valueType);

            return keyType;
        }

        /// <summary>
        /// Tests whether the list's items are their unitialized value.
        /// </summary>
        /// <param name="list">The list.</param>
        /// <returns>Whether the list's items are their unitialized value</returns>
        public static bool ItemsUnitializedValue<T>(IList<T> list)
        {


            Type elementType = GetCollectionItemType(list.GetType());

            if (elementType.IsValueType)
            {
                object unitializedValue = CreateUnitializedValue(elementType);

                for (int i = 0; i < list.Count; i++)
                {
                    if (!list[i].Equals(unitializedValue))
                        return false;
                }
            }
            else if (elementType.IsClass)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    object value = list[i];

                    if (value != null)
                        return false;
                }
            }
            else
            {
                throw new Exception(string.Format("Type {0} is neither a ValueType or a Class.", elementType));
            }

            return true;
        }

        /// <summary>
        /// Gets the member's underlying type.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>The underlying type of the member.</returns>
        public static Type GetMemberUnderlyingType(MemberInfo member)
        {


            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                default:
                    throw new ArgumentException("MemberInfo must be of type FieldInfo, PropertyInfo or EventInfo", "member");
            }
        }

        /// <summary>
        /// Determines whether the member is an indexed property.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <returns>
        /// 	<c>true</c> if the member is an indexed property; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsIndexedProperty(MemberInfo member)
        {


            PropertyInfo propertyInfo = member as PropertyInfo;

            if (propertyInfo != null)
                return IsIndexedProperty(propertyInfo);
            else
                return false;
        }

        /// <summary>
        /// Determines whether the property is an indexed property.
        /// </summary>
        /// <param name="property">The property.</param>
        /// <returns>
        /// 	<c>true</c> if the property is an indexed property; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsIndexedProperty(PropertyInfo property)
        {


            return (property.GetIndexParameters().Length > 0);
        }

        /// <summary>
        /// Gets the member's value on the object.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="target">The target object.</param>
        /// <returns>The member's value on the object.</returns>
        public static object GetMemberValue(MemberInfo member, object target)
        {



            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    return ((FieldInfo)member).GetValue(target);
                case MemberTypes.Property:
                    try
                    {
                        return ((PropertyInfo)member).GetValue(target, null);
                    }
                    catch (TargetParameterCountException e)
                    {
                        throw new ArgumentException(string.Format("MemberInfo '{0}' has index parameters", member.Name), e);
                    }
                default:
                    throw new ArgumentException(string.Format("MemberInfo '{0}' is not of type FieldInfo or PropertyInfo", CultureInfo.InvariantCulture, member.Name), "member");
            }
        }

        /// <summary>
        /// Sets the member's value on the target object.
        /// </summary>
        /// <param name="member">The member.</param>
        /// <param name="target">The target.</param>
        /// <param name="value">The value.</param>
        public static void SetMemberValue(MemberInfo member, object target, object value)
        {



            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    ((FieldInfo)member).SetValue(target, value);
                    break;
                case MemberTypes.Property:
                    ((PropertyInfo)member).SetValue(target, value, null);
                    break;
                default:
                    throw new ArgumentException(string.Format("MemberInfo '{0}' must be of type FieldInfo or PropertyInfo", member.Name), "member");
            }
        }

        /// <summary>
        /// Determines whether the specified MemberInfo can be read.
        /// </summary>
        /// <param name="member">The MemberInfo to determine whether can be read.</param>
        /// /// <param name="nonPublic">if set to <c>true</c> then allow the member to be gotten non-publicly.</param>
        /// <returns>
        /// 	<c>true</c> if the specified MemberInfo can be read; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanReadMemberValue(MemberInfo member, bool nonPublic)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    FieldInfo fieldInfo = (FieldInfo)member;

                    if (nonPublic)
                        return true;
                    else if (fieldInfo.IsPublic)
                        return true;
                    return false;
                case MemberTypes.Property:
                    PropertyInfo propertyInfo = (PropertyInfo)member;

                    if (!propertyInfo.CanRead)
                        return false;
                    if (nonPublic)
                        return true;
                    return (propertyInfo.GetGetMethod(nonPublic) != null);
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines whether the specified MemberInfo can be set.
        /// </summary>
        /// <param name="member">The MemberInfo to determine whether can be set.</param>
        /// <param name="nonPublic">if set to <c>true</c> then allow the member to be set non-publicly.</param>
        /// <returns>
        /// 	<c>true</c> if the specified MemberInfo can be set; otherwise, <c>false</c>.
        /// </returns>
        public static bool CanSetMemberValue(MemberInfo member, bool nonPublic)
        {
            switch (member.MemberType)
            {
                case MemberTypes.Field:
                    FieldInfo fieldInfo = (FieldInfo)member;

                    if (fieldInfo.IsInitOnly)
                        return false;
                    if (nonPublic)
                        return true;
                    else if (fieldInfo.IsPublic)
                        return true;
                    return false;
                case MemberTypes.Property:
                    PropertyInfo propertyInfo = (PropertyInfo)member;

                    if (!propertyInfo.CanWrite)
                        return false;
                    if (nonPublic)
                        return true;
                    return (propertyInfo.GetSetMethod(nonPublic) != null);
                default:
                    return false;
            }
        }



        

        public static T GetAttribute<T>(ICustomAttributeProvider attributeProvider) where T : Attribute
        {
            return GetAttribute<T>(attributeProvider, true);
        }

        public static T GetAttribute<T>(ICustomAttributeProvider attributeProvider, bool inherit) where T : Attribute
        {
            T[] attributes = GetAttributes<T>(attributeProvider, inherit);

            return CollectionUtils.GetSingleItem(attributes, true);
        }

        public static T[] GetAttributes<T>(ICustomAttributeProvider attributeProvider, bool inherit) where T : Attribute
        {


            // http://hyperthink.net/blog/getcustomattributes-gotcha/
            // ICustomAttributeProvider doesn't do inheritance

            if (attributeProvider is Assembly)
                return (T[])Attribute.GetCustomAttributes((Assembly)attributeProvider, typeof(T), inherit);

            if (attributeProvider is MemberInfo)
                return (T[])Attribute.GetCustomAttributes((MemberInfo)attributeProvider, typeof(T), inherit);

            if (attributeProvider is Module)
                return (T[])Attribute.GetCustomAttributes((Module)attributeProvider, typeof(T), inherit);

            if (attributeProvider is ParameterInfo)
                return (T[])Attribute.GetCustomAttributes((ParameterInfo)attributeProvider, typeof(T), inherit);

            return (T[])attributeProvider.GetCustomAttributes(typeof(T), inherit);
        }

        public static string GetNameAndAssessmblyName(Type t)
        {


            return t.FullName + ", " + t.Assembly.GetName().Name;
        }

        public static void SplitFullyQualifiedTypeName(string fullyQualifiedTypeName, out string typeName, out string assemblyName)
        {
            int? assemblyDelimiterIndex = GetAssemblyDelimiterIndex(fullyQualifiedTypeName);

            if (assemblyDelimiterIndex != null)
            {
                typeName = fullyQualifiedTypeName.Substring(0, assemblyDelimiterIndex.Value).Trim();
                assemblyName = fullyQualifiedTypeName.Substring(assemblyDelimiterIndex.Value + 1, fullyQualifiedTypeName.Length - assemblyDelimiterIndex.Value - 1).Trim();
            }
            else
            {
                typeName = fullyQualifiedTypeName;
                assemblyName = null;
            }

        }

        private static int? GetAssemblyDelimiterIndex(string fullyQualifiedTypeName)
        {
            // we need to get the first comma following all surrounded in brackets because of generic types
            // e.g. System.Collections.Generic.Dictionary`2[[System.String, mscorlib,Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089],[System.String, mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]], mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
            int scope = 0;
            for (int i = 0; i < fullyQualifiedTypeName.Length; i++)
            {
                char current = fullyQualifiedTypeName[i];
                switch (current)
                {
                    case '[':
                        scope++;
                        break;
                    case ']':
                        scope--;
                        break;
                    case ',':
                        if (scope == 0)
                            return i;
                        break;
                }
            }

            return null;
        }

    }

    internal delegate T Creator<T>();

    internal static class CollectionUtils
    {



        /// <summary>
        /// Determines whether the collection is null or empty.
        /// </summary>
        /// <param name="collection">The collection.</param>
        /// <returns>
        /// 	<c>true</c> if the collection is null or empty; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNullOrEmpty<T>(ICollection<T> collection)
        {
            if (collection != null)
            {
                return (collection.Count == 0);
            }
            return true;
        }



        #region GetSingleItem
        public static bool TryGetSingleItem<T>(IList<T> list, out T value)
        {
            return TryGetSingleItem<T>(list, false, out value);
        }

        public static bool TryGetSingleItem<T>(IList<T> list, bool returnDefaultIfEmpty, out T value)
        {
            return MiscellaneousUtils.TryAction<T>(delegate { return GetSingleItem(list, returnDefaultIfEmpty); }, out value);
        }

        public static T GetSingleItem<T>(IList<T> list)
        {
            return GetSingleItem<T>(list, false);
        }

        public static T GetSingleItem<T>(IList<T> list, bool returnDefaultIfEmpty)
        {
            if (list.Count == 1)
                return list[0];
            else if (returnDefaultIfEmpty && list.Count == 0)
                return default(T);
            else
                throw new Exception(string.Format("Expected single {0} in list but got {1}.", typeof(T), list.Count));
        }
        #endregion


    }
}