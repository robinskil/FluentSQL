using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FluentSQL.Builder.ExpressionResolvers;

namespace FluentSQL
{
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
}