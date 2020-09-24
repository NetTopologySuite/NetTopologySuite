using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    public class OverlayNGStrictModeTest : GeometryTestCase
    {
        [Test]
        public void TestPolygonTouchALPIntersection()
        {
            var a = Read("POLYGON ((10 10, 10 30, 30 30, 30 10, 10 10))");
            var b = Read("POLYGON ((40 10, 30 10, 35 15, 30 15, 30 20, 35 20, 25 30, 40 30, 40 10))");
            var expected = Read("POLYGON ((30 25, 25 30, 30 30, 30 25))");
            var actual = Intersection(a, b);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestPolygonTouchALIntersection()
        {
            var a = Read("POLYGON ((10 30, 60 30, 60 10, 10 10, 10 30))");
            var b = Read("POLYGON ((10 50, 60 50, 60 30, 30 30, 10 10, 10 50))");
            var expected = Read("POLYGON ((30 30, 10 10, 10 30, 30 30))");
            var actual = Intersection(a, b);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestPolygonTouchLPIntersection()
        {
            var a = Read("POLYGON ((10 10, 10 30, 30 30, 30 10, 10 10))");
            var b = Read("POLYGON ((40 25, 30 25, 30 20, 35 20, 30 15, 40 15, 40 25))");
            var expected = Read("LINESTRING (30 25, 30 20)");
            var actual = Intersection(a, b);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestLineTouchLPIntersection()
        {
            var a = Read("LINESTRING (10 10, 20 10, 20 20, 30 10)");
            var b = Read("LINESTRING (10 10, 30 10)");
            var expected = Read("LINESTRING (10 10, 20 10)");
            var actual = Intersection(a, b);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestPolygonResultMixedIntersection()
        {
            var a = Read("POLYGON ((10 30, 60 30, 60 10, 10 10, 10 30))");
            var b = Read("POLYGON ((10 50, 60 50, 60 30, 30 30, 10 10, 10 50))");
            var expected = Read("POLYGON ((30 30, 10 10, 10 30, 30 30))");
            var actual = Intersection(a, b);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestPolygonResultLineIntersection()
        {
            var a = Read("POLYGON ((10 20, 20 20, 20 10, 10 10, 10 20))");
            var b = Read("POLYGON ((30 20, 30 10, 20 10, 20 20, 30 20))");
            var expected = Read("LINESTRING (20 20, 20 10)");
            var actual = Intersection(a, b);
            CheckEqual(expected, actual);
        }

        [Test, Description("Symmetric Difference is the one exception to the Strict Mode homogeneous output rule.")]
        public void TestPolygonLineSymDifference()
        {
            var a = Read("POLYGON ((10 20, 20 20, 20 10, 10 10, 10 20))");
            var b = Read("LINESTRING (15 15, 25 15)");
            var expected =
                Read(
                    "GEOMETRYCOLLECTION (POLYGON ((20 20, 20 15, 20 10, 10 10, 10 20, 20 20)), LINESTRING (20 15, 25 15))");
            var actual = SymDifference(a, b);
            CheckEqual(expected, actual);
        }

        [Test]
        public void testPolygonLineUnion()
        {
            var a = Read("POLYGON ((10 20, 20 20, 20 10, 10 10, 10 20))");
            var b = Read("LINESTRING (15 15, 25 15)");
            var expected = Read("GEOMETRYCOLLECTION (POLYGON ((20 20, 20 15, 20 10, 10 10, 10 20, 20 20)), LINESTRING (20 15, 25 15))");
            var actual = Union(a, b);
            CheckEqual(expected, actual);
        }

        [Test, Description("Check that result does not include collapsed line intersection")]
        public void TestPolygonIntersectionCollapse()
        {
            var a = Read("POLYGON ((1 1, 1 5, 3 5, 3 2, 9 1, 1 1))");
            var b = Read("POLYGON ((7 5, 9 5, 9 1, 7 1, 7 5))");
            var expected = Read("POLYGON EMPTY");
            var actual = Intersection(a, b, 1);
            CheckEqual(expected, actual);
        }

        [Test]
        public void TestPolygonUnionCollapse()
        {
            var a = Read("POLYGON ((1 1, 1 5, 3 5, 3 1.4, 7 1, 1 1))");
            var b = Read("POLYGON ((7 5, 9 5, 9 1, 7 1, 7 5))");
            var expected = Read("MULTIPOLYGON (((1 1, 1 5, 3 5, 3 1, 1 1)), ((7 1, 7 5, 9 5, 9 1, 7 1)))");
            var actual = Union(a, b, 1);
            CheckEqual(expected, actual);
        }

        static Geometry Intersection(Geometry a, Geometry b)
        {
            var ov = new NetTopologySuite.Operation.OverlayNG.OverlayNG(a, b, SpatialFunction.Intersection);
            ov.StrictMode = true;
            return ov.GetResult();
        }
        static Geometry Intersection(Geometry a, Geometry b, double scaleFactor)
        {
            return Overlay(a, b, scaleFactor, SpatialFunction.Intersection);
        }


        static Geometry SymDifference(Geometry a, Geometry b)
        {
            var ov = new NetTopologySuite.Operation.OverlayNG.OverlayNG(a, b, SpatialFunction.SymDifference);
            ov.StrictMode = true;
            return ov.GetResult();
        }

        static Geometry Union(Geometry a, Geometry b)
        {
            return Overlay(a, b, SpatialFunction.Union);
        }

        static Geometry Union(Geometry a, Geometry b, double scaleFactor)
        {
            return Overlay(a, b, scaleFactor, SpatialFunction.Union);
        }

        static Geometry Overlay(Geometry a, Geometry b, SpatialFunction opCode)
        {
            var ov = new NetTopologySuite.Operation.OverlayNG.OverlayNG(a, b, opCode);
            ov.StrictMode = true;
            return ov.GetResult();
        }

        static Geometry Overlay(Geometry a, Geometry b, double scaleFactor, SpatialFunction opCode)
        {
            var pm = new PrecisionModel(scaleFactor);
            var ov = new NetTopologySuite.Operation.OverlayNG.OverlayNG(a, b, pm, opCode);
            ov.StrictMode = true;
            return ov.GetResult();
        }
    }

}
