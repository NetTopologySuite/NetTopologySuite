using System;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO
{
    /// <summary>
    /// Tests for reading WKB.
    /// </summary>
    /// <author>Martin Davis</author>
    [TestFixtureAttribute]
    public class WKBReaderTest
    {
        [TestAttribute]
        public void TestPolygonEmpty()
        {
            WKTReader reader = new WKTReader();
            IGeometry geom = reader.Read("POLYGON EMPTY");
            CheckWkbGeometry(geom.AsBinary(), "POLYGON EMPTY");
        }

        [TestAttribute]
        public void TestShortPolygons()
        {
            // one point
            CheckWkbGeometry("0000000003000000010000000140590000000000004069000000000000",
                             "POLYGON ((100 200, 100 200, 100 200, 100 200))");
            // two point
            CheckWkbGeometry(
                "000000000300000001000000024059000000000000406900000000000040590000000000004069000000000000",
                "POLYGON ((100 200, 100 200, 100 200, 100 200))");
        }

        [TestAttribute]
        public void TestSinglePointLineString()
        {
            CheckWkbGeometry("00000000020000000140590000000000004069000000000000",
                             "LINESTRING (100 200, 100 200)");
        }

        /// <summary>
        /// After removing the 39 bytes of MBR info at the front, and the
        /// end-of-geometry byte, * Spatialite native BLOB is very similar
        /// to WKB, except instead of a endian marker at the start of each
        /// geometry in a multi-geometry, it has a start marker of 0x69.
        /// Endianness is determined by the endian value of the multigeometry.
        /// </summary>
        [TestAttribute]
        public void TestSpatialiteMultiGeometry()
        {
            //multipolygon
            CheckWkbGeometry(
                "01060000000200000069030000000100000004000000000000000000444000000000000044400000000000003440000000000080464000000000008046400000000000003E4000000000000044400000000000004440690300000001000000040000000000000000003E40000000000000344000000000000034400000000000002E40000000000000344000000000000039400000000000003E400000000000003440",
                "MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)), ((30 20, 20 15, 20 25, 30 20)))");

            //multipoint
            CheckWkbGeometry(
                "0104000000020000006901000000000000000000F03F000000000000F03F690100000000000000000000400000000000000040",
                "MULTIPOINT(1 1, 2 2)");

            //multiline
            CheckWkbGeometry(
                "010500000002000000690200000003000000000000000000244000000000000024400000000000003440000000000000344000000000000024400000000000004440690200000004000000000000000000444000000000000044400000000000003E400000000000003E40000000000000444000000000000034400000000000003E400000000000002440",
                "MULTILINESTRING ((10 10, 20 20, 10 40), (40 40, 30 30, 40 20, 30 10))");

            //geometrycollection
            CheckWkbGeometry(
                "010700000002000000690100000000000000000010400000000000001840690200000002000000000000000000104000000000000018400000000000001C400000000000002440",
                "GEOMETRYCOLLECTION(POINT(4 6),LINESTRING(4 6,7 10))");
        }

        [TestAttribute, Ignore("Not yet implemented satisfactorily.")]
        public void TestIllFormedWKB()
        {
            // WKB is missing LinearRing entry
            CheckWkbGeometry("00000000030000000140590000000000004069000000000000",
                             "POLYGON ((100 200, 100 200, 100 200, 100 200)");
        }

        private static void CheckWkbGeometry(String wkbHex, String expectedWKT)
        {
            CheckWkbGeometry(WKBReader.HexToBytes(wkbHex), expectedWKT);
        }

        private static void CheckWkbGeometry(byte[] wkb, String expectedWKT)
        {
            WKBReader wkbReader = new WKBReader();
            IGeometry g2 = wkbReader.Read(wkb);

            WKTReader reader = new WKTReader();
            IGeometry expected = reader.Read(expectedWKT);

            bool isEqual = (expected.CompareTo(g2 /*, Comp2*/) == 0);
            Assert.IsTrue(isEqual);

        }

        [TestAttribute]
        public void TestBase64TextFiles()
        {
            TestBase64TextFile(@"D:\Development\Codeplex.TFS\SharpMap\Branches\1.0\UnitTests\TestData\Base 64.txt");
        }

        private static void TestBase64TextFile(string file)
        {
            if (!File.Exists(file))
            {
                Assert.Ignore("File not present ({0})", file);
                return;
            }

            byte[] wkb = ConvertBase64(file);
            WKBReader wkbReader = new WKBReader();
            IGeometry geom = null;
            Assert.DoesNotThrow(() => geom = wkbReader.Read(wkb));
        }

        private static byte[] ConvertBase64(string file)
        {
            byte[] res = null;
            using (StreamReader sr = new StreamReader(file))
            {
                StringBuilder sb = new StringBuilder(sr.ReadLine());
                while (!sr.EndOfStream)
                    sb.AppendLine(sr.ReadLine());
                res = System.Convert.FromBase64String(sb.ToString());
            }
            return res;
        }
    }
}