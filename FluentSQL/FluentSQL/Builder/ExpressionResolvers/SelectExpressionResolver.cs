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
        private Dictionary<ParameterExpression, TypeMapping> _typeMappings;
        
        public SelectExpressionResolver(IProvider provider, LambdaExpression expression, ref int parameterCounter,IReadOnlyCollection<VariableNode> variableNodes) : base(provider, expression, ref parameterCounter,variableNodes)
        {
            _typeMappings = new Dictionary<ParameterExpression, TypeMapping>();
        }

        public override string GetSqlExpression()
        {
            if (Expression.Body is MemberInitExpression memberInitExpression)
            {
                foreach (var memberBinding in memberInitExpression.Bindings)
                {
                    if (memberBinding is MemberAssignment memberAssignment && memberAssignment.Expression is MemberExpression memberAssignmentExpression 
                                                                           && memberAssignmentExpression.Expression is ParameterExpression parameterExpression)
                    {
                        if (!_typeMappings.ContainsKey(parameterExpression))
                        {
                            _typeMappings.Add(parameterExpression,new TypeMapping(null));
                        }
                        // _columnsToInclude.Add($"{ParameterBoundSqlVariables[parameterExpression].VariableName}.{memberAssignmentExpression.Member.Name} AS" +
                        //                       $" {ParameterBoundSqlVariables[parameterExpression].VariableName}_{memberAssignmentExpression.Member.Name}");
                        // if (!baseMappingDictionary.ContainsKey(ParameterBoundSqlVariables[parameterExpression]))
                        // {
                        //     baseMappingDictionary.Add(ParameterBoundSqlVariables[parameterExpression],new List<(string columnName, PropertyInfo propertyToMapTo)>());
                        // }
                        // baseMappingDictionary[ParameterBoundSqlVariables[parameterExpression]].Add(
                        //     ($"{memberAssignmentExpression.Member.Name}",memberAssignment.Member as PropertyInfo));
                    }
                }
                return $"SELECT {string.Join(",",null)}";
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

        public List<TypeMapping> GenerateMappings()
        {
            return _typeMappings.Values.ToList();
        }
    }
}