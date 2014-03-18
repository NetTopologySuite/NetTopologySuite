using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Tests.NUnit
{
    public class GeometryUtils 
    {
	    //TODO: allow specifying GeometryFactory
	    public static WKTReader reader = new WKTReader();
	
        public static IList<IGeometry> ReadWKT(String[] inputWKT)
        {
            var geometries = new List<IGeometry>();
            foreach (var geomWkt in inputWKT)
            {
                geometries.Add(reader.Read(geomWkt));
            }
            return geometries;
        }
  
        public static IGeometry ReadWKT(String inputWKT)
        {
            return reader.Read(inputWKT);
        }

        public static IList<IGeometry> ReadWKTFile(String filename) 
        {
#if !PCL
            var fileRdr = new WKTFileReader(filename, reader);
#else
            var fileRdr = new WKTFileReader(new BufferedStream(new FileStream(filename, FileMode.Open),2048) , reader);
#endif
            var geoms = fileRdr.Read();
            return geoms;
        }

#if PCL
        public static IList<IGeometry> ReadWKTFile(Stream stream)
        {
            var fileRdr = new WKTFileReader(stream, new WKTReader());
            var geoms = fileRdr.Read();
            return geoms;
        }
#endif
  
        public static bool IsEqual(IGeometry a, IGeometry b)
        {
            IGeometry a2 = Normalize(a);
            IGeometry b2 = Normalize(b);
            return a2.EqualsExact(b2);
        }
  
        public static IGeometry Normalize(IGeometry g)
        {
            Geometry g2 = (Geometry) g.Clone();
            g2.Normalize();
            return g2;
        }
    }
}
