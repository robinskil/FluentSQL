using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluentSQL
{
    public class MappingInfo
    {
        private sealed class MappingInfoEqualityComparer : IEqualityComparer<MappingInfo>
        {
            public bool Equals(MappingInfo x, MappingInfo y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.MapToType == y.MapToType &&
                       PropertiesToBeMappedEqual(x.MappedPropertiesForType, y.MappedPropertiesForType);
            }

            private bool PropertiesToBeMappedEqual(IReadOnlyList<(string, PropertyInfo)> properties1,
                IReadOnlyCollection<(string, PropertyInfo)> properties2)
            {
                if (properties1.Count != properties2.Count)
                {
                    return false;
                }

                return properties1.All(columnMappedProperty =>
                    properties2.Any(a =>
                        a.Item1 == columnMappedProperty.Item1 && a.Item2 == columnMappedProperty.Item2));
            }

            /// <summary>
            /// ToDo: Improve hash function to reduce hash collisions.
            /// </summary>
            /// <param name="obj"></param>
            /// <returns></returns>
            public int GetHashCode(MappingInfo obj)
            {
                unchecked
                {
                    var hashCode = obj.MapToType.GetHashCode();
                    foreach (var kv in obj.MappedPropertiesForType)
                    {
                        hashCode = (hashCode * 397) ^ kv.Item1.GetHashCode();
                    }

                    return hashCode;
                }
            }
        }

        public static IEqualityComparer<MappingInfo> MappingInfoComparer { get; } = new MappingInfoEqualityComparer();

        public MappingInfo(Type mapToType, IReadOnlyList<(string, PropertyInfo)> propertiesToMap, bool isAnonymousType = false)
        {
            MapToType = mapToType;
            MappedPropertiesForType = propertiesToMap;
            IsAnonymousType = isAnonymousType;
        }

        public Type MapToType { get; }
        public bool IsAnonymousType { get; }

        public IReadOnlyList<(string, PropertyInfo)> MappedPropertiesForType { get; }
    }
}