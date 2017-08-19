using System;
using System.Reflection;

namespace NetTopologySuite
{
    internal static class TypeExtensions
    {
#if !HAS_SYSTEM_TYPE_ISASSIGNABLEFROM
#if !HAS_SYSTEM_REFLECTION_TYPEINFO
#error Must have either one or the other.
#endif
        internal static bool IsAssignableFrom(this Type type, Type other) => type.GetTypeInfo().IsAssignableFrom(other.GetTypeInfo());
#endif
    }
}
