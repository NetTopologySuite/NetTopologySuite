using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace NetTopologySuite.Tests.NUnit
{
    public class MiscellaneousTest
    {

        private readonly GeometryFactory _geometryFactory;
        private readonly WKTReader _reader;

        public MiscellaneousTest()
        {
            var gs = new NtsGeometryServices(PrecisionModel.Fixed.Value, 0);
            _geometryFactory = gs.CreateGeometryFactory();
            _reader = new WKTReader(gs);
        }

        
        [Test]
        public void TestEnvelopeCloned()
        {
            var a = _reader.Read("LINESTRING(0 0, 10 10)");
            //Envelope is lazily initialized [Jon Aquino]
            var aenv = a.EnvelopeInternal;
            var b = (Geometry) a.Copy();
            Assert.IsTrue(!ReferenceEquals(a.EnvelopeInternal, b.EnvelopeInternal));
        }

        [Test]
        public void testCreateEmptyGeometry()
        {
            Assert.IsTrue(_geometryFactory.CreatePoint((Coordinate) null).IsEmpty);
            Assert.IsTrue(_geometryFactory.CreateLinearRing(new Coordinate[] {}).IsEmpty);
            Assert.IsTrue(_geometryFactory.CreateLineString(new Coordinate[] {}).IsEmpty);
            Assert.IsTrue(
                _geometryFactory.CreatePolygon(_geometryFactory.CreateLinearRing(new Coordinate[] {}), new LinearRing[] {})
                               .IsEmpty);
            Assert.IsTrue(_geometryFactory.CreateMultiPolygon(new Polygon[] {}).IsEmpty);
            Assert.IsTrue(_geometryFactory.CreateMultiLineString(new LineString[] {}).IsEmpty);
            Assert.IsTrue(_geometryFactory.CreateMultiPoint(new Point[] {}).IsEmpty);

            Assert.IsTrue(_geometryFactory.CreatePoint((Coordinate) null).IsSimple);
            Assert.IsTrue(_geometryFactory.CreateLinearRing(new Coordinate[] {}).IsSimple);
            /**
             * @todo Enable when #isSimple implemented
             */
            //    Assert.IsTrue(geometryFactory.CreateLineString(new Coordinate[] { }).IsSimple);
            //    Assert.IsTrue(geometryFactory.CreatePolygon(geometryFactory.CreateLinearRing(new Coordinate[] { }), new LinearRing[] { }).IsSimple);
            //    Assert.IsTrue(geometryFactory.CreateMultiPolygon(new Polygon[] { }).IsSimple);
            //    Assert.IsTrue(geometryFactory.CreateMultiLineString(new LineString[] { }).IsSimple);
            //    Assert.IsTrue(geometryFactory.CreateMultiPoint(new Point[] { }).IsSimple);

            Assert.IsTrue(_geometryFactory.CreatePoint((Coordinate) null).Boundary.IsEmpty);
            Assert.IsTrue(_geometryFactory.CreateLinearRing(new Coordinate[] {}).Boundary.IsEmpty);
            Assert.IsTrue(_geometryFactory.CreateLineString(new Coordinate[] {}).Boundary.IsEmpty);
            Assert.IsTrue(
                _geometryFactory.CreatePolygon(_geometryFactory.CreateLinearRing(new Coordinate[] {}), new LinearRing[] {})
                               .Boundary.IsEmpty);
            Assert.IsTrue(_geometryFactory.CreateMultiPolygon(new Polygon[] {}).Boundary.IsEmpty);
            Assert.IsTrue(_geometryFactory.CreateMultiLineString(new LineString[] {}).Boundary.IsEmpty);
            Assert.IsTrue(_geometryFactory.CreateMultiPoint(new Point[] {}).Boundary.IsEmpty);

            Assert.IsTrue(_geometryFactory.CreateLinearRing((CoordinateSequence) null).IsEmpty);
            Assert.IsTrue(_geometryFactory.CreateLineString((Coordinate[]) null).IsEmpty);
            Assert.IsTrue(_geometryFactory.CreatePolygon(null, null).IsEmpty);
            Assert.IsTrue(_geometryFactory.CreateMultiPolygon(null).IsEmpty);
            Assert.IsTrue(_geometryFactory.CreateMultiLineString(null).IsEmpty);
            Assert.IsTrue(_geometryFactory.CreateMultiPoint((Point[]) null).IsEmpty);

            Assert.AreEqual(-1, (int) (_geometryFactory.CreatePoint((Coordinate) null)).BoundaryDimension);
            Assert.AreEqual(-1, (int) (_geometryFactory.CreateLinearRing((CoordinateSequence) null)).BoundaryDimension);
            Assert.AreEqual(0, (int) (_geometryFactory.CreateLineString((Coordinate[]) null)).BoundaryDimension);
            Assert.AreEqual(1, (int) (_geometryFactory.CreatePolygon(null, null)).BoundaryDimension);
            Assert.AreEqual(1, (int) (_geometryFactory.CreateMultiPolygon(null)).BoundaryDimension);
            Assert.AreEqual(0, (int) (_geometryFactory.CreateMultiLineString(null)).BoundaryDimension);
            Assert.AreEqual(-1, (int) (_geometryFactory.CreateMultiPoint((Point[]) null)).BoundaryDimension);

            Assert.AreEqual(0, (_geometryFactory.CreatePoint((Coordinate) null)).NumPoints);
            Assert.AreEqual(0, (_geometryFactory.CreateLinearRing((CoordinateSequence) null)).NumPoints);
            Assert.AreEqual(0, (_geometryFactory.CreateLineString((Coordinate[]) null)).NumPoints);
            Assert.AreEqual(0, (_geometryFactory.CreatePolygon(null, null)).NumPoints);
            Assert.AreEqual(0, (_geometryFactory.CreateMultiPolygon(null)).NumPoints);
            Assert.AreEqual(0, (_geometryFactory.CreateMultiLineString(null)).NumPoints);
            Assert.AreEqual(0, (_geometryFactory.CreateMultiPoint((Point[]) null)).NumPoints);

            Assert.AreEqual(0, (_geometryFactory.CreatePoint((Coordinate) null)).Coordinates.Length);
            Assert.AreEqual(0, (_geometryFactory.CreateLinearRing((CoordinateSequence) null)).Coordinates.Length);
            Assert.AreEqual(0, (_geometryFactory.CreateLineString((Coordinate[]) null)).Coordinates.Length);
            Assert.AreEqual(0, (_geometryFactory.CreatePolygon(null, null)).Coordinates.Length);
            Assert.AreEqual(0, (_geometryFactory.CreateMultiPolygon(null)).Coordinates.Length);
            Assert.AreEqual(0, (_geometryFactory.CreateMultiLineString(null)).Coordinates.Length);
            Assert.AreEqual(0, (_geometryFactory.CreateMultiPoint((Point[]) null)).Coordinates.Length);
        }

        [Test]
        public void testBoundaryOfEmptyGeometry()
        {
            Assert.IsTrue(_geometryFactory.CreatePoint((Coordinate) null).Boundary.GetType() ==
                          typeof (GeometryCollection));
            Assert.IsTrue(_geometryFactory.CreateLinearRing(new Coordinate[] {}).Boundary.GetType() ==
                          typeof (MultiPoint));
            Assert.IsTrue(_geometryFactory.CreateLineString(new Coordinate[] {}).Boundary.GetType() ==
                          typeof (MultiPoint));
            Assert.IsTrue(
                _geometryFactory.CreatePolygon(_geometryFactory.CreateLinearRing(new Coordinate[] {}), new LinearRing[] {})
                               .Boundary.GetType() == typeof (MultiLineString));
            Assert.IsTrue(_geometryFactory.CreateMultiPolygon(new Polygon[] {}).Boundary.GetType() ==
                          typeof (MultiLineString));
            Assert.IsTrue(_geometryFactory.CreateMultiLineString(new LineString[] {}).Boundary.GetType() ==
                          typeof (MultiPoint));
            Assert.IsTrue(_geometryFactory.CreateMultiPoint(new Point[] {}).Boundary.GetType() ==
                          typeof (GeometryCollection));
            try
            {
                var b = _geometryFactory.CreateGeometryCollection(new Geometry[] {}).Boundary;
                Assert.IsTrue(false);
            }
            catch (Exception)
            {
            }
        }

        [Test]
        public void testToPointArray()
        {
            var list = new List<Geometry>();
            list.Add(_geometryFactory.CreatePoint(new Coordinate(0, 0)));
            list.Add(_geometryFactory.CreatePoint(new Coordinate(10, 0)));
            list.Add(_geometryFactory.CreatePoint(new Coordinate(10, 10)));
            list.Add(_geometryFactory.CreatePoint(new Coordinate(0, 10)));
            list.Add(_geometryFactory.CreatePoint(new Coordinate(0, 0)));
            var points = GeometryFactory.ToPointArray(list);
            Assert.AreEqual(10, points[1].X, 1E-1);
            Assert.AreEqual(0, points[1].Y, 1E-1);
        }

        public void testPolygonCoordinates()
        {
            var p = (Polygon) _reader.Read(
                "POLYGON ( (0 0, 100 0, 100 100, 0 100, 0 0), "
                + "          (20 20, 20 80, 80 80, 80 20, 20 20)) ");
            var coordinates = p.Coordinates;
            Assert.AreEqual(10, p.NumPoints);
            Assert.AreEqual(10, coordinates.Length);
            Assert.AreEqual(new Coordinate(0, 0), coordinates[0]);
            Assert.AreEqual(new Coordinate(20, 20), coordinates[9]);
        }

        [Test]
        public void testEmptyPoint()
        {
            var p = _geometryFactory.CreatePoint((Coordinate) null);
            Assert.AreEqual(0, (int) p.Dimension);
            Assert.AreEqual(new Envelope(), p.EnvelopeInternal);
            Assert.IsTrue(p.IsSimple);
            Assert.IsTrue(double.IsNaN(p.X));
            Assert.IsTrue(double.IsNaN(p.Y));
            Assert.AreEqual("POINT EMPTY", p.ToString());
            Assert.AreEqual("POINT EMPTY", p.AsText());
        }

        [Test]
        public void testEmptyLineString()
        {
            var l = _geometryFactory.CreateLineString((Coordinate[]) null);
            Assert.AreEqual(1, (int) l.Dimension);
            Assert.AreEqual(new Envelope(), l.EnvelopeInternal);
            /**
             * @todo Enable when #isSimple implemented
             */
            //    Assert.IsTrue(l.IsSimple);
            Assert.AreEqual(null, l.StartPoint);
            Assert.AreEqual(null, l.EndPoint);
            Assert.IsTrue(!l.IsClosed);
            Assert.IsTrue(!l.IsRing);
        }

        [Test]
        public void testEmptyLinearRing()
        {
            var l = _geometryFactory.CreateLinearRing((CoordinateSequence) null);
            Assert.AreEqual(1, (int) l.Dimension);
            Assert.AreEqual(new Envelope(), l.EnvelopeInternal);
            Assert.IsTrue(l.IsSimple);
            Assert.AreEqual(null, l.StartPoint);
            Assert.AreEqual(null, l.EndPoint);
            Assert.IsTrue(l.IsClosed);
            Assert.IsTrue(l.IsRing);
        }

        [Test]
        public void testEmptyPolygon()
        {
            var p = _geometryFactory.CreatePolygon(null, null);
            Assert.AreEqual(2, (int) p.Dimension);
            Assert.AreEqual(new Envelope(), p.EnvelopeInternal);
            Assert.IsTrue(p.IsSimple);
        }

        [Test]
        public void testEmptyGeometryCollection()
        {
            var g = _geometryFactory.CreateGeometryCollection(null);
            Assert.AreEqual(-1, (int) g.Dimension);
            Assert.AreEqual(new Envelope(), g.EnvelopeInternal);
            Assert.IsTrue(g.IsSimple);
        }

        [Test]
        public void testEmptyMultiPoint()
        {
            var g = _geometryFactory.CreateMultiPoint((Point[]) null);
            Assert.AreEqual(0, (int) g.Dimension);
            Assert.AreEqual(new Envelope(), g.EnvelopeInternal);
            /**
             * @todo Enable when #isSimple implemented
             */
            //    Assert.IsTrue(g.IsSimple);
        }

        [Test]
        public void testEmptyMultiLineString()
        {
            var g = _geometryFactory.CreateMultiLineString(null);
            Assert.AreEqual(1, (int) g.Dimension);
            Assert.AreEqual(new Envelope(), g.EnvelopeInternal);
            /**
             * @todo Enable when #isSimple implemented
             */
            //    Assert.IsTrue(g.IsSimple);
            Assert.IsTrue(!g.IsClosed);
        }

        [Test]
        public void testEmptyMultiPolygon()
        {
            var g = _geometryFactory.CreateMultiPolygon(null);
            Assert.AreEqual(2, (int) g.Dimension);
            Assert.AreEqual(new Envelope(), g.EnvelopeInternal);
            Assert.IsTrue(g.IsSimple);
        }

        [Test]
        public void testGetGeometryType()
        {
            var g = _geometryFactory.CreateMultiPolygon(null);
            Assert.AreEqual("MultiPolygon", g.GeometryType);
        }

        [Test]
        public void testMultiPolygonIsSimple1()
        {
            var g = _reader.Read("MULTIPOLYGON (((10 10, 10 20, 20 20, 20 15, 10 10)), ((60 60, 70 70, 80 60, 60 60)))");
            Assert.IsTrue(g.IsSimple);
        }

        [Test]
        public void testPointIsSimple()
        {
            var g = _reader.Read("POINT (10 10)");
            Assert.IsTrue(g.IsSimple);
        }

        public void testPointBoundary()
        {
            var g = _reader.Read("POINT (10 10)");
            Assert.IsTrue(g.Boundary.IsEmpty);
        }

        /**
         * @todo Enable when #isSimple implemented
         */
        //  public void testMultiPointIsSimple1()  {
        //    Geometry g = reader.read("MULTIPOINT(10 10, 20 20, 30 30)");
        //    Assert.IsTrue(g.IsSimple);
        //  }

        [Test]
        public void testMultiPointBoundary()
        {
            var g = _reader.Read("MULTIPOINT(10 10, 20 20, 30 30)");
            Assert.IsTrue(g.Boundary.IsEmpty);
        }

        /**
         * @todo Enable when #isSimple implemented
         */
        //  public void testMultiPointIsSimple2()  {
        //    Geometry g = reader.read("MULTIPOINT(10 10, 30 30, 30 30)");
        //    Assert.IsTrue(! g.IsSimple);
        //  }

        /**
         * @todo Enable when #isSimple implemented
         */
        //  public void testLineStringIsSimple1()  {
        //    Geometry g = reader.read("LINESTRING(10 10, 20 10, 15 20)");
        //    Assert.IsTrue(g.IsSimple);
        //  }

        [Test]
        public void testLineStringBoundary1()
        {
            var g = (LineString) _reader.Read("LINESTRING(10 10, 20 10, 15 20)");
            Assert.IsTrue(g.Boundary is MultiPoint);
            var boundary = (MultiPoint) g.Boundary;
            Assert.IsTrue(boundary.GetGeometryN(0).Equals(g.StartPoint));
            Assert.IsTrue(boundary.GetGeometryN(1).Equals(g.EndPoint));
        }

        [Test]
        public void testLineStringBoundary2()
        {
            var g = (LineString) _reader.Read("LINESTRING(10 10, 20 10, 15 20, 10 10)");
            Assert.IsTrue(g.Boundary.IsEmpty);
        }

        /**
         * @todo Enable when #isSimple implemented
         */
        //  public void testLineStringIsSimple2()  {
        //    Geometry g = reader.read("LINESTRING(10 10, 20 10, 15 20, 15 0)");
        //    Assert.IsTrue(! g.IsSimple);
        //  }

        [Test]
        public void testLinearRingIsSimple()
        {
            Coordinate[] coordinates =
                {
                    new CoordinateZ(10, 10, 0),
                    new CoordinateZ(10, 20, 0),
                    new CoordinateZ(20, 20, 0),
                    new CoordinateZ(20, 15, 0),
                    new CoordinateZ(10, 10, 0)
                };
            var linearRing = _geometryFactory.CreateLinearRing(coordinates);
            Assert.IsTrue(linearRing.IsSimple);
        }

        [Test]
        public void testPolygonIsSimple()
        {
            var g = _reader.Read("POLYGON((10 10, 10 20, 202 0, 20 15, 10 10))");
            Assert.IsTrue(g.IsSimple);
        }

        [Test]
        public void testPolygonBoundary()
        {
            var g = _reader.Read("POLYGON("
                                + "(0 0, 40 0, 40 40, 0 40, 0 0),"
                                + "(10 10, 30 10, 30 30, 10 30, 10 10))");
            var b = _reader.Read("MULTILINESTRING("
                                + "(0 0, 40 0, 40 40, 0 40, 0 0),"
                                + "(10 10, 30 10, 30 30, 10 30, 10 10))");
            Assert.IsTrue(b.EqualsExact(g.Boundary));
        }

        [Test]
        public void testMultiPolygonBoundary1()
        {
            var g = _reader.Read("MULTIPOLYGON("
                                + "(  (0 0, 40 0, 40 40, 0 40, 0 0),"
                                + "   (10 10, 30 10, 30 30, 10 30, 10 10)  ),"
                                + "(  (200 200, 210 200, 210 210, 200 200) )  )");
            var b = _reader.Read("MULTILINESTRING("
                                + "(0 0, 40 0, 40 40, 0 40, 0 0),"
                                + "(10 10, 30 10, 30 30, 10 30, 10 10),"
                                + "(200 200, 210 200, 210 210, 200 200))");
            Assert.IsTrue(b.EqualsExact(g.Boundary));
        }

        [Test]
        public void testMultiPolygonIsSimple2()
        {
            var g = _reader.Read("MULTIPOLYGON("
                                + "((10 10, 10 20, 20 20, 20 15, 10 10)), "
                                + "((60 60, 70 70, 80 60, 60 60))  )");
            Assert.IsTrue(g.IsSimple);
        }

        //  public void testGeometryCollectionIsSimple1()  {
        //    Geometry g = reader.read("GEOMETRYCOLLECTION("
        //          + "LINESTRING(0 0,  100 0),"
        //          + "LINESTRING(0 10, 100 10))");
        //    Assert.IsTrue(g.IsSimple);
        //  }

        //  public void testGeometryCollectionIsSimple2()  {
        //    Geometry g = reader.read("GEOMETRYCOLLECTION("
        //          + "LINESTRING(0 0,  100 0),"
        //          + "LINESTRING(50 0, 100 10))");
        //    Assert.IsTrue(! g.IsSimple);
        //  }

        /**
         * @todo Enable when #isSimple implemented
         */
        //  public void testMultiLineStringIsSimple1()  {
        //    Geometry g = reader.read("MULTILINESTRING("
        //          + "(0 0,  100 0),"
        //          + "(0 10, 100 10))");
        //    Assert.IsTrue(g.IsSimple);
        //  }

        /**
         * @todo Enable when #isSimple implemented
         */
        //  public void testMultiLineStringIsSimple2()  {
        //    Geometry g = reader.read("MULTILINESTRING("
        //          + "(0 0,  100 0),"
        //          + "(50 0, 100 10))");
        //    Assert.IsTrue(! g.IsSimple);
        //  }

        [Test]
        public void testMultiLineStringBoundary1()
        {
            var g = _reader.Read("MULTILINESTRING("
                                + "(0 0,  100 0, 50 50),"
                                + "(50 50, 50 -50))");
            var m = _reader.Read("MULTIPOINT(0 0, 50 -50)");
            Assert.IsTrue(m.EqualsExact(g.Boundary));
        }

        [Test]
        public void testMultiLineStringBoundary2()
        {
            var g = _reader.Read("MULTILINESTRING("
                                + "(0 0,  100 0, 50 50),"
                                + "(50 50, 50 0))");
            var m = _reader.Read("MULTIPOINT(0 0, 50 0)");
            Assert.IsTrue(m.EqualsExact(g.Boundary));
        }

        //  public void testGeometryCollectionBoundary1()  {
        //    Geometry g = reader.read("GEOMETRYCOLLECTION("
        //          + "POLYGON((0 0, 100 0, 100 100, 0 100, 0 0)),"
        //          + "LINESTRING(200 100, 200 0))");
        //    Geometry b = reader.read("GEOMETRYCOLLECTION("
        //          + "LINESTRING(0 0, 100 0, 100 100, 0 100, 0 0),"
        //          + "LINESTRING(200 100, 200 0))");
        //    Assert.AreEqual(b, g.Boundary);
        //    Assert.IsTrue(! g.equals(g.Boundary));
        //  }

        //  public void testGeometryCollectionBoundary2()  {
        //    Geometry g = reader.read("GEOMETRYCOLLECTION("
        //          + "POLYGON((0 0, 100 0, 100 100, 0 100, 0 0)),"
        //          + "LINESTRING(50 50, 60 60))");
        //    Geometry b = reader.read("GEOMETRYCOLLECTION("
        //          + "LINESTRING(0 0, 100 0, 100 100, 0 100, 0 0))");
        //    Assert.AreEqual(b, g.Boundary);
        //  }

        //  public void testGeometryCollectionBoundary3()  {
        //    Geometry g = reader.read("GEOMETRYCOLLECTION("
        //          + "POLYGON((0 0, 100 0, 100 100, 0 100, 0 0)),"
        //          + "LINESTRING(50 50, 150 50))");
        //    Geometry b = reader.read("GEOMETRYCOLLECTION("
        //          + "LINESTRING(0 0, 100 0, 100 100, 0 100, 0 0),"
        //          + "POINT(150 50))");
        //    Assert.AreEqual(b, g.Boundary);
        //  }

        [Test]
        public void testCoordinateNaN()
        {
            var c1 = new Coordinate();
            Assert.IsTrue(!double.IsNaN(c1.X));
            Assert.IsTrue(!double.IsNaN(c1.Y));
            Assert.IsTrue(double.IsNaN(c1.Z));

            var c2 = new Coordinate(3, 4);
            Assert.AreEqual(3, c2.X, 1E-10);
            Assert.AreEqual(4, c2.Y, 1E-10);
            Assert.IsTrue(double.IsNaN(c2.Z));

            Assert.AreEqual(c1, c1);
            Assert.AreEqual(c2, c2);
            Assert.IsTrue(!c1.Equals(c2));
            Assert.AreEqual(new Coordinate(), new Coordinate(0, 0));
            Assert.AreEqual(new Coordinate(3, 5), new Coordinate(3, 5));
            Assert.AreEqual(new CoordinateZ(3, 5, double.NaN), new CoordinateZ(3, 5, double.NaN));
            Assert.IsTrue(new CoordinateZ(3, 5, 0).Equals(new CoordinateZ(3, 5, double.NaN)));
        }

        [Test]
        public void testPredicatesReturnFalseForEmptyGeometries()
        {
            var p1 = new GeometryFactory().CreatePoint((Coordinate) null);
            var p2 = new GeometryFactory().CreatePoint(new Coordinate(5, 5));
            Assert.AreEqual(false, p1.Equals(p2));
            Assert.AreEqual(true, p1.Disjoint(p2));
            Assert.AreEqual(false, p1.Intersects(p2));
            Assert.AreEqual(false, p1.Touches(p2));
            Assert.AreEqual(false, p1.Crosses(p2));
            Assert.AreEqual(false, p1.Within(p2));
            Assert.AreEqual(false, p1.Contains(p2));
            Assert.AreEqual(false, p1.Overlaps(p2));

            Assert.AreEqual(false, p2.Equals(p1));
            Assert.AreEqual(true, p2.Disjoint(p1));
            Assert.AreEqual(false, p2.Intersects(p1));
            Assert.AreEqual(false, p2.Touches(p1));
            Assert.AreEqual(false, p2.Crosses(p1));
            Assert.AreEqual(false, p2.Within(p1));
            Assert.AreEqual(false, p2.Contains(p1));
            Assert.AreEqual(false, p2.Overlaps(p1));
        }
    }
}
