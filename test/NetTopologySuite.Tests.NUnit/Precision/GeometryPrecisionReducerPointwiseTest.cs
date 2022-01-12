using NetTopologySuite.Geometries;
using NetTopologySuite.Precision;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Tests.NUnit.Precision
{
    public class GeometryPrecisionReducerPointwiseTest : GeometryTestCase
    {

        [Test]
        public void TestLineWithCollapse()
        {
            CheckReducePointwise(
                "LINESTRING (0 0,  0.1 0,  1 0)",
                "LINESTRING (0 0,  0   0,  1 0)");
        }

        [Test]
        public void TestLineDuplicatePointsPreserved()
        {
            CheckReducePointwise(
                "LINESTRING (0 0,  0.1 0,  0.1 0,  1 0, 1 0)",
                "LINESTRING (0 0,  0   0,  0   0,  1 0, 1 0)");
        }

        [Test]
        public void TestLineFullCollapse()
        {
            CheckReducePointwise(
                "LINESTRING (0 0,  0.1 0)",
                "LINESTRING (0 0,  0   0)");
        }

        [Test]
        public void TestPolygonFullCollapse()
        {
            CheckReducePointwise(
                "POLYGON ((0.1 0.3, 0.3 0.3, 0.3 0.1, 0.1 0.1, 0.1 0.3))",
                "POLYGON ((0 0, 0 0, 0 0, 0 0, 0 0))");
        }

        [Test]
        public void TestPolygonWithCollapsedLine()
        {
            CheckReducePointwise(
                "POLYGON ((10 10, 100 100, 200 10.1, 300 10, 10 10))",
                "POLYGON ((10 10, 100 100, 200 10,   300 10, 10 10))");
        }

        [Test]
        public void TestPolygonWithCollapsedPoint()
        {
            CheckReducePointwise(
                "POLYGON ((10 10, 100 100, 200 10.1, 300 100, 400 10, 10 10))",
                "POLYGON ((10 10, 100 100, 200 10,   300 100, 400 10, 10 10))");
        }

        //=======================================

        private void CheckReducePointwise(string wkt, string wktExpected)
        {
            var g = Read(wkt);
            var gExpected = Read(wktExpected);
            var pm = new PrecisionModel(1);
            var gReduce = GeometryPrecisionReducer.ReducePointwise(g, pm);
            AssertEqualsExactAndHasSameFactory(gExpected, gReduce);
        }

        private void AssertEqualsExactAndHasSameFactory(Geometry expected, Geometry actual)
        {
            CheckEqual(expected, actual);
            Assert.That(actual.Factory, Is.SameAs(expected.Factory), "Factories are not the same");
        }
    }
}
