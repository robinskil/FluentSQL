using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FluentSQL.Info;
using FluentSQL.Mapping;

namespace FluentSQL.Builder.ExpressionResolvers
{
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

        public List<TypeMapping> GenerateMappings()
        {
            var mappings = new List<TypeMapping>();
            foreach (var sqlVariable in _sqlVariables)
            {
                var properties = TypeCache.GetPropertyInfos(sqlVariable.Type);
                var propertiesToMap = new List<(string, PropertyInfo)>();
                foreach (var propertyInfo in properties)
                {
                    propertiesToMap.Add((Provider.GetColumnName(propertyInfo),propertyInfo));
                }
                var typeMapping = new TypeMapping(sqlVariable.Type);
                mappings.Add();                
            }
            return mappings;
        }
    }
}