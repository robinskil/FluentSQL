using System.Collections.Generic;

namespace FluentSQL.Mapping.Processors
{
    public interface IMappingProcess
    {
        void Process(IReadOnlyDictionary<TypeMapping, MappedCollection> mappedCollections);
    }
}