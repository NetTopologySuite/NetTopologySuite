using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.IO;
using NetTopologySuite.Tests.NUnit.Utilities;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public class ConvexHullTest
    {
        private readonly GeometryFactory _geometryFactory;
        private readonly WKTReader _reader;

        public ConvexHullTest()
        {
            PrecisionModel precisionModel = new PrecisionModel(1000);
            _geometryFactory = new GeometryFactory(precisionModel, 0);
            _reader = new WKTReader(_geometryFactory);
        }

        [Test]
        public void TestManyIdenticalPoints()
        {
            var pts = new Coordinate[100];
            for (int i = 0; i < 99; i++)
                pts[i] = new Coordinate(0, 0);
            pts[99] = new Coordinate(1, 1);
            var ch = new ConvexHull(pts, _geometryFactory);
            var actualGeometry = ch.GetConvexHull();
            var expectedGeometry = _reader.Read("LINESTRING (0 0, 1 1)");
            Assert.IsTrue(actualGeometry.EqualsExact(expectedGeometry));
        }

        [Test]
        public void TestAllIdenticalPoints()
        {
            var pts = new Coordinate[100];
            for (int i = 0; i < 100; i++)
                pts[i] = new Coordinate(0, 0);
            var ch = new ConvexHull(pts, _geometryFactory);
            var actualGeometry = ch.GetConvexHull();
            var expectedGeometry = _reader.Read("POINT (0 0)");
            Assert.IsTrue(expectedGeometry.EqualsExact(actualGeometry));
        }

        [Test]
        public void Test1()
        {
            var reader = new WKTReader(new GeometryFactory(new PrecisionModel(1), 0));
            var lineString = (LineString)reader.Read("LINESTRING (30 220, 240 220, 240 220)");
            var convexHull = (LineString)reader.Read("LINESTRING (30 220, 240 220)");
            Assert.IsTrue(convexHull.EqualsExact(lineString.ConvexHull()));
        }

        [Test]
        public void Test2()
        {
            var reader = new WKTReader(new GeometryFactory(new PrecisionModel(1), 0));
            var geometry = reader.Read("MULTIPOINT (130 240, 130 240, 130 240, 570 240, 570 240, 570 240, 650 240)");
            var convexHull = (LineString)reader.Read("LINESTRING (130 240, 650 240)");
            Assert.IsTrue(convexHull.EqualsExact(geometry.ConvexHull()));
        }

        [Test]
        public void Test3()
        {
            var reader = new WKTReader(new GeometryFactory(new PrecisionModel(1), 0));
            var geometry = reader.Read("MULTIPOINT (0 0, 0 0, 10 0)");
            var convexHull = (LineString)reader.Read("LINESTRING (0 0, 10 0)");
            Assert.IsTrue(convexHull.EqualsExact(geometry.ConvexHull()));
        }

        [Test]
        public void Test4()
        {
            var reader = new WKTReader(new GeometryFactory(new PrecisionModel(1), 0));
            var geometry = reader.Read("MULTIPOINT (0 0, 10 0, 10 0)");
            var convexHull = (LineString)reader.Read("LINESTRING (0 0, 10 0)");
            Assert.IsTrue(convexHull.EqualsExact(geometry.ConvexHull()));
        }

        [Test]
        public void Test5()
        {
            var reader = new WKTReader(new GeometryFactory(new PrecisionModel(1), 0));
            var geometry = reader.Read("MULTIPOINT (0 0, 5 0, 10 0)");
            var convexHull = (LineString)reader.Read("LINESTRING (0 0, 10 0)");
            Assert.IsTrue(convexHull.EqualsExact(geometry.ConvexHull()));
        }

        [Test]
        public void Test6()
        {
            var reader = new WKTReader(new GeometryFactory(new PrecisionModel(1), 0));
            var actualGeometry = reader.Read("MULTIPOINT (0 0, 5 1, 10 0)").ConvexHull();
            var expectedGeometry = reader.Read("POLYGON ((0 0, 5 1, 10 0, 0 0))");
            Assert.IsTrue(actualGeometry.EqualsTopologically(expectedGeometry));
        }

        // TJackson - Not included in NTS because there is no longer a ToCoordinateArray method on ConvexHull
        //public void testToArray() throws Exception {
        //    TestConvexHull convexHull = new TestConvexHull(geometryFactory.createGeometryCollection(null));
        //    Stack stack = new Stack();
        //    stack.push(new Coordinate(0, 0));
        //    stack.push(new Coordinate(1, 1));
        //    stack.push(new Coordinate(2, 2));
        //    Object[] array1 = convexHull.toCoordinateArray(stack);
        //    assertEquals(3, array1.length);
        //    assertEquals(new Coordinate(0, 0), array1[0]);
        //    assertEquals(new Coordinate(1, 1), array1[1]);
        //    assertEquals(new Coordinate(2, 2), array1[2]);
        //    assertTrue(!array1[0].equals(array1[1]));
        //}

        //private static class TestConvexHull extends ConvexHull {
        //    protected Coordinate[] toCoordinateArray(Stack stack) {
        //        return super.toCoordinateArray(stack);
        //    }
        //    public TestConvexHull(Geometry geometry) {
        //        super(geometry);
        //    }
        //}

        [Test]
        public void Test7()
        {
            var reader = new WKTReader(new GeometryFactory(new PrecisionModel(1), 0));
            var geometry = reader.Read("MULTIPOINT (0 0, 0 0, 5 0, 5 0, 10 0, 10 0)");
            var convexHull = (LineString)reader.Read("LINESTRING (0 0, 10 0)");
            Assert.IsTrue(convexHull.EqualsExact(geometry.ConvexHull()));
        }

        [TestCaseSource(typeof(PointwiseGeometryAggregationTestCases))]
        public void TestStaticAggregation(ICollection<Geometry> geoms)
        {
            var actual = ConvexHull.Create(geoms);

            // JTS doesn't usually bother doing anything special about nulls,
            // so our ports of their stuff will suffer the same.
            geoms = geoms?.Where(g => g != null).ToArray() ?? Array.Empty<Geometry>();

            var combinedGeometry = GeometryCombiner.Combine(geoms);

            // JTS also doesn't fear giving us nulls back from its algorithms.
            var expected = combinedGeometry?.ConvexHull();

            if (expected?.IsEmpty == false)
            {
                Assert.That(expected.EqualsTopologically(actual));
            }
            else
            {
                Assert.That(actual.IsEmpty);
            }
        }
    }
}
