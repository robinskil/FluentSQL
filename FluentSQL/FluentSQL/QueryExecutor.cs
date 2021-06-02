using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Reflection;

namespace FluentSQL
{
    public class QueryExecutor
    {
        private readonly QueryBuilder _builder;
        private readonly IProvider _provider;

        public QueryExecutor(QueryBuilder builder, IProvider provider)
        {
            _builder = builder;
            _provider = provider;
        }

        public List<T> ExecuteQueryAndMap<T>() where T : class
        {
            using var command = _builder.GetDbCommand();
            using var connection = _provider.GetConnection();
            connection.Open();
            command.Connection = connection;
            using var reader = command.ExecuteReader();
            var objects = GetMappedObjects<T>(reader);
            connection.Close();
            return objects;
        }

        public List<T> GetMappedObjects<T>(DbDataReader dataReader) where T : class
        {
            var mappings = _builder.GetMappings();
            var objects = new List<T>();
            while (dataReader.Read())
            {
                //We map everything we can every row.
                foreach (var mapping in mappings)
                {
                    if (mapping.MappingInfo.IsAnonymousType)
                    {
                        objects.Add(MapAnonymousObject(dataReader,mapping) as T);
                    }
                    else
                    {
                        var obj = Activator.CreateInstance(mapping.MappingInfo.MapToType, BindingFlags.Public | BindingFlags.Instance,
                            null, null, CultureInfo.CurrentCulture);
                        foreach (var propertyToMap in mapping.MappingInfo.MappedPropertiesForType)
                        {
                            var val = dataReader[$"{mapping.SqlVariableName}_{propertyToMap.Item1}"];
                            propertyToMap.Item2.SetMethod.Invoke(obj,BindingFlags.Instance|BindingFlags.Public,null,new []{val},CultureInfo.CurrentCulture);
                        }
                        objects.Add(obj as T);
                    }

                }
            }
            return objects;
        }

        private object MapAnonymousObject(DbDataReader dbDataReader, Mapping mapping)
        {
            List<object> properties = new List<object>();
            foreach (var (columnName, _) in mapping.MappingInfo.MappedPropertiesForType)
            {
                properties.Add(dbDataReader[$"{mapping.SqlVariableName}_{columnName}"]);                
            }
            return Activator.CreateInstance(mapping.MappingInfo.MapToType, BindingFlags.Public | BindingFlags.Instance, null,
                properties, CultureInfo.CurrentCulture);
        }
        
    }
}