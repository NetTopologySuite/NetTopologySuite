using NetTopologySuite.IO;
using NUnit.Framework;
using System;

namespace NetTopologySuite.Tests.NUnit.IO
{
    public class WKTReaderFixStructureTest : GeometryTestCase
    {
        private readonly WKTReader readerFix;
        private readonly WKTReader reader;

        public WKTReaderFixStructureTest()
        {
            reader = new WKTReader();
            readerFix = new WKTReader { FixStructure = true };
        }

        [Test]
        public void TestLineaStringShort()
        {
            CheckFixStructure("LINESTRING (0 0)");
        }

        [Test]
        public void TestLinearRingUnclosed()
        {
            CheckFixStructure("LINEARRING (0 0, 0 1, 1 0)");
        }

        [Test]
        public void TestLinearRingShort()
        {
            CheckFixStructure("LINEARRING (0 0, 0 1)");
        }

        [Test]
        public void TestPolygonShort()
        {
            CheckFixStructure("POLYGON ((0 0))");
        }

        [Test]
        public void TestPolygonUnclosed()
        {
            CheckFixStructure("POLYGON ((0 0, 0 1, 1 0))");
        }

        [Test]
        public void TestPolygonUnclosedHole()
        {
            CheckFixStructure("POLYGON ((0 0, 0 10, 10 0, 0 0), (0 0, 1 0, 0 1))");
        }

        [Test]
        public void TestCollection()
        {
            CheckFixStructure("GEOMETRYCOLLECTION (LINESTRING (0 0), LINEARRING (0 0, 0 1), POLYGON ((0 0, 0 10, 10 0, 0 0), (0 0, 1 0, 0 1)) )");
        }

        private void CheckFixStructure(string wkt)
        {
            CheckHasBadStructure(wkt);
            CheckFixed(wkt);
        }

        private void CheckFixed(string wkt)
        {
            // if not fixed will fail with IllegalArgumentException 
            readerFix.Read(wkt);
        }

        private void CheckHasBadStructure(string wkt)
        {
            try
            {
                reader.Read(wkt);
                Assert.Fail("Input does not have non-closed rings");
            }
            catch (Exception e)
            {
                // ok, do nothing
            }
        }
    }
}
