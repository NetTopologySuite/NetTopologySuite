using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm.Distance;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Planargraph;
using NUnit.Framework;
namespace NetTopologySuite.Samples.Tests.Github
{
    [TestFixture, Ignore("Problem in Framework")]
    public class Issue28Fixture
    {
        [Test]
        public void test_wkt_wkb_result()
        {
            var failed = TestWktWkb(0, GeometryFactory.Default, Issue27Fixture.Poly1Wkt, Issue27Fixture.Poly1Wkb);
            failed |= TestWktWkb(1, GeometryFactory.Default, Issue27Fixture.Poly2Wkt, Issue27Fixture.Poly2Wkb);
            Assert.IsFalse(failed);
        }
        private static bool TestWktWkb(int number, IGeometryFactory factory, string wkt, string wkb)
        {
            WKTReader r = new WKTReader(factory);
            IGeometry wktGeom = r.Read(wkt);
            WKBReader s = new WKBReader(factory);
            IGeometry wkbGeom = s.Read(WKBReader.HexToBytes(wkb));
            try
            {
                Assert.AreEqual(wkb, WKBWriter.ToHex(wktGeom.AsBinary()), "wkb's don't match");
                Assert.IsTrue(DiscreteHausdorffDistance.Distance(wktGeom, wkbGeom) < 1e-9, number + ": DiscreteHausdorffDistance.Distance(wktGeom, wkbGeom) < 1e-9");
                if (!wktGeom.EqualsExact(wkbGeom))
                {
                    Assert.AreEqual(wkt, wktGeom.AsText(), number + ": wkt.Equals(wktGeom.AsText())");
                    var wktGeom2 = s.Read(wktGeom.AsBinary());
                    Assert.AreEqual(wkt, wktGeom2.AsText(), number + ": wkt.Equals(wktGeom2.AsText())");
                    var diff = wkbGeom.Difference(wktGeom);
                    Assert.IsTrue(false, number + ": wktGeom.EqualsExact(wkbGeom)\n" + diff.AsText());
                }
                return false;
            }
            catch (AssertionException ex)
            {
                Console.WriteLine(ex.Message);
                return true;
            }
        }
        [Test]
        public void TestNumber()
        {
            const string theNumberString = "6232756.00054126";
            const double theNumber = 6232756.00054126; //6232756.0005412595;
            var theBytes = BitConverter.GetBytes(theNumber);
            Console.WriteLine("{0:R} -> 0x{1}", theNumber, WKBWriter.ToHex(theBytes));
            var theWkbedNumber = BitConverter.ToDouble(theBytes,0);
            Console.WriteLine("0x{1} -> {0:R}", theWkbedNumber, WKBWriter.ToHex(theBytes));
           //The result of JTS
            theBytes[0] = (byte)(theBytes[0] + 1);
            theWkbedNumber = BitConverter.ToDouble(theBytes, 0);
            Console.WriteLine("0x{1} -> {0:R}", theWkbedNumber, WKBWriter.ToHex(theBytes));
            Assert.AreEqual(theNumber, theWkbedNumber);
        }
    }
}
