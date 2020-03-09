#nullable disable
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;
using ParseException = NetTopologySuite.IO.ParseException;

namespace NetTopologySuite.Tests.NUnit.Utilities
{
    public class IOUtil
    {
        public static Geometry Read(string wkt)
        {
            var rdr = new WKTReader();
            try
            {
                return rdr.Read(wkt);
            }
            catch (ParseException ex)
            {
                throw new AssertionException("Failed to read file", ex);
            }
        }
    }
}