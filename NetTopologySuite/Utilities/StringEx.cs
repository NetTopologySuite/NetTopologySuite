using System;
using System.Text.RegularExpressions;

namespace NetTopologySuite.Utilities
{
    public static class StringEx
    {
        public static bool IsNullOrWhitespaceNTS(string s)
        {
            return String.IsNullOrEmpty(s) || Regex.IsMatch(s, @"^[\s]+$");
        }
    }
}
