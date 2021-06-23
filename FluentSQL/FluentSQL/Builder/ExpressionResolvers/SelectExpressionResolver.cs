using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FluentSQL.Builder.ExpressionResolvers;
using FluentSQL.Info;
using FluentSQL.Mapping;

namespace FluentSQL
{
    public class SelectExpressionResolver : ExpressionResolver, ISelectExpressionResolver
    {
        private readonly FluentSqlOptions _options;
        private readonly IReadOnlyCollection<VariableNode> _variableNodes;
        private TypeMapping _typeMapping;

        public SelectExpressionResolver(FluentSqlOptions options, LambdaExpression expression, ref int parameterCounter,
            IReadOnlyCollection<VariableNode> variableNodes) : base(options.Provider, expression, ref parameterCounter,
            variableNodes)
        {
            _options = options;
            _variableNodes = variableNodes;
        }

        public override string GetSqlExpression()
        {
            if (Expression.Body is MemberInitExpression memberInitExpression)
            {
                return $"SELECT {string.Join(",", ResolveMemberInitExpression(memberInitExpression))}";
            }
            else if(Expression.Body is NewExpression newExpression)
            {
                return $"SELECT {string.Join(",", ResolveNewExpression(newExpression))}";
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private List<string> ResolveMemberInitExpression(MemberInitExpression memberInitExpression)
        {
            var columns = new List<string>();
            _typeMapping = new TypeMapping(memberInitExpression.Type);
            foreach (var memberBinding in memberInitExpression.Bindings)
            {
                if (memberBinding is MemberAssignment memberAssignment &&
                    memberAssignment.Expression is MemberExpression memberAccessExpression &&
                    memberAccessExpression.Expression is ParameterExpression parameterExpression)
                {
                    var columnQueryName =
                        $"{ParameterBoundVariableNodes[parameterExpression].VariableName}.{memberAccessExpression.Member.Name} AS " +
                        $"{ParameterBoundVariableNodes[parameterExpression].VariableName}_{memberAccessExpression.Member.Name}";
                    columns.Add(columnQueryName);
                    _typeMapping.ColumnMappedProperties.Add((
                        $"{ParameterBoundVariableNodes[parameterExpression].VariableName}_{memberAccessExpression.Member.Name}",
                        memberAssignment.Member as PropertyInfo));
                }
            }
            return columns;
        }

        private List<string> ResolveNewExpression(NewExpression newExpression)
        {
            var columns = new List<string>();
            _typeMapping = new TypeMapping(newExpression.Type);
            _typeMapping.PrimaryKey = _options.TableInfos[_variableNodes.First().Type].PrimaryKey;
            foreach (var argument in newExpression.Arguments)
            {
                if (argument is MemberExpression memberAccessExpression &&
                    memberAccessExpression.Expression is ParameterExpression parameterExpression)
                {
                    _typeMapping.PropertiesForConstructorMappedType.Enqueue(
                        memberAccessExpression.Member as PropertyInfo);
                    var columnQueryName =
                        $"{ParameterBoundVariableNodes[parameterExpression].VariableName}.{memberAccessExpression.Member.Name} AS " +
                        $"{ParameterBoundVariableNodes[parameterExpression].VariableName}_{memberAccessExpression.Member.Name}";
                    _typeMapping.ColumnMappedProperties.Add((
                        $"{ParameterBoundVariableNodes[parameterExpression].VariableName}_{memberAccessExpression.Member.Name}",
                        memberAccessExpression.Member as PropertyInfo));
                    columns.Add(columnQueryName);
                }
                else throw new NotImplementedException();
            }

            return columns;
        }

        public sealed override void ValidateExpression()
        {
            if (!(Expression.Body is NewExpression) && !(Expression.Body is MemberInitExpression memberInitExpression))
            {
                throw new Exception($"Invalid expression given to {nameof(SelectExpressionResolver)}");
            }
        }

        public TypeMapping GenerateMapping()
        {
            return _typeMapping;
        }
    }
}