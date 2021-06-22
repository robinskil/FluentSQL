using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace FluentSQL.Mapping.Processors
{
    public class MappingProcessor
    {
        private static IEnumerable<IMappingProcess> _processes;
        public static void RegisterProcesses(IEnumerable<IMappingProcess> processes)
        {
            _processes = processes ?? throw new ArgumentNullException(nameof(processes));
        }
        
        private readonly TypeMapping _initialTypeMapping;
        private readonly Dictionary<TypeMapping, MappedCollection> _typeMappedKeyBasedObjects;

        public IReadOnlyDictionary<TypeMapping, MappedCollection> TypeMappedKeyBasedObjects => _typeMappedKeyBasedObjects;

        public MappingProcessor(HashSet<TypeMapping> typeMappings, TypeMapping initialTypeMapping)
        {
            _initialTypeMapping = initialTypeMapping;
            _typeMappedKeyBasedObjects = typeMappings.ToDictionary(a => a,a => new MappedCollection());
        }

        public void MapFromDataReader(DbDataReader dbDataReader)
        {
            foreach (var mappedKeyBasedObject in _typeMappedKeyBasedObjects)
            {
                var obj = Instantiator.InstantiateObject(mappedKeyBasedObject.Key, dbDataReader);
                var key = Instantiator.InstantiatePrimaryKey(mappedKeyBasedObject.Key, dbDataReader);
                if (!mappedKeyBasedObject.Value.PrimaryKeyMappedObjects.ContainsKey(key))
                {
                    mappedKeyBasedObject.Value.PrimaryKeyMappedObjects.Add(key,obj);
                }

                foreach (var includedProperty in mappedKeyBasedObject.Key.IncludedProperties)
                {
                    var foreignKeyObject = Instantiator.InstantiateForeignKey(includedProperty,
                        mappedKeyBasedObject.Key.ColumnMappedProperties, dbDataReader);
                    if (!mappedKeyBasedObject.Value.IncludedForeignKeyMappedObjects[includedProperty].ContainsKey(foreignKeyObject))
                    {
                        mappedKeyBasedObject.Value.IncludedForeignKeyMappedObjects[includedProperty]
                            .Add(foreignKeyObject,mappedKeyBasedObject.Value.PrimaryKeyMappedObjects[key]);
                    }
                }
                
                foreach (var includedByProperty in mappedKeyBasedObject.Key.IncludedByOthers)
                {
                    var foreignKeyObject = Instantiator.InstantiateForeignKey(includedByProperty,
                        mappedKeyBasedObject.Key.ColumnMappedProperties, dbDataReader);
                    if (!mappedKeyBasedObject.Value.IncludedByForeignKeyMappedObjects[includedByProperty].ContainsKey(foreignKeyObject))
                    {
                        mappedKeyBasedObject.Value.IncludedByForeignKeyMappedObjects[includedByProperty]
                            .Add(foreignKeyObject,mappedKeyBasedObject.Value.PrimaryKeyMappedObjects[key]);
                    }
                }
            }
        }

        public void StartProcesses()
        {
            foreach (var mappingProcess in _processes)
            {
                mappingProcess.Process(TypeMappedKeyBasedObjects);
            }
        }

        public List<T> GetObjects<T>()
        {
            return TypeMappedKeyBasedObjects[_initialTypeMapping].PrimaryKeyMappedObjects.ToList() as List<T>;
        }
    }
}