using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using FluentSQL.Builder.ExpressionResolvers;

namespace FluentSQL
{
    public class SelectExpressionResolver : ExpressionResolver, ISelectExpressionResolver
    {
        private List<string> _columnsToInclude;
        private List<Mapping.Mapping> _mappings;
        
        
        public SelectExpressionResolver(IProvider provider, LambdaExpression expression, ref int parameterCounter,IReadOnlyCollection<SqlVariable> sqlVariables) : base(provider, expression, ref parameterCounter,sqlVariables)
        {
            _columnsToInclude = new List<string>();
            _mappings = new List<Mapping.Mapping>();
        }

        public override string GetSqlExpression()
        {
            var baseMappingDictionary = new Dictionary<SqlVariable, List<(string columnName,PropertyInfo propertyToMapTo)>>();
            if (Expression.Body is MemberInitExpression memberInitExpression)
            {
                foreach (var memberBinding in memberInitExpression.Bindings)
                {
                    if (memberBinding is MemberAssignment memberAssignment && memberAssignment.Expression is MemberExpression memberAssignmentExpression 
                                                                           && memberAssignmentExpression.Expression is ParameterExpression parameterExpression)
                    {
                        _columnsToInclude.Add($"{ParameterBoundSqlVariables[parameterExpression].VariableName}.{memberAssignmentExpression.Member.Name} AS" +
                                              $" {ParameterBoundSqlVariables[parameterExpression].VariableName}_{memberAssignmentExpression.Member.Name}");
                        if (!baseMappingDictionary.ContainsKey(ParameterBoundSqlVariables[parameterExpression]))
                        {
                            baseMappingDictionary.Add(ParameterBoundSqlVariables[parameterExpression],new List<(string columnName, PropertyInfo propertyToMapTo)>());
                        }
                        baseMappingDictionary[ParameterBoundSqlVariables[parameterExpression]].Add(
                            ($"{memberAssignmentExpression.Member.Name}",memberAssignment.Member as PropertyInfo));
                    }
                }

                foreach (var mappingPerVariable in baseMappingDictionary)
                {
                    _mappings.Add(new Mapping.Mapping(mappingPerVariable.Key.VariableName,mappingPerVariable.Key.Type,mappingPerVariable.Value));
                }
                return $"SELECT {string.Join(",",_columnsToInclude)}";
            }
            else
            {
                ResolveNewExpression(Expression.Body as NewExpression);
                return $"SELECT";
            }
        }

        private void ResolveNewExpression(NewExpression newExpression)
        {
            // var propertiesToMap = new List<(string, PropertyInfo)>();
            // _mappings.Add(new Mapping(ParameterBoundSqlVariables[parameter].VariableName,propertiesToMap));
            // foreach (var argument in newExpression.Arguments)
            // {
            //     var expr = argument as MemberExpression;
            //     var parameter = expr.Expression as ParameterExpression;
            //     propertiesToMap.Add();
            // }
            //a : student => { a.University }
        }

        public sealed override void ValidateExpression()
        {
            if (!(Expression.Body is NewExpression) && !(Expression.Body is MemberInitExpression memberInitExpression))
            {
                throw new Exception($"Invalid expression given to {nameof(SelectExpressionResolver)}");
            }
        }

        public List<Mapping.Mapping> GenerateMappings()
        {
            return _mappings;
        }
    }
}