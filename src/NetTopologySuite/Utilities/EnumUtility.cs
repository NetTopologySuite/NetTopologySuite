using System;

namespace NetTopologySuite.Utilities
{
#pragma warning disable 1591
    [Obsolete]
    public static class EnumUtility
    {
        public static object Parse(Type type, string value)
        {
            return Enum.Parse(type, value, false);
        }

        public static string Format(Type type, object value, string formatString)
        {
            return Enum.Format(type, value, formatString);
        }
    }
#pragma warning restore 1591
}
