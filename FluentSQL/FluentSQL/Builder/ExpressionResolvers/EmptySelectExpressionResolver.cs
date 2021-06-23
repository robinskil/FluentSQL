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
        private TypeMapping _typeMapping;

        public EmptySelectExpressionResolver(IProvider provider,ref int parameterCounter, IReadOnlyCollection<VariableNode> variableNodes) : base(provider, null, ref parameterCounter, variableNodes)
        {
            _variableNodes = variableNodes;
        }

        protected override void MapSqlVariables(IReadOnlyCollection<VariableNode> variables, LambdaExpression expression)
        {
            
        }

        public override string GetSqlExpression()
        {
            var currentNode = _variableNodes.First();
            var typeMapping = new TypeMapping(currentNode.Type);
            var columns = new List<string>();
            foreach (var properties in TypeCache.GetPropertyInfos(currentNode.Type))
            {
                columns.Add($"{currentNode.VariableName}.{Provider.GetColumnName(properties)} AS {currentNode.VariableName}_{Provider.GetColumnName(properties)}");
                typeMapping.ColumnMappedProperties.Add(($"{currentNode.VariableName}_{Provider.GetColumnName(properties)}", properties));
            }
            _typeMapping = typeMapping;
            return $"SELECT {string.Join(",", columns)}";
        }

        public override void ValidateExpression()
        {
            
        }

        public TypeMapping GenerateMapping()
        {
            return _typeMapping;
        }
    }
}