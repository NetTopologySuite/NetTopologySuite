using System;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Tests.NUnit.IO
{
    /// <summary>
    /// Tests for reading WKB.
    /// </summary>
    /// <author>Martin Davis</author>
    [TestFixture]
    public class WKBReaderTest
    {
        private static readonly WKTReader Rdr = new WKTReader();

        [Test]
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

        [Test]
        public void TestSinglePointLineString()
        {
            CheckWkbGeometry("00000000020000000140590000000000004069000000000000", "LINESTRING (100 200, 100 200)");
        }

        [Test, Ignore("Not yet implemented satisfactorily.")]
        public void TestIllFormedWKB()
        {
            // WKB is missing LinearRing entry
            CheckWkbGeometry("00000000030000000140590000000000004069000000000000",
                             "POLYGON ((100 200, 100 200, 100 200, 100 200)");
        }


        //private static readonly CoordinateSequenceComparator Comp2 = new CoordinateSequenceComparator(2);

        private static void CheckWkbGeometry(String wkbHex, String expectedWKT)
        {
            var wkbReader = new WKBReader();
            var wkb = WKBReader.HexToBytes(wkbHex);
            var g2 = wkbReader.Read(wkb);

            var expected = Rdr.Read(expectedWKT);

            var isEqual = (expected.CompareTo(g2 /*, Comp2*/) == 0);
            Assert.IsTrue(isEqual);

        }
    }
}