using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace FluentSQL.Mapping.Processors
{
    public class MappingProcessor
    {
        private readonly TypeMapping _initialTypeMapping;

        public MappingProcessor(TypeMapping initialTypeMapping)
        {
            _initialTypeMapping = initialTypeMapping;
        }

        public List<T> GetObjects<T>(DbDataReader dbDataReader)
        {
            var objects = new List<object>();
            while (dbDataReader.Read())
            {
                objects.Add(Instantiator.InstantiateObject(_initialTypeMapping, dbDataReader));
            }
            return objects.Cast<T>().ToList();
        }
    }
}