using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixtureAttribute]
    public class ConvexHullTest
    {
        private readonly IGeometryFactory _geometryFactory;
        private readonly WKTReader _reader;

        public ConvexHullTest()
        {
            IPrecisionModel precisionModel = new PrecisionModel(1000);
            _geometryFactory = new GeometryFactory(precisionModel, 0);
            _reader = new WKTReader(_geometryFactory);
        }

        [TestAttribute]
        public void TestManyIdenticalPoints()
        {
            Coordinate[] pts = new Coordinate[100];
            for (int i = 0; i < 99; i++)
                pts[i] = new Coordinate(0, 0);
            pts[99] = new Coordinate(1, 1);
            ConvexHull ch = new ConvexHull(pts, _geometryFactory);
            IGeometry actualGeometry = ch.GetConvexHull();
            IGeometry expectedGeometry = _reader.Read("LINESTRING (0 0, 1 1)");
            Assert.IsTrue(actualGeometry.EqualsExact(expectedGeometry));
        }

        [TestAttribute]
        public void TestAllIdenticalPoints()
        {
            Coordinate[] pts = new Coordinate[100];
            for (int i = 0; i < 100; i++)
                pts[i] = new Coordinate(0, 0);
            ConvexHull ch = new ConvexHull(pts, _geometryFactory);
            IGeometry actualGeometry = ch.GetConvexHull();
            IGeometry expectedGeometry = _reader.Read("POINT (0 0)");
            Assert.IsTrue(expectedGeometry.EqualsExact(actualGeometry));
        }

        [TestAttribute]
        public void Test1()
        {
            WKTReader reader = new WKTReader(new GeometryFactory(new PrecisionModel(1), 0));
            LineString lineString = (LineString)reader.Read("LINESTRING (30 220, 240 220, 240 220)");
            LineString convexHull = (LineString)reader.Read("LINESTRING (30 220, 240 220)");
            Assert.IsTrue(convexHull.EqualsExact(lineString.ConvexHull()));
        }

        [TestAttribute]
        public void Test2()
        {
            WKTReader reader = new WKTReader(new GeometryFactory(new PrecisionModel(1), 0));
            IGeometry geometry = reader.Read("MULTIPOINT (130 240, 130 240, 130 240, 570 240, 570 240, 570 240, 650 240)");
            LineString convexHull = (LineString)reader.Read("LINESTRING (130 240, 650 240)");
            Assert.IsTrue(convexHull.EqualsExact(geometry.ConvexHull()));
        }

        [TestAttribute]
        public void Test3()
        {
            WKTReader reader = new WKTReader(new GeometryFactory(new PrecisionModel(1), 0));
            IGeometry geometry = reader.Read("MULTIPOINT (0 0, 0 0, 10 0)");
            LineString convexHull = (LineString)reader.Read("LINESTRING (0 0, 10 0)");
            Assert.IsTrue(convexHull.EqualsExact(geometry.ConvexHull()));
        }

        [TestAttribute]
        public void Test4()
        {
            WKTReader reader = new WKTReader(new GeometryFactory(new PrecisionModel(1), 0));
            IGeometry geometry = reader.Read("MULTIPOINT (0 0, 10 0, 10 0)");
            LineString convexHull = (LineString)reader.Read("LINESTRING (0 0, 10 0)");
            Assert.IsTrue(convexHull.EqualsExact(geometry.ConvexHull()));
        }

        [TestAttribute]
        public void Test5()
        {
            WKTReader reader = new WKTReader(new GeometryFactory(new PrecisionModel(1), 0));
            IGeometry geometry = reader.Read("MULTIPOINT (0 0, 5 0, 10 0)");
            LineString convexHull = (LineString)reader.Read("LINESTRING (0 0, 10 0)");
            Assert.IsTrue(convexHull.EqualsExact(geometry.ConvexHull()));
        }

        [TestAttribute]
        public void Test6()
        {
            WKTReader reader = new WKTReader(new GeometryFactory(new PrecisionModel(1), 0));
            IGeometry actualGeometry = reader.Read("MULTIPOINT (0 0, 5 1, 10 0)").ConvexHull();
            IGeometry expectedGeometry = reader.Read("POLYGON ((0 0, 5 1, 10 0, 0 0))");
            Assert.IsTrue(actualGeometry.Equals(expectedGeometry));
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

        [TestAttribute]
        public void Test7()
        {
            WKTReader reader = new WKTReader(new GeometryFactory(new PrecisionModel(1), 0));
            IGeometry geometry = reader.Read("MULTIPOINT (0 0, 0 0, 5 0, 5 0, 10 0, 10 0)");
            LineString convexHull = (LineString)reader.Read("LINESTRING (0 0, 10 0)");
            Assert.IsTrue(convexHull.EqualsExact(geometry.ConvexHull()));
        }

    }
}
