using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using Assert = NUnit.Framework.Assert;

namespace NetTopologySuite.Tests.NUnit
{
    public class MiscellaneousTest
    {

        private static readonly IPrecisionModel precisionModel = new PrecisionModel(1);
        private static readonly IGeometryFactory geometryFactory = new GeometryFactory(precisionModel, 0);
        private static WKTReader reader = new WKTReader(geometryFactory);

        [TestAttribute]
        public void TestEnvelopeCloned()
        {
            var a = reader.Read("LINESTRING(0 0, 10 10)");
            //Envelope is lazily initialized [Jon Aquino]
            var aenv = a.EnvelopeInternal;
            var b = (IGeometry) a.Clone();
            Assert.IsTrue(!ReferenceEquals(a.EnvelopeInternal, b.EnvelopeInternal));
        }

        [TestAttribute]
        public void testCreateEmptyGeometry()
        {
            Assert.IsTrue(geometryFactory.CreatePoint((Coordinate) null).IsEmpty);
            Assert.IsTrue(geometryFactory.CreateLinearRing(new Coordinate[] {}).IsEmpty);
            Assert.IsTrue(geometryFactory.CreateLineString(new Coordinate[] {}).IsEmpty);
            Assert.IsTrue(
                geometryFactory.CreatePolygon(geometryFactory.CreateLinearRing(new Coordinate[] {}), new LinearRing[] {})
                               .IsEmpty);
            Assert.IsTrue(geometryFactory.CreateMultiPolygon(new Polygon[] {}).IsEmpty);
            Assert.IsTrue(geometryFactory.CreateMultiLineString(new LineString[] {}).IsEmpty);
            Assert.IsTrue(geometryFactory.CreateMultiPoint(new Point[] {}).IsEmpty);

            Assert.IsTrue(geometryFactory.CreatePoint((Coordinate) null).IsSimple);
            Assert.IsTrue(geometryFactory.CreateLinearRing(new Coordinate[] {}).IsSimple);
            /**
             * @todo Enable when #isSimple implemented
             */
            //    Assert.IsTrue(geometryFactory.CreateLineString(new Coordinate[] { }).IsSimple);
            //    Assert.IsTrue(geometryFactory.CreatePolygon(geometryFactory.CreateLinearRing(new Coordinate[] { }), new LinearRing[] { }).IsSimple);
            //    Assert.IsTrue(geometryFactory.CreateMultiPolygon(new Polygon[] { }).IsSimple);
            //    Assert.IsTrue(geometryFactory.CreateMultiLineString(new LineString[] { }).IsSimple);
            //    Assert.IsTrue(geometryFactory.CreateMultiPoint(new Point[] { }).IsSimple);

            Assert.IsTrue(geometryFactory.CreatePoint((Coordinate) null).Boundary.IsEmpty);
            Assert.IsTrue(geometryFactory.CreateLinearRing(new Coordinate[] {}).Boundary.IsEmpty);
            Assert.IsTrue(geometryFactory.CreateLineString(new Coordinate[] {}).Boundary.IsEmpty);
            Assert.IsTrue(
                geometryFactory.CreatePolygon(geometryFactory.CreateLinearRing(new Coordinate[] {}), new LinearRing[] {})
                               .Boundary.IsEmpty);
            Assert.IsTrue(geometryFactory.CreateMultiPolygon(new Polygon[] {}).Boundary.IsEmpty);
            Assert.IsTrue(geometryFactory.CreateMultiLineString(new LineString[] {}).Boundary.IsEmpty);
            Assert.IsTrue(geometryFactory.CreateMultiPoint(new Point[] {}).Boundary.IsEmpty);

            Assert.IsTrue(geometryFactory.CreateLinearRing((ICoordinateSequence) null).IsEmpty);
            Assert.IsTrue(geometryFactory.CreateLineString((Coordinate[]) null).IsEmpty);
            Assert.IsTrue(geometryFactory.CreatePolygon(null, null).IsEmpty);
            Assert.IsTrue(geometryFactory.CreateMultiPolygon(null).IsEmpty);
            Assert.IsTrue(geometryFactory.CreateMultiLineString(null).IsEmpty);
            Assert.IsTrue(geometryFactory.CreateMultiPoint((Point[]) null).IsEmpty);

            Assert.AreEqual(-1, (int) (geometryFactory.CreatePoint((Coordinate) null)).BoundaryDimension);
            Assert.AreEqual(-1, (int) (geometryFactory.CreateLinearRing((ICoordinateSequence) null)).BoundaryDimension);
            Assert.AreEqual(0, (int) (geometryFactory.CreateLineString((Coordinate[]) null)).BoundaryDimension);
            Assert.AreEqual(1, (int) (geometryFactory.CreatePolygon(null, null)).BoundaryDimension);
            Assert.AreEqual(1, (int) (geometryFactory.CreateMultiPolygon(null)).BoundaryDimension);
            Assert.AreEqual(0, (int) (geometryFactory.CreateMultiLineString(null)).BoundaryDimension);
            Assert.AreEqual(-1, (int) (geometryFactory.CreateMultiPoint((Point[]) null)).BoundaryDimension);

            Assert.AreEqual(0, (geometryFactory.CreatePoint((Coordinate) null)).NumPoints);
            Assert.AreEqual(0, (geometryFactory.CreateLinearRing((ICoordinateSequence) null)).NumPoints);
            Assert.AreEqual(0, (geometryFactory.CreateLineString((Coordinate[]) null)).NumPoints);
            Assert.AreEqual(0, (geometryFactory.CreatePolygon(null, null)).NumPoints);
            Assert.AreEqual(0, (geometryFactory.CreateMultiPolygon(null)).NumPoints);
            Assert.AreEqual(0, (geometryFactory.CreateMultiLineString(null)).NumPoints);
            Assert.AreEqual(0, (geometryFactory.CreateMultiPoint((Point[]) null)).NumPoints);

            Assert.AreEqual(0, (geometryFactory.CreatePoint((Coordinate) null)).Coordinates.Length);
            Assert.AreEqual(0, (geometryFactory.CreateLinearRing((ICoordinateSequence) null)).Coordinates.Length);
            Assert.AreEqual(0, (geometryFactory.CreateLineString((Coordinate[]) null)).Coordinates.Length);
            Assert.AreEqual(0, (geometryFactory.CreatePolygon(null, null)).Coordinates.Length);
            Assert.AreEqual(0, (geometryFactory.CreateMultiPolygon(null)).Coordinates.Length);
            Assert.AreEqual(0, (geometryFactory.CreateMultiLineString(null)).Coordinates.Length);
            Assert.AreEqual(0, (geometryFactory.CreateMultiPoint((Point[]) null)).Coordinates.Length);
        }

