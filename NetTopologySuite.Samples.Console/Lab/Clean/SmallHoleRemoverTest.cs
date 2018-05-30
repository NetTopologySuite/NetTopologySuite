using System;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Tests.NUnit;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Lab.Clean
{
    [TestFixture, Category("Lab")]
    public class SmallHoleRemoverTest : GeometryTestCase
    {
        [Test]
        public void TestNoHole()
        {
            CheckHolesRemoved("POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))",
                "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))");
        }

        [Test]
        public void TestOneLarge()
        {
            CheckHolesRemoved("POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200), (130 180, 175 180, 175 136, 130 136, 130 180))",
                "POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200), (130 180, 175 180, 175 136, 130 136, 130 180))");
        }

        [Test]
        public void TestOneSmall()
        {
            CheckHolesRemoved("POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200), (130 160, 140 150, 130 150, 130 160))",
                "POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))");
        }

        [Test]
        public void TestOneLargeOneSmall()
        {
            CheckHolesRemoved("POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200), (130 160, 140 150, 130 150, 130 160), (150 190, 190 190, 190 150, 150 150, 150 190))",
                "POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200), (150 190, 190 190, 190 150, 150 150, 150 190))");
        }

        [Test]
        public void TestOneSmallMp()
        {
            CheckHolesRemoved("MULTIPOLYGON (((1 9, 9 9, 9 1, 1 1, 1 9), (2 5, 2 2, 12 2, 2 5)), ((21 9, 25 9, 25 5, 21 5, 21 9)))",
                "MULTIPOLYGON (((1 9, 9 9, 9 1, 1 1, 1 9)), ((21 9, 25 9, 25 5, 21 5, 21 9)))");
        }

        [Test]
        public void TestOneSmallGc()
        {
            CheckHolesRemoved("GEOMETRYCOLLECTION (POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9), (2 5, 2 2, 12 2, 2 5)), LINESTRING (15 9, 19 5))",
                "GEOMETRYCOLLECTION (POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9)), LINESTRING (15 9, 19 5))");
        }

        private void CheckHolesRemoved(String inputWKT, String expectedWKT)
        {
            IGeometry input = Read(inputWKT);
            IGeometry expected = Read(expectedWKT);
            IGeometry actual = SmallHoleRemover.Clean(input, 100);
            CheckEqual(expected, actual);
        }
    }
}