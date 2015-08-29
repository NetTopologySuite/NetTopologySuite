using System;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;
using ParseException = GeoAPI.IO.ParseException;

namespace NetTopologySuite.Tests.NUnit.Utilities
{
    public class IOUtil
    {
        public static IGeometry Read(String wkt)
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