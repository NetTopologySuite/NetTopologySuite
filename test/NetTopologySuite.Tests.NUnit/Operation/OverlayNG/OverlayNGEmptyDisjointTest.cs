using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    using OverlayNG = NetTopologySuite.Operation.OverlayNG.OverlayNG;

    public class OverlayNGEmptyDisjointTest : OverlayNGTestCase
    {
        [Test]
        public void TestEmptyGCBothIntersection()
        {
            var a = Read("GEOMETRYCOLLECTION EMPTY");
            var b = Read("GEOMETRYCOLLECTION EMPTY");
            var expected = Read("GEOMETRYCOLLECTION EMPTY");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestEmptyAPolygonIntersection()
        {
            var a = Read("POLYGON EMPTY");
            var b = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var expected = Read("POLYGON EMPTY");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestEmptyBIntersection()
        {
            var a = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var b = Read("POLYGON EMPTY");
            var expected = Read("POLYGON EMPTY");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestEmptyABIntersection()
        {
            var a = Read("POLYGON EMPTY");
            var b = Read("POLYGON EMPTY");
            var expected = Read("POLYGON EMPTY");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestEmptyADifference()
        {
            var a = Read("POLYGON EMPTY");
            var b = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var expected = Read("POLYGON EMPTY");
            var actual = Difference(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestEmptyAUnion()
        {
            var a = Read("POLYGON EMPTY");
            var b = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var expected = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestEmptyASymDifference()
        {
            var a = Read("POLYGON EMPTY");
            var b = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var expected = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var actual = SymDifference(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestEmptyLinePolygonIntersection()
        {
            var a = Read("LINESTRING EMPTY");
            var b = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var expected = Read("LINESTRING EMPTY");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestEmptyLinePolygonDifference()
        {
            var a = Read("LINESTRING EMPTY");
            var b = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var expected = Read("LINESTRING EMPTY");
            var actual = Difference(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestEmptyPointPolygonIntersection()
        {
            var a = Read("POINT EMPTY");
            var b = Read("POLYGON ((1 0, 2 5, 3 0, 1 0))");
            var expected = Read("POINT EMPTY");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestDisjointIntersection()
        {
            var a = Read("POLYGON ((60 90, 90 90, 90 60, 60 60, 60 90))");
            var b = Read("POLYGON ((200 300, 300 300, 300 200, 200 200, 200 300))");
            var expected = Read("POLYGON EMPTY");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestDisjointIntersectionNoOpt()
        {
            var a = Read("POLYGON ((60 90, 90 90, 90 60, 60 60, 60 90))");
            var b = Read("POLYGON ((200 300, 300 300, 300 200, 200 200, 200 300))");
            var expected = Read("POLYGON EMPTY");
            var actual = IntersectionNoOpt(a, b, 1);
            CheckEqual(expected, actual);
        }

        public static Geometry IntersectionNoOpt(Geometry a, Geometry b, double scaleFactor)
        {
            var pm = new PrecisionModel(scaleFactor);
            var ov = new OverlayNG(a, b, pm, NetTopologySuite.Operation.Overlay.SpatialFunction.Intersection);
            ov.Optimized = false;
            return ov.GetResult();
        }

    }
}