        [TestAttribute]
        public void testBoundaryOfEmptyGeometry()
        {
            Assert.IsTrue(geometryFactory.CreatePoint((Coordinate) null).Boundary.GetType() ==
                          typeof (GeometryCollection));
            Assert.IsTrue(geometryFactory.CreateLinearRing(new Coordinate[] {}).Boundary.GetType() ==
                          typeof (MultiPoint));
            Assert.IsTrue(geometryFactory.CreateLineString(new Coordinate[] {}).Boundary.GetType() ==
                          typeof (MultiPoint));
            Assert.IsTrue(
                geometryFactory.CreatePolygon(geometryFactory.CreateLinearRing(new Coordinate[] {}), new LinearRing[] {})
                               .Boundary.GetType() == typeof (MultiLineString));
            Assert.IsTrue(geometryFactory.CreateMultiPolygon(new Polygon[] {}).Boundary.GetType() ==
                          typeof (MultiLineString));
            Assert.IsTrue(geometryFactory.CreateMultiLineString(new LineString[] {}).Boundary.GetType() ==
                          typeof (MultiPoint));
            Assert.IsTrue(geometryFactory.CreateMultiPoint(new Point[] {}).Boundary.GetType() ==
                          typeof (GeometryCollection));
            try
            {
                var b = geometryFactory.CreateGeometryCollection(new IGeometry[] {}).Boundary;
                Assert.IsTrue(false);
            }
            catch (ArgumentException e)
            {
            }
        }

        [TestAttribute]
        public void testToPointArray()
        {
            var list = new List<IGeometry>();
            list.Add(geometryFactory.CreatePoint(new Coordinate(0, 0)));
            list.Add(geometryFactory.CreatePoint(new Coordinate(10, 0)));
            list.Add(geometryFactory.CreatePoint(new Coordinate(10, 10)));
            list.Add(geometryFactory.CreatePoint(new Coordinate(0, 10)));
            list.Add(geometryFactory.CreatePoint(new Coordinate(0, 0)));
            var points = GeometryFactory.ToPointArray(list);
            Assert.AreEqual(10, points[1].X, 1E-1);
            Assert.AreEqual(0, points[1].Y, 1E-1);
        }

        public void testPolygonCoordinates()
        {
            Polygon p = (Polygon) reader.Read(
                "POLYGON ( (0 0, 100 0, 100 100, 0 100, 0 0), "
                + "          (20 20, 20 80, 80 80, 80 20, 20 20)) ");
            Coordinate[] coordinates = p.Coordinates;
            Assert.AreEqual(10, p.NumPoints);
            Assert.AreEqual(10, coordinates.Length);
            Assert.AreEqual(new Coordinate(0, 0), coordinates[0]);
            Assert.AreEqual(new Coordinate(20, 20), coordinates[9]);
        }

