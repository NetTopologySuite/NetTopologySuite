using System.Globalization;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
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
        private readonly GeometryFactory _factory = new GeometryFactory(new PrecisionModel(1), 0, PackedCoordinateSequenceFactory.DoubleFactory);
        private readonly WKTWriter _writer = new WKTWriter();
        private readonly WKTWriter _writer3D = new WKTWriter(3);
        private readonly WKTWriter _writer2DM = new WKTWriter(3) { OutputOrdinates = Ordinates.XYM };

        private CultureInfo overriddenCurrentCulture;

        [SetUp]
        public void SetCurrentCulture()
        {
            this.overriddenCurrentCulture = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pt-BR");
        }

        [TearDown]
        public void ResetCurrentCulture()
        {
            CultureInfo.CurrentCulture = this.overriddenCurrentCulture;
        }

        [Test]
        public void TestWritePointWithDecimals()
        {
            var point = _factory.CreatePoint(new Coordinate(10.5, 10.5));
            Assert.AreEqual("POINT (10.5 10.5)", _writer.Write(point));
        }

        [Test]
        public void TestWritePoint()
        {
            var point = _factory.CreatePoint(new Coordinate(10, 10));
            Assert.AreEqual("POINT (10 10)", _writer.Write(point));
        }

        [Test]
        public void TestWriteLineString()
        {
            Coordinate[] coordinates =
            {
                new CoordinateZ(10, 10, 0),
                new CoordinateZ(20, 20, 0),
                new CoordinateZ(30, 40, 0)
            };
            var lineString = _factory.CreateLineString(coordinates);
            Assert.AreEqual("LINESTRING (10 10, 20 20, 30 40)", _writer.Write(lineString));
        }

        [Test]
        public void TestWritePolygon()
        {
            Coordinate[] coordinates =
            {
                new CoordinateZ(10, 10, 0),
                new CoordinateZ(10, 20, 0),
                new CoordinateZ(20, 20, 0),
                new CoordinateZ(20, 15, 0),
                new CoordinateZ(10, 10, 0)
            };
            var linearRing = _factory.CreateLinearRing(coordinates);
            var polygon = _factory.CreatePolygon(linearRing, new LinearRing[] { });
            Assert.AreEqual("POLYGON ((10 10, 10 20, 20 20, 20 15, 10 10))", _writer.Write(polygon));
        }

        [Test]
        public void TestWriteMultiPoint()
        {
            Point[] points =
            {
                _factory.CreatePoint(new CoordinateZ(10, 10, 0)),
                _factory.CreatePoint(new CoordinateZ(20, 20, 0))
            };
            var multiPoint = _factory.CreateMultiPoint(points);
            Assert.AreEqual("MULTIPOINT ((10 10), (20 20))", _writer.Write(multiPoint));
        }

        [Test]
        public void TestWriteMultiLineString()
        {
            Coordinate[] coordinates1 =
            {
                new CoordinateZ(10, 10, 0),
                new CoordinateZ(20, 20, 0)
            };
            var lineString1 = _factory.CreateLineString(coordinates1);
            Coordinate[] coordinates2 =
            {
                new CoordinateZ(15, 15, 0),
                new CoordinateZ(30, 15, 0)
            };
            var lineString2 = _factory.CreateLineString(coordinates2);
            LineString[] lineStrings = { lineString1, lineString2 };
            var multiLineString = _factory.CreateMultiLineString(lineStrings);
            Assert.AreEqual(
                "MULTILINESTRING ((10 10, 20 20), (15 15, 30 15))",
                _writer.Write(multiLineString));
        }

        [Test]
        public void TestWriteMultiPolygon()
        {
            Coordinate[] coordinates1 =
            {
                new CoordinateZ(10, 10, 0),
                new CoordinateZ(10, 20, 0),
                new CoordinateZ(20, 20, 0),
                new CoordinateZ(20, 15, 0),
                new CoordinateZ(10, 10, 0)
            };
            var linearRing1 = _factory.CreateLinearRing(coordinates1);
            var polygon1 = _factory.CreatePolygon(linearRing1, new LinearRing[] { });
            Coordinate[] coordinates2 =
            {
                new CoordinateZ(60, 60, 0),
                new CoordinateZ(70, 70, 0),
                new CoordinateZ(80, 60, 0),
                new CoordinateZ(60, 60, 0)
            };
            var linearRing2 = _factory.CreateLinearRing(coordinates2);
            var polygon2 = _factory.CreatePolygon(linearRing2, new LinearRing[] { });
            Polygon[] polygons = { polygon1, polygon2 };
            var multiPolygon = _factory.CreateMultiPolygon(polygons);
            Assert.AreEqual(
                "MULTIPOLYGON (((10 10, 10 20, 20 20, 20 15, 10 10)), ((60 60, 70 70, 80 60, 60 60)))",
                _writer.Write(multiPolygon));
        }

        [Test]
        public void TestWriteGeometryCollection()
        {
            var point1 = _factory.CreatePoint(new Coordinate(10, 10));
            var point2 = _factory.CreatePoint(new Coordinate(30, 30));
            Coordinate[] coordinates =
            {
                new CoordinateZ(15, 15, 0),
                new CoordinateZ(20, 20, 0)
            };
            var lineString1 = _factory.CreateLineString(coordinates);
            Geometry[] geometries = { point1, point2, lineString1 };
            var geometryCollection = _factory.CreateGeometryCollection(geometries);
            Assert.AreEqual(
                "GEOMETRYCOLLECTION (POINT (10 10), POINT (30 30), LINESTRING (15 15, 20 20))",
                _writer.Write(geometryCollection));
        }

        /// <summary>
        /// When writing "large numbers" (i.e: more than 15 digits), some precision is lost.
        /// </summary>
        /// <seealso href="https://code.google.com/p/nettopologysuite/issues/detail?id=171"/>
        /// <seealso href="http://stackoverflow.com/questions/2105096/why-is-tostring-rounding-my-double-value"/>
        [Test]
        public void TestWriteLargeNumbers1()
        {
            var precisionModel = new PrecisionModel(1E9);
            var geometryFactory = new GeometryFactory(precisionModel, 0);
            var point1 = geometryFactory.CreatePoint(new Coordinate(123456789012345678d, 10E9));
            Assert.AreEqual(123456789012345680d, point1.X);
            Assert.AreEqual(10000000000d, point1.Y);
            string actual = point1.AsText();
            Assert.AreNotEqual("POINT (123456789012345680 10000000000)", actual);
            Assert.AreEqual("POINT (123456789012346000 10000000000)", actual);
        }

        [Test]
        public void TestWriteLargeNumbers2()
        {
            var precisionModel = new PrecisionModel(1E9);
            var geometryFactory = new GeometryFactory(precisionModel, 0);
            var point1 = geometryFactory.CreatePoint(new Coordinate(1234d, 10E9));
            Assert.AreEqual("POINT (1234 10000000000)", point1.AsText());
        }

        /// <summary>
        /// When writing "large numbers" (i.e: more than 15 digits), some precision is lost.
        /// </summary>
        /// <seealso href="https://code.google.com/p/nettopologysuite/issues/detail?id=171"/>
        /// <seealso href="http://stackoverflow.com/questions/2105096/why-is-tostring-rounding-my-double-value"/>
        [Test]
        public void TestWriteLargeNumbers3()
        {
            var precisionModel = new PrecisionModel(1E9);
            var geometryFactory = new GeometryFactory(precisionModel, 0);
            var point1 = geometryFactory.CreatePoint(new Coordinate(123456789012345678000000E9d, 10E9));
            Assert.AreEqual(123456789012345690000000000000000d, point1.X);
            Assert.AreEqual(10000000000d, point1.Y);
            Assert.AreNotEqual("POINT (123456789012345690000000000000000 10000000000)", point1.AsText());
            Assert.AreEqual("POINT (123456789012346000000000000000000 10000000000)", point1.AsText());
        }

        [Test]
        public void TestWrite3D()
        {
            var geometryFactory = new GeometryFactory();
            var point = geometryFactory.CreatePoint(new CoordinateZ(1, 1, 1));
            string wkt = _writer3D.Write(point);
            Assert.AreEqual("POINT Z(1 1 1)", wkt);
            wkt = _writer2DM.Write(point);
            Assert.AreEqual("POINT (1 1)", wkt);
        }
        [Test]
        public void TestWrite3D_withNaN()
        {
            var geometryFactory = new GeometryFactory();
            Coordinate[] coordinates = { new Coordinate(1, 1),
                                 new CoordinateZ(2, 2, 2) };
            var line = geometryFactory.CreateLineString(coordinates);
            string wkt = _writer3D.Write(line);
            Assert.AreEqual("LINESTRING Z(1 1 NaN, 2 2 2)", wkt);
            wkt = _writer2DM.Write(line);
            Assert.AreEqual("LINESTRING (1 1, 2 2)", wkt);
        }

        [Test]
        public void TestProperties()
        {
            Assert.That(_writer.OutputOrdinates, Is.EqualTo(Ordinates.XY));
            Assert.That(_writer3D.OutputOrdinates, Is.EqualTo(Ordinates.XYZ));
            Assert.That(_writer2DM.OutputOrdinates, Is.EqualTo(Ordinates.XYM));

            var gf = new GeometryFactory(PackedCoordinateSequenceFactory.DoubleFactory);
            var writer3DM = new WKTWriter(4);
            Assert.That(writer3DM.OutputOrdinates, Is.EqualTo(Ordinates.XYZM));

            writer3DM.OutputOrdinates = Ordinates.XY;
            Assert.That(writer3DM.OutputOrdinates, Is.EqualTo(Ordinates.XY));
            writer3DM.OutputOrdinates = Ordinates.XYZ;
            Assert.That(writer3DM.OutputOrdinates, Is.EqualTo(Ordinates.XYZ));
            writer3DM.OutputOrdinates = Ordinates.XYM;
            Assert.That(writer3DM.OutputOrdinates, Is.EqualTo(Ordinates.XYM));
            writer3DM.OutputOrdinates = Ordinates.XYZM;
            Assert.That(writer3DM.OutputOrdinates, Is.EqualTo(Ordinates.XYZM));
        }

        [Test]
        public void TestMicrosoftSqlServer()
        {
            var writer = WKTWriter.ForMicrosoftSqlServer();

            var pt2D = GeometryFactory.Default.CreatePoint(new Coordinate(1, 2));
            var pt3D = GeometryFactory.Default.CreatePoint(new CoordinateZ(1, 2, 3));
            var pt2DM = GeometryFactory.Default.CreatePoint(new CoordinateM(1, 2, 4));
            var pt3DM = GeometryFactory.Default.CreatePoint(new CoordinateZM(1, 2, 3, 4));

            Assert.That(writer.Write(pt2D), Is.EqualTo("POINT (1 2)"));
            Assert.That(writer.Write(pt3D), Is.EqualTo("POINT (1 2 3)"));
            Assert.That(writer.Write(pt2DM), Is.EqualTo("POINT (1 2 NULL 4)"));
            Assert.That(writer.Write(pt3DM), Is.EqualTo("POINT (1 2 3 4)"));
        }
    }
}
