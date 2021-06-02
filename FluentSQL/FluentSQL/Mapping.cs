using System;
using System.Collections.Generic;
using System.Reflection;

namespace FluentSQL
{
    public class Mapping
    {
        public string SqlVariableName { get; }
        public MappingInfo MappingInfo { get; }

        public Mapping(string sqlVariableName, Type mapToType, IReadOnlyList<(string, PropertyInfo)> propertiesToMap)
        {
            SqlVariableName = sqlVariableName;
            MappingInfo = new MappingInfo(mapToType,propertiesToMap);
        }
    }
}