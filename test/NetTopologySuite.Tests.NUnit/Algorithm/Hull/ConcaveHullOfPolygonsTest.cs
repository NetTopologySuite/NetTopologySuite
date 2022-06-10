using System;
using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Algorithm.Hull;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm.Hull
{
    public class ConcaveHullOfPolygonsTest : GeometryTestCase
    {
        [Test]
        public void TestEmpty()
        {
            string wkt = "MULTIPOLYGON EMPTY";
            CheckHullTight(wkt, 1000,
                "POLYGON EMPTY");
        }

        [Test]
        public void TestPolygon()
        {
            string wkt = "POLYGON ((1 9, 5 8, 9 9, 4 4, 7 1, 2 1, 1 9))";
            CheckHullTight(wkt, 1000,
                "POLYGON ((1 9, 5 8, 9 9, 4 4, 7 1, 2 1, 1 9))");
            CheckHull(wkt, 1000,
                "POLYGON ((1 9, 9 9, 7 1, 2 1, 1 9))");
        }

        [Test]
        public void TestSimple()
        {
            string wkt = "MULTIPOLYGON (((100 200, 100 300, 150 250, 200 300, 200 200, 100 200)), ((100 100, 200 100, 150 50, 100 100)))";
            CheckHullTight(wkt, 1000,
                "POLYGON ((100 100, 100 200, 100 300, 150 250, 200 300, 200 200, 200 100, 150 50, 100 100))");
            CheckHull(wkt, 1000,
                "POLYGON ((100 100, 100 200, 100 300, 200 300, 200 200, 200 100, 150 50, 100 100))");
        }

        [Test]
        public void TestSimpleNeck()
        {
            string wkt = "MULTIPOLYGON (((1 9, 5 8, 9 9, 9 6, 6 4, 4 4, 1 6, 1 9)), ((1 1, 4 3, 6 3, 9 1, 1 1)))";
            CheckHullTight(wkt, 0, wkt);
            CheckHullTight(wkt, 2,
                "POLYGON ((6 3, 9 1, 1 1, 4 3, 4 4, 1 6, 1 9, 5 8, 9 9, 9 6, 6 4, 6 3))");
            CheckHullTight(wkt, 6,
                "POLYGON ((1 1, 1 6, 1 9, 5 8, 9 9, 9 6, 9 1, 1 1))");
        }

        [Test]
        public void TestPoly3Concave1()
        {
            CheckHullTight("MULTIPOLYGON (((1 5, 5 8, 5 5, 1 5)), ((5 1, 1 4, 5 4, 5 1)), ((6 8, 9 6, 7 5, 9 4, 6 1, 6 8)))",
               100, "POLYGON ((6 8, 9 6, 7 5, 9 4, 6 1, 5 1, 1 4, 1 5, 5 8, 6 8))");
        }

        [Test]
        public void TestPoly3Concave3()
        {
            string wkt = "MULTIPOLYGON (((0 7, 4 10, 3 7, 5 6, 4 5, 0 7)), ((4 0, 0 2, 3 4, 5 3, 4 0)), ((9 10, 8 8, 10 9, 8 5, 10 3, 7 0, 6 3, 7 4, 7 6, 5 9, 9 10)))";

            CheckHullTight(wkt, 0, wkt);
            CheckHullTight(wkt, 2,
                "POLYGON ((5 3, 4 0, 0 2, 3 4, 4 5, 0 7, 4 10, 5 9, 9 10, 8 8, 10 9, 8 5, 10 3, 7 0, 6 3, 5 3))");
            CheckHullTight(wkt, 4,
                "POLYGON ((4 0, 0 2, 3 4, 4 5, 0 7, 4 10, 5 9, 9 10, 8 8, 10 9, 8 5, 10 3, 7 0, 4 0))");
            CheckHullTight(wkt, 100,
                "POLYGON ((0 7, 4 10, 9 10, 8 8, 10 9, 8 5, 10 3, 7 0, 4 0, 0 2, 0 7))");

            CheckHullByLenRatio(wkt, 0, wkt);
            CheckHullByLenRatio(wkt, 0.2,
                "POLYGON ((5 9, 9 10, 10 9, 8 5, 10 3, 7 0, 6 3, 5 3, 4 0, 0 2, 3 4, 4 5, 0 7, 4 10, 5 9))");
            CheckHullByLenRatio(wkt, 0.5,
                "POLYGON ((5 9, 9 10, 10 9, 8 5, 10 3, 7 0, 4 0, 0 2, 3 4, 4 5, 0 7, 4 10, 5 9))");
            CheckHullByLenRatio(wkt, 1,
                "POLYGON ((9 10, 10 9, 10 3, 7 0, 4 0, 0 2, 0 7, 4 10, 9 10))");
        }

        [Test]
        public void TestPoly3WithHole()
        {
            string wkt = "MULTIPOLYGON (((1 9, 5 9, 5 7, 3 7, 3 5, 1 5, 1 9)), ((1 4, 3 4, 3 2, 5 2, 5 0, 1 0, 1 4)), ((6 9, 8 9, 9 5, 8 0, 6 0, 6 2, 8 5, 6 7, 6 9)))";
            CheckHullWithHoles(wkt, 0.9, wkt);
            CheckHullWithHoles(wkt, 1,
                "POLYGON ((1 0, 1 4, 1 5, 1 9, 5 9, 6 9, 8 9, 9 5, 8 0, 6 0, 5 0, 1 0), (3 2, 5 2, 6 2, 8 5, 6 7, 5 7, 3 7, 3 5, 3 4, 3 2))");

            CheckHullWithHoles(wkt, 2.5,
                "POLYGON ((1 5, 1 9, 5 9, 6 9, 8 9, 9 5, 8 0, 6 0, 5 0, 1 0, 1 4, 1 5), (3 4, 3 2, 5 2, 6 2, 8 5, 6 7, 5 7, 3 7, 3 5, 3 4))");
            CheckHullWithHoles(wkt, 4,
                "POLYGON ((1 5, 1 9, 5 9, 6 9, 8 9, 9 5, 8 0, 6 0, 5 0, 1 0, 1 4, 1 5), (5 2, 6 2, 8 5, 6 7, 5 7, 3 5, 5 2))");
            CheckHullWithHoles(wkt, 9,
                "POLYGON ((6 9, 8 9, 9 5, 8 0, 6 0, 5 0, 1 0, 1 4, 1 5, 1 9, 5 9, 6 9))");
        }

        private void CheckHull(string wkt, double maxLen, string wktExpected)
        {
            var geom = Read(wkt);
            var actual = ConcaveHullOfPolygons.ConcaveHullByLength(geom, maxLen);
            var expected = Read(wktExpected);
            CheckEqual(expected, actual);
        }

        private void CheckHullByLenRatio(string wkt, double lenRatio, string wktExpected)
        {
            var geom = Read(wkt);
            var actual = ConcaveHullOfPolygons.ConcaveHullByLengthRatio(geom, lenRatio);
            var expected = Read(wktExpected);
            CheckEqual(expected, actual);
        }

        private void CheckHullTight(string wkt, double maxLen, string wktExpected)
        {
            var geom = Read(wkt);
            var actual = ConcaveHullOfPolygons.ConcaveHullByLength(geom, maxLen, true, false);
            var expected = Read(wktExpected);
            CheckEqual(expected, actual);
        }

        private void CheckHullWithHoles(string wkt, double maxLen, string wktExpected)
        {
            var geom = Read(wkt);
            var actual = ConcaveHullOfPolygons.ConcaveHullByLength(geom, maxLen, false, true);
            var expected = Read(wktExpected);
            CheckEqual(expected, actual);
        }
    }
}
