using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using FluentSQL.Info;
using FluentSQL.Mapping;

namespace FluentSQL
{
    public class FluentSqlOptions
    {
        public Dictionary<Type,TableInfo> TableInfos { get; }
        internal IProvider Provider { get; private set; }
        public FluentSqlOptions()
        {
            TableInfos = new Dictionary<Type, TableInfo>();
        }
        public FluentSqlOptions SelectProvider(IProvider provider)
        {
            if (provider == null) throw new ArgumentNullException(nameof(provider));
            Provider = provider;
            return this;
        }

        private TableInfo GetOrAddTableInfo(Type type)
        {
            if (!TableInfos.ContainsKey(type))
            {
                TableInfos.Add(type,new TableInfo(type));                
            }
            return TableInfos[type];
        }
        
        public FluentSqlOptions SetPrimaryKey<T>(Expression<Func<T, object>> keyExpression) where T : class
        {
            var tableInfo = GetOrAddTableInfo(typeof(T));
            if (keyExpression.Body is not NewExpression newExpression)
                throw new NotImplementedException();
            else
            {
                var primaryKey = new PrimaryKey(newExpression.Type);
                foreach (var argument in newExpression.Arguments)
                {
                    if (argument is MemberExpression memberExpression)
                    {
                        primaryKey.PushOnConstructorPrimaryKeyType.Enqueue(memberExpression.Member as PropertyInfo);
                    }
                    else throw new NotImplementedException();
                }
                tableInfo.PrimaryKey = primaryKey;
            }
            return this;
        }

        public FluentSqlOptions AddForeignKeyReference<T, K>(Expression<Func<T, K>> propertyMapExpression,
            Expression<Func<K,T>> mapBackExpression,
            Expression<Func<T, K, bool>> foreignKeyExpression)
        {
            var foreignKey = new ForeignKey();
            foreignKey.FromTable = GetOrAddTableInfo(typeof(T));
            foreignKey.ToTable = GetOrAddTableInfo(typeof(K));
            if (propertyMapExpression.Body is MemberExpression property)
            {
                foreignKey.FromTableMapProperty = property.Member as PropertyInfo;
            }
            if (mapBackExpression.Body is MemberExpression mapBackProperty)
            {
                foreignKey.ToTableMapProperty = mapBackProperty.Member as PropertyInfo;
            }
            if (foreignKeyExpression.Body is BinaryExpression binaryExpression)
            {
                var dic = PropertiesUsedForeignKeyEquality(binaryExpression);
                foreignKey.PropertyInfosFromTypeToPushOnEqualityType = dic[foreignKey.FromTable.TableType];
                foreignKey.PropertyInfosToTypeToPushOnEqualityType = dic[foreignKey.ToTable.TableType];
                foreignKey.EqualityType = GetTupleTypeDefinition(dic.First().Value);
            }
            return this;
        }
        
