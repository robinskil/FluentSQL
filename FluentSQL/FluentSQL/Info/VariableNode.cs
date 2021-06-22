using System;
using System.Collections.Generic;
using System.Reflection;

namespace FluentSQL.Info
{
    public class VariableNode
    {
        private sealed class VariableNameEqualityComparer : IEqualityComparer<VariableNode>
        {
            public bool Equals(VariableNode x, VariableNode y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return string.Equals(x.VariableName, y.VariableName, StringComparison.InvariantCulture);
            }

            public int GetHashCode(VariableNode obj)
            {
                return (obj.VariableName != null ? StringComparer.InvariantCulture.GetHashCode(obj.VariableName) : 0);
            }
        }
        public static IEqualityComparer<VariableNode> VariableNameComparer { get; } = new VariableNameEqualityComparer();
        public string VariableName { get; set; }
        public Type Type { get; set; }
        public VariableNodeRelation Parent { get; set; }
        public List<VariableNodeRelation> Children { get; set; }
    }

    public class VariableNodeRelation
    {
        public VariableNode VariableNode { get; set; }
        public PropertyInfo RelationProperty { get; set; }
    }
}