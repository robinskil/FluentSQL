using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FluentSQL.Mapping.Processors
{
    public class JoinMappingProcess : IMappingProcess
    {
        public void Process(IReadOnlyDictionary<TypeMapping, MappedCollection> mappedCollections)
        {
            foreach (var typeMappingCollection in mappedCollections)
            {
                var baseCollection = typeMappingCollection.Value;
                foreach (var include in baseCollection.IncludedForeignKeyMappedObjects)
                {
                    var joinedCollection = mappedCollections[include.Key.IncludedTypeMapping].IncludedByForeignKeyMappedObjects
                        .First(a => a.Key.IncludedByProperty == include.Key.IncludedProperty);
                    JoinSingleTypeMapping(include.Key.IncludedProperty,include.Value,joinedCollection.Value);
                }
            }
        }

        private void JoinSingleTypeMapping(PropertyInfo mapOnProperty,Dictionary<object,object> collection, Dictionary<object,object> collectionJoined)
        {
            foreach (var keyValue in collection)
            {
                if (mapOnProperty.PropertyType.GetInterfaces().Contains(typeof(IEnumerable<>)))
                {
                    var map = mapOnProperty.GetValue(keyValue.Value) as IList;
                    if (map == null)
                        map = new List<object>();
                    map.Add(collectionJoined[keyValue.Key]);
                }
                else
                {
                    mapOnProperty.SetValue(keyValue.Value,collectionJoined[keyValue.Value]);
                }
            }
        }
    }
}