using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixture]
    public class LineStringImplTest
    {
        private IPrecisionModel precisionModel;
        private IGeometryFactory geometryFactory;
        WKTReader reader;

        public LineStringImplTest()
        {
            precisionModel = new PrecisionModel(1000);
            geometryFactory = new GeometryFactory(precisionModel, 0);
            reader = new WKTReader(geometryFactory);
        }

        [Test]
        public void TestIsSimple()
        {
            LineString l1 = (LineString) reader.Read("LINESTRING (0 0, 10 10, 10 0, 0 10, 0 0)");
            Assert.IsTrue(! l1.IsSimple);
            LineString l2 = (LineString) reader.Read("LINESTRING (0 0, 10 10, 10 0, 0 10)");
            Assert.IsTrue(! l2.IsSimple);
        }

        [Test]
        public void TestIsCoordinate()
        {
            LineString l = (LineString) reader.Read("LINESTRING (0 0, 10 10, 10 0)");
            Assert.IsTrue(l.IsCoordinate(new Coordinate(0, 0)));
            Assert.IsTrue(! l.IsCoordinate(new Coordinate(5, 0)));
        }

        [Test]
        public void TestUnclosedLinearRing()
        {
            try {
            geometryFactory.CreateLinearRing(new Coordinate[]{
                new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(1, 1), new Coordinate(2, 1)});
            Assert.IsTrue(false);
            }
            catch (Exception e) {
                Assert.IsTrue(e is ArgumentException);
            }
        }

        [Test]
        public void TestEquals1()
        {
            LineString l1 = (LineString) reader.Read("LINESTRING(1.111 2.222, 3.333 4.444)");
            LineString l2 = (LineString) reader.Read("LINESTRING(1.111 2.222, 3.333 4.444)");
            Assert.IsTrue(l1.Equals(l2));
        }

        [Test]
        public void TestEquals2() 
        {
            LineString l1 = (LineString) reader.Read("LINESTRING(1.111 2.222, 3.333 4.444)");
            LineString l2 = (LineString) reader.Read("LINESTRING(3.333 4.444, 1.111 2.222)");
            Assert.IsTrue(l1.Equals(l2));
        }

        [Test]
        public void TestEquals3()
        {
            LineString l1 = (LineString) reader.Read("LINESTRING(1.111 2.222, 3.333 4.444)");
            LineString l2 = (LineString) reader.Read("LINESTRING(3.333 4.443, 1.111 2.222)");
            Assert.IsTrue(! l1.Equals(l2));
        }

        [Test]
        public void TestEquals4()
        {
            LineString l1 = (LineString) reader.Read("LINESTRING(1.111 2.222, 3.333 4.444)");
            LineString l2 = (LineString) reader.Read("LINESTRING(3.333 4.4445, 1.111 2.222)");
            Assert.IsTrue(! l1.Equals(l2));
        }

        [Test]
        public void TestEquals5()
        {
            LineString l1 = (LineString)reader.Read("LINESTRING(1.111 2.222, 3.333 4.444)");
            LineString l2 = (LineString)reader.Read("LINESTRING(3.333 4.4446, 1.111 2.222)");
            Assert.IsTrue(!l1.Equals(l2));
        }

        [Test]
        public void TestEquals6()
        {
            LineString l1 = (LineString) reader.Read("LINESTRING(1.111 2.222, 3.333 4.444, 5.555 6.666)");
            LineString l2 = (LineString) reader.Read("LINESTRING(1.111 2.222, 3.333 4.444, 5.555 6.666)");
            Assert.IsTrue(l1.Equals(l2));
        }

        [Test]
        public void TestEquals7() 
        {
            LineString l1 = (LineString) reader.Read("LINESTRING(1.111 2.222, 5.555 6.666, 3.333 4.444)");
            LineString l2 = (LineString) reader.Read("LINESTRING(1.111 2.222, 3.333 4.444, 5.555 6.666)");
            Assert.IsTrue(!l1.Equals(l2));
        }

        [Test]
        public void TestGetCoordinates()
        {
            LineString l = (LineString) reader.Read("LINESTRING(1.111 2.222, 5.555 6.666, 3.333 4.444)");
            Coordinate[] coordinates = l.Coordinates;
            Assert.AreEqual(new Coordinate(5.555, 6.666), coordinates[1]);
        }

        [Test]
        public void TestIsClosed()
        {
            LineString l = (LineString) reader.Read("LINESTRING EMPTY");
            Assert.IsTrue(l.IsEmpty);
            Assert.IsTrue(! l.IsClosed);

            ILinearRing r = geometryFactory.CreateLinearRing((ICoordinateSequence)null);
            Assert.IsTrue(r.IsEmpty);
            // TODO: Verify that this change was correct.  The IsClosed property checks if the shape IsEmpty, and if so always returns false, both in NTS and JTS, so I am not sure how this assert was correct?  I have altered to assert that IsClosed is false.
            //Assert.IsTrue(r.IsClosed);
            Assert.IsTrue(! r.IsClosed);

            IMultiLineString m = geometryFactory.CreateMultiLineString(
                  new ILineString[] {l, r});
            Assert.IsTrue(!geometryFactory.CreateLinearRing(m.Coordinates).IsClosed);

            IMultiLineString m2 = geometryFactory.CreateMultiLineString(
                  new ILineString[] {r});
            Assert.IsTrue(! geometryFactory.CreateLinearRing(m2.Coordinates).IsClosed);
        }

        [Test]
        public void TestGetGeometryType()
        {
            LineString l = (LineString) reader.Read("LINESTRING EMPTY");
            Assert.AreEqual("LineString", l.GeometryType);
        }

        [Test]
        public void TestEquals8()
        {
            WKTReader reader = new WKTReader(new GeometryFactory(new PrecisionModel(1000), 0));
            MultiLineString l1 = (MultiLineString) reader.Read("MULTILINESTRING((1732328800 519578384, 1732026179 519976285, 1731627364 519674014, 1731929984 519276112, 1732328800 519578384))");
            MultiLineString l2 = (MultiLineString) reader.Read("MULTILINESTRING((1731627364 519674014, 1731929984 519276112, 1732328800 519578384, 1732026179 519976285, 1731627364 519674014))");
            Assert.IsTrue(l1.Equals(l2));
        }

        [Test]
        public void TestEquals9()
        {
            WKTReader reader = new WKTReader(new GeometryFactory(new PrecisionModel(1), 0));
            MultiLineString l1 = (MultiLineString) reader.Read("MULTILINESTRING((1732328800 519578384, 1732026179 519976285, 1731627364 519674014, 1731929984 519276112, 1732328800 519578384))");
            MultiLineString l2 = (MultiLineString) reader.Read("MULTILINESTRING((1731627364 519674014, 1731929984 519276112, 1732328800 519578384, 1732026179 519976285, 1731627364 519674014))");
            Assert.IsTrue(l1.Equals(l2));
        }

        [Test]
        public void TestEquals10()
        {
            WKTReader reader = new WKTReader(new GeometryFactory(new PrecisionModel(1), 0));
            IGeometry l1 = reader.Read("POLYGON((1732328800 519578384, 1732026179 519976285, 1731627364 519674014, 1731929984 519276112, 1732328800 519578384))");
            IGeometry l2 = reader.Read("POLYGON((1731627364 519674014, 1731929984 519276112, 1732328800 519578384, 1732026179 519976285, 1731627364 519674014))");
            l1.Normalize();
            l2.Normalize();
            Assert.IsTrue(l1.EqualsExact(l2));
        }

        [Test]
        public void TestFiveZeros() {
            ILineString ls = new GeometryFactory().CreateLineString(new Coordinate[]{
                      new Coordinate(0, 0),
                      new Coordinate(0, 0),
                      new Coordinate(0, 0),
                      new Coordinate(0, 0),
                      new Coordinate(0, 0)});
            Assert.IsTrue(ls.IsClosed);
        }

        [Test]
        public void TestLinearRingConstructor()  {
            try {
              ILinearRing ring =
                new GeometryFactory().CreateLinearRing(
                  new Coordinate[] {
                    new Coordinate(0, 0),
                    new Coordinate(10, 10),
                    new Coordinate(0, 0)});
              Assert.IsTrue(false);
            } catch (ArgumentException e) {
              Assert.IsTrue(true);
            }
        }
    }
}
