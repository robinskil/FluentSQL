using System;
using System.Collections.Generic;
using System.Reflection;

namespace FluentSQL.Info
{
    public class PrimaryKey
    {
        public Type TypeOfPrimaryKey { get; }
        public Queue<PropertyInfo> PushOnConstructorPrimaryKeyType { get; }

        public PrimaryKey(Type type)
        {
            TypeOfPrimaryKey = type;
            PushOnConstructorPrimaryKeyType = new();
        }
    }
}