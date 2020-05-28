using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    public class OverlayNGPointsTest : GeometryTestCase
    {
        [Test]
        public void TestSimpleIntersection()
        {
            var a = Read("MULTIPOINT ((1 1), (2 1))");
            var b = Read("POINT (2 1)");
            var expected = Read("POINT (2 1)");
            CheckEqual(expected, OverlayNGTest.Intersection(a, b, 1));
        }

        [Test]
        public void TestSimpleMergeIntersection()
        {
            var a = Read("MULTIPOINT ((1 1), (1.5 1.1), (2 1), (2.1 1.1))");
            var b = Read("POINT (2 1)");
            var expected = Read("POINT (2 1)");
            CheckEqual(expected, OverlayNGTest.Intersection(a, b, 1));
        }

        [Test]
        public void TestSimpleUnion()
        {
            var a = Read("MULTIPOINT ((1 1), (2 1))");
            var b = Read("POINT (2 1)");
            var expected = Read("MULTIPOINT ((1 1), (2 1))");
            CheckEqual(expected, OverlayNGTest.Union(a, b, 1));
        }

        [Test]
        public void TestSimpleDifference()
        {
            var a = Read("MULTIPOINT ((1 1), (2 1))");
            var b = Read("POINT (2 1)");
            var expected = Read("POINT (1 1)");
            CheckEqual(expected, OverlayNGTest.Difference(a, b, 1));
        }

        [Test]
        public void TestSimpleSymDifference()
        {
            var a = Read("MULTIPOINT ((1 2), (1 1), (2 2), (2 1))");
            var b = Read("MULTIPOINT ((2 2), (2 1), (3 2), (3 1))");
            var expected = Read("MULTIPOINT ((1 2), (1 1), (3 2), (3 1))");
            CheckEqual(expected, OverlayNGTest.SymDifference(a, b, 1));
        }

        [Test]
        public void TestSimpleFloatUnion()
        {
            var a = Read("MULTIPOINT ((1 1), (1.5 1.1), (2 1), (2.1 1.1))");
            var b = Read("MULTIPOINT ((1.5 1.1), (2 1), (2 1.2))");
            var expected = Read("MULTIPOINT ((1 1), (1.5 1.1), (2 1), (2 1.2), (2.1 1.1))");
            CheckEqual(expected, OverlayNGTest.Union(a, b));
        }

        [Test]
        public void TestDisjointPointsRoundedIntersection()
        {
            var a = Read("POINT (10.1 10)");
            var b = Read("POINT (10 10.1)");
            var expected = Read("POINT (10 10)");
            CheckEqual(expected, OverlayNGTest.Intersection(a, b, 1));
        }

        [Test]
        public void TestEmptyIntersection()
        {
            var a = Read("MULTIPOINT ((1 1), (3 1))");
            var b = Read("POINT (2 1)");
            var expected = Read("POINT EMPTY");
            CheckEqual(expected, OverlayNGTest.Intersection(a, b, 1));
        }

        [Test]
        public void TestEmptyInputIntersection()
        {
            var a = Read("MULTIPOINT ((1 1), (3 1))");
            var b = Read("POINT EMPTY");
            var expected = Read("POINT EMPTY");
            CheckEqual(expected, OverlayNGTest.Intersection(a, b, 1));
        }

        [Test]
        public void TestEmptyInputUUnion()
        {
            var a = Read("MULTIPOINT ((1 1), (3 1))");
            var b = Read("POINT EMPTY");
            var expected = Read("MULTIPOINT ((1 1), (3 1))");
            CheckEqual(expected, OverlayNGTest.Union(a, b, 1));
        }

        [Test]
        public void TestEmptyDifference()
        {
            var a = Read("MULTIPOINT ((1 1), (3 1))");
            var b = Read("MULTIPOINT ((1 1), (2 1), (3 1))");
            var expected = Read("POINT EMPTY");
            CheckEqual(expected, OverlayNGTest.Difference(a, b, 1));
        }
    }
}
