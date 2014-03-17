using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixtureAttribute]
    public class GeometryImplTest
    {
        private IPrecisionModel precisionModel;
        private IGeometryFactory geometryFactory;
        WKTReader reader;
        WKTReader readerFloat;

        public GeometryImplTest()
        {
            precisionModel = new PrecisionModel(1);
            geometryFactory = new GeometryFactory(precisionModel, 0);
            reader = new WKTReader(geometryFactory);
            readerFloat = new WKTReader();
        }

        [TestAttribute]
        public void TestPolygonRelate()
        {
            IGeometry bigPolygon = reader.Read(
                    "POLYGON ((0 0, 0 50, 50 50, 50 0, 0 0))");
            IGeometry smallPolygon = reader.Read(
                    "POLYGON ((10 10, 10 30, 30 30, 30 10, 10 10))");
            Assert.IsTrue(bigPolygon.Contains(smallPolygon));
        }

        [TestAttribute]
        public void TestEmptyGeometryCentroid()
        {
            Assert.IsTrue(reader.Read("POINT EMPTY").IsEmpty);
            Assert.IsTrue(reader.Read("POLYGON EMPTY").IsEmpty);
            Assert.IsTrue(reader.Read("LINESTRING EMPTY").IsEmpty);
            Assert.IsTrue(reader.Read("GEOMETRYCOLLECTION EMPTY").IsEmpty);
            Assert.IsTrue(reader.Read("GEOMETRYCOLLECTION(GEOMETRYCOLLECTION EMPTY, GEOMETRYCOLLECTION EMPTY)").IsEmpty);
            Assert.IsTrue(reader.Read("MULTIPOLYGON EMPTY").IsEmpty);
            Assert.IsTrue(reader.Read("MULTILINESTRING EMPTY").IsEmpty);
            Assert.IsTrue(reader.Read("MULTIPOINT EMPTY").IsEmpty);
        }

        [TestAttribute]
        public void TestNoOutgoingDirEdgeFound()
        {
            doTestFromCommcast2003AtYahooDotCa(reader);
        }

        [TestAttribute]
        public void TestOutOfMemoryError()
        {
            doTestFromCommcast2003AtYahooDotCa(new WKTReader());
        }



        [TestAttribute]
        public void TestDepthMismatchAssertionFailedException()
        {
            //register@robmeek.com reported an assertion failure
            //("depth mismatch at (160.0, 300.0, Nan)") [Jon Aquino 10/28/2003]
            reader
                .Read("MULTIPOLYGON (((100 300, 100 400, 200 400, 200 300, 100 300)),"
                    + "((160 300, 160 400, 260 400, 260 300, 160 300)),"
                    + "((160 300, 160 200, 260 200, 260 300, 160 300)))").Buffer(0);
        }

        private void doTestFromCommcast2003AtYahooDotCa(WKTReader reader)
        {
    	    readerFloat.Read(
                "POLYGON ((708653.498611049 2402311.54647056, 708708.895756966 2402203.47250014, 708280.326454234 2402089.6337791, 708247.896591321 2402252.48269854, 708367.379593851 2402324.00761653, 708248.882609455 2402253.07294874, 708249.523621829 2402244.3124463, 708261.854734465 2402182.39086576, 708262.818392579 2402183.35452387, 708653.498611049 2402311.54647056))")
                  .Intersection(reader.Read(
                    "POLYGON ((708258.754920656 2402197.91172757, 708257.029447455 2402206.56901508, 708652.961095455 2402312.65463437, 708657.068786251 2402304.6356364, 708258.754920656 2402197.91172757))"));
        }

        [Ignore("The equalseHash assert for the differentStart geometry is causing a failure in the test.  The problem is caused by a difference in the logic between JTS and NTS.  JTS computes the hash based on the bounding rectangle, which is the same for both shapes, but NTS computes it based on the coordinates in the shape, which is actually delgated to the derived type via an abstract method.  In theory two polygons with the same number of points could have the same bounding rectangle, but have points in different positions - think of a five pointed star, with the inner points in different locations, while the outer points are the same, and therefor defined the bounding rectangle.  On the other hand, two shapes that are equivalent, but have different start points should really have the same hash code.  The logic for GetHashCode on geometries needs to be reviewed before enabling this test again.")]
        public void TestEquals()
        {
            IGeometry g = reader.Read("POLYGON ((0 0, 0 50, 50 50, 50 0, 0 0))");
            IGeometry same = reader.Read("POLYGON ((0 0, 0 50, 50 50, 50 0, 0 0))");
            IGeometry differentStart = reader.Read(
                    "POLYGON ((0 50, 50 50, 50 0, 0 0, 0 50))");
            IGeometry differentFourth = reader.Read(
                    "POLYGON ((0 0, 0 50, 50 50, 50 -99, 0 0))");
            IGeometry differentSecond = reader.Read(
                    "POLYGON ((0 0, 0 99, 50 50, 50 0, 0 0))");
            DoTestEquals(g, same, true, true, true, true);
            DoTestEquals(g, differentStart, true, true, false, true);  // NTS casts from object to IGeometry if possible, so changed a equalsObject to be true not false
            DoTestEquals(g, differentFourth, false, false, false, false);
            DoTestEquals(g, differentSecond, false, false, false, false);
        }

        private void DoTestEquals(IGeometry a, IGeometry b, bool equalsGeometry,
            bool equalsObject, bool equalsExact, bool equalsHash)
        {
            Assert.AreEqual(equalsGeometry, a.Equals(b));
            Assert.AreEqual(equalsObject, a.Equals((Object) b));
            Assert.AreEqual(equalsExact, a.EqualsExact(b));
            Assert.AreEqual(equalsHash, a.GetHashCode() == b.GetHashCode());
        }

        [TestAttribute]
        public void TestInvalidateEnvelope()
        {
            IGeometry g = reader.Read("POLYGON ((0 0, 0 50, 50 50, 50 0, 0 0))");
            Assert.AreEqual(new Envelope(0, 50, 0, 50), g.EnvelopeInternal);
            g.Apply(new CoordinateFilter());
            Assert.AreEqual(new Envelope(0, 50, 0, 50), g.EnvelopeInternal);
            g.GeometryChanged();
            Assert.AreEqual(new Envelope(1, 51, 1, 51), g.EnvelopeInternal);
        }

        class CoordinateFilter : ICoordinateFilter
        {
            public void Filter(Coordinate coord) {
                coord.X += 1;
                coord.Y += 1;
            }
        }

        [TestAttribute]
        public void TestEquals1()
        {
            IGeometry polygon1 = reader.Read(
                    "POLYGON ((0 0, 0 50, 50 50, 50 0, 0 0))");
            IGeometry polygon2 = reader.Read(
                    "POLYGON ((50 50, 50 0, 0 0, 0 50, 50 50))");
            Assert.IsTrue(polygon1.Equals(polygon2));
        }

        [TestAttribute]
        public void TestEqualsWithNull()
        {
            IGeometry polygon = reader.Read("POLYGON ((0 0, 0 50, 50 50, 50 0, 0 0))");
            Assert.IsTrue(!polygon.Equals(null));
            Object g = null;
            Assert.IsTrue(!polygon.Equals(g));
        }

        [Ignore("This was commented out in JTS - not sure why")]
        public void TestEquals2()
        {
            //    Geometry lineString = reader.read("LINESTRING(0 0, 0 50, 50 50, 50 0, 0 0)");
            //    Geometry geometryCollection = reader.read("GEOMETRYCOLLECTION ( LINESTRING(0 0  , 0  50), "
            //                                                                 + "LINESTRING(0 50 , 50 50), "
            //                                                                 + "LINESTRING(50 50, 50 0 ), "
            //                                                                 + "LINESTRING(50 0 , 0  0 ) )");
            //    Assert.IsTrue(lineString.equals(geometryCollection));
        }        

        [TestAttribute]
        public void TestEqualsExactForLinearRings()
        {
            ILinearRing x = geometryFactory.CreateLinearRing(new Coordinate[] {
                        new Coordinate(0, 0), new Coordinate(100, 0),
                        new Coordinate(100, 100), new Coordinate(0, 0)
                    });
            ILinearRing somethingExactlyEqual = geometryFactory.CreateLinearRing(new Coordinate[] {
                        new Coordinate(0, 0), new Coordinate(100, 0),
                        new Coordinate(100, 100), new Coordinate(0, 0)
                    });
            ILinearRing somethingNotEqualButSameClass = geometryFactory.CreateLinearRing(new Coordinate[] {
                        new Coordinate(0, 0), new Coordinate(100, 0),
                        new Coordinate(100, 555), new Coordinate(0, 0)
                    });
            ILinearRing sameClassButEmpty = geometryFactory.CreateLinearRing((ICoordinateSequence)null);
            ILinearRing anotherSameClassButEmpty = geometryFactory.CreateLinearRing((ICoordinateSequence)null);
            LineCollectionFactory collectionFactory = new LineCollectionFactory();

            DoTestEqualsExact(x, somethingExactlyEqual,
                somethingNotEqualButSameClass, sameClassButEmpty,
                anotherSameClassButEmpty, collectionFactory);

        //    LineString somethingEqualButNotExactly = geometryFactory.createLineString(new Coordinate[] {
        //          new Coordinate(0, 0), new Coordinate(100, 0), new Coordinate(100, 100),
        //          new Coordinate(0, 0) });
        //
        //    doTestEqualsExact(x, somethingExactlyEqual, somethingEqualButNotExactly,
        //          somethingNotEqualButSameClass);
        }

        [TestAttribute]
        public void TestEqualsExactForLineStrings()
        {
            ILineString x = geometryFactory.CreateLineString(new Coordinate[] {
                        new Coordinate(0, 0), new Coordinate(100, 0),
                        new Coordinate(100, 100)
                    });
            ILineString somethingExactlyEqual = geometryFactory.CreateLineString(new Coordinate[] {
                        new Coordinate(0, 0), new Coordinate(100, 0),
                        new Coordinate(100, 100)
                    });
            ILineString somethingNotEqualButSameClass = geometryFactory.CreateLineString(new Coordinate[] {
                        new Coordinate(0, 0), new Coordinate(100, 0),
                        new Coordinate(100, 555)
                    });
            ILineString sameClassButEmpty = geometryFactory.CreateLineString((Coordinate[])null);
            ILineString anotherSameClassButEmpty = geometryFactory.CreateLineString((Coordinate[])null);
            LineCollectionFactory collectionFactory = new LineCollectionFactory();

            DoTestEqualsExact(x, somethingExactlyEqual,
                somethingNotEqualButSameClass, sameClassButEmpty,
                anotherSameClassButEmpty, collectionFactory);

            LineCollectionFactory collectionFactory2 = new LineCollectionFactory();

            DoTestEqualsExact(x, somethingExactlyEqual,
                somethingNotEqualButSameClass, sameClassButEmpty,
                anotherSameClassButEmpty, collectionFactory2);
        }

        [TestAttribute]
        public void TestEqualsExactForPoints()
        {
            IPoint x = geometryFactory.CreatePoint(new Coordinate(100, 100));
            IPoint somethingExactlyEqual = geometryFactory.CreatePoint(new Coordinate(
                        100, 100));
            IPoint somethingNotEqualButSameClass = geometryFactory.CreatePoint(new Coordinate(
                        999, 100));
            IPoint sameClassButEmpty = geometryFactory.CreatePoint((Coordinate)null);
            IPoint anotherSameClassButEmpty = geometryFactory.CreatePoint((Coordinate)null);
            PointCollectionFactory collectionFactory = new PointCollectionFactory();

            DoTestEqualsExact(x, somethingExactlyEqual,
                somethingNotEqualButSameClass, sameClassButEmpty,
                anotherSameClassButEmpty, collectionFactory);
        }

        [TestAttribute]
        public void TestEqualsExactForPolygons()
        {
            Polygon x = (Polygon) reader.Read(
                    "POLYGON ((0 0, 0 50, 50 50, 50 0, 0 0))");
            Polygon somethingExactlyEqual = (Polygon) reader.Read(
                    "POLYGON ((0 0, 0 50, 50 50, 50 0, 0 0))");
            Polygon somethingNotEqualButSameClass = (Polygon) reader.Read(
                    "POLYGON ((50 50, 50 0, 0 0, 0 50, 50 50))");
            Polygon sameClassButEmpty = (Polygon) reader.Read("POLYGON EMPTY");
            Polygon anotherSameClassButEmpty = (Polygon) reader.Read(
                    "POLYGON EMPTY");
            PolygonCollectionFactory collectionFactory = new PolygonCollectionFactory();

            DoTestEqualsExact(x, somethingExactlyEqual,
                somethingNotEqualButSameClass, sameClassButEmpty,
                anotherSameClassButEmpty, collectionFactory);
        }

        [TestAttribute]
        public void TestEqualsExactForGeometryCollections()
        {
            IGeometry polygon1 = (Polygon) reader.Read(
                    "POLYGON ((0 0, 0 50, 50 50, 50 0, 0 0))");
            IGeometry polygon2 = (Polygon) reader.Read(
                    "POLYGON ((50 50, 50 0, 0 0, 0 50, 50 50))");
            IGeometryCollection x = geometryFactory.CreateGeometryCollection(new IGeometry[] {
                        polygon1, polygon2
                    });
            IGeometryCollection somethingExactlyEqual = geometryFactory.CreateGeometryCollection(new IGeometry[] {
                        polygon1, polygon2
                    });
            IGeometryCollection somethingNotEqualButSameClass = geometryFactory.CreateGeometryCollection(new IGeometry[] {
                        polygon2
                    });
            IGeometryCollection sameClassButEmpty = geometryFactory.CreateGeometryCollection(null);
            IGeometryCollection anotherSameClassButEmpty = geometryFactory.CreateGeometryCollection(null);
            GeometryCollectionFactory collectionFactory = new GeometryCollectionFactory() ;

            DoTestEqualsExact(x, somethingExactlyEqual,
                somethingNotEqualButSameClass, sameClassButEmpty,
                anotherSameClassButEmpty, collectionFactory);
        }

        private void DoTestEqualsExact(IGeometry x, 
            IGeometry somethingExactlyEqual,
            IGeometry somethingNotEqualButSameClass,
            IGeometry sameClassButEmpty,
            IGeometry anotherSameClassButEmpty, 
            ICollectionFactory collectionFactory)
        {
            IGeometry emptyDifferentClass;

            if (x is Point) {
                emptyDifferentClass = geometryFactory.CreateGeometryCollection(null);
            } else {
                emptyDifferentClass = geometryFactory.CreatePoint((Coordinate)null);
            }

            IGeometry somethingEqualButNotExactly = geometryFactory.CreateGeometryCollection(new IGeometry[] { x });

            DoTestEqualsExact(x, somethingExactlyEqual,
                collectionFactory.CreateCollection(new IGeometry[] { x }, geometryFactory),
                somethingNotEqualButSameClass);

            DoTestEqualsExact(sameClassButEmpty, anotherSameClassButEmpty,
                emptyDifferentClass, x);


            /**
             * Test comparison of non-empty versus empty.
             */
            DoTestEqualsExact(x, somethingExactlyEqual,
                sameClassButEmpty, sameClassButEmpty);
        

            DoTestEqualsExact(collectionFactory.CreateCollection(
                    new IGeometry[] { x, x }, geometryFactory),
                collectionFactory.CreateCollection(
                    new IGeometry[] { x, somethingExactlyEqual }, geometryFactory),
                somethingEqualButNotExactly,
                collectionFactory.CreateCollection(
                    new IGeometry[] { x, somethingNotEqualButSameClass }, geometryFactory));
        }

        private void DoTestEqualsExact(IGeometry x, 
            IGeometry somethingExactlyEqual,
            IGeometry somethingEqualButNotExactly,
            IGeometry somethingNotEqualButSameClass)  {
            IGeometry differentClass;

            if (x is Point) {
                differentClass = reader.Read(
                        "POLYGON ((0 0, 0 50, 50 43949, 50 0, 0 0))");
            } else {
                differentClass = reader.Read("POINT ( 2351 1563 )");
            }

            Assert.IsTrue(x.EqualsExact(x));
            Assert.IsTrue(x.EqualsExact(somethingExactlyEqual));
            Assert.IsTrue(somethingExactlyEqual.EqualsExact(x));
            Assert.IsTrue(!x.EqualsExact(somethingEqualButNotExactly));
            Assert.IsTrue(!somethingEqualButNotExactly.EqualsExact(x));
            Assert.IsTrue(!x.EqualsExact(somethingEqualButNotExactly));
            Assert.IsTrue(!somethingEqualButNotExactly.EqualsExact(x));
            Assert.IsTrue(!x.EqualsExact(differentClass));
            Assert.IsTrue(!differentClass.EqualsExact(x));
        }

        private interface ICollectionFactory {
            IGeometry CreateCollection(IGeometry[] geometries, IGeometryFactory geometryFactory);
        }

        class GeometryCollectionFactory : ICollectionFactory
        {
            public IGeometry CreateCollection(IGeometry[] geometries, IGeometryFactory geometryFactory) {
                return geometryFactory.CreateGeometryCollection(geometries);
            }
        }

        class PointCollectionFactory : ICollectionFactory
        {
            public IGeometry CreateCollection(IGeometry[] geometries, IGeometryFactory geometryFactory) {
                return geometryFactory.CreateMultiPoint(GeometryFactory.ToPointArray(geometries));
            }
        }

        class LineCollectionFactory : ICollectionFactory
        {
            public IGeometry CreateCollection(IGeometry[] geometries, IGeometryFactory geometryFactory) {
                return geometryFactory.CreateMultiLineString(GeometryFactory.ToLineStringArray(geometries));
            }
        }

        class PolygonCollectionFactory : ICollectionFactory
        {
            public IGeometry CreateCollection(IGeometry[] geometries, IGeometryFactory geometryFactory) {
                return geometryFactory.CreateMultiPolygon(GeometryFactory.ToPolygonArray(geometries));
            }
        }

    }
}
