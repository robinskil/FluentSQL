using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace FluentSQL.Builder.ExpressionResolvers
{
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
}