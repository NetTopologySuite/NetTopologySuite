using System.Collections.Generic;
using System.IO;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace Open.Topology.TestRunner.Utility
{
    public class IOUtility
    {
        //public static Geometry ReadGeometriesFromFile(String filename, GeometryFactory geomFact)
        //{
        //    var ext = Path.GetExtension(filename);
        //    if (string.Equals(ext, ".shp", StringComparison.CurrentCultureIgnoreCase))
        //        return ReadGeometriesFromShapefile(filename, geomFact);
        //    if (string.Equals(ext, ".wkb", StringComparison.CurrentCultureIgnoreCase))
        //        return ReadGeometryFromWKBHexFile(filename, geomFact);
        //    return ReadGeometriesFromWktFile(filename, geomFact);
        //}

        //private static Geometry ReadGeometriesFromShapefile(String filename, GeometryFactory geomFact)
        //{
        //    var shpfile = new ShapefileReader(filename, geomFact);
        //    var result = shpfile.ReadAll();
        //    return result;
        //}

        private static Geometry ReadGeometryFromWKBHexFile(string filename, NtsGeometryServices geomServ)
        {
            return ReadGeometryFromWkbHexString(File.OpenText(filename).ReadToEnd(), geomServ);
        }

        private static Geometry ReadGeometryFromWkbHexString(string wkbHexFile, NtsGeometryServices geomServ)
        {
            var reader = new WKBReader(geomServ);
            string wkbHex = CleanHex(wkbHexFile);
            return reader.Read(WKBReader.HexToBytes(wkbHex));
        }

        private static string CleanHex(string hexStuff)
        {
            return System.Text.RegularExpressions.Regex.Replace(hexStuff, "[^0123456789ABCDEFabcdef]", "");
        }

        private static Geometry ReadGeometriesFromWktFile(string filename, NtsGeometryServices geomServ)
        {
            return ReadGeometriesFromWktString(File.OpenText(filename).ReadToEnd(), geomServ);
        }

        /**
         * Reads one or more WKT geometries from a string.
         *
         * @param wkt
         * @param geomFact
         * @return
         * @throws ParseException
         * @throws IOException
         */

        public static Geometry ReadGeometriesFromWktString(string wkt, NtsGeometryServices geomServ)
        {
            var reader = new WKTReader(geomServ);
            var fileReader = new WKTFileReader(new StringReader(wkt), reader);
            var geomList = fileReader.Read();

            if (geomList.Count == 1)
                return geomList[0];

            var geomFact = geomList[0].Factory;
            return geomFact.CreateGeometryCollection(GeometryFactory.ToGeometryArray(geomList));
        }

        public static Geometry ReadGeometriesFromWkbHexString(string wkb, NtsGeometryServices services)
        {
            var reader = new WKBReader(services);
            var fileReader = new WKBHexFileReader(reader);
            var geomList = new List<Geometry>();
            using (var ms = new MemoryStream())
            {
                new StreamWriter(ms).Write(wkb);
                geomList.AddRange(fileReader.Read(ms));
            }

            if (geomList.Count == 1)
                return geomList[1];

            return services.CreateGeometryFactory().CreateGeometryCollection(GeometryFactory.ToGeometryArray(geomList));
        }
    }
}
