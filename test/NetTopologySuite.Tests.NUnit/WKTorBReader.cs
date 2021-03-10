using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Tests.NUnit
{
    /// <summary>
    /// Reads a geometry from a string that is either WKT or WKB format.
    /// </summary>
    /// <author>mdavis</author>
    class WKTorBReader
    {

        public static Geometry Read(string geomStr, NtsGeometryServices geomServ)
        {
            var rdr = new WKTorBReader(geomServ);
            return rdr.Read(geomStr);
        }

        public static bool IsWKB(string str)
        {
            return IsHex(str, MAX_CHARS_TO_CHECK);
        }

        private static bool IsHex(string str, int maxCharsToTest)
        {
            for (int i = 0; i < maxCharsToTest && i < str.Length; i++)
            {
                char ch = str[i];
                if (!IsHexDigit(ch))
                    return false;
            }

            return true;
        }

        private static bool IsHexDigit(char ch)
        {
            if (char.IsDigit(ch)) return true;
            char chLow = char.ToLowerInvariant(ch);
            if (chLow >= 'a' && chLow <= 'f') return true;
            return false;
        }

        private const int MAX_CHARS_TO_CHECK = 6;
        private readonly NtsGeometryServices _geomServ;

        public WKTorBReader(NtsGeometryServices geomServ)
        {
            _geomServ = geomServ;
        }

        public Geometry Read(string geomStr)
        {
            string trimStr = geomStr.Trim();
            if (IsWKB(trimStr))
            {
                return ReadWKBHex(trimStr, _geomServ);
            }

            return ReadWKT(trimStr, _geomServ);

        }

        public static Geometry ReadWKT(string wkt, NtsGeometryServices geomServ)
        {
            var rdr = new WKTReader(geomServ);
            return rdr.Read(wkt);
        }

        public static Geometry ReadWKBHex(string wkb, NtsGeometryServices geomServ)
        {
            var rdr = new WKBReader(geomServ);
            return rdr.Read(WKBReader.HexToBytes(wkb));
        }
    }
}
