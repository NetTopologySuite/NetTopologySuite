using System;

namespace NetTopologySuite.Utilities
{
    public static class EnumUtility
    {
        public static object Parse(Type type, string value)
        {
            return Enum.Parse(type, value);
        }

        public static string Format(Type type, object value, string formatString)
        {
            return Enum.Format(type, value, formatString);
        }
    }
}