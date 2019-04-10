using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Precision;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Precision
{
    [TestFixture]
    public class GeometryPrecisionReducerTest
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
            var g = reader.Read("POLYGON (( 0 0, 0 1.4, 1.4 1.4, 1.4 0, 0 0 ))");
            var g2 = reader.Read("POLYGON (( 0 0, 0 1, 1 1, 1 0, 0 0 ))");
            var gReduce = reducer.Reduce(g);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
        }

        [Test]
        public void TestTinySquareCollapse()
        {
            var g = reader.Read("POLYGON (( 0 0, 0 .4, .4 .4, .4 0, 0 0 ))");
            var g2 = reader.Read("POLYGON EMPTY");
            var gReduce = reducer.Reduce(g);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
        }

        [Test]
        public void TestSquareCollapse()
        {
            var g = reader.Read("POLYGON (( 0 0, 0 1.4, .4 .4, .4 0, 0 0 ))");
            var g2 = reader.Read("POLYGON EMPTY");
            var gReduce = reducer.Reduce(g);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
        }

        [Test]
        public void TestSquareKeepCollapse()
        {
            var g = reader.Read("POLYGON (( 0 0, 0 1.4, .4 .4, .4 0, 0 0 ))");
            var g2 = reader.Read("POLYGON EMPTY");
            var gReduce = reducerKeepCollapse.Reduce(g);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
        }

        [Test]
        public void TestLine()
        {
            var g = reader.Read("LINESTRING ( 0 0, 0 1.4 )");
            var g2 = reader.Read("LINESTRING (0 0, 0 1)");
            var gReduce = reducer.Reduce(g);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
        }

        [Test]
        public void TestLineRemoveCollapse()
        {
            var g = reader.Read("LINESTRING ( 0 0, 0 .4 )");
            var g2 = reader.Read("LINESTRING EMPTY");
            var gReduce = reducer.Reduce(g);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
        }

        [Test]
        public void TestLineKeepCollapse()
        {
            var g = reader.Read("LINESTRING ( 0 0, 0 .4 )");
            var g2 = reader.Read("LINESTRING ( 0 0, 0 0 )");
            var gReduce = reducerKeepCollapse.Reduce(g);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
        }

        [Test]
        public void TestPolgonWithCollapsedLine()
        {
            var g = reader.Read("POLYGON ((10 10, 100 100, 200 10.1, 300 10, 10 10))");
            var g2 = reader.Read("POLYGON ((10 10, 100 100, 200 10, 10 10))");
            var gReduce = reducer.Reduce(g);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
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
            var g = reader.Read("POLYGON ((10 10, 100 100, 200 10.1, 300 100, 400 10, 10 10))");
            var g2 = reader.Read("MULTIPOLYGON (((10 10, 100 100, 200 10, 10 10)), ((200 10, 300 100, 400 10, 200 10)))");
            var gReduce = reducer.Reduce(g);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
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

    }
}