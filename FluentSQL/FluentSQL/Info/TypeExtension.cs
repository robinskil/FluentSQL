using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FluentSQL.Info
{
    public static class TypeExtension {

        public static bool IsAnonymousType(this Type type) {
            bool hasCompilerGeneratedAttribute = type.GetCustomAttributes(typeof(CompilerGeneratedAttribute), false).Any();
            bool nameContainsAnonymousType = type.FullName.Contains("AnonymousType");
            bool isAnonymousType = hasCompilerGeneratedAttribute && nameContainsAnonymousType;
            return isAnonymousType;
        }
    }
}