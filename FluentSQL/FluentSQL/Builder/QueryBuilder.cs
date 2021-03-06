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
        private readonly HashSet<VariableNode> _variableNodes;
        private readonly IProvider _provider;
        private readonly FluentSqlOptions _options;
        private int _parameterCounter;
        private ISelectExpressionResolver _selectExpressionResolver;
        private IExpressionResolver _fromExpressionResolver;
        private LambdaExpression _whereExpression;

        public QueryBuilder(FluentSqlOptions options, Type originTableType)
        {
            _provider = options.Provider;
            _options = options;
            _variableNodes = new HashSet<VariableNode>(VariableNode.VariableNameComparer);
            AddVariable(originTableType);
            _headNode = _variableNodes.First();
            InitializeBaseExpressionResolvers();
        }

        private void InitializeBaseExpressionResolvers()
        {
            _selectExpressionResolver = new EmptySelectExpressionResolver(_provider,ref _parameterCounter, _variableNodes);
            _fromExpressionResolver = new FromExpressionResolver(_provider, ref _parameterCounter, _variableNodes);
        }

        public void AddSelectExpression(LambdaExpression selectExpression)
        {
            if (selectExpression == null) throw new ArgumentNullException(nameof(selectExpression));
            if (_selectExpressionResolver.GetType() == typeof(SelectExpressionResolver)) throw new Exception("Cannot assign multiple select expressions.");
            _selectExpressionResolver = new SelectExpressionResolver(_options,selectExpression,ref _parameterCounter,_variableNodes);
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

        public TypeMapping GetMapping()
        {
            return _selectExpressionResolver.GenerateMapping();
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