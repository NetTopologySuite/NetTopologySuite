using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Precision;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Precision
{
    [TestFixture]
    public class GeometryPrecisionReducerTest : GeometryTestCase
    {
        private PrecisionModel pmFloat;
        private PrecisionModel pmFixed1;
        private GeometryPrecisionReducer reducer;
        private GeometryPrecisionReducer reducerKeepCollapse;

        private GeometryFactory gfFloat;
        WKTReader reader;

        public GeometryPrecisionReducerTest()
        {
            pmFloat = new PrecisionModel();
            pmFixed1 = new PrecisionModel(1);
            reducer = new GeometryPrecisionReducer(pmFixed1);
            reducerKeepCollapse = new GeometryPrecisionReducer(pmFixed1);

            gfFloat = new GeometryFactory(pmFloat, 0);
            reader = new WKTReader(gfFloat);

            reducerKeepCollapse.RemoveCollapsedComponents = false;
        }

        [Test]
        public void TestSquare()
        {

            CheckReduceSameFactory("POLYGON (( 0 0, 0 1.4, 1.4 1.4, 1.4 0, 0 0 ))",
                "POLYGON (( 0 0, 0 1, 1 1, 1 0, 0 0 ))");
        }

        [Test]
        public void TestTinySquareCollapse()

        {
            CheckReduceSameFactory("POLYGON (( 0 0, 0 .4, .4 .4, .4 0, 0 0 ))",
                "POLYGON EMPTY");
        }

        [Test]
        public void TestSquareCollapse()

        {
            CheckReduceSameFactory("POLYGON (( 0 0, 0 1.4, .4 .4, .4 0, 0 0 ))",
                "POLYGON EMPTY");
        }

        [Test]
        public void TestSquareKeepCollapse()

        {
            CheckReduceSameFactory("POLYGON (( 0 0, 0 1.4, .4 .4, .4 0, 0 0 ))",
                "POLYGON EMPTY");
        }

        [Test]
        public void TestLine()

        {
            CheckReduceExactSameFactory("LINESTRING ( 0 0, 0 1.4 )",
                "LINESTRING (0 0, 0 1)");
        }

        [Test]
        public void TestLineNotNoded()

        {
            CheckReduceExactSameFactory("LINESTRING(1 1, 3 3, 9 9, 5.1 5, 2.1 2)",
                "LINESTRING(1 1, 3 3, 9 9, 5 5, 2 2)");
        }

        [Test]
        public void TestLineRemoveCollapse()

        {
            CheckReduceExactSameFactory("LINESTRING ( 0 0, 0 .4 )",
                "LINESTRING EMPTY");
        }

        [Test]
        public void TestLineKeepCollapse()

        {
            CheckReduceExactSameFactory(reducerKeepCollapse,
                "LINESTRING ( 0 0, 0 .4 )",
                "LINESTRING ( 0 0, 0 0 )");
        }

        [Test]
        public void TestPoint()

        {
            CheckReduceExactSameFactory("POINT(1.1 4.9)",
                "POINT(1 5)");
        }

        [Test]
        public void TestMultiPoint()

        {
            CheckReduceExactSameFactory("MULTIPOINT( (1.1 4.9),(1.2 4.8), (3.3 6.6))",
                "MULTIPOINT((1 5), (1 5), (3 7))");
        }

        [Test]
        public void TestPolgonWithCollapsedLine()
        {
            CheckReduceSameFactory("POLYGON ((10 10, 100 100, 200 10.1, 300 10, 10 10))",
                "POLYGON ((10 10, 100 100, 200 10, 10 10))");
        }

        [Test]
        public void TestPolgonWithCollapsedLinePointwise()
        {
            var g = reader.Read("POLYGON ((10 10, 100 100, 200 10.1, 300 10, 10 10))");
            var g2 = reader.Read("POLYGON ((10 10, 100 100, 200 10,   300 10, 10 10))");
            var gReduce = GeometryPrecisionReducer.ReducePointwise(g, pmFixed1);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
        }

        [Test]
        public void TestPolgonWithCollapsedPoint()
        {
            CheckReduceSameFactory("POLYGON ((10 10, 100 100, 200 10.1, 300 100, 400 10, 10 10))",
                "MULTIPOLYGON (((10 10, 100 100, 200 10, 10 10)), ((200 10, 300 100, 400 10, 200 10)))");
        }

        [Test]
        public void TestPolgonWithCollapsedPointPointwise()
        {
            var g = reader.Read("POLYGON ((10 10, 100 100, 200 10.1, 300 100, 400 10, 10 10))");
            var g2 = reader.Read("POLYGON ((10 10, 100 100, 200 10,   300 100, 400 10, 10 10))");
            var gReduce = GeometryPrecisionReducer.ReducePointwise(g, pmFixed1);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
        }

        private static void AssertEqualsExactAndHasSameFactory(Geometry a, Geometry b)
        {
            Assert.IsTrue(a.EqualsExact(b));
            Assert.IsTrue(a.Factory == b.Factory);
        }

        private void CheckReduceExactSameFactory(string wkt, string wktExpected)
        {
            CheckReduceExactSameFactory(reducer, wkt, wktExpected);
        }

        private void CheckReduceExactSameFactory(GeometryPrecisionReducer reducer,
            string wkt,
            string wktExpected)
        {
            var g = Read(wkt);
            var expected = Read(wktExpected);
            var actual = reducer.Reduce(g);
            Assert.That(actual.EqualsExact(expected), Is.True);
            Assert.That(expected.Factory, Is.EqualTo(expected.Factory));
        }

        private void CheckReduceSameFactory(
            string wkt,
            string wktExpected)
        {
            var g = Read(wkt);
            var expected = Read(wktExpected);
            var actual = reducer.Reduce(g);
            CheckEqual(expected, actual);
            Assert.That(expected.Factory, Is.EqualTo(expected.Factory));
        }

    }
}
