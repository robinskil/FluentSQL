using System;
using System.Data.Common;
using System.Reflection;

namespace FluentSQL
{
    public interface IProvider
    {
        DbConnection GetConnection();
        string GetTableName(Type type);
        string GetColumnName(PropertyInfo propertyInfo);
        DbParameter CreateParameter<T>(T parameterValue, string parameterName);
        DbCommand CreateCommand();
    }
}