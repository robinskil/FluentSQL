using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Microsoft.Data.SqlClient;

namespace FluentSQL
{
    public sealed class Driver
    {
        private readonly IProvider _provider;

        public Driver(IProvider provider)
        {
            _provider = provider;
        }

        public IFluentQueryable<T> From<T>() where T : class
        {
            return new FluentQueryable<T>(_provider);
        }
    }

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

    public interface IProvider
    {
        DbConnection GetConnection();
        string GetTableName(Type type);
        string GetColumnName(PropertyInfo propertyInfo);
        DbParameter CreateParameter<T>(T parameterValue, string parameterName);
        DbCommand CreateCommand();
    }

    public interface IFluentQueryable<T>
    {
        T First();
        List<T> ToList();
        IFluentQueryable<TOut> Select<TOut>(Expression<Func<T,TOut>> selectExpression) where TOut : class;
        IFluentQueryable<T> Select(Expression<Func<T, T>> selectExpression);
    }

    public class FluentQueryable<T> : IFluentQueryable<T> where T : class
    {
        private readonly IProvider _provider;
        private readonly QueryBuilder _queryBuilder;
        private readonly QueryExecutor _queryExecutor;

        public FluentQueryable(IProvider provider)
        {
            _provider = provider;
            _queryBuilder = new QueryBuilder(provider, typeof(T));
            _queryExecutor = new QueryExecutor(_queryBuilder, provider);
        }

        protected FluentQueryable(IProvider provider, QueryBuilder queryBuilder, QueryExecutor queryExecutor)
        {
            _provider = provider;
            _queryBuilder = queryBuilder;
            _queryExecutor = queryExecutor;
        }
        
        public T First()
        {
            return _queryExecutor.ExecuteQueryAndMap<T>().First();
        }

        public List<T> ToList()
        {
            return _queryExecutor.ExecuteQueryAndMap<T>();
        }

        public IFluentQueryable<TOut> Select<TOut>(Expression<Func<T, TOut>> selectExpression) where TOut : class
        {
            _queryBuilder.AddSelectExpression(selectExpression);
            return new FluentQueryable<TOut>(_provider,_queryBuilder,_queryExecutor);
        }

        public IFluentQueryable<T> Select(Expression<Func<T, T>> selectExpression)
        {
            _queryBuilder.AddSelectExpression(selectExpression);
            return this;
        }
    }

    public sealed class QueryBuilder
    {
        private HashSet<SqlVariable> _sqlVariables;
        private readonly IProvider _provider;
        private readonly Type _originTableType;
        private int _parameterCounter;
        private ISelectExpressionResolver _selectExpressionResolver;
        private IExpressionResolver _fromExpressionResolver;
        private LambdaExpression _whereExpression;

        public QueryBuilder(IProvider provider, Type originTableType)
        {
            _provider = provider;
            _originTableType = originTableType;
            _sqlVariables = new HashSet<SqlVariable>(SqlVariable.VariableNameComparer);
            AddVariable(_originTableType);
            InitializeBaseExpressionResolvers();
        }

        private void InitializeBaseExpressionResolvers()
        {
            _selectExpressionResolver = new EmptySelectExpressionResolver(_provider,ref _parameterCounter,_sqlVariables);
            _fromExpressionResolver = new FromExpressionResolver(_provider, ref _parameterCounter, _sqlVariables);
        }

        public void AddSelectExpression(LambdaExpression selectExpression)
        {
            if (selectExpression == null) throw new ArgumentNullException(nameof(selectExpression));
            if (_selectExpressionResolver.GetType() == typeof(SelectExpressionResolver)) throw new Exception("Cannot assign multiple select expressions.");
            _selectExpressionResolver = new SelectExpressionResolver(_provider,selectExpression,ref _parameterCounter,_sqlVariables);
        }

        public void AddWhereExpression(LambdaExpression lambdaExpression)
        {
            _whereExpression = lambdaExpression ?? throw new ArgumentNullException(nameof(lambdaExpression));
        }

        public DbCommand GetDbCommand()
        {
            var command = _provider.CreateCommand();
            command.CommandText = GenerateSqlQuery();
            foreach (var sqlParameter in GetAllParameters())
            {
                command.Parameters.Add(sqlParameter);
            }
            return command;
        }

        public List<Mapping> GetMappings()
        {
            return _selectExpressionResolver.GenerateMappings();
        }

        private IReadOnlyList<DbParameter> GetAllParameters()
        {
            return _selectExpressionResolver.GetParameters().Concat(_fromExpressionResolver.GetParameters()).ToList();
        }

