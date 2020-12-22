using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixture]
    public class LineStringImplTest
    {
        private readonly GeometryFactory geometryFactory;
        private readonly WKTReader reader;

        public LineStringImplTest()
        {
            var gs = new NtsGeometryServices(new PrecisionModel(1000));
            geometryFactory = gs.CreateGeometryFactory(0);
            reader = new WKTReader(gs);
        }

        [Test]
        public void TestIsSimple()
        {
            var l1 = (LineString)reader.Read("LINESTRING (0 0, 10 10, 10 0, 0 10, 0 0)");
            Assert.IsTrue(!l1.IsSimple);
            var l2 = (LineString)reader.Read("LINESTRING (0 0, 10 10, 10 0, 0 10)");
            Assert.IsTrue(!l2.IsSimple);
        }

        [Test]
        public void TestIsCoordinate()
        {
            var l = (LineString)reader.Read("LINESTRING (0 0, 10 10, 10 0)");
            Assert.IsTrue(l.IsCoordinate(new Coordinate(0, 0)));
            Assert.IsTrue(!l.IsCoordinate(new Coordinate(5, 0)));
        }

        [Test]
        public void TestUnclosedLinearRing()
        {
            try
            {
                geometryFactory.CreateLinearRing(new Coordinate[]{
                new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(1, 1), new Coordinate(2, 1)});
                Assert.IsTrue(false);
            }
            catch (Exception e)
            {
                Assert.IsTrue(e is ArgumentException);
            }
        }

        [Test]
        public void TestEquals1()
        {
            var l1 = (LineString)reader.Read("LINESTRING(1.111 2.222, 3.333 4.444)");
            var l2 = (LineString)reader.Read("LINESTRING(1.111 2.222, 3.333 4.444)");
            Assert.IsTrue(l1.Equals(l2));
        }

        [Test]
        public void TestEquals2()
        {
            var l1 = (LineString)reader.Read("LINESTRING(1.111 2.222, 3.333 4.444)");
            var l2 = (LineString)reader.Read("LINESTRING(3.333 4.444, 1.111 2.222)");
            Assert.IsTrue(l1.Equals(l2));
        }

        [Test]
        public void TestEquals3()
        {
            var l1 = (LineString)reader.Read("LINESTRING(1.111 2.222, 3.333 4.444)");
            var l2 = (LineString)reader.Read("LINESTRING(3.333 4.443, 1.111 2.222)");
            Assert.IsTrue(!l1.Equals(l2));
        }

        [Test]
        public void TestEquals4()
        {
            var l1 = (LineString)reader.Read("LINESTRING(1.111 2.222, 3.333 4.444)");
            var l2 = (LineString)reader.Read("LINESTRING(3.333 4.4445, 1.111 2.222)");
            Assert.IsTrue(!l1.Equals(l2));
        }

        [Test]
        public void TestEquals5()
        {
            var l1 = (LineString)reader.Read("LINESTRING(1.111 2.222, 3.333 4.444)");
            var l2 = (LineString)reader.Read("LINESTRING(3.333 4.4446, 1.111 2.222)");
            Assert.IsTrue(!l1.Equals(l2));
        }

        [Test]
        public void TestEquals6()
        {
            var l1 = (LineString)reader.Read("LINESTRING(1.111 2.222, 3.333 4.444, 5.555 6.666)");
            var l2 = (LineString)reader.Read("LINESTRING(1.111 2.222, 3.333 4.444, 5.555 6.666)");
            Assert.IsTrue(l1.Equals(l2));
        }

        [Test]
        public void TestEquals7()
        {
            var l1 = (LineString)reader.Read("LINESTRING(1.111 2.222, 5.555 6.666, 3.333 4.444)");
            var l2 = (LineString)reader.Read("LINESTRING(1.111 2.222, 3.333 4.444, 5.555 6.666)");
            Assert.IsTrue(!l1.Equals(l2));
        }

        [Test]
        public void TestGetCoordinates()
        {
            var l = (LineString)reader.Read("LINESTRING(1.111 2.222, 5.555 6.666, 3.333 4.444)");
            var coordinates = l.Coordinates;
            Assert.AreEqual(new Coordinate(5.555, 6.666), coordinates[1]);
        }

        [Test]
        public void TestIsClosed()
        {
            var l = (LineString)reader.Read("LINESTRING EMPTY");
            Assert.IsTrue(l.IsEmpty);
            Assert.IsTrue(!l.IsClosed);

            var r = geometryFactory.CreateLinearRing((CoordinateSequence)null);
            Assert.IsTrue(r.IsEmpty);
            Assert.IsTrue(r.IsClosed);

            var m = geometryFactory.CreateMultiLineString(
                  new LineString[] { l, r });
            Assert.IsTrue(!m.IsClosed);

            var m2 = geometryFactory.CreateMultiLineString(
                  new LineString[] { r });
            Assert.IsTrue(!m2.IsClosed);
        }

        [Test]
        public void TestGetGeometryType()
        {
            var l = (LineString)reader.Read("LINESTRING EMPTY");
            Assert.AreEqual("LineString", l.GeometryType);
        }

        [Test]
        public void TestEquals8()
        {
            var reader = new WKTReader(new NtsGeometryServices(new PrecisionModel(1000), 0));
            var l1 = (MultiLineString)reader.Read("MULTILINESTRING((1732328800 519578384, 1732026179 519976285, 1731627364 519674014, 1731929984 519276112, 1732328800 519578384))");
            var l2 = (MultiLineString)reader.Read("MULTILINESTRING((1731627364 519674014, 1731929984 519276112, 1732328800 519578384, 1732026179 519976285, 1731627364 519674014))");
            Assert.IsTrue(l1.Equals(l2));
        }

        [Test]
        public void TestEquals9()
        {
            var reader = new WKTReader(new NtsGeometryServices(new PrecisionModel(1), 0));
            var l1 = (MultiLineString)reader.Read("MULTILINESTRING((1732328800 519578384, 1732026179 519976285, 1731627364 519674014, 1731929984 519276112, 1732328800 519578384))");
            var l2 = (MultiLineString)reader.Read("MULTILINESTRING((1731627364 519674014, 1731929984 519276112, 1732328800 519578384, 1732026179 519976285, 1731627364 519674014))");
            Assert.IsTrue(l1.Equals(l2));
        }

        [Test]
        public void TestEquals10()
        {
            var reader = new WKTReader(new NtsGeometryServices(new PrecisionModel(1), 0));
            var l1 = reader.Read("POLYGON((1732328800 519578384, 1732026179 519976285, 1731627364 519674014, 1731929984 519276112, 1732328800 519578384))");
            var l2 = reader.Read("POLYGON((1731627364 519674014, 1731929984 519276112, 1732328800 519578384, 1732026179 519976285, 1731627364 519674014))");
            l1.Normalize();
            l2.Normalize();
            Assert.IsTrue(l1.EqualsExact(l2));
        }

        [Test]
        public void TestFiveZeros()
        {
            var ls = new GeometryFactory().CreateLineString(new Coordinate[]{
                      new Coordinate(0, 0),
                      new Coordinate(0, 0),
                      new Coordinate(0, 0),
                      new Coordinate(0, 0),
                      new Coordinate(0, 0)});
            Assert.IsTrue(ls.IsClosed);
        }

        [Test]
        public void TestLinearRingConstructor()
        {
            try
            {
                var ring =
                  new GeometryFactory().CreateLinearRing(
                    new Coordinate[] {
                    new Coordinate(0, 0),
                    new Coordinate(10, 10),
                    new Coordinate(0, 0)});
                Assert.IsTrue(false);
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(true);
            }
        }
    }
}
