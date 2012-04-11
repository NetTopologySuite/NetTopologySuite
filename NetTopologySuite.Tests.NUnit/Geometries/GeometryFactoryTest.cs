using System;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
/**
 * Tests for {@link GeometryFactory}.
 *
 * @version 1.13
 */

    [TestFixture]
    public class GeometryFactoryTest
    {
        private readonly static IPrecisionModel PrecModel = new PrecisionModel();
        private readonly static IGeometryFactory Factory = new GeometryFactory(PrecModel, 0);
        private readonly WKTReader _reader = new WKTReader(Factory);

        [Test]
        public void TestCreateGeometry()
        {
            CheckCreateGeometryExact("POINT EMPTY");
            CheckCreateGeometryExact("POINT ( 10 20 )");
            CheckCreateGeometryExact("LINESTRING EMPTY");
            CheckCreateGeometryExact("LINESTRING(0 0, 10 10)");
            CheckCreateGeometryExact("MULTILINESTRING ((50 100, 100 200), (100 100, 150 200))");
            CheckCreateGeometryExact("POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))");
            CheckCreateGeometryExact(
                "MULTIPOLYGON (((100 200, 200 200, 200 100, 100 100, 100 200)), ((300 200, 400 200, 400 100, 300 100, 300 200)))");
            CheckCreateGeometryExact(
                "GEOMETRYCOLLECTION (POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200)), LINESTRING (250 100, 350 200), POINT (350 150))");
        }

        [Test]
        public void TestDeepCopy()
        {
            var g = (IPoint) Read("POINT ( 10 10) ");
            var g2 = Factory.CreateGeometry(g);
            g.CoordinateSequence.SetOrdinate(0, 0, 99);
            Assert.IsTrue(!g.EqualsExact(g2));
        }

        private void CheckCreateGeometryExact(String wkt)
        {
            var g = Read(wkt);
            var g2 = Factory.CreateGeometry(g);
            Assert.IsTrue(g.EqualsExact(g2));
        }

        private IGeometry Read(String wkt)
        {
            return _reader.Read(wkt);
        }
    }
}