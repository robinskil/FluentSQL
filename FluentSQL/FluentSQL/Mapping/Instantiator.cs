using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using FluentSQL.Info;

namespace FluentSQL.Mapping
{
    public static class Instantiator
    {
        public static object InstantiatePrimaryKey(TypeMapping typeMapping, DbDataReader dbDataReader)
        {
            return Activator.CreateInstance(typeMapping.PrimaryKey.TypeOfPrimaryKey, BindingFlags.Instance, null,
                typeMapping.PrimaryKey.PushOnConstructorPrimaryKeyType.Select(a =>
                    dbDataReader[typeMapping.ColumnMappedProperties.First((tuple => tuple.propertyInfo == a)).columnName]));
        }

        public static object InstantiateObject(TypeMapping typeMapping, DbDataReader dbDataReader)
        {
            if (typeMapping.MappedType.IsAnonymousType())
            {
                return Activator.CreateInstance(typeMapping.MappedType, BindingFlags.Instance, null,
                    typeMapping.PropertiesForConstructorMappedType.Select(a =>
                        dbDataReader[
                            typeMapping.ColumnMappedProperties.First((tuple => tuple.propertyInfo == a)).columnName]));
            }
            else
            {
                var obj =  Activator.CreateInstance(typeMapping.MappedType, BindingFlags.Default);
                typeMapping.ColumnMappedProperties.ForEach(tuple =>
                    tuple.propertyInfo.SetValue(obj,dbDataReader[tuple.columnName]));
                return obj;
            }
        }

        public static object InstantiateForeignKey(IForeignKeyCreationInfo foreignKeyCreationInfo, 
            IEnumerable<(string columnName, PropertyInfo propertyInfo)> columnMappedProperties,DbDataReader dbDataReader)
        {
            return Activator.CreateInstance(foreignKeyCreationInfo.ForeignKeyType,BindingFlags.Instance, null,
                foreignKeyCreationInfo.PropertiesToCreateForeignKey.Select(a =>
                dbDataReader[columnMappedProperties.First((tuple => tuple.propertyInfo == a)).columnName]));
        }
        
        
        
        /*
         *  TypeMapping => (Primary Key,List<anonymous_object>)
         *                 (List<Foreign Key,List<anonymous_object>>)
         * 
         */
    }
}