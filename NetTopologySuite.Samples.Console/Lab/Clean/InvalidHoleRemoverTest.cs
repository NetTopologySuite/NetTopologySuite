using GeoAPI.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Tests.NUnit;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Lab.Clean
{
    [TestFixture, Category("Lab")]
    public class InvalidHoleRemoverTest : GeometryTestCase
    {
        [Test]
        public void NoHole()
        {
            CheckHolesRemoved("POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))",
                "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))");
        }

        [Test]
        public void OneValid()
        {
            CheckHolesRemoved("POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9), (5 5, 5 2, 8 2, 5 5))",
                "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9), (5 5, 5 2, 8 2, 5 5))");
        }

        [Test]
        public void OneOutside()
        {
            CheckHolesRemoved("POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9), (11 5, 11 2, 14 2, 11 5))",
                "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))");
        }

        [Test]
        public void OneValidOneOutside()
        {
            CheckHolesRemoved("POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9), (11 5, 11 2, 14 2, 11 5), (2 5, 2 2, 5 2, 2 5))",
                "POLYGON ((1 1, 1 9, 9 9, 9 1, 1 1), (2 2, 5 2, 2 5, 2 2))");
        }

        [Test]
        public void OneOverlapping()
        {
            CheckHolesRemoved("POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9), (2 5, 2 2, 12 2, 2 5))",
                "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))");
        }

        [Test]
        public void OneOverlappingMP()
        {
            CheckHolesRemoved("MULTIPOLYGON (((1 9, 9 9, 9 1, 1 1, 1 9), (2 5, 2 2, 12 2, 2 5)), ((21 9, 25 9, 25 5, 21 5, 21 9)))",
                "MULTIPOLYGON (((1 9, 9 9, 9 1, 1 1, 1 9)), ((21 9, 25 9, 25 5, 21 5, 21 9)))");
        }

        [Test]
        public void OneOverlappingGC()
        {
            CheckHolesRemoved("GEOMETRYCOLLECTION (POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9), (2 5, 2 2, 12 2, 2 5)), LINESTRING (15 9, 19 5))",
                "GEOMETRYCOLLECTION (POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9)), LINESTRING (15 9, 19 5))");
        }

        [Test]
        private void CheckHolesRemoved(string inputWKT, string expectedWKT)
        {
            IGeometry input = read(inputWKT);
            IGeometry expected = read(expectedWKT);

            IGeometry actual = InvalidHoleRemover.Clean(input);
            CheckEqual(expected, actual);
        }

        private IGeometry read(string wkt)
        {
            WKTReader reader = new WKTReader();
            return reader.Read(wkt);
        }
    }
}
