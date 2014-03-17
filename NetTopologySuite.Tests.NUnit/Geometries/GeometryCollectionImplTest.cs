using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixtureAttribute]
    public class GeometryCollectionImplTest
    {
        private IPrecisionModel precisionModel;
        private IGeometryFactory geometryFactory;
        WKTReader reader;

        public GeometryCollectionImplTest()
        {
            precisionModel = new PrecisionModel(1000);
            geometryFactory = new GeometryFactory(precisionModel, 0);
            reader = new WKTReader(geometryFactory);
        }

        [TestAttribute]
        public void TestGetDimension()
        {
            GeometryCollection g = (GeometryCollection)reader.Read("GEOMETRYCOLLECTION (POINT (10 10), POINT (30 30), LINESTRING (15 15, 20 20))");
            Assert.AreEqual(1, (int)g.Dimension);
        }

        [TestAttribute]
        public void TestGetCoordinates()
        {
            GeometryCollection g = (GeometryCollection)reader.Read("GEOMETRYCOLLECTION (POINT (10 10), POINT (30 30), LINESTRING (15 15, 20 20))");
            Coordinate[] coordinates = g.Coordinates;
            Assert.AreEqual(4, g.NumPoints);
            Assert.AreEqual(4, coordinates.Length);
            Assert.AreEqual(new Coordinate(10, 10), coordinates[0]);
            Assert.AreEqual(new Coordinate(20, 20), coordinates[3]);
        }

        [TestAttribute]
        public void TestGeometryCollectionIterator()
        {
            GeometryCollection g = (GeometryCollection)reader.Read(
                  "GEOMETRYCOLLECTION (GEOMETRYCOLLECTION (POINT (10 10)))");
            GeometryCollectionEnumerator i = new GeometryCollectionEnumerator(g);
            //The NTS GeometryCollectionEnumerator does not have a HasNext property, and the interfaces is slightly different
            //assertTrue(i.hasNext());
            //assertTrue(i.next() instanceof GeometryCollection);
            //assertTrue(i.next() instanceof GeometryCollection);
            //assertTrue(i.next() instanceof Point);
            Assert.IsTrue(i.MoveNext());
            Assert.IsTrue(i.Current is GeometryCollection);
            Assert.IsTrue(i.MoveNext());
            Assert.IsTrue(i.Current is GeometryCollection);
            Assert.IsTrue(i.MoveNext());
            Assert.IsTrue(i.Current is Point);
        }

        [TestAttribute]
        public void TestGetLength()
        {
            GeometryCollection g = (GeometryCollection)new WKTReader().Read(
                  "MULTIPOLYGON("
                  + "((0 0, 10 0, 10 10, 0 10, 0 0), (3 3, 3 7, 7 7, 7 3, 3 3)),"
                  + "((100 100, 110 100, 110 110, 100 110, 100 100), (103 103, 103 107, 107 107, 107 103, 103 103)))");
            Assert.AreEqual(112, g.Length, 1E-15);
        }
    }
}
