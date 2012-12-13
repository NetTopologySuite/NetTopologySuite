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
        public static IGeometry readGeometriesFromFile(String filename, IGeometryFactory geomFact)
        {
            var ext = Path.GetExtension(filename);
            if (string.Equals(ext, ".shp", StringComparison.CurrentCultureIgnoreCase))
                return readGeometriesFromShapefile(filename, geomFact);
            if (string.Equals(ext, ".wkb", StringComparison.CurrentCultureIgnoreCase))
                return readGeometryFromWKBHexFile(filename, geomFact);
            return readGeometriesFromWKTFile(filename, geomFact);
        }

        private static IGeometry readGeometriesFromShapefile(String filename, IGeometryFactory geomFact)
        {
            var shpfile = new ShapefileReader(filename, geomFact);
            var result = shpfile.ReadAll();
            return result;
        }

        private static IGeometry readGeometryFromWKBHexFile(String filename, IGeometryFactory geomFact)
        {
            return readGeometryFromWKBHexString(File.OpenText(filename).ReadToEnd(), geomFact);
        }

        private static IGeometry readGeometryFromWKBHexString(String wkbHexFile, IGeometryFactory geomFact)
        {
            var reader = new WKBReader();
            var wkbHex = cleanHex(wkbHexFile);
            return reader.Read(WKBReader.HexToBytes(wkbHex));
        }

        private static String cleanHex(String hexStuff)
        {
            return System.Text.RegularExpressions.Regex.Replace(hexStuff, "[^0123456789ABCDEFabcdef]", "");
        }

        private static IGeometry readGeometriesFromWKTFile(String filename, IGeometryFactory geomFact)
        {
            return readGeometriesFromWKTString(File.OpenText(filename).ReadToEnd(), geomFact);
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

        public static IGeometry readGeometriesFromWKTString(String wkt, IGeometryFactory geomFact)
        {
            var reader = new WKTReader(geomFact);
            WKTFileReader fileReader = new WKTFileReader(new StringReader(wkt), reader);
            var geomList = fileReader.Read();

            if (geomList.Count == 1)
                return geomList[0];

            return geomFact.CreateGeometryCollection(GeometryFactory.ToGeometryArray(geomList));
        }

        public static IGeometry readGeometriesFromWKBHexString(String wkb, IGeometryFactory geomFact)
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