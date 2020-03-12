using System.Collections.Generic;
using System.IO;
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

        public static IList<Geometry> ReadWKT(IList<string> inputWKT)
        {
            var geometries = new List<Geometry>();
            for (int i = 0; i < inputWKT.Count; i++)
            {
                geometries.Add(IOUtil.Reader.Read(inputWKT[i]));
            }

            return geometries;
        }

        public static Geometry ReadWKT(string inputWKT)
        {
            return IOUtil.Reader.Read(inputWKT);
        }

        public static IList<Geometry> ReadWKTFile(string filename)
        {
            var fileRdr = new WKTFileReader(filename, IOUtil.Reader);
            var geoms = fileRdr.Read();
            return geoms;
        }

        public static IList<Geometry> ReadWKTFile(TextReader rdr)
        {
            var fileRdr = new WKTFileReader(rdr, IOUtil.Reader);
            var geoms = fileRdr.Read();
            return geoms;
        }

        public static WKTReader Reader { get; } = new WKTReader();

    }

}
