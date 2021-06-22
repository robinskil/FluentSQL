using System.Collections.Generic;
using System.Linq.Expressions;
using FluentSQL.Info;
using FluentSQL.Mapping;

namespace FluentSQL.Builder.ExpressionResolvers
{
    public class JoinExpressionResolver : ExpressionResolver
    {
        public JoinExpressionResolver(IProvider provider,List<TypeMapping> typeMappings, LambdaExpression expression, ref int parameterCounter, IReadOnlyCollection<VariableNode> variableNodes) : base(provider, expression, ref parameterCounter, variableNodes)
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