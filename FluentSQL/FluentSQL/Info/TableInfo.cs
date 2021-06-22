using System;
using System.Collections.Generic;
using FluentSQL.Mapping;

namespace FluentSQL.Info
{
    public class TableInfo
    {
        private sealed class TableTypeEqualityComparer : IEqualityComparer<TableInfo>
        {
            public bool Equals(TableInfo x, TableInfo y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x.TableType == y.TableType;
            }

            public int GetHashCode(TableInfo obj)
            {
                return (obj.TableType != null ? obj.TableType.GetHashCode() : 0);
            }
        }

        public static IEqualityComparer<TableInfo> TableTypeComparer { get; } = new TableTypeEqualityComparer();
        public Type TableType { get; }
        public PrimaryKey PrimaryKey { get; set; }
        public List<ForeignKey> ForeignKeys { get; }
        public TableInfo(Type type)
        {
            ForeignKeys = new List<ForeignKey>();
            TableType = type;
        }
    }
}