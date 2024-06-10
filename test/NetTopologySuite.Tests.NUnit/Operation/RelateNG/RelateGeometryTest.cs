using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.RelateNG;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.RelateNG
{
    public class RelateGeometryTest : GeometryTestCase
    {
        [Test]
        public void TestUniquePoints()
        {
            var geom = Read("MULTIPOINT ((0 0), (5 5), (5 0), (0 0))");
            var rgeom = new RelateGeometry(geom);
            var pts = rgeom.UniquePoints;
            Assert.That(pts.Count, Is.EqualTo(3), "Unique pts size");
        }

        [Test]
        public void TestBoundary()
        {
            var geom = Read("MULTILINESTRING ((0 0, 9 9), (9 9, 5 1))");
            var rgeom = new RelateGeometry(geom);
            Assert.That(rgeom.HasBoundary, "hasBoundary");
        }

        [Test]
        public void TestHasDimension()
        {
            var geom = Read("GEOMETRYCOLLECTION (POLYGON ((1 9, 5 9, 5 5, 1 5, 1 9)), LINESTRING (1 1, 5 4), POINT (6 5))");
            var rgeom = new RelateGeometry(geom);
            Assert.That(rgeom.HasDimension(Dimension.P), "HasDimension Dimension.P");
            Assert.That(rgeom.HasDimension(Dimension.L), "HasDimension Dimension.L");
            Assert.That(rgeom.HasDimension(Dimension.A), "HasDimension Dimension.A");
        }

        [Test]
        public void TestDimension()
        {
            CheckDimension("POINT (0 0)", Dimension.P, Dimension.P);
            CheckDimension("LINESTRING (0 0, 0 0)", Dimension.L, Dimension.P);
            CheckDimension("LINESTRING (0 0, 9 9)", Dimension.L, Dimension.L);
            CheckDimension("POLYGON ((1 9, 5 9, 5 5, 1 5, 1 9))", Dimension.A, Dimension.A);
            CheckDimension("GEOMETRYCOLLECTION (POLYGON ((1 9, 5 9, 5 5, 1 5, 1 9)), LINESTRING (1 1, 5 4), POINT (6 5))", Dimension.A, Dimension.A);
            CheckDimension("GEOMETRYCOLLECTION (POLYGON EMPTY, LINESTRING (1 1, 5 4), POINT (6 5))", Dimension.A, Dimension.L);
        }

        private void CheckDimension(string wkt, Dimension expectedDim, Dimension expectedDimReal)
        {
            var geom = Read(wkt);
            var rgeom = new RelateGeometry(geom);
            Assert.That(rgeom.Dimension, Is.EqualTo(expectedDim));
            Assert.That(rgeom.DimensionReal, Is.EqualTo(expectedDimReal));
        }

    }
}
