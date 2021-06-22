using System.Collections.Generic;

namespace FluentSQL.Mapping
{
    public class MappedCollection
    {
        public Dictionary<object,object> PrimaryKeyMappedObjects { get; }
        public Dictionary<Include,Dictionary<object,object>> IncludedForeignKeyMappedObjects { get; }
        public Dictionary<IncludedBy,Dictionary<object,object>> IncludedByForeignKeyMappedObjects { get; }
    
        public MappedCollection()
        {
            PrimaryKeyMappedObjects = new Dictionary<object, object>();
            IncludedForeignKeyMappedObjects = new Dictionary<Include, Dictionary<object, object>>();
            IncludedByForeignKeyMappedObjects = new Dictionary<IncludedBy, Dictionary<object, object>>();
        }
    }
}