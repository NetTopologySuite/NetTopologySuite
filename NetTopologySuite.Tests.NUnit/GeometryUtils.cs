using System.Collections.Generic;
using System.IO;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Tests.NUnit
{
    public class GeometryUtils
    {
        //TODO: allow specifying GeometryFactory
        public static WKTReader reader = new WKTReader();

        public static IList<Geometry> ReadWKT(string[] inputWKT)
        {
            var geometries = new List<Geometry>();
            foreach (string geomWkt in inputWKT)
            {
                geometries.Add(reader.Read(geomWkt));
            }
            return geometries;
        }

        public static Geometry ReadWKT(string inputWKT)
        {
            return reader.Read(inputWKT);
        }

        public static IList<Geometry> ReadWKTFile(Stream stream)
        {
            var fileRdr = new WKTFileReader(new StreamReader(stream), new WKTReader());
            var geoms = fileRdr.Read();
            return geoms;
        }

        public static bool IsEqual(Geometry a, Geometry b)
        {
            var a2 = Normalize(a);
            var b2 = Normalize(b);
            return a2.EqualsExact(b2);
        }

        public static Geometry Normalize(Geometry g)
        {
            var g2 = (Geometry) g.Copy();
            g2.Normalize();
            return g2;
        }
    }
}
