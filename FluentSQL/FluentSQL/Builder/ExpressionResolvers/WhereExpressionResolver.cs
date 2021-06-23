using System.Collections.Generic;
using System.Linq.Expressions;
using FluentSQL.Info;

namespace FluentSQL.Builder.ExpressionResolvers
{
    public class WhereExpressionResolver: ExpressionResolver
    {
        public WhereExpressionResolver(IProvider provider, LambdaExpression expression, ref int parameterCounter, IReadOnlyCollection<VariableNode> variableNodes) : base(provider, expression, ref parameterCounter, variableNodes)
        {
        }

        public override string GetSqlExpression()
        {
            throw new System.NotImplementedException();
        }

        public override void ValidateExpression()
        {
            throw new System.NotImplementedException();
        }
    }
}