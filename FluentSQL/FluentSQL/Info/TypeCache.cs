using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluentSQL.Info
{
    public static class TypeCache
    {
        private static readonly ConcurrentDictionary<Type, List<PropertyInfo>> CachedTypeProperties;

        static TypeCache()
        {
            CachedTypeProperties = new ConcurrentDictionary<Type, List<PropertyInfo>>();
        }

        public static IReadOnlyList<PropertyInfo> GetPropertyInfos(Type type)
        {
            if (!CachedTypeProperties.ContainsKey(type))
            {
                CachedTypeProperties.AddOrUpdate(type, GetPropertyInfosFromAssembly,
                    ((existingKey, list) => list));
            }
            return CachedTypeProperties[type];
        }

        private static List<PropertyInfo> GetPropertyInfosFromAssembly(Type type)
        {
            return type.GetProperties(BindingFlags.Instance | BindingFlags.Public).Where(p => p.PropertyType.IsValueType || p.PropertyType == typeof(string)).ToList();
        }
        
    }
}