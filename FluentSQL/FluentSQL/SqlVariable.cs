using System;
using System.Collections.Generic;

namespace FluentSQL
{
    public class SqlVariable
    {
        private sealed class VariableNameEqualityComparer : IEqualityComparer<SqlVariable>
        {
            public bool Equals(SqlVariable x, SqlVariable y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.VariableName, y.VariableName, StringComparison.InvariantCulture);
            }

            public int GetHashCode(SqlVariable obj)
            {
                return (obj.VariableName != null ? StringComparer.InvariantCulture.GetHashCode(obj.VariableName) : 0);
            }
        }

        public static IEqualityComparer<SqlVariable> VariableNameComparer { get; } = new VariableNameEqualityComparer();

        public Type Type { get; set; }
        public string VariableName { get; set; }
    }
}