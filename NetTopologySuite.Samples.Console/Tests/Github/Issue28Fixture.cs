using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm.Distance;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    [TestFixture]
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
                Assert.IsTrue(DiscreteHausdorffDistance.Distance(wktGeom, wkbGeom) < 1e-9, number + ": DiscreteHausdorffDistance.Distance(wktGeom, wkbGeom) < 1e-9");
                if (!wktGeom.EqualsExact(wkbGeom))
                {
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
    }
}