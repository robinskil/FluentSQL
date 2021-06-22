using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using FluentSQL.Info;

namespace FluentSQL.Builder.ExpressionResolvers
{
    public abstract class ExpressionResolver
    {
        private int _parameterCounter;
        private readonly List<DbParameter> _expressionParameters;
        protected IProvider Provider { get; }
        protected LambdaExpression Expression { get; }
        protected IReadOnlyDictionary<ParameterExpression, VariableNode> ParameterBoundSqlVariables { get; private set; }
        
        public IReadOnlyList<DbParameter> GetParameters() => _expressionParameters;

        protected ExpressionResolver(IProvider provider, LambdaExpression expression, ref int parameterCounter, IReadOnlyCollection<VariableNode> variableNodes)
        {
            _parameterCounter = parameterCounter;
            _expressionParameters = new List<DbParameter>();
            Provider = provider;
            Expression = expression;
            ValidateExpression();
            MapSqlVariables(variableNodes, expression);
        }

        protected virtual void MapSqlVariables(IReadOnlyCollection<VariableNode> variableNodes, LambdaExpression expression)
        {
            if (variableNodes.Count != expression.Parameters.Count)
                throw new Exception("Method Parameters count don't match variable node count.");
            var validatedVariables = new (ParameterExpression, VariableNode)[variableNodes.Count];
            int counter = 0;
            foreach (var variableNode in variableNodes)
            {
                validatedVariables[counter] = (expression.Parameters[counter], variableNode);
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
}