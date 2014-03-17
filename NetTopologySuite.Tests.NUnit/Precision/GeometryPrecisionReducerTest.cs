using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Precision;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Precision
{
    [TestFixtureAttribute]
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

        [TestAttribute]
        public void TestSquare()
        {
            IGeometry g = reader.Read("POLYGON (( 0 0, 0 1.4, 1.4 1.4, 1.4 0, 0 0 ))");
            IGeometry g2 = reader.Read("POLYGON (( 0 0, 0 1, 1 1, 1 0, 0 0 ))");
            IGeometry gReduce = reducer.Reduce(g);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
        }

        [TestAttribute]
        public void TestTinySquareCollapse()
        {
            IGeometry g = reader.Read("POLYGON (( 0 0, 0 .4, .4 .4, .4 0, 0 0 ))");
            IGeometry g2 = reader.Read("POLYGON EMPTY");
            IGeometry gReduce = reducer.Reduce(g);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
        }

        [TestAttribute]
        public void TestSquareCollapse()
        {
            IGeometry g = reader.Read("POLYGON (( 0 0, 0 1.4, .4 .4, .4 0, 0 0 ))");
            IGeometry g2 = reader.Read("POLYGON EMPTY");
            IGeometry gReduce = reducer.Reduce(g);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
        }

        [TestAttribute]
        public void TestSquareKeepCollapse()
        {
            IGeometry g = reader.Read("POLYGON (( 0 0, 0 1.4, .4 .4, .4 0, 0 0 ))");
            IGeometry g2 = reader.Read("POLYGON EMPTY");
            IGeometry gReduce = reducerKeepCollapse.Reduce(g);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
        }

        [TestAttribute]
        public void TestLine()
        {
            IGeometry g = reader.Read("LINESTRING ( 0 0, 0 1.4 )");
            IGeometry g2 = reader.Read("LINESTRING (0 0, 0 1)");
            IGeometry gReduce = reducer.Reduce(g);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
        }

        [TestAttribute]
        public void TestLineRemoveCollapse()
        {
            IGeometry g = reader.Read("LINESTRING ( 0 0, 0 .4 )");
            IGeometry g2 = reader.Read("LINESTRING EMPTY");
            IGeometry gReduce = reducer.Reduce(g);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
        }

        [TestAttribute]
        public void TestLineKeepCollapse()
        {
            IGeometry g = reader.Read("LINESTRING ( 0 0, 0 .4 )");
            IGeometry g2 = reader.Read("LINESTRING ( 0 0, 0 0 )");
            IGeometry gReduce = reducerKeepCollapse.Reduce(g);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
        }

        [TestAttribute]
        public void TestPolgonWithCollapsedLine()
        {
            IGeometry g = reader.Read("POLYGON ((10 10, 100 100, 200 10.1, 300 10, 10 10))");
            IGeometry g2 = reader.Read("POLYGON ((10 10, 100 100, 200 10, 10 10))");
            IGeometry gReduce = reducer.Reduce(g);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
        }

        [TestAttribute]
        public void TestPolgonWithCollapsedLinePointwise()
        {
            IGeometry g = reader.Read("POLYGON ((10 10, 100 100, 200 10.1, 300 10, 10 10))");
            IGeometry g2 = reader.Read("POLYGON ((10 10, 100 100, 200 10,   300 10, 10 10))");
            IGeometry gReduce = GeometryPrecisionReducer.ReducePointwise(g, pmFixed1);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
        }

        [TestAttribute]
        public void TestPolgonWithCollapsedPoint()
        {
            IGeometry g = reader.Read("POLYGON ((10 10, 100 100, 200 10.1, 300 100, 400 10, 10 10))");
            IGeometry g2 = reader.Read("MULTIPOLYGON (((10 10, 100 100, 200 10, 10 10)), ((200 10, 300 100, 400 10, 200 10)))");
            IGeometry gReduce = reducer.Reduce(g);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
        }

        [TestAttribute]
        public void TestPolgonWithCollapsedPointPointwise()
        {
            IGeometry g = reader.Read("POLYGON ((10 10, 100 100, 200 10.1, 300 100, 400 10, 10 10))");
            IGeometry g2 = reader.Read("POLYGON ((10 10, 100 100, 200 10,   300 100, 400 10, 10 10))");
            IGeometry gReduce = GeometryPrecisionReducer.ReducePointwise(g, pmFixed1);
            AssertEqualsExactAndHasSameFactory(gReduce, g2);
        }

        private static void AssertEqualsExactAndHasSameFactory(IGeometry a, IGeometry b)
        {
            Assert.IsTrue(a.EqualsExact(b));
            Assert.IsTrue(a.Factory == b.Factory);
        }

    }
}