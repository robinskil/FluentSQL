using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using FluentSQL.Builder.ExpressionResolvers;
using FluentSQL.Info;
using FluentSQL.Mapping;

namespace FluentSQL
{
    public sealed class QueryBuilder
    {
        private VariableNode _headNode;
        private HashSet<VariableNode> _variableNodes;
        private readonly IProvider _provider;
        private readonly FluentSqlOptions _options;
        private readonly Type _originTableType;
        private int _parameterCounter;
        private ISelectExpressionResolver _selectExpressionResolver;
        private IExpressionResolver _fromExpressionResolver;
        private LambdaExpression _whereExpression;

        public QueryBuilder(FluentSqlOptions options, Type originTableType)
        {
            _provider = options.Provider;
            _options = options;
            _originTableType = originTableType;
            _variableNodes = new HashSet<VariableNode>(VariableNode.VariableNameComparer);
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

        public void AddJoinExpression(LambdaExpression lambdaExpression)
        {
            
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

        public List<TypeMapping> GetMappings()
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
            var variableNode = new VariableNode() {VariableName = type.Name, Type = type};
            while (_variableNodes.Contains(variableNode))
            {
                variableNode.VariableName = variableNode.Type.Name + counter;
                counter++;
            }
            _variableNodes.Add(variableNode);
        }
    }
}