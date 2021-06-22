using System.Collections.Generic;
using System.Linq.Expressions;
using FluentSQL.Mapping;

namespace FluentSQL.Builder.ExpressionResolvers
{
    public class JoinExpressionResolver : ExpressionResolver
    {
        public JoinExpressionResolver(IProvider provider,List<TypeMapping> typeMappings, LambdaExpression expression, ref int parameterCounter, IReadOnlyCollection<SqlVariable> sqlVariables) : base(provider, expression, ref parameterCounter, sqlVariables)
        {
        }

        public override string GetSqlExpression()
        {
            throw new System.NotImplementedException();
        }

        public void UpdateTypeMappings(List<TypeMapping> typeMappings)
        {
            
        }

        public override void ValidateExpression()
        {
            throw new System.NotImplementedException();
        }
    }
}