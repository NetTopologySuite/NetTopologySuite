using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Precision;
using NUnit.Framework;
namespace NetTopologySuite.Tests.NUnit.Precision
{
    [TestFixtureAttribute]
    public class SimpleGeometryPrecisionReducerTest
    {
        private PrecisionModel pmFloat;
        private PrecisionModel pmFixed1;
        private SimpleGeometryPrecisionReducer reducer;
        private SimpleGeometryPrecisionReducer reducerKeepCollapse;
        private GeometryFactory gfFloat;
        WKTReader reader;
        public SimpleGeometryPrecisionReducerTest()
        {
            pmFloat = new PrecisionModel();
            pmFixed1 = new PrecisionModel(1);
            reducer = new SimpleGeometryPrecisionReducer(pmFixed1);
            reducerKeepCollapse = new SimpleGeometryPrecisionReducer(pmFixed1);
            gfFloat = new GeometryFactory(pmFloat, 0);
            reader = new WKTReader(gfFloat);
            reducerKeepCollapse.RemoveCollapsedComponents = false;
        }
        [TestAttribute]
        public void TestSquare()
        {
            var g = reader.Read("POLYGON (( 0 0, 0 1.4, 1.4 1.4, 1.4 0, 0 0 ))");
            var g2 = reader.Read("POLYGON (( 0 0, 0 1, 1 1, 1 0, 0 0 ))");
            var gReduce = reducer.Reduce(g);
            Assert.IsTrue(gReduce.EqualsExact(g2));
        }
        [TestAttribute]
        public void TestTinySquareCollapse()
        {
            var g = reader.Read("POLYGON (( 0 0, 0 .4, .4 .4, .4 0, 0 0 ))");
            var g2 = reader.Read("POLYGON EMPTY");
            var gReduce = reducer.Reduce(g);
            Assert.IsTrue(gReduce.EqualsExact(g2));
        }
        [TestAttribute]
        public void TestSquareCollapse()
        {
            var g = reader.Read("POLYGON (( 0 0, 0 1.4, .4 .4, .4 0, 0 0 ))");
            var g2 = reader.Read("POLYGON EMPTY");
            var gReduce = reducer.Reduce(g);
            Assert.IsTrue(gReduce.EqualsExact(g2));
        }
        [TestAttribute]
        public void TestSquareKeepCollapse()
        {
            var g = reader.Read("POLYGON (( 0 0, 0 1.4, .4 .4, .4 0, 0 0 ))");
            var g2 = reader.Read("POLYGON (( 0 0, 0 1, 0 0, 0 0, 0 0 ))");
            var gReduce = reducerKeepCollapse.Reduce(g);
            Assert.IsTrue(gReduce.EqualsExact(g2));
        }
        [TestAttribute]
        public void TestLine()
        {
            var g = reader.Read("LINESTRING ( 0 0, 0 1.4 )");
            var g2 = reader.Read("LINESTRING (0 0, 0 1)");
            var gReduce = reducer.Reduce(g);
            Assert.IsTrue(gReduce.EqualsExact(g2));
        }
        [TestAttribute]
        public void TestLineRemoveCollapse()
        {
            var g = reader.Read("LINESTRING ( 0 0, 0 .4 )");
            var g2 = reader.Read("LINESTRING EMPTY");
            var gReduce = reducer.Reduce(g);
            Assert.IsTrue(gReduce.EqualsExact(g2));
        }
        [TestAttribute]
        public void TestLineKeepCollapse()
        {
            var g = reader.Read("LINESTRING ( 0 0, 0 .4 )");
            var g2 = reader.Read("LINESTRING ( 0 0, 0 0 )");
            var gReduce = reducerKeepCollapse.Reduce(g);
            Assert.IsTrue(gReduce.EqualsExact(g2));
        }
    }
}
