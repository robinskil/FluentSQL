using System;
using System.Collections.Generic;
using System.Reflection;
using FluentSQL.Info;

namespace FluentSQL.Mapping
{
    public sealed class TypeMapping
    {
        /// <summary>
        /// These are the columns mapped from the query to the object. These are all value types + string
        /// </summary>
        public List<(string columnName, PropertyInfo propertyInfo)> ColumnMappedProperties { get; } = new();
        /// <summary>
        /// 
        /// </summary>
        public PrimaryKey PrimaryKey { get; set; }
        public List<Include> IncludedProperties { get; }
        public List<IncludedBy> IncludedByOthers { get; }
        public Type MappedType { get; }
        public Queue<PropertyInfo> PropertiesForConstructorMappedType { get; } = new();

        public TypeMapping(Type mappedType)
        {
            MappedType = mappedType;
        }
    }

    public interface IForeignKeyCreationInfo
    {
        public Type ForeignKeyType { get;  }
        public Queue<PropertyInfo> PropertiesToCreateForeignKey { get; }
    }

    public class Include : IForeignKeyCreationInfo
    {
        public PropertyInfo IncludedProperty { get; }
        public TypeMapping IncludedTypeMapping { get; }
        public Type ForeignKeyType { get; }
        public Queue<PropertyInfo> PropertiesToCreateForeignKey { get; }
        
        public Include(PropertyInfo includedProperty, Type foreignKeyType, TypeMapping includedTypeMapping)
        {
            IncludedProperty = includedProperty;
            ForeignKeyType = foreignKeyType;
            IncludedTypeMapping = includedTypeMapping;
            PropertiesToCreateForeignKey = new();
        }
    }

    public class IncludedBy : IForeignKeyCreationInfo
    {
        public PropertyInfo IncludedByProperty { get; }
        public Type ForeignKeyType { get; }
        public Queue<PropertyInfo> PropertiesToCreateForeignKey { get; }
        public TypeMapping IncludedByTypeMapping { get; }
        
        public IncludedBy(Type foreignKeyType, TypeMapping includedByTypeMapping, PropertyInfo includedByProperty)
        {
            ForeignKeyType = foreignKeyType;
            IncludedByTypeMapping = includedByTypeMapping;
            IncludedByProperty = includedByProperty;
            PropertiesToCreateForeignKey = new();
        }

    }
}