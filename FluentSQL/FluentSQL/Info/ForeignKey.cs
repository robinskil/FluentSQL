using System;
using System.Collections.Generic;
using System.Reflection;

namespace FluentSQL.Info
{
    public class ForeignKey
    {
        public TableInfo FromTable { get; set; }
        public PropertyInfo FromTableMapProperty { get; set; }
        public TableInfo ToTable { get; set; }
        public PropertyInfo ToTableMapProperty { get; set; }
        public Type EqualityType { get; set; }
        public List<PropertyInfo> PropertyInfosFromTypeToPushOnEqualityType { get; set; }
        public List<PropertyInfo> PropertyInfosToTypeToPushOnEqualityType { get; set; }
    }
}