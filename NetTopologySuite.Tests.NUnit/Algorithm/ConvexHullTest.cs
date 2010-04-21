using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using GeoAPI.IO.WellKnownText;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using NUnit.Framework;
using Coord = NetTopologySuite.Coordinates.Simple.Coordinate;
using CoordFac = NetTopologySuite.Coordinates.Simple.CoordinateFactory;
using CoordSeqFac = NetTopologySuite.Coordinates.Simple.CoordinateSequenceFactory;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public class ConvexHullTest
    {
        private static IGeometryFactory<Coord> _geometryFactory;
        private static IWktGeometryReader<Coord> _reader;

        static ConvexHullTest()
        {
            _geometryFactory = new GeometryFactory<Coord>(new CoordSeqFac(new CoordFac(1d)));
            _reader = _geometryFactory.WktReader;
        }
        [Test]
        public void Test1()
        {

            ILineString<Coord> lineString = (ILineString<Coord>)_reader.Read("LINESTRING (30 220, 240 220, 240 220)");
            ILineString<Coord> convexHull = (ILineString<Coord>)_reader.Read("LINESTRING (30 220, 240 220)");
            ILineString<Coord> norm = lineString.ConvexHull() as ILineString<Coord>;
            norm.Normalize();
            Assert.IsTrue(convexHull.EqualsExact(norm));
        }

        [Test]
        public void Test2()
        {

            IGeometry<Coord> geometry = _reader.Read("MULTIPOINT (130 240, 130 240, 130 240, 570 240, 570 240, 570 240, 650 240)");
            ILineString<Coord> convexHull = (ILineString<Coord>)_reader.Read("LINESTRING (130 240, 650 240)");
            Assert.IsTrue(convexHull.EqualsExact(geometry.ConvexHull()));
        }

        [Test]
        public void Test3()
        {

            IGeometry<Coord> geometry = _reader.Read("MULTIPOINT (0 0, 0 0, 10 0)");
            ILineString<Coord> convexHull = (ILineString<Coord>)_reader.Read("LINESTRING (0 0, 10 0)");
            IGeometry<Coord> norm = geometry.ConvexHull();
            norm.Normalize();
            
            Assert.IsTrue(convexHull.EqualsExact(norm));
        }

        [Test]
        public void Test4()
        {

            IGeometry<Coord> geometry = _reader.Read("MULTIPOINT (0 0, 10 0, 10 0)");
            ILineString<Coord> convexHull = (ILineString<Coord>)_reader.Read("LINESTRING (0 0, 10 0)");
            IGeometry<Coord> norm = geometry.ConvexHull();
            norm.Normalize();

            Assert.IsTrue(convexHull.EqualsExact(norm));
        }
        [Test]

        public void Test5()
        {

            IGeometry<Coord> geometry = _reader.Read("MULTIPOINT (0 0, 5 0, 10 0)");
            ILineString<Coord> convexHull = (ILineString<Coord>)_reader.Read("LINESTRING (0 0, 10 0)");
            IGeometry<Coord> norm = geometry.ConvexHull();
            norm.Normalize();

            Assert.IsTrue(convexHull.EqualsExact(norm));
        }
        [Test]

        public void Test6()
        {

            IGeometry<Coord> actualGeometry = _reader.Read("MULTIPOINT (0 0, 5 1, 10 0)").ConvexHull();
            IGeometry<Coord> expectedGeometry = _reader.Read("POLYGON ((0 0, 5 1, 10 0, 0 0))");
            actualGeometry.Normalize();
            Assert.AreEqual(expectedGeometry.ToString(), actualGeometry.ToString());
        }
        /*
[Test]
  public void TestToArray()  {
    TestConvexHull convexHull = new TestConvexHull(_geometryFactory.CreateGeometryCollection(null));
    Stack<Coord> stack = new Stack<Coord>();
    stack.Push(new Coordinate(0, 0));
    stack.Push(new Coordinate(1, 1));
    stack.Push(new Coordinate(2, 2));
    Object[] array1 = convexHull.toCoordinateArray(stack);
    Assert.IsEqual(3, array1.length);
    Assert.IsEqual(new Coordinate(0, 0), array1[0]);
    Assert.IsEqual(new Coordinate(1, 1), array1[1]);
    Assert.IsEqual(new Coordinate(2, 2), array1[2]);
    Assert.IsTrue(!array1[0].equals(array1[1]));
  }

  private class TestConvexHull : ConvexHull<Coord> {
    protected Coordinate[] toCoordinateArray(Stack<Coord> stack) {
      return super.toCoordinateArray(stack);
    }
    public TestConvexHull(IGeometry<Coord> geometry)
        :base(geometry)
    {
    }
  }
         */
        [Test]

        public void Test7()
        {

            IGeometry<Coord> geometry = _reader.Read("MULTIPOINT (0 0, 0 0, 5 0, 5 0, 10 0, 10 0)");
            ILineString<Coord> convexHull = (ILineString<Coord>)_reader.Read("LINESTRING (0 0, 10 0)");
            IGeometry<Coord> norm = geometry.ConvexHull();
            norm.Normalize();

            Assert.IsTrue(convexHull.EqualsExact(norm));
        }

    }
}