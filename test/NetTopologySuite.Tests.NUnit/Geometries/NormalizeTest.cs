using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixture]
    public class NormalizeTest
    {
        private readonly WKTReader _reader;

        public NormalizeTest()
        {
            var gs = new NtsGeometryServices(PrecisionModel.Fixed.Value, 0);
            _reader = new WKTReader(gs);
        }

        [Test]
        public void TestNormalizePoint()
        {
            var point = (Point)_reader.Read("POINT (30 30)");
            point.Normalize();
            Assert.AreEqual(new Coordinate(30, 30), point.Coordinate);
        }

        [Test]
        public void TestNormalizeEmptyPoint()
        {
            var point = (Point)_reader.Read("POINT EMPTY");
            point.Normalize();
            Assert.AreEqual(null, point.Coordinate);
        }

        [Test]
        public void TestComparePoint()
        {
            var p1 = (Point)_reader.Read("POINT (30 30)");
            var p2 = (Point)_reader.Read("POINT (30 40)");
            Assert.IsTrue(p1.CompareTo(p2) < 0);
        }

        [Test]
        public void TestCompareEmptyPoint()
        {
            var p1 = (Point)_reader.Read("POINT (30 30)");
            var p2 = (Point)_reader.Read("POINT EMPTY");
            Assert.IsTrue(p1.CompareTo(p2) > 0);
        }

        [Test]
        public void TestNormalizeMultiPoint()
        {
            var m = (MultiPoint)_reader.Read(
                    "MULTIPOINT(30 20, 10 10, 20 20, 30 30, 20 10)");
            m.Normalize();
            var expectedValue = (MultiPoint)_reader.Read(
                    "MULTIPOINT(10 10, 20 10, 20 20, 30 20, 30 30)");
            AssertAreEqualExact(expectedValue, m);
            var unexpectedValue = (MultiPoint)_reader.Read(
                    "MULTIPOINT(20 10, 20 20, 30 20, 30 30, 10 10)");
            Assert.IsTrue(!m.EqualsExact(unexpectedValue));
        }

        [Test]
        public void TestNormalizeLineString1()
        {
            var l = (LineString)_reader.Read(
                    "LINESTRING (20 20, 160 40, 160 100, 100 120, 60 60)");
            l.Normalize();
            var expectedValue = (LineString)_reader.Read(
                    "LINESTRING (20 20, 160 40, 160 100, 100 120, 60 60)");
            AssertAreEqualExact(expectedValue, l);
        }

        [Test]
        public void TestNormalizeLineString2()
        {
            var l = (LineString)_reader.Read(
                  "LINESTRING (20 20, 160 40, 160 100, 100 120, 60 60)");
            l.Normalize();
            var expectedValue = (LineString)_reader.Read(
                  "LINESTRING (20 20, 160 40, 160 100, 100 120, 60 60)");
            AssertAreEqualExact(expectedValue, l);
        }

        [Test]
        public void TestNormalizeLineString3()
        {
            var l = (LineString)_reader.Read(
                  "LINESTRING (200 240, 140 160, 80 160, 160 80, 80 80)");
            l.Normalize();
            var expectedValue = (LineString)_reader.Read(
                  "LINESTRING (80 80, 160 80, 80 160, 140 160, 200 240)");
            AssertAreEqualExact(expectedValue, l);
        }

        [Test]
        public void TestNormalizeLineString4()
        {
            var l = (LineString)_reader.Read(
                  "LINESTRING (200 240, 140 160, 80 160, 160 80, 80 80)");
            l.Normalize();
            var expectedValue = (LineString)_reader.Read(
                  "LINESTRING (80 80, 160 80, 80 160, 140 160, 200 240)");
            AssertAreEqualExact(expectedValue, l);
        }

        [Test]
        public void TestNormalizeLineString5()
        {
            var l = (LineString)_reader.Read(
                  "LINESTRING (200 340, 140 240, 140 160, 60 240, 140 240, 200 340)");
            l.Normalize();
            var expectedValue = (LineString)_reader.Read(
                  "LINESTRING (200 340, 140 240, 60 240, 140 160, 140 240, 200 340)");
            AssertAreEqualExact(expectedValue, l);
        }

        [Test]
        public void TestNormalizeStringNoSideEffect()
        {
            var l = (LineString) _reader.Read("LINESTRING (200 240, 140 160, 80 160, 160 80, 80 80)");
            var refL = (LineString) _reader.Read("LINESTRING (200 240, 140 160)");
            var seg = l.Factory.CreateLineString(new Coordinate[] { l.GetCoordinateN(0), l.GetCoordinateN(1) });
            Assert.That(refL.EqualsExact(seg), Is.True);
            l.Normalize();
            Assert.That(refL.EqualsExact(seg), Is.True);
        }

        [Test]
        public void TestNormalizeEmptyLineString()
        {
            var l = (LineString)_reader.Read("LINESTRING EMPTY");
            l.Normalize();
            var expectedValue = (LineString)_reader.Read("LINESTRING EMPTY");
            AssertAreEqualExact(expectedValue, l);
        }

        [Test]
        public void TestNormalizeEmptyPolygon()
        {
            var actualValue = (Polygon)_reader.Read("POLYGON EMPTY");
            actualValue.Normalize();
            var expectedValue = (Polygon)_reader.Read("POLYGON EMPTY");
            AssertAreEqualExact(expectedValue, actualValue);
        }

        [Test]
        public void TestNormalizePolygon1()
        {
            var actualValue = (Polygon)_reader.Read(
                  "POLYGON ((120 320, 240 200, 120 80, 20 200, 120 320), (60 200, 80 220, 80 200, 60 200), (160 200, 180 200, 180 220, 160 200), (120 140, 140 140, 140 160, 120 140), (140 240, 140 220, 120 260, 140 240))");
            actualValue.Normalize();
            var expectedValue = (Polygon)_reader.Read(
                  "POLYGON ((20 200, 120 320, 240 200, 120 80, 20 200), (60 200, 80 200, 80 220, 60 200), (120 140, 140 140, 140 160, 120 140), (120 260, 140 220, 140 240, 120 260), (160 200, 180 200, 180 220, 160 200))");
            AssertAreEqualExact(expectedValue, actualValue);
        }

        [Test]
        public void TestNormalizeMultiLineString()
        {
            var actualValue = (MultiLineString)_reader.Read(
                  "MULTILINESTRING ((200 260, 180 320, 260 340), (120 180, 140 100, 40 80), (200 180, 220 160, 200 180), (100 280, 120 260, 140 260, 140 240, 120 240, 120 260, 100 280))");
            actualValue.Normalize();
            var expectedValue = (MultiLineString)_reader.Read(
                  "MULTILINESTRING ((40 80, 140 100, 120 180), (100 280, 120 260, 120 240, 140 240, 140 260, 120 260, 100 280), (200 180, 220 160, 200 180), (200 260, 180 320, 260 340))");
            AssertAreEqualExact(expectedValue, actualValue);
        }

        [Test]
        public void TestNormalizeMultiPolygon()
        {
            var actualValue = (MultiPolygon)_reader.Read(
                  "MULTIPOLYGON (((40 360, 40 280, 140 280, 140 360, 40 360), (60 340, 60 300, 120 300, 120 340, 60 340)), ((140 200, 260 200, 260 100, 140 100, 140 200), (160 180, 240 180, 240 120, 160 120, 160 180)))");
            actualValue.Normalize();
            var expectedValue = (MultiPolygon)_reader.Read(
                  "MULTIPOLYGON (((40 280, 40 360, 140 360, 140 280, 40 280), (60 300, 120 300, 120 340, 60 340, 60 300)), ((140 100, 140 200, 260 200, 260 100, 140 100), (160 120, 240 120, 240 180, 160 180, 160 120)))");
            AssertAreEqualExact(expectedValue, actualValue);
        }

        [Test]
        public void TestNormalizeGeometryCollection()
        {
            var actualValue = (GeometryCollection)_reader.Read(
                  "GEOMETRYCOLLECTION (LINESTRING (200 300, 200 280, 220 280, 220 320, 180 320), POINT (140 220), POLYGON ((100 80, 100 160, 20 160, 20 80, 100 80), (40 140, 40 100, 80 100, 80 140, 40 140)), POINT (100 240))");
            actualValue.Normalize();
            var expectedValue = (GeometryCollection)_reader.Read(
                  "GEOMETRYCOLLECTION (POINT (100 240), POINT (140 220), LINESTRING (180 320, 220 320, 220 280, 200 280, 200 300), POLYGON ((20 80, 20 160, 100 160, 100 80, 20 80), (40 100, 80 100, 80 140, 40 140, 40 100)))");
            AssertAreEqualExact(expectedValue, actualValue);
        }

        private void AssertAreEqualExact(Geometry expectedValue, Geometry actualValue)
        {
            Assert.IsTrue(actualValue.EqualsExact(expectedValue), "Expected " + expectedValue + " but encountered " + actualValue);
        }

        [Test]
        public void TestNormalizePackedCoordinateSequence()
        {
            var pcsGs = new NtsGeometryServices(PackedCoordinateSequenceFactory.DoubleFactory);
            var pcsReader = new WKTReader(pcsGs);
            var geom = pcsReader.Read("LINESTRING (100 100, 0 0)");
            geom.Normalize();
            // force PackedCoordinateSequence to be copied with empty coordinate cache
            var clone = (Geometry) geom.Copy();
            AssertAreEqualExact(geom, clone);
        }

    }
}
