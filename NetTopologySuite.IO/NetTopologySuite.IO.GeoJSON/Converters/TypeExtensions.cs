#if PCL && !PCL40
using System;
using System.Reflection;

namespace NetTopologySuite.IO.Converters
{
    internal static class TypeExtensions
    {
        internal static bool IsAssignableFrom(this Type t1, Type t2)
        {
            return t1.GetTypeInfo().IsAssignableFrom(t2.GetTypeInfo());
        }
    }
}
#endif
