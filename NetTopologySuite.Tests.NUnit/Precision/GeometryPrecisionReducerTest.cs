using GeoAPI.Geometries;
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
            IGeometry g = reader.Read("POLYGON (( 0 0, 0 1.4, 1.4 1.4, 1.4 0, 0 0 ))");
            IGeometry g2 = reader.Read("POLYGON (( 0 0, 0 1, 1 1, 1 0, 0 0 ))");
            IGeometry gReduce = reducer.Reduce(g);
            Assert.IsTrue(gReduce.EqualsExact(g2));
        }

        [Test]
        public void TestTinySquareCollapse()
        {
            IGeometry g = reader.Read("POLYGON (( 0 0, 0 .4, .4 .4, .4 0, 0 0 ))");
            IGeometry g2 = reader.Read("POLYGON EMPTY");
            IGeometry gReduce = reducer.Reduce(g);
            Assert.IsTrue(gReduce.EqualsExact(g2));
        }

        [Test]
        public void TestSquareCollapse()
        {
            IGeometry g = reader.Read("POLYGON (( 0 0, 0 1.4, .4 .4, .4 0, 0 0 ))");
            IGeometry g2 = reader.Read("POLYGON EMPTY");
            IGeometry gReduce = reducer.Reduce(g);
            Assert.IsTrue(gReduce.EqualsExact(g2));
        }

        [Test]
        public void TestSquareKeepCollapse()
        {
            IGeometry g = reader.Read("POLYGON (( 0 0, 0 1.4, .4 .4, .4 0, 0 0 ))");
            IGeometry g2 = reader.Read("POLYGON EMPTY");
            IGeometry gReduce = reducerKeepCollapse.Reduce(g);
            Assert.IsTrue(gReduce.EqualsExact(g2));
        }

        [Test]
        public void TestLine()
        {
            IGeometry g = reader.Read("LINESTRING ( 0 0, 0 1.4 )");
            IGeometry g2 = reader.Read("LINESTRING (0 0, 0 1)");
            IGeometry gReduce = reducer.Reduce(g);
            Assert.IsTrue(gReduce.EqualsExact(g2));
        }

        [Test]
        public void TestLineRemoveCollapse()
        {
            IGeometry g = reader.Read("LINESTRING ( 0 0, 0 .4 )");
            IGeometry g2 = reader.Read("LINESTRING EMPTY");
            IGeometry gReduce = reducer.Reduce(g);
            Assert.IsTrue(gReduce.EqualsExact(g2));
        }

        [Test]
        public void TestLineKeepCollapse()
        {
            IGeometry g = reader.Read("LINESTRING ( 0 0, 0 .4 )");
            IGeometry g2 = reader.Read("LINESTRING ( 0 0, 0 0 )");
            IGeometry gReduce = reducerKeepCollapse.Reduce(g);
            Assert.IsTrue(gReduce.EqualsExact(g2));
        }

        [Test]
        public void TestPolgonWithCollapsedLine()
        {
            IGeometry g = reader.Read("POLYGON ((10 10, 100 100, 200 10.1, 300 10, 10 10))");
            IGeometry g2 = reader.Read("POLYGON ((10 10, 100 100, 200 10, 10 10))");
            IGeometry gReduce = reducer.Reduce(g);
            Assert.IsTrue(gReduce.EqualsExact(g2));
        }

        [Test]
        public void TestPolgonWithCollapsedLinePointwise()
        {
            IGeometry g = reader.Read("POLYGON ((10 10, 100 100, 200 10.1, 300 10, 10 10))");
            IGeometry g2 = reader.Read("POLYGON ((10 10, 100 100, 200 10,   300 10, 10 10))");
            IGeometry gReduce = GeometryPrecisionReducer.ReducePointwise(g, pmFixed1);
            Assert.IsTrue(gReduce.EqualsExact(g2));
        }

        [Test]
        public void TestPolgonWithCollapsedPoint()
        {
            IGeometry g = reader.Read("POLYGON ((10 10, 100 100, 200 10.1, 300 100, 400 10, 10 10))");
            IGeometry g2 = reader.Read("MULTIPOLYGON (((10 10, 100 100, 200 10, 10 10)), ((200 10, 300 100, 400 10, 200 10)))");
            IGeometry gReduce = reducer.Reduce(g);
            Assert.IsTrue(gReduce.EqualsExact(g2));
        }

        [Test]
        public void TestPolgonWithCollapsedPointPointwise()
        {
            IGeometry g = reader.Read("POLYGON ((10 10, 100 100, 200 10.1, 300 100, 400 10, 10 10))");
            IGeometry g2 = reader.Read("POLYGON ((10 10, 100 100, 200 10,   300 100, 400 10, 10 10))");
            IGeometry gReduce = GeometryPrecisionReducer.ReducePointwise(g, pmFixed1);
            Assert.IsTrue(gReduce.EqualsExact(g2));
        }
    }
}