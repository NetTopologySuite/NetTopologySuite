using System;
using System.Text.RegularExpressions;

namespace NetTopologySuite.Utilities
{
    internal static class StringEx
    {
        /// <summary>
        /// Framework replacement for string.IsNullOrWhitespace
        /// </summary>
        /// <param name="s">The string to test</param>
        /// <returns>A value indicating if <paramref name="s"/> is null, empty or only contains whitespace characters</returns>
        public static bool IsNullOrWhitespaceNTS(string s)
        {
            return string.IsNullOrEmpty(s) || Regex.IsMatch(s, @"^[\s]+$");
        }
    }
}
