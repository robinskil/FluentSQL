using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Reflection;
using FluentSQL.Mapping.Processors;

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

        private List<T> GetMappedObjects<T>(DbDataReader dataReader) where T : class
        {
            var mapping = _builder.GetMapping();
            var mappingProcessor = new MappingProcessor(mapping);
            return mappingProcessor.GetObjects<T>(dataReader);
        }
    }
}