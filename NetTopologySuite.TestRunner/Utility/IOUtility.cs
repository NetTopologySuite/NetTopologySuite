using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace Open.Topology.TestRunner.Utility
{
    public class IOUtility
    {
        public static IGeometry ReadGeometriesFromFile(String filename, IGeometryFactory geomFact)
        {
            var ext = Path.GetExtension(filename);
            if (string.Equals(ext, ".shp", StringComparison.CurrentCultureIgnoreCase))
                return ReadGeometriesFromShapefile(filename, geomFact);
            if (string.Equals(ext, ".wkb", StringComparison.CurrentCultureIgnoreCase))
                return ReadGeometryFromWKBHexFile(filename, geomFact);
            return ReadGeometriesFromWktFile(filename, geomFact);
        }

        private static IGeometry ReadGeometriesFromShapefile(String filename, IGeometryFactory geomFact)
        {
            var shpfile = new ShapefileReader(filename, geomFact);
            var result = shpfile.ReadAll();
            return result;
        }

        private static IGeometry ReadGeometryFromWKBHexFile(String filename, IGeometryFactory geomFact)
        {
            return ReadGeometryFromWkbHexString(File.OpenText(filename).ReadToEnd(), geomFact);
        }

        private static IGeometry ReadGeometryFromWkbHexString(String wkbHexFile, IGeometryFactory geomFact)
        {
            var reader = new WKBReader();
            var wkbHex = CleanHex(wkbHexFile);
            return reader.Read(WKBReader.HexToBytes(wkbHex));
        }

        private static String CleanHex(String hexStuff)
        {
            return System.Text.RegularExpressions.Regex.Replace(hexStuff, "[^0123456789ABCDEFabcdef]", "");
        }

        private static IGeometry ReadGeometriesFromWktFile(String filename, IGeometryFactory geomFact)
        {
            return ReadGeometriesFromWktString(File.OpenText(filename).ReadToEnd(), geomFact);
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

        public static IGeometry ReadGeometriesFromWktString(String wkt, IGeometryFactory geomFact)
        {
            var reader = new WKTReader(geomFact);
            WKTFileReader fileReader = new WKTFileReader(new StringReader(wkt), reader);
            var geomList = fileReader.Read();

            if (geomList.Count == 1)
                return geomList[0];

            return geomFact.CreateGeometryCollection(GeometryFactory.ToGeometryArray(geomList));
        }

        public static IGeometry ReadGeometriesFromWkbHexString(String wkb, IGeometryFactory geomFact)
        {
            var reader = new WKBReader(geomFact);
            var fileReader = new WKBHexFileReader(reader);
            var geomList = new List<IGeometry>();
            using (var ms = new MemoryStream())
            {
                new StreamWriter(ms).Write(wkb);
                geomList.AddRange(fileReader.Read(ms));
            }

            if (geomList.Count == 1)
                return geomList[1];

            return geomFact.CreateGeometryCollection(GeometryFactory.ToGeometryArray(geomList));
        }
    }
}