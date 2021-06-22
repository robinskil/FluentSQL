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
        private readonly IReadOnlyCollection<VariableNode> _variableNodes;

        public EmptySelectExpressionResolver(IProvider provider,ref int parameterCounter, IReadOnlyCollection<VariableNode> variableNodes) : base(provider, null, ref parameterCounter, variableNodes)
        {
            _variableNodes = variableNodes;
        }

        protected override void MapSqlVariables(IReadOnlyCollection<VariableNode> variables, LambdaExpression expression)
        {
            
        }

        public override string GetSqlExpression()
        {
            return $"SELECT {string.Join(",",_variableNodes.Select(a => string.Join(",",TypeCache.GetPropertyInfos(a.Type).Select(c => $"{a.VariableName}.{Provider.GetColumnName(c)} AS {a.VariableName}_{Provider.GetColumnName(c)}"))))}";
        }

        public override void ValidateExpression()
        {
            
        }

        public List<TypeMapping> GenerateMappings()
        {
            var mappings = new List<TypeMapping>();
            foreach (var variableNode in _variableNodes)
            {
                var properties = TypeCache.GetPropertyInfos(variableNode.Type);
                var propertiesToMap = new List<(string, PropertyInfo)>();
                foreach (var propertyInfo in properties)
                {
                    propertiesToMap.Add((Provider.GetColumnName(propertyInfo),propertyInfo));
                }
                var typeMapping = new TypeMapping(variableNode.Type);
                //mappings.Add();             
            }
            return mappings;
        }
    }
}