        public FluentSqlOptions AddForeignKeyReference<T, K>(Expression<Func<T, K>> propertyMapExpression,
            Expression<Func<K,IEnumerable<T>>> mapBackExpression,
            Expression<Func<T, K, bool>> foreignKeyExpression)
        {
            var foreignKey = new ForeignKey();
            foreignKey.FromTable = GetOrAddTableInfo(typeof(T));
            foreignKey.ToTable = GetOrAddTableInfo(typeof(K));
            if (propertyMapExpression.Body is MemberExpression property)
            {
                foreignKey.FromTableMapProperty = property.Member as PropertyInfo;
            }
            if (mapBackExpression.Body is MemberExpression mapBackProperty)
            {
                foreignKey.ToTableMapProperty = mapBackProperty.Member as PropertyInfo;
            }
            if (foreignKeyExpression.Body is BinaryExpression binaryExpression)
            {
                var dic = PropertiesUsedForeignKeyEquality(binaryExpression);
                foreignKey.PropertyInfosFromTypeToPushOnEqualityType = dic[foreignKey.FromTable.TableType];
                foreignKey.PropertyInfosToTypeToPushOnEqualityType = dic[foreignKey.ToTable.TableType];
                foreignKey.EqualityType = GetTupleTypeDefinition(dic.First().Value);
            }
            return this;
        }
        
        
        public FluentSqlOptions AddForeignKeyReference<T, K>(Expression<Func<T, IEnumerable<K>>> propertyMapExpression,
            Expression<Func<K,T>> mapBackExpression,
            Expression<Func<T, K, bool>> foreignKeyExpression)
        {
            var foreignKey = new ForeignKey();
            foreignKey.FromTable = GetOrAddTableInfo(typeof(T));
            foreignKey.ToTable = GetOrAddTableInfo(typeof(K));
            if (propertyMapExpression.Body is MemberExpression property)
            {
                foreignKey.FromTableMapProperty = property.Member as PropertyInfo;
            }
            if (mapBackExpression.Body is MemberExpression mapBackProperty)
            {
                foreignKey.ToTableMapProperty = mapBackProperty.Member as PropertyInfo;
            }
            if (foreignKeyExpression.Body is BinaryExpression binaryExpression)
            {
                var dic = PropertiesUsedForeignKeyEquality(binaryExpression);
                foreignKey.PropertyInfosFromTypeToPushOnEqualityType = dic[foreignKey.FromTable.TableType];
                foreignKey.PropertyInfosToTypeToPushOnEqualityType = dic[foreignKey.ToTable.TableType];
                foreignKey.EqualityType = GetTupleTypeDefinition(dic.First().Value);
            }
            return this;
        }

        private Type GetTupleTypeDefinition(IReadOnlyCollection<PropertyInfo> propertyInfos)
        {
            return propertyInfos.Count switch
            {
                1 => typeof(Tuple<object>),
                2 => typeof(Tuple<object, object>),
                3 => typeof(Tuple<object, object,object>),
                4 => typeof(Tuple<object, object, object,object>),
                5 => typeof(Tuple<object, object, object,object,object>),
                6 => typeof(Tuple<object, object, object,object, object,object>),
                7 => typeof(Tuple<object, object, object,object, object,object,object>),
                _ => throw new NotImplementedException()
            };
        }

        private Dictionary<Type, List<PropertyInfo>> PropertiesUsedForeignKeyEquality(BinaryExpression binaryExpression)
        {
            if (binaryExpression.Left is BinaryExpression binaryExpressionLeft &&
                binaryExpression.Right is BinaryExpression binaryExpressionRight)
            {
                var dic = new Dictionary<Type, List<PropertyInfo>>();
                foreach (var kv in PropertiesUsedForeignKeyEquality(binaryExpressionLeft))
                {
                    if(!dic.ContainsKey(kv.Key))
                    {
                        dic.Add(kv.Key,new List<PropertyInfo>());
                    }
                    dic[kv.Key].AddRange(kv.Value);
                }
                foreach (var kv in PropertiesUsedForeignKeyEquality(binaryExpressionRight))
                {
                    if(!dic.ContainsKey(kv.Key))
                    {
                        dic.Add(kv.Key,new List<PropertyInfo>());
                    }
                    dic[kv.Key].AddRange(kv.Value);
                }
                return dic;
            }
            else if (binaryExpression.Left is MemberExpression{Expression: ParameterExpression parameterExpressionLeft} memberExpressionLeft &&
                     binaryExpression.Right is MemberExpression{Expression:ParameterExpression parameterExpressionRight} memberExpressionRight)
            {
                var dic = new Dictionary<Type, List<PropertyInfo>>();
                if (!dic.ContainsKey(parameterExpressionLeft.Type))
                {
                    dic.Add(parameterExpressionLeft.Type,new List<PropertyInfo>());
                }
                if (!dic.ContainsKey(parameterExpressionRight.Type))
                {
                    dic.Add(parameterExpressionRight.Type,new List<PropertyInfo>());
                }
                dic[parameterExpressionLeft.Type].Add(memberExpressionLeft.Member as PropertyInfo);
                dic[parameterExpressionRight.Type].Add(memberExpressionRight.Member as PropertyInfo);
                return dic;
            }
            else throw new NotImplementedException();
        }

        public void Validate()
        {
        }
    }
}