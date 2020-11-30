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
            CheckWKB("POINT EMPTY", ByteOrder.LittleEndian, false, "0101000000000000000000F8FF000000000000F8FF");
        }

        [Test]
        public void TestPointEmpty3D()
        {
            CheckWKB("POINT EMPTY", ByteOrder.LittleEndian, true,
                //JTS test value: "0101000080000000000000F87F000000000000F87F000000000000F87F");
                "01E9030080000000000000F8FF000000000000F8FF000000000000F8FF");
        }

        void CheckWKB(string wkt, ByteOrder byteOrder, bool emitZ, string expectedWKBHex)
        {
            var geom = Read(wkt);
            var wkbWriter = new WKBWriter(byteOrder, false, emitZ);
            wkbWriter.Strict = false;
            byte[] wkb = wkbWriter.Write(geom);
            string wkbHex = WKBWriter.ToHex(wkb);

            Assert.AreEqual(expectedWKBHex, wkbHex);
        }
    }

}
