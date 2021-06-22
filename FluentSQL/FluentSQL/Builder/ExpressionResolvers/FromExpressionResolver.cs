using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentSQL.Builder.ExpressionResolvers;
using FluentSQL.Info;

namespace FluentSQL
{
    public class FromExpressionResolver : ExpressionResolver, IExpressionResolver
    {
        private readonly IReadOnlyCollection<VariableNode> _variableNodes;

        public FromExpressionResolver(IProvider provider, ref int parameterCounter, IReadOnlyCollection<VariableNode> variableNodes) : base(provider, null, ref parameterCounter, variableNodes)
        {
            _variableNodes = variableNodes;
        }

        protected override void MapSqlVariables(IReadOnlyCollection<VariableNode> variables, LambdaExpression expression)
        {
        }

        public override string GetSqlExpression()
        {
            return $"FROM {Provider.GetTableName(_variableNodes.First().Type)} AS {_variableNodes.First().VariableName}";
        }

        public override void ValidateExpression()
        {
        }
    }
}