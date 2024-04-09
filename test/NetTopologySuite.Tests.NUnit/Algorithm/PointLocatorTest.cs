using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public class PointLocatorTest
    {
        private WKTReader reader = new WKTReader();

        [Test]
        public void TestBox()
        {
            RunPtLocator(Location.Interior, new Coordinate(10, 10),
                "POLYGON ((0 0, 0 20, 20 20, 20 0, 0 0))");
        }

        [Test]
        public void TestComplexRing()
        {
            RunPtLocator(Location.Interior, new Coordinate(0, 0),
                "POLYGON ((-40 80, -40 -80, 20 0, 20 -100, 40 40, 80 -80, 100 80, 140 -20, 120 140, 40 180,     60 40, 0 120, -20 -20, -40 80))");
        }

        [Test]
        public void TestLinearRingLineString()
        {
            RunPtLocator(Location.Boundary, new Coordinate(0, 0),
                "GEOMETRYCOLLECTION( LINESTRING(0 0, 10 10), LINEARRING(10 10, 10 20, 20 10, 10 10))");
        }

        [Test]
        public void TestPointInsideLinearRing()
        {
            RunPtLocator(Location.Exterior, new Coordinate(11, 11),
                "LINEARRING(10 10, 10 20, 20 10, 10 10)");
        }

        [Test]
        public void TestPolygon() 
        {
            var pointLocator = new PointLocator();
            var polygon = reader.Read("POLYGON ((70 340, 430 50, 70 50, 70 340))");
            Assert.That(pointLocator.Locate(new Coordinate(420, 340), polygon), Is.EqualTo(Location.Exterior));
            Assert.That(pointLocator.Locate(new Coordinate(350, 50), polygon), Is.EqualTo(Location.Boundary));
            Assert.That(pointLocator.Locate(new Coordinate(410, 50), polygon), Is.EqualTo(Location.Boundary));
            Assert.That(pointLocator.Locate(new Coordinate(190, 150), polygon), Is.EqualTo(Location.Interior));
        }

        [Test]
        public void TestRingBoundaryNodeRule()
        {
            string wkt = "LINEARRING(10 10, 10 20, 20 10, 10 10)";
            var pt = new Coordinate(10, 10);
            RunPtLocator(Location.Interior, pt, wkt, BoundaryNodeRules.Mod2BoundaryRule);
            RunPtLocator(Location.Boundary, pt, wkt, BoundaryNodeRules.EndpointBoundaryRule);
            RunPtLocator(Location.Interior, pt, wkt, BoundaryNodeRules.MonoValentEndpointBoundaryRule);
            RunPtLocator(Location.Boundary, pt, wkt, BoundaryNodeRules.MultivalentEndpointBoundaryRule);
        }

        private void RunPtLocator(Location expected, Coordinate pt, string wkt)
        {
            var geom = reader.Read(wkt);
            var pointLocator = new PointLocator();
            var loc = pointLocator.Locate(pt, geom);
            Assert.AreEqual(expected, loc);
        }

        private void RunPtLocator(Location expected, Coordinate pt, string wkt,
            IBoundaryNodeRule bnr)
        {
            var geom = reader.Read(wkt);
            var pointLocator = new PointLocator(bnr);
            var loc = pointLocator.Locate(pt, geom);
            Assert.AreEqual(expected, loc);
        }
    }
}