        [TestAttribute]
        public void testEmptyPoint()
        {
            var p = geometryFactory.CreatePoint((Coordinate) null);
            Assert.AreEqual(0, (int) p.Dimension);
            Assert.AreEqual(new Envelope(), p.EnvelopeInternal);
            Assert.IsTrue(p.IsSimple);
            try
            {
                var tmp = p.X;
                Assert.IsTrue(false);
            }
            catch (ArgumentOutOfRangeException e1)
            {
            }
            try
            {
                var tmp = p.Y;
                Assert.IsTrue(false);
            }
            catch (ArgumentOutOfRangeException e2)
            {
            }

            Assert.AreEqual("POINT EMPTY", p.ToString());
            Assert.AreEqual("POINT EMPTY", p.AsText());
        }

        [TestAttribute]
        public void testEmptyLineString()
        {
            var l = geometryFactory.CreateLineString((Coordinate[]) null);
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

        [TestAttribute]
        public void testEmptyLinearRing()
        {
            var l = geometryFactory.CreateLinearRing((ICoordinateSequence) null);
            Assert.AreEqual(1, (int) l.Dimension);
            Assert.AreEqual(new Envelope(), l.EnvelopeInternal);
            Assert.IsTrue(l.IsSimple);
            Assert.AreEqual(null, l.StartPoint);
            Assert.AreEqual(null, l.EndPoint);
            Assert.IsTrue(l.IsClosed);
            Assert.IsTrue(l.IsRing);
        }

        [TestAttribute]
        public void testEmptyPolygon()
        {
            var p = geometryFactory.CreatePolygon(null, null);
            Assert.AreEqual(2, (int) p.Dimension);
            Assert.AreEqual(new Envelope(), p.EnvelopeInternal);
            Assert.IsTrue(p.IsSimple);
        }

        [TestAttribute]
        public void testEmptyGeometryCollection()
        {
            var g = geometryFactory.CreateGeometryCollection(null);
            Assert.AreEqual(-1, (int) g.Dimension);
            Assert.AreEqual(new Envelope(), g.EnvelopeInternal);
            Assert.IsTrue(g.IsSimple);
        }

        [TestAttribute]
        public void testEmptyMultiPoint()
        {
            var g = geometryFactory.CreateMultiPoint((Point[]) null);
            Assert.AreEqual(0, (int) g.Dimension);
            Assert.AreEqual(new Envelope(), g.EnvelopeInternal);
            /**
             * @todo Enable when #isSimple implemented
             */
            //    Assert.IsTrue(g.IsSimple);
        }

        [TestAttribute]
        public void testEmptyMultiLineString()
        {
            var g = geometryFactory.CreateMultiLineString(null);
            Assert.AreEqual(1, (int) g.Dimension);
            Assert.AreEqual(new Envelope(), g.EnvelopeInternal);
            /**
             * @todo Enable when #isSimple implemented
             */
            //    Assert.IsTrue(g.IsSimple);
            Assert.IsTrue(!g.IsClosed);
        }

        [TestAttribute]
        public void testEmptyMultiPolygon()
        {
            var g = geometryFactory.CreateMultiPolygon(null);
            Assert.AreEqual(2, (int) g.Dimension);
            Assert.AreEqual(new Envelope(), g.EnvelopeInternal);
            Assert.IsTrue(g.IsSimple);
        }

        [TestAttribute]
        public void testGetGeometryType()
        {
            var g = geometryFactory.CreateMultiPolygon(null);
            Assert.AreEqual("MultiPolygon", g.GeometryType);
        }

        [TestAttribute]
        public void testMultiPolygonIsSimple1()
        {
            var g = reader.Read("MULTIPOLYGON (((10 10, 10 20, 20 20, 20 15, 10 10)), ((60 60, 70 70, 80 60, 60 60)))");
            Assert.IsTrue(g.IsSimple);
        }

        [TestAttribute]
        public void testPointIsSimple()
        {
            var g = reader.Read("POINT (10 10)");
            Assert.IsTrue(g.IsSimple);
        }

        public void testPointBoundary()
        {
            var g = reader.Read("POINT (10 10)");
            Assert.IsTrue(g.Boundary.IsEmpty);
        }

        /**
         * @todo Enable when #isSimple implemented
         */
        //  public void testMultiPointIsSimple1()  {
        //    Geometry g = reader.read("MULTIPOINT(10 10, 20 20, 30 30)");
        //    Assert.IsTrue(g.IsSimple);
        //  }

        [TestAttribute]
        public void testMultiPointBoundary()
        {
            var g = reader.Read("MULTIPOINT(10 10, 20 20, 30 30)");
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

        [TestAttribute]
        public void testLineStringBoundary1()
        {
            var g = (LineString) reader.Read("LINESTRING(10 10, 20 10, 15 20)");
            Assert.IsTrue(g.Boundary is IMultiPoint);
            var boundary = (IMultiPoint) g.Boundary;
            Assert.IsTrue(boundary.GetGeometryN(0).Equals(g.StartPoint));
            Assert.IsTrue(boundary.GetGeometryN(1).Equals(g.EndPoint));
        }

        [TestAttribute]
        public void testLineStringBoundary2()
        {
            var g = (LineString) reader.Read("LINESTRING(10 10, 20 10, 15 20, 10 10)");
            Assert.IsTrue(g.Boundary.IsEmpty);
        }

        /**
         * @todo Enable when #isSimple implemented
         */
        //  public void testLineStringIsSimple2()  {
        //    Geometry g = reader.read("LINESTRING(10 10, 20 10, 15 20, 15 0)");
        //    Assert.IsTrue(! g.IsSimple);
        //  }

        [TestAttribute]
        public void testLinearRingIsSimple()
        {
            Coordinate[] coordinates =
                {
                    new Coordinate(10, 10, 0),
                    new Coordinate(10, 20, 0),
                    new Coordinate(20, 20, 0),
                    new Coordinate(20, 15, 0),
                    new Coordinate(10, 10, 0)
                };
            var linearRing = geometryFactory.CreateLinearRing(coordinates);
            Assert.IsTrue(linearRing.IsSimple);
        }

        [TestAttribute]
        public void testPolygonIsSimple()
        {
            var g = reader.Read("POLYGON((10 10, 10 20, 202 0, 20 15, 10 10))");
            Assert.IsTrue(g.IsSimple);
        }

        [TestAttribute]
        public void testPolygonBoundary()
        {
            var g = reader.Read("POLYGON("
                                + "(0 0, 40 0, 40 40, 0 40, 0 0),"
                                + "(10 10, 30 10, 30 30, 10 30, 10 10))");
            var b = reader.Read("MULTILINESTRING("
                                + "(0 0, 40 0, 40 40, 0 40, 0 0),"
                                + "(10 10, 30 10, 30 30, 10 30, 10 10))");
            Assert.IsTrue(b.EqualsExact(g.Boundary));
        }

        [TestAttribute]
        public void testMultiPolygonBoundary1()
        {
            var g = reader.Read("MULTIPOLYGON("
                                + "(  (0 0, 40 0, 40 40, 0 40, 0 0),"
                                + "   (10 10, 30 10, 30 30, 10 30, 10 10)  ),"
                                + "(  (200 200, 210 200, 210 210, 200 200) )  )");
            var b = reader.Read("MULTILINESTRING("
                                + "(0 0, 40 0, 40 40, 0 40, 0 0),"
                                + "(10 10, 30 10, 30 30, 10 30, 10 10),"
                                + "(200 200, 210 200, 210 210, 200 200))");
            Assert.IsTrue(b.EqualsExact(g.Boundary));
        }

        [TestAttribute]
        public void testMultiPolygonIsSimple2()
        {
            var g = reader.Read("MULTIPOLYGON("
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

        [TestAttribute]
        public void testMultiLineStringBoundary1()
        {
            var g = reader.Read("MULTILINESTRING("
                                + "(0 0,  100 0, 50 50),"
                                + "(50 50, 50 -50))");
            var m = reader.Read("MULTIPOINT(0 0, 50 -50)");
            Assert.IsTrue(m.EqualsExact(g.Boundary));
        }

        [TestAttribute]
        public void testMultiLineStringBoundary2()
        {
            var g = reader.Read("MULTILINESTRING("
                                + "(0 0,  100 0, 50 50),"
                                + "(50 50, 50 0))");
            var m = reader.Read("MULTIPOINT(0 0, 50 0)");
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

        [TestAttribute]
        public void testCoordinateNaN()
        {
            Coordinate c1 = new Coordinate();
            Assert.IsTrue(!Double.IsNaN(c1.X));
            Assert.IsTrue(!Double.IsNaN(c1.Y));
            Assert.IsTrue(Double.IsNaN(c1.Z));

            Coordinate c2 = new Coordinate(3, 4);
            Assert.AreEqual(3, c2.X, 1E-10);
            Assert.AreEqual(4, c2.Y, 1E-10);
            Assert.IsTrue(Double.IsNaN(c2.Z));

            Assert.AreEqual(c1, c1);
            Assert.AreEqual(c2, c2);
            Assert.IsTrue(!c1.Equals(c2));
            Assert.AreEqual(new Coordinate(), new Coordinate(0, 0));
            Assert.AreEqual(new Coordinate(3, 5), new Coordinate(3, 5));
            Assert.AreEqual(new Coordinate(3, 5, Double.NaN), new Coordinate(3, 5, Double.NaN));
            Assert.IsTrue(new Coordinate(3, 5, 0).Equals(new Coordinate(3, 5, Double.NaN)));
        }

        [TestAttribute]
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