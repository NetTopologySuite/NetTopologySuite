using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    /*
     * Test named predicate short-circuits
     */
    [TestFixture]
    public class PredicateShortCircuitTest
    {
        WKTReader rdr = new WKTReader();

        string[] polyInsidePoly =
        { "POLYGON (( 0 0, 100 0, 100 100, 0 100, 0 0 ))",
          "POLYGON (( 10 10, 90 10, 90 90, 10 90, 10 10 ))" };
        string[] polyPartiallyOverlapsPoly =
        { "POLYGON (( 10 10, 100 10, 100 100, 10 100, 10 10 ))",
          "POLYGON (( 0 0, 90 0, 90 90, 0 90, 0 0 ))" };
        string[] polyTouchesPolyAtPoint =
        { "POLYGON (( 10 10, 100 10, 100 100, 10 100, 10 10 ))",
          "POLYGON (( 0 0, 10 0, 10 10, 0 10, 0 0 ))" };
        string[] polyTouchesPolyAtLine =
        { "POLYGON (( 10 10, 100 10, 100 100, 10 100, 10 10 ))",
          "POLYGON (( 10 0, 10 10, 20 10, 20 0, 10 0 ))" };
        string[] polyInsideHoleInPoly =
        { "POLYGON (( 40 40, 40 60, 60 60, 60 40, 40 40 ))",
          "POLYGON (( 0 0, 100 0, 100 100, 0 100, 0 0), ( 10 10, 90 10, 90 90, 10 90, 10 10))" };

        [Test]
        public void TestAll()
        {
            DoPredicates(polyInsidePoly);
            DoPredicates(polyPartiallyOverlapsPoly);
            DoPredicates(polyTouchesPolyAtPoint);
            DoPredicates(polyTouchesPolyAtLine);
            DoPredicates(polyInsideHoleInPoly);
        }

        public void DoPredicates(string[] wkt)
        {
            var a = rdr.Read(wkt[0]);
            var b = rdr.Read(wkt[1]);
            DoPredicates(a, b);
            DoPredicates(b, a);
        }

        public void DoPredicates(Geometry a, Geometry b)
        {
            var im = a.Relate(b);
            Assert.IsTrue(a.Contains(b) == im.IsContains());
            Assert.IsTrue(a.Crosses(b) == im.IsCrosses(a.Dimension, b.Dimension));
            Assert.IsTrue(a.Disjoint(b) == im.IsDisjoint());
            Assert.IsTrue(a.Equals(b) == im.IsEquals(a.Dimension, b.Dimension));
            Assert.IsTrue(a.Intersects(b) == im.IsIntersects());
            Assert.IsTrue(a.Overlaps(b) == im.IsOverlaps(a.Dimension, b.Dimension));
            Assert.IsTrue(a.Touches(b) == im.IsTouches(a.Dimension, b.Dimension));
            Assert.IsTrue(a.Within(b) == im.IsWithin());
        }
    }
}
