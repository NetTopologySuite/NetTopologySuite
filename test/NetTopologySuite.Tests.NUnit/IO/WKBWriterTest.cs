using System;
using System.ComponentModel;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO
{
    /**
     * Tests for WKB which test output explicitly.
     * 
     * @author Martin Davis
     *
     */
    public class WKBWriterTest : GeometryTestCase
    {

        [Test]
        public void TestSRID()
        {
            var gf = new GeometryFactory();
            var p1 = gf.CreatePoint(new Coordinate(1, 2));
            p1.SRID = 1234;

            //first write out without srid set
            var w = new WKBWriter(ByteOrder.BigEndian);
            byte[] wkb = w.Write(p1);

            //check the 3rd bit of the second byte, should be unset
            byte b = (byte) (wkb[1] & 0x20);
            Assert.AreEqual(0, b);

            //read geometry back in
            var r = new WKBReader();
            var p2 = (Point) r.Read(wkb);

            Assert.IsTrue(p1.EqualsExact(p2));
            //NOTE: this differs from JTS-WKBReader, where SRID = 0 when handleSRID = false;
            Assert.AreEqual(-1, p2.SRID);

            //not write out with srid set
            w = new WKBWriter(ByteOrder.BigEndian, true);
            wkb = w.Write(p1);

            //check the 3rd bit of the second byte, should be set
            b = (byte) (wkb[1] & 0x20);
            Assert.AreEqual(0x20, b);

            int srid = ((int) (wkb[5] & 0xff) << 24) | ((int) (wkb[6] & 0xff) << 16) |
                       ((int) (wkb[7] & 0xff) << 8) | ((int) (wkb[8] & 0xff));

            Assert.AreEqual(1234, srid);

            r = new WKBReader();
            p2 = (Point) r.Read(wkb);

            //read the geometry back in
            Assert.IsTrue(p1.EqualsExact(p2));
            Assert.AreEqual(1234, p2.SRID);
        }

        [Test]
        public void TestPointEmpty2D()
        {
            CheckWKB("POINT EMPTY", ByteOrder.LittleEndian, false, 0, "0101000000000000000000F8FF000000000000F8FF");
        }

        [Test]
        public void TestPointEmpty3D()
        {
            CheckWKB("POINT EMPTY", ByteOrder.LittleEndian, true, 0,
                //JTS test value: "0101000080000000000000F87F000000000000F87F000000000000F87F");
                "01E9030080000000000000F8FF000000000000F8FF000000000000F8FF");
        }

        [Test]
        public void TestPolygonEmpty2DSRID()
        {
            CheckWKB("POLYGON EMPTY", ByteOrder.LittleEndian, false, 4326, "0103000020E610000000000000");
        }

        [Test]
        public void TestPolygonEmpty2D()
        {
            CheckWKB("POLYGON EMPTY", false, "010300000000000000");
        }

        [Test]
        public void TestPolygonEmpty3D()
        {
            CheckWKB("POLYGON EMPTY", true, "01EB03008000000000"); //JTS:"010300008000000000"
        }

        [Test]
        public void TestMultiPolygonEmpty2D()
        {
            CheckWKB("MULTIPOLYGON EMPTY", false, "010600000000000000");
        }

        [Test]
        public void TestMultiPolygonEmpty3D()
        {
            CheckWKB("MULTIPOLYGON EMPTY", true, "01EE03008000000000"); // JTS:"010600008000000000"
        }

        [Test]
        public void TestMultiPolygonEmpty2DSRID()
        {
            CheckWKB("MULTIPOLYGON EMPTY", ByteOrder.LittleEndian, false, 4326, "0106000020E610000000000000");
        }

        [Test]
        public void TestMultiPolygon()
        {
            CheckWKB(
                "MULTIPOLYGON(((0 0,0 10,10 10,10 0,0 0),(1 1,1 9,9 9,9 1,1 1)),((-9 0,-9 10,-1 10,-1 0,-9 0)))",
                ByteOrder.LittleEndian,
                false,
                4326,
                "0106000020E61000000200000001030000000200000005000000000000000000000000000000000000000000000000000000000000000000244000000000000024400000000000002440000000000000244000000000000000000000000000000000000000000000000005000000000000000000F03F000000000000F03F000000000000F03F0000000000002240000000000000224000000000000022400000000000002240000000000000F03F000000000000F03F000000000000F03F0103000000010000000500000000000000000022C0000000000000000000000000000022C00000000000002440000000000000F0BF0000000000002440000000000000F0BF000000000000000000000000000022C00000000000000000");
        }

        [Test]
        public void TestGeometryCollection()
        {
            CheckWKB(
                "GEOMETRYCOLLECTION(POINT(0 1),POINT(0 1),POINT(2 3),LINESTRING(2 3,4 5),LINESTRING(0 1,2 3),LINESTRING(4 5,6 7),POLYGON((0 0,0 10,10 10,10 0,0 0),(1 1,1 9,9 9,9 1,1 1)),POLYGON((0 0,0 10,10 10,10 0,0 0),(1 1,1 9,9 9,9 1,1 1)),POLYGON((-9 0,-9 10,-1 10,-1 0,-9 0)))",
                ByteOrder.LittleEndian,
                false,
                4326,
                "0107000020E61000000900000001010000000000000000000000000000000000F03F01010000000000000000000000000000000000F03F01010000000000000000000040000000000000084001020000000200000000000000000000400000000000000840000000000000104000000000000014400102000000020000000000000000000000000000000000F03F000000000000004000000000000008400102000000020000000000000000001040000000000000144000000000000018400000000000001C4001030000000200000005000000000000000000000000000000000000000000000000000000000000000000244000000000000024400000000000002440000000000000244000000000000000000000000000000000000000000000000005000000000000000000F03F000000000000F03F000000000000F03F0000000000002240000000000000224000000000000022400000000000002240000000000000F03F000000000000F03F000000000000F03F01030000000200000005000000000000000000000000000000000000000000000000000000000000000000244000000000000024400000000000002440000000000000244000000000000000000000000000000000000000000000000005000000000000000000F03F000000000000F03F000000000000F03F0000000000002240000000000000224000000000000022400000000000002240000000000000F03F000000000000F03F000000000000F03F0103000000010000000500000000000000000022C0000000000000000000000000000022C00000000000002440000000000000F0BF0000000000002440000000000000F0BF000000000000000000000000000022C00000000000000000");
        }


        [Test]
        public void TestWkbLineStringZM()
        {
            var sLineZM = new GeometryFactory().CreateLineString(new Coordinate[]{new CoordinateZM(1,2,3,4), new CoordinateZM(5,6,7,8)});
            byte[] wkb = new WKBWriter() { HandleOrdinates = Ordinates.XYZM }.Write(sLineZM);

            var dLineZM = (LineString)new WKBReader().Read(wkb);

            Assert.That(dLineZM, Is.EqualTo(sLineZM));
            var sCoords = sLineZM.Coordinates;
            var dCoords = dLineZM.Coordinates;

            Assert.That(dCoords[0].X, Is.EqualTo(sCoords[0].X));
            Assert.That(dCoords[0].Y, Is.EqualTo(sCoords[0].Y));
            Assert.That(dCoords[0].Z, Is.EqualTo(sCoords[0].Z));
            Assert.That(dCoords[0].M, Is.EqualTo(sCoords[0].M));

            Assert.That(dCoords[1].X, Is.EqualTo(sCoords[1].X));
            Assert.That(dCoords[1].Y, Is.EqualTo(sCoords[1].Y));
            Assert.That(dCoords[1].Z, Is.EqualTo(sCoords[1].Z));
            Assert.That(dCoords[1].M, Is.EqualTo(sCoords[1].M));
        }

        void CheckWKB(string wkt, bool emitZ, string expectedWKBHex)
        {
            CheckWKB(wkt, ByteOrder.LittleEndian, emitZ,  -1, expectedWKBHex);
        }

        void CheckWKB(string wkt, ByteOrder byteOrder, bool emitZ, int srid, string expectedWKBHex)
        {
            var geom = Read(wkt);

            // set SRID if not -1
            bool includeSRID = false;
            if (srid > 0)
            {
                includeSRID = true;
                geom.SRID = srid;
            }

            var wkbWriter = new WKBWriter(byteOrder, includeSRID, emitZ) {Strict = false};
            byte[] wkb = wkbWriter.Write(geom);
            string wkbHex = WKBWriter.ToHex(wkb);

            Assert.AreEqual(expectedWKBHex, wkbHex);
        }
    }

}