        private string GenerateSqlQuery()
        {
            return _selectExpressionResolver.GetSqlExpression() + " " + _fromExpressionResolver.GetSqlExpression();
        }

        private void AddVariable(Type type)
        {
            var counter = 1;
            var sqlVariable = new SqlVariable() {VariableName = type.Name, Type = type};
            while (_sqlVariables.Contains(sqlVariable))
            {
                sqlVariable.VariableName = sqlVariable.Type.Name + counter;
                counter++;
            }

            _sqlVariables.Add(sqlVariable);
        }
    }

    public abstract class ExpressionResolver
    {
        private int _parameterCounter;
        private readonly List<DbParameter> _expressionParameters;
        protected IProvider Provider { get; }
        protected LambdaExpression Expression { get; }
        protected IReadOnlyDictionary<ParameterExpression,SqlVariable> ParameterBoundSqlVariables { get; private set; }
        
        public IReadOnlyList<DbParameter> GetParameters() => _expressionParameters;

        protected ExpressionResolver(IProvider provider, LambdaExpression expression, ref int parameterCounter, IReadOnlyCollection<SqlVariable> sqlVariables)
        {
            _parameterCounter = parameterCounter;
            _expressionParameters = new List<DbParameter>();
            Provider = provider;
            Expression = expression;
            ValidateExpression();
            MapSqlVariables(sqlVariables,expression);
        }

        protected virtual void MapSqlVariables(IReadOnlyCollection<SqlVariable> variables, LambdaExpression expression)
        {
            if (variables.Count != expression.Parameters.Count)
                throw new Exception("Method Parameters count don't match sql variable count.");
            var validatedVariables = new (ParameterExpression, SqlVariable)[variables.Count];
            int counter = 0;
            foreach (var sqlVariable in variables)
            {
                validatedVariables[counter] = (expression.Parameters[counter],sqlVariable);
                counter++;
            }
            ParameterBoundSqlVariables = validatedVariables.ToDictionary(a => a.Item1,a => a.Item2);
        }

        public string AddParameter(object value)
        {
            var parameterName = $"P{_parameterCounter}";
            _parameterCounter+=1;
            _expressionParameters.Add(Provider.CreateParameter(value,parameterName));
            return parameterName;
        }
        public abstract string GetSqlExpression();
        public abstract void ValidateExpression();

    }

    public static class TypeCache
    {
        private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> CachedTypeProperties;

        static TypeCache()
        {
            CachedTypeProperties = new ConcurrentDictionary<Type, List<PropertyInfo>>();
        }

        public static IReadOnlyList<PropertyInfo> GetPropertyInfos(Type type)
        {
            if (!CachedTypeProperties.ContainsKey(type))
            {
                CachedTypeProperties.AddOrUpdate(type, GetPropertyInfosFromAssembly,
                    ((existingKey, list) => list));
            }
            return CachedTypeProperties[type];
        }

