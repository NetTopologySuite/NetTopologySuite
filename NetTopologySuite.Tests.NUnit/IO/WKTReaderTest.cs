using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO
{
    /// <summary>
    /// Test for <see cref="WKTReader" />
    /// </summary>
    [TestFixture]
    public class WKTReaderTest
    {
        WKTWriter writer = new WKTWriter();
        private IPrecisionModel precisionModel;
        private IGeometryFactory geometryFactory;
        WKTReader reader;

        public WKTReaderTest()
        {
            precisionModel = new PrecisionModel(1);
            geometryFactory = new GeometryFactory(precisionModel, 0);
            reader = new WKTReader(geometryFactory);
        }

        [Test]
        public void TestReadNaN()
        {
            Assert.AreEqual("POINT (10 10)", writer.Write(reader.Read("POINT (10 10 NaN)")));
            Assert.AreEqual("POINT (10 10)", writer.Write(reader.Read("POINT (10 10 nan)")));
            Assert.AreEqual("POINT (10 10)", writer.Write(reader.Read("POINT (10 10 NAN)")));
        }

        [Test]
        public void TestReadPoint()
        {
            Assert.AreEqual("POINT (10 10)", writer.Write(reader.Read("POINT (10 10)")));
            Assert.AreEqual("POINT EMPTY", writer.Write(reader.Read("POINT EMPTY")));
        }

        [Test]
        public void TestReadLineString()
        {
            Assert.AreEqual("LINESTRING (10 10, 20 20, 30 40)", writer.Write(reader.Read("LINESTRING (10 10, 20 20, 30 40)")));
            Assert.AreEqual("LINESTRING EMPTY", writer.Write(reader.Read("LINESTRING EMPTY")));
        }

        [Test]
        public void TestReadLinearRing()
        {
            try
            {
                reader.Read("LINEARRING (10 10, 20 20, 30 40, 10 99)");
            }
            catch (ArgumentException e)
            {
                Assert.IsTrue(e.Message.IndexOf("must form a closed linestring") > -1);
            }
            Assert.AreEqual("LINEARRING (10 10, 20 20, 30 40, 10 10)", writer.Write(reader.Read("LINEARRING (10 10, 20 20, 30 40, 10 10)")));
            Assert.AreEqual("LINEARRING EMPTY", writer.Write(reader.Read("LINEARRING EMPTY")));
        }

        [Test]
        public void TestReadPolygon()
        {
            Assert.AreEqual("POLYGON ((10 10, 10 20, 20 20, 20 15, 10 10))", writer.Write(reader.Read("POLYGON ((10 10, 10 20, 20 20, 20 15, 10 10))")));
            Assert.AreEqual("POLYGON EMPTY", writer.Write(reader.Read("POLYGON EMPTY")));
        }

        [Test]
        public void TestReadMultiPoint()
        {
            Assert.AreEqual("MULTIPOINT ((10 10), (20 20))", writer.Write(reader.Read("MULTIPOINT ((10 10), (20 20))")));
            Assert.AreEqual("MULTIPOINT EMPTY", writer.Write(reader.Read("MULTIPOINT EMPTY")));
        }

        [Test]
        public void TestReadMultiLineString()
        {
            Assert.AreEqual("MULTILINESTRING ((10 10, 20 20), (15 15, 30 15))", writer.Write(reader.Read("MULTILINESTRING ((10 10, 20 20), (15 15, 30 15))")));
            Assert.AreEqual("MULTILINESTRING EMPTY", writer.Write(reader.Read("MULTILINESTRING EMPTY")));
        }

        [Test]
        public void TestReadMultiPolygon()
        {
            Assert.AreEqual("MULTIPOLYGON (((10 10, 10 20, 20 20, 20 15, 10 10)), ((60 60, 70 70, 80 60, 60 60)))", writer.Write(reader.Read("MULTIPOLYGON (((10 10, 10 20, 20 20, 20 15, 10 10)), ((60 60, 70 70, 80 60, 60 60)))")));
            Assert.AreEqual("MULTIPOLYGON EMPTY", writer.Write(reader.Read("MULTIPOLYGON EMPTY")));
        }

        [Test]
        public void TestReadGeometryCollection()
        {
            Assert.AreEqual("GEOMETRYCOLLECTION (POINT (10 10), POINT (30 30), LINESTRING (15 15, 20 20))", writer.Write(reader.Read("GEOMETRYCOLLECTION (POINT (10 10), POINT (30 30), LINESTRING (15 15, 20 20))")));
            Assert.AreEqual("GEOMETRYCOLLECTION (POINT (10 10), LINEARRING EMPTY, LINESTRING (15 15, 20 20))", writer.Write(reader.Read("GEOMETRYCOLLECTION (POINT (10 10), LINEARRING EMPTY, LINESTRING (15 15, 20 20))")));
            Assert.AreEqual("GEOMETRYCOLLECTION (POINT (10 10), LINEARRING (10 10, 20 20, 30 40, 10 10), LINESTRING (15 15, 20 20))", writer.Write(reader.Read("GEOMETRYCOLLECTION (POINT (10 10), LINEARRING (10 10, 20 20, 30 40, 10 10), LINESTRING (15 15, 20 20))")));
            Assert.AreEqual("GEOMETRYCOLLECTION EMPTY", writer.Write(reader.Read("GEOMETRYCOLLECTION EMPTY")));
        }

        [Test]
        public void TestReadZ()
        {
            Assert.AreEqual(new Coordinate(1, 2, 3), reader.Read("POINT (1 2 3)").Coordinate);
        }

        [Test]
        public void TestReadLargeNumbers()
        {
            PrecisionModel precisionModel = new PrecisionModel(1E9);
            GeometryFactory geometryFactory = new GeometryFactory(precisionModel, 0);
            WKTReader reader = new WKTReader(geometryFactory);
            IGeometry point1 = reader.Read("POINT (123456789.01234567890 10)");
            IPoint point2 = geometryFactory.CreatePoint(new Coordinate(123456789.01234567890, 10));
            Assert.AreEqual(point1.Coordinate.X, point2.Coordinate.X, 1E-7);
            Assert.AreEqual(point1.Coordinate.Y, point2.Coordinate.Y, 1E-7);
        }
    }
}
