using System;
using System.Data.Common;
using System.Reflection;
using Microsoft.Data.SqlClient;

namespace FluentSQL
{
    public sealed class SqlServerProvider : IProvider
    {
        private readonly string _connectionString;

        public SqlServerProvider(string connectionString)
        {
            _connectionString = connectionString;
        }
        public DbConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public string GetTableName(Type type)
        {
            return type.Name;
        }

        public string GetColumnName(PropertyInfo propertyInfo)
        {
            return propertyInfo.Name;
        }

        public DbParameter CreateParameter<T>(T parameterValue, string parameterName)
        {
            return new SqlParameter(parameterName,parameterValue);
        }

        public DbCommand CreateCommand()
        {
            return new SqlCommand();
        }
    }
}