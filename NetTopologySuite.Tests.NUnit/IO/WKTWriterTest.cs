using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO
{
    /// <summary>
    /// Test for <see cref="WKTWriter"/>.
    /// </summary>
    /// <version>1.7</version>
    public class WKTWriterTest
    {
        private readonly IGeometryFactory _factory = new GeometryFactory(new PrecisionModel(1), 0);
        private readonly WKTWriter _writer = new WKTWriter();
        private readonly WKTWriter _writer3D = new WKTWriter(3);

        [TestAttribute]
        public void TestWritePoint()
        {
            IPoint point = _factory.CreatePoint(new Coordinate(10, 10));
            Assert.AreEqual("POINT (10 10)", _writer.Write(point));
        }

        [TestAttribute]
        public void TestWriteLineString()
        {
            Coordinate[] coordinates = 
            { 
                new Coordinate(10, 10, 0),
                new Coordinate(20, 20, 0),
                new Coordinate(30, 40, 0) 
            };
            ILineString lineString = _factory.CreateLineString(coordinates);
            Assert.AreEqual("LINESTRING (10 10, 20 20, 30 40)", _writer.Write(lineString));
        }

        [TestAttribute]
        public void TestWritePolygon()
        {
            Coordinate[] coordinates =
            {
                new Coordinate(10, 10, 0),
                new Coordinate(10, 20, 0),
                new Coordinate(20, 20, 0),
                new Coordinate(20, 15, 0),
                new Coordinate(10, 10, 0) 
            };
            ILinearRing linearRing = _factory.CreateLinearRing(coordinates);
            IPolygon polygon = _factory.CreatePolygon(linearRing, new LinearRing[] { });
            Assert.AreEqual("POLYGON ((10 10, 10 20, 20 20, 20 15, 10 10))", _writer.Write(polygon));
        }

        [TestAttribute]
        public void TestWriteMultiPoint()
        {
            IPoint[] points = 
            { 
                _factory.CreatePoint(new Coordinate(10, 10, 0)),
                _factory.CreatePoint(new Coordinate(20, 20, 0)) 
            };
            IMultiPoint multiPoint = _factory.CreateMultiPoint(points);
            Assert.AreEqual("MULTIPOINT ((10 10), (20 20))", _writer.Write(multiPoint));
        }

        [TestAttribute]
        public void TestWriteMultiLineString()
        {
            Coordinate[] coordinates1 = 
            {
                new Coordinate(10, 10, 0),
                new Coordinate(20, 20, 0) 
            };
            ILineString lineString1 = _factory.CreateLineString(coordinates1);
            Coordinate[] coordinates2 = 
            {
                new Coordinate(15, 15, 0),
                new Coordinate(30, 15, 0) 
            };
            ILineString lineString2 = _factory.CreateLineString(coordinates2);
            ILineString[] lineStrings = { lineString1, lineString2 };
            IMultiLineString multiLineString = _factory.CreateMultiLineString(lineStrings);
            Assert.AreEqual(
                "MULTILINESTRING ((10 10, 20 20), (15 15, 30 15))", 
                _writer.Write(multiLineString));
        }

        [TestAttribute]
        public void TestWriteMultiPolygon()
        {
            Coordinate[] coordinates1 = 
            { 
                new Coordinate(10, 10, 0),
                new Coordinate(10, 20, 0),
                new Coordinate(20, 20, 0),
                new Coordinate(20, 15, 0),
                new Coordinate(10, 10, 0) 
            };
            ILinearRing linearRing1 = _factory.CreateLinearRing(coordinates1);
            IPolygon polygon1 = _factory.CreatePolygon(linearRing1, new LinearRing[] { });
            Coordinate[] coordinates2 = 
            {
                new Coordinate(60, 60, 0),
                new Coordinate(70, 70, 0),
                new Coordinate(80, 60, 0),
                new Coordinate(60, 60, 0) 
            };
            ILinearRing linearRing2 = _factory.CreateLinearRing(coordinates2);
            IPolygon polygon2 = _factory.CreatePolygon(linearRing2, new LinearRing[] { });
            IPolygon[] polygons = { polygon1, polygon2 };
            IMultiPolygon multiPolygon = _factory.CreateMultiPolygon(polygons);
            Assert.AreEqual(
                "MULTIPOLYGON (((10 10, 10 20, 20 20, 20 15, 10 10)), ((60 60, 70 70, 80 60, 60 60)))", 
                _writer.Write(multiPolygon));
        }

        [TestAttribute]
        public void TestWriteGeometryCollection()
        {
            IPoint point1 = _factory.CreatePoint(new Coordinate(10, 10));
            IPoint point2 = _factory.CreatePoint(new Coordinate(30, 30));
            Coordinate[] coordinates =
            {
                new Coordinate(15, 15, 0),
                new Coordinate(20, 20, 0) 
            };
            ILineString lineString1 = _factory.CreateLineString(coordinates);
            IGeometry[] geometries = { point1, point2, lineString1 };
            IGeometryCollection geometryCollection = _factory.CreateGeometryCollection(geometries);
            Assert.AreEqual(
                "GEOMETRYCOLLECTION (POINT (10 10), POINT (30 30), LINESTRING (15 15, 20 20))", 
                _writer.Write(geometryCollection));
        }

        /// <summary>
        /// When writing "large numbers" (i.e: more than 15 digits), some precision is lost.
        /// </summary>
        /// <seealso href="https://code.google.com/p/nettopologysuite/issues/detail?id=171"/>
        /// <seealso href="http://stackoverflow.com/questions/2105096/why-is-tostring-rounding-my-double-value"/>
        [TestAttribute]
        public void TestWriteLargeNumbers1()
        {
            IPrecisionModel precisionModel = new PrecisionModel(1E9);
            IGeometryFactory geometryFactory = new GeometryFactory(precisionModel, 0);
            IPoint point1 = geometryFactory.CreatePoint(new Coordinate(123456789012345678d, 10E9));
            Assert.AreEqual(123456789012345680d, point1.X);
            Assert.AreEqual(10000000000d, point1.Y);
            string actual = point1.AsText();
            Assert.AreNotEqual("POINT (123456789012345680 10000000000)", actual);
            Assert.AreEqual("POINT (123456789012346000 10000000000)", actual);            
        }

        [TestAttribute]
        public void TestWriteLargeNumbers2()
        {
            IPrecisionModel precisionModel = new PrecisionModel(1E9);
            IGeometryFactory geometryFactory = new GeometryFactory(precisionModel, 0);
            IPoint point1 = geometryFactory.CreatePoint(new Coordinate(1234d, 10E9));
            Assert.AreEqual("POINT (1234 10000000000)", point1.AsText());
        }

        /// <summary>
        /// When writing "large numbers" (i.e: more than 15 digits), some precision is lost.
        /// </summary>
        /// <seealso href="https://code.google.com/p/nettopologysuite/issues/detail?id=171"/>
        /// <seealso href="http://stackoverflow.com/questions/2105096/why-is-tostring-rounding-my-double-value"/>
        [TestAttribute]
        public void TestWriteLargeNumbers3()
        {
            IPrecisionModel precisionModel = new PrecisionModel(1E9);
            IGeometryFactory geometryFactory = new GeometryFactory(precisionModel, 0);
            IPoint point1 = geometryFactory.CreatePoint(new Coordinate(123456789012345678000000E9d, 10E9));
            Assert.AreEqual(123456789012345690000000000000000d, point1.X);
            Assert.AreEqual(10000000000d, point1.Y);
            Assert.AreNotEqual("POINT (123456789012345690000000000000000 10000000000)", point1.AsText());
            Assert.AreEqual("POINT (123456789012346000000000000000000 10000000000)", point1.AsText());
        }

        [TestAttribute]
        public void TestWrite3D()
        {
            IGeometryFactory geometryFactory = new GeometryFactory();
            IPoint point = geometryFactory.CreatePoint(new Coordinate(1, 1, 1));
            String wkt = _writer3D.Write(point);
            Assert.AreEqual("POINT (1 1 1)", wkt);
        }
        [TestAttribute]
        public void testWrite3D_withNaN()
        {
            IGeometryFactory geometryFactory = new GeometryFactory();
            Coordinate[] coordinates = { new Coordinate(1, 1),
                                 new Coordinate(2, 2, 2) };
            ILineString line = geometryFactory.CreateLineString(coordinates);
            String wkt = _writer3D.Write(line);
            Assert.AreEqual("LINESTRING (1 1, 2 2 2)", wkt);
        }
    }
}