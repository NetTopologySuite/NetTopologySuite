using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using GeoAPI.Operation.Buffer;

namespace NetTopologySuite.Utilities
{
    public static class StringEx
    {
        public static bool IsNullOrWhitespaceNTS(this string s)
        {
            return s == null || Regex.IsMatch(s, @"^[\s]+$");
        }
    }
}