        private static List<PropertyInfo> GetPropertyInfosFromAssembly(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string)).ToList();
        }
        
    }

    public class FromExpressionResolver : ExpressionResolver, IExpressionResolver
    {
        private readonly IReadOnlyCollection<SqlVariable> _sqlVariables;

        public FromExpressionResolver(IProvider provider, ref int parameterCounter, IReadOnlyCollection<SqlVariable> sqlVariables) : base(provider, null, ref parameterCounter, sqlVariables)
        {
            _sqlVariables = sqlVariables;
        }

        protected override void MapSqlVariables(IReadOnlyCollection<SqlVariable> variables, LambdaExpression expression)
        {
        }

        public override string GetSqlExpression()
        {
            return $"FROM {Provider.GetTableName(_sqlVariables.First().Type)} AS {_sqlVariables.First().VariableName}";
        }

        public override void ValidateExpression()
        {
        }
    }

    public interface IExpressionResolver
    {
        string GetSqlExpression();
        void ValidateExpression();
        IReadOnlyList<DbParameter> GetParameters();
    }

    public interface ISelectExpressionResolver : IExpressionResolver
    {
        List<Mapping> GenerateMappings();
    }
    public class EmptySelectExpressionResolver : ExpressionResolver, ISelectExpressionResolver
    {
        private readonly IReadOnlyCollection<SqlVariable> _sqlVariables;

        public EmptySelectExpressionResolver(IProvider provider,ref int parameterCounter, IReadOnlyCollection<SqlVariable> sqlVariables) : base(provider, null, ref parameterCounter, sqlVariables)
        {
            _sqlVariables = sqlVariables;
        }

        protected override void MapSqlVariables(IReadOnlyCollection<SqlVariable> variables, LambdaExpression expression)
        {
            
        }

        public override string GetSqlExpression()
        {
            return $"SELECT {string.Join(",",_sqlVariables.Select(a => string.Join(",",TypeCache.GetPropertyInfos(a.Type).Select(c => $"{a.VariableName}.{Provider.GetColumnName(c)} AS {a.VariableName}_{Provider.GetColumnName(c)}"))))}";
        }

        public override void ValidateExpression()
        {
            
        }

        public List<Mapping> GenerateMappings()
        {
            var mappings = new List<Mapping>();
            foreach (var sqlVariable in _sqlVariables)
            {
                var properties = TypeCache.GetPropertyInfos(sqlVariable.Type);
                var propertiesToMap = new List<(string, PropertyInfo)>();
                foreach (var propertyInfo in properties)
                {
                    propertiesToMap.Add((Provider.GetColumnName(propertyInfo),propertyInfo));
                }
                mappings.Add(new Mapping(sqlVariable.VariableName,sqlVariable.Type,propertiesToMap));                
            }
            return mappings;
        }
    }

    public class SelectExpressionResolver : ExpressionResolver, ISelectExpressionResolver
    {
        private List<string> _columnsToInclude;
        private List<Mapping> _mappings;
        
        
        public SelectExpressionResolver(IProvider provider, LambdaExpression expression, ref int parameterCounter,IReadOnlyCollection<SqlVariable> sqlVariables) : base(provider, expression, ref parameterCounter,sqlVariables)
        {
            _columnsToInclude = new List<string>();
            _mappings = new List<Mapping>();
        }

        public override string GetSqlExpression()
        {
            var baseMappingDictionary = new Dictionary<SqlVariable, List<(string columnName,PropertyInfo propertyToMapTo)>>();
            if (Expression.Body is MemberInitExpression memberInitExpression)
            {
                foreach (var memberBinding in memberInitExpression.Bindings)
                {
                    if (memberBinding is MemberAssignment memberAssignment && memberAssignment.Expression is MemberExpression memberAssignmentExpression 
                                                                           && memberAssignmentExpression.Expression is ParameterExpression parameterExpression)
                    {
                        _columnsToInclude.Add($"{ParameterBoundSqlVariables[parameterExpression].VariableName}.{memberAssignmentExpression.Member.Name} AS" +
                                              $" {ParameterBoundSqlVariables[parameterExpression].VariableName}_{memberAssignmentExpression.Member.Name}");
                        if (!baseMappingDictionary.ContainsKey(ParameterBoundSqlVariables[parameterExpression]))
                        {
                            baseMappingDictionary.Add(ParameterBoundSqlVariables[parameterExpression],new List<(string columnName, PropertyInfo propertyToMapTo)>());
                        }
                        baseMappingDictionary[ParameterBoundSqlVariables[parameterExpression]].Add(
                            ($"{memberAssignmentExpression.Member.Name}",memberAssignment.Member as PropertyInfo));
                    }
                }

                foreach (var mappingPerVariable in baseMappingDictionary)
                {
                    _mappings.Add(new Mapping(mappingPerVariable.Key.VariableName,mappingPerVariable.Key.Type,mappingPerVariable.Value));
                }
                return $"SELECT {string.Join(",",_columnsToInclude)}";
            }
            else
            {
                ResolveNewExpression(Expression.Body as NewExpression);
                return $"SELECT";
            }
        }

        private void ResolveNewExpression(NewExpression newExpression)
        {
            var propertiesToMap = new List<(string, PropertyInfo)>();
            _mappings.Add(new Mapping(ParameterBoundSqlVariables[parameter].VariableName,propertiesToMap));
            foreach (var argument in newExpression.Arguments)
            {
                var expr = argument as MemberExpression;
                var parameter = expr.Expression as ParameterExpression;
                propertiesToMap.Add();
            }
            //a : student => { a.University }
        }

        public sealed override void ValidateExpression()
        {
            if (!(Expression.Body is NewExpression) && !(Expression.Body is MemberInitExpression memberInitExpression))
            {
                throw new Exception($"Invalid expression given to {nameof(SelectExpressionResolver)}");
            }
        }

        public List<Mapping> GenerateMappings()
        {
            return _mappings;
        }
    }
}