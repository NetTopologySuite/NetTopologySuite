using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixtureAttribute]
    public class NormalizeTest
    {
        private IPrecisionModel precisionModel;
        private IGeometryFactory geometryFactory;
        WKTReader reader;

        public NormalizeTest()
        {
            precisionModel = new PrecisionModel(1);
            geometryFactory = new GeometryFactory(precisionModel, 0);
            reader = new WKTReader(geometryFactory);
        }

        [TestAttribute]
        public void TestNormalizePoint()
        {
            var point = (Point)reader.Read("POINT (30 30)");
            point.Normalize();
            Assert.AreEqual(new Coordinate(30, 30), point.Coordinate);
        }

        [TestAttribute]
        public void TestNormalizeEmptyPoint()
        {
            var point = (Point)reader.Read("POINT EMPTY");
            point.Normalize();
            Assert.AreEqual(null, point.Coordinate);
        }

        [TestAttribute]
        public void TestComparePoint()
        {
            var p1 = (Point)reader.Read("POINT (30 30)");
            var p2 = (Point)reader.Read("POINT (30 40)");
            Assert.IsTrue(p1.CompareTo(p2) < 0);
        }

        [TestAttribute]
        public void TestCompareEmptyPoint()
        {
            var p1 = (Point)reader.Read("POINT (30 30)");
            var p2 = (Point)reader.Read("POINT EMPTY");
            Assert.IsTrue(p1.CompareTo(p2) > 0);
        }

        [TestAttribute]
        public void TestNormalizeMultiPoint()
        {
            var m = (MultiPoint)reader.Read(
                    "MULTIPOINT(30 20, 10 10, 20 20, 30 30, 20 10)");
            m.Normalize();
            var expectedValue = (MultiPoint)reader.Read(
                    "MULTIPOINT(10 10, 20 10, 20 20, 30 20, 30 30)");
            AssertAreEqualExact(expectedValue, m);
            var unexpectedValue = (MultiPoint)reader.Read(
                    "MULTIPOINT(20 10, 20 20, 30 20, 30 30, 10 10)");
            Assert.IsTrue(!m.EqualsExact(unexpectedValue));
        }

        [TestAttribute]
        public void TestNormalizeLineString1()
        {
            var l = (LineString)reader.Read(
                    "LINESTRING (20 20, 160 40, 160 100, 100 120, 60 60)");
            l.Normalize();
            var expectedValue = (LineString)reader.Read(
                    "LINESTRING (20 20, 160 40, 160 100, 100 120, 60 60)");
            AssertAreEqualExact(expectedValue, l);
        }

        [TestAttribute]
        public void TestNormalizeLineString2()
        {
            var l = (LineString)reader.Read(
                  "LINESTRING (20 20, 160 40, 160 100, 100 120, 60 60)");
            l.Normalize();
            var expectedValue = (LineString)reader.Read(
                  "LINESTRING (20 20, 160 40, 160 100, 100 120, 60 60)");
            AssertAreEqualExact(expectedValue, l);
        }

        [TestAttribute]
        public void TestNormalizeLineString3()
        {
            var l = (LineString)reader.Read(
                  "LINESTRING (200 240, 140 160, 80 160, 160 80, 80 80)");
            l.Normalize();
            var expectedValue = (LineString)reader.Read(
                  "LINESTRING (80 80, 160 80, 80 160, 140 160, 200 240)");
            AssertAreEqualExact(expectedValue, l);
        }

        [TestAttribute]
        public void TestNormalizeLineString4()
        {
            var l = (LineString)reader.Read(
                  "LINESTRING (200 240, 140 160, 80 160, 160 80, 80 80)");
            l.Normalize();
            var expectedValue = (LineString)reader.Read(
                  "LINESTRING (80 80, 160 80, 80 160, 140 160, 200 240)");
            AssertAreEqualExact(expectedValue, l);
        }

        [TestAttribute]
        public void TestNormalizeLineString5()
        {
            var l = (LineString)reader.Read(
                  "LINESTRING (200 340, 140 240, 140 160, 60 240, 140 240, 200 340)");
            l.Normalize();
            var expectedValue = (LineString)reader.Read(
                  "LINESTRING (200 340, 140 240, 60 240, 140 160, 140 240, 200 340)");
            AssertAreEqualExact(expectedValue, l);
        }

        [Test]
        public void TestNormalizeStringNoSideEffect()
        {
            var l = (ILineString) reader.Read("LINESTRING (200 240, 140 160, 80 160, 160 80, 80 80)");
            var refL = (ILineString) reader.Read("LINESTRING (200 240, 140 160)");
            var seg = l.Factory.CreateLineString(new Coordinate[] { l.GetCoordinateN(0), l.GetCoordinateN(1) });
            Assert.That(refL.EqualsExact(seg), Is.True);
            l.Normalize();
            Assert.That(refL.EqualsExact(seg), Is.True);
        }

        [TestAttribute]
        public void TestNormalizeEmptyLineString()
        {
            var l = (LineString)reader.Read("LINESTRING EMPTY");
            l.Normalize();
            var expectedValue = (LineString)reader.Read("LINESTRING EMPTY");
            AssertAreEqualExact(expectedValue, l);
        }

        [TestAttribute]
        public void TestNormalizeEmptyPolygon()
        {
            var actualValue = (Polygon)reader.Read("POLYGON EMPTY");
            actualValue.Normalize();
            var expectedValue = (Polygon)reader.Read("POLYGON EMPTY");
            AssertAreEqualExact(expectedValue, actualValue);
        }

        [TestAttribute]
        public void TestNormalizePolygon1()
        {
            var actualValue = (Polygon)reader.Read(
                  "POLYGON ((120 320, 240 200, 120 80, 20 200, 120 320), (60 200, 80 220, 80 200, 60 200), (160 200, 180 200, 180 220, 160 200), (120 140, 140 140, 140 160, 120 140), (140 240, 140 220, 120 260, 140 240))");
            actualValue.Normalize();
            var expectedValue = (Polygon)reader.Read(
                  "POLYGON ((20 200, 120 320, 240 200, 120 80, 20 200), (60 200, 80 200, 80 220, 60 200), (120 140, 140 140, 140 160, 120 140), (120 260, 140 220, 140 240, 120 260), (160 200, 180 200, 180 220, 160 200))");
            AssertAreEqualExact(expectedValue, actualValue);
        }

        [TestAttribute]
        public void TestNormalizeMultiLineString()
        {
            var actualValue = (MultiLineString)reader.Read(
                  "MULTILINESTRING ((200 260, 180 320, 260 340), (120 180, 140 100, 40 80), (200 180, 220 160, 200 180), (100 280, 120 260, 140 260, 140 240, 120 240, 120 260, 100 280))");
            actualValue.Normalize();
            var expectedValue = (MultiLineString)reader.Read(
                  "MULTILINESTRING ((40 80, 140 100, 120 180), (100 280, 120 260, 120 240, 140 240, 140 260, 120 260, 100 280), (200 180, 220 160, 200 180), (200 260, 180 320, 260 340))");
            AssertAreEqualExact(expectedValue, actualValue);
        }

        [TestAttribute]
        public void TestNormalizeMultiPolygon()
        {
            var actualValue = (MultiPolygon)reader.Read(
                  "MULTIPOLYGON (((40 360, 40 280, 140 280, 140 360, 40 360), (60 340, 60 300, 120 300, 120 340, 60 340)), ((140 200, 260 200, 260 100, 140 100, 140 200), (160 180, 240 180, 240 120, 160 120, 160 180)))");
            actualValue.Normalize();
            var expectedValue = (MultiPolygon)reader.Read(
                  "MULTIPOLYGON (((40 280, 40 360, 140 360, 140 280, 40 280), (60 300, 120 300, 120 340, 60 340, 60 300)), ((140 100, 140 200, 260 200, 260 100, 140 100), (160 120, 240 120, 240 180, 160 180, 160 120)))");
            AssertAreEqualExact(expectedValue, actualValue);
        }

        [TestAttribute]
        public void TestNormalizeGeometryCollection()
        {
            var actualValue = (GeometryCollection)reader.Read(
                  "GEOMETRYCOLLECTION (LINESTRING (200 300, 200 280, 220 280, 220 320, 180 320), POINT (140 220), POLYGON ((100 80, 100 160, 20 160, 20 80, 100 80), (40 140, 40 100, 80 100, 80 140, 40 140)), POINT (100 240))");
            actualValue.Normalize();
            var expectedValue = (GeometryCollection)reader.Read(
                  "GEOMETRYCOLLECTION (POINT (100 240), POINT (140 220), LINESTRING (180 320, 220 320, 220 280, 200 280, 200 300), POLYGON ((20 80, 20 160, 100 160, 100 80, 20 80), (40 100, 80 100, 80 140, 40 140, 40 100)))");
            AssertAreEqualExact(expectedValue, actualValue);
        }

        private void AssertAreEqualExact(IGeometry expectedValue, IGeometry actualValue)
        {
            Assert.IsTrue(actualValue.EqualsExact(expectedValue), "Expected " + expectedValue + " but encountered " + actualValue);
        }

        [TestAttribute]
        public void TestNormalizePackedCoordinateSequence()
        {
            var pcsFactory = new GeometryFactory(PackedCoordinateSequenceFactory.DoubleFactory);
            var pcsReader = new WKTReader(pcsFactory);
            var geom = pcsReader.Read("LINESTRING (100 100, 0 0)");
            geom.Normalize();
            // force PackedCoordinateSequence to be copied with empty coordinate cache
            var clone = (IGeometry) geom.Copy();
            AssertAreEqualExact(geom, clone);
        }

    }
}
