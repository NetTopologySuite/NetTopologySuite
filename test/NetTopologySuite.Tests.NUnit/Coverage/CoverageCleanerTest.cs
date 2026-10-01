using System.Collections.Generic;
using NetTopologySuite.Coverage;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Coverage
{
    public class CoverageCleanerTest : GeometryTestCase
    {
        [Test]
        public void TestCoverageWithEmpty()
        {
            CheckClean(
                "GEOMETRYCOLLECTION (POLYGON ((1 9, 9 9, 9 4, 1 4, 1 9)), POLYGON EMPTY, POLYGON ((2 1, 2 5, 8 5, 8 1, 2 1)))",
                "GEOMETRYCOLLECTION (POLYGON ((1 4, 1 9, 9 9, 9 4, 8 4, 2 4, 1 4)), POLYGON EMPTY, POLYGON ((8 1, 2 1, 2 4, 8 4, 8 1)))");
        }

        [Test]
        public void TestSingleNearMatch()
        {
            CheckCleanSnap(ReadArray(
                "POLYGON ((1 9, 9 9, 9 4.99, 1 5, 1 9))",
                "POLYGON ((1 1, 1 5, 9 5, 9 1, 1 1))"),
                0.1);
        }

        [Test]
        public void TestManyNearMatches()
        {
            CheckCleanSnap(ReadArray(
                "POLYGON ((1 9, 9 9, 9 5, 8 5, 7 5, 4 5.5, 3 5, 2 5, 1 5, 1 9))",
                "POLYGON ((1 1, 1 4.99, 2 5.01, 3.01 4.989, 5 3, 6.99 4.99, 7.98 4.98, 9 5, 9 1, 1 1))"),
                0.1);
        }

        // Tests that if interior point lies in a spike that is snapped away, polygon is still in result
        [Test]
        public void TestPolygonSnappedPreserved()
        {
            CheckCleanSnap(ReadArray(
                "POLYGON ((90 0, 10 0, 89.99 30, 90 100, 90 0))"),
                0.1,
                ReadArray(
                    "POLYGON ((90 0, 10 0, 89.99 30, 90 0))"));
        }

        // Tests that if interior point lies in a spike that is snapped away, polygon is still in result
        [Test]
        public void TestPolygonsSnappedPreserved()
        {
            CheckCleanSnap(ReadArray(
                "POLYGON ((0 0, 0 2, 5 2, 5 8, 5.01 0, 0 0))",
                "POLYGON ((0 8, 5 8, 5 2, 0 2, 0 8))"),
                0.02,
                ReadArray(
                    "POLYGON ((0 0, 0 2, 5 2, 5.01 0, 0 0))",
                    "POLYGON ((0 8, 5 8, 5 2, 0 2, 0 8))"));
        }

        // Tests that a collapsed polygon due to snapping is returned as EMPTY
        [Test]
        public void TestPolygonsSnappedCollapse()
        {
            CheckCleanSnap(ReadArray(
                "POLYGON ((1 1, 1 9, 6 5, 9 1, 1 1))",
                "POLYGON ((9 1, 6 5.1, 1 9, 9 9, 9 1))",
                "POLYGON ((9 1, 6 5, 1 9, 6 5.1, 9 1))"),
                1,
                ReadArray(
                    "POLYGON ((6 5, 9 1, 1 1, 1 9, 6 5))",
                    "POLYGON ((9 9, 9 1, 6 5, 1 9, 9 9))",
                    "POLYGON EMPTY"));
        }

        [Test]
        public void TestMergeGapToLongestBorder()
        {
            CheckCleanGapWidth(
                "GEOMETRYCOLLECTION (POLYGON ((1 9, 9 9, 9 5, 1 5, 1 9)), POLYGON ((5 1, 5 5, 1 5, 5 1)), POLYGON ((5 1, 5.1 5, 9 5, 5 1)))",
                1,
                "GEOMETRYCOLLECTION (POLYGON ((5.1 5, 5 5, 1 5, 1 9, 9 9, 9 5, 5.1 5)), POLYGON ((5 1, 1 5, 5 5, 5 1)), POLYGON ((5 1, 5 5, 5.1 5, 9 5, 5 1)))");
        }

        private const string CovWithGaps = "GEOMETRYCOLLECTION (POLYGON ((1 3, 9 3, 9 1, 1 1, 1 3)), POLYGON ((1 3, 1 9, 4 9, 4 3, 3 4, 1 3)), POLYGON ((4 9, 7 9, 7 3, 6 5, 5 5, 4 3, 4 9)), POLYGON ((7 9, 9 9, 9 3, 8 3.1, 7 3, 7 9)))";

        [Test]
        public void TestMergeGapWidth_0()
        {
            CheckCleanGapWidth(CovWithGaps,
                0,
                "GEOMETRYCOLLECTION (POLYGON ((9 3, 9 1, 1 1, 1 3, 4 3, 7 3, 9 3)), POLYGON ((1 9, 4 9, 4 3, 3 4, 1 3, 1 9)), POLYGON ((6 5, 5 5, 4 3, 4 9, 7 9, 7 3, 6 5)), POLYGON ((7 9, 9 9, 9 3, 8 3.1, 7 3, 7 9)))");
        }

        [Test]
        public void TestMergeGapWidth_1()
        {
            CheckCleanGapWidth(CovWithGaps,
                1,
                "GEOMETRYCOLLECTION (POLYGON ((7 3, 9 3, 9 1, 1 1, 1 3, 4 3, 7 3)), POLYGON ((1 9, 4 9, 4 3, 1 3, 1 9)), POLYGON ((7 3, 6 5, 5 5, 4 3, 4 9, 7 9, 7 3)), POLYGON ((7 9, 9 9, 9 3, 7 3, 7 9)))");
        }

        [Test]
        public void TestMergeGapWidth_2()
        {
            CheckCleanGapWidth(CovWithGaps,
                2,
                "GEOMETRYCOLLECTION (POLYGON ((9 3, 9 1, 1 1, 1 3, 4 3, 7 3, 9 3)), POLYGON ((1 9, 4 9, 4 3, 1 3, 1 9)), POLYGON ((7 3, 4 3, 4 9, 7 9, 7 3)), POLYGON ((9 9, 9 3, 7 3, 7 9, 9 9)))");
        }

        private const string CovWithOverlap = "GEOMETRYCOLLECTION (POLYGON ((1 3, 5 3, 4 1, 1 1, 1 3)), POLYGON ((1 3, 1 9, 4 9, 4 3, 3 1.9, 1 3)))";

        [Test]
        public void TestMergeOverlapMinArea()
        {
            CheckCleanOverlapMerge(CovWithOverlap,
                CoverageCleaner.MergeMinArea,
                "GEOMETRYCOLLECTION (POLYGON ((5 3, 4 1, 1 1, 1 3, 4 3, 5 3)), POLYGON ((1 9, 4 9, 4 3, 1 3, 1 9)))");
        }

        [Test]
        public void TestMergeOverlapMaxArea()
        {
            CheckCleanOverlapMerge(CovWithOverlap,
                CoverageCleaner.MergeMaxArea,
                "GEOMETRYCOLLECTION (POLYGON ((1 1, 1 3, 3 1.9, 4 3, 5 3, 4 1, 1 1)), POLYGON ((1 3, 1 9, 4 9, 4 3, 3 1.9, 1 3)))");
        }

        [Test]
        public void TestMergeOverlapMinId()
        {
            CheckCleanOverlapMerge(CovWithOverlap,
                CoverageCleaner.MergeMinIndex,
                "GEOMETRYCOLLECTION (POLYGON ((5 3, 4 1, 1 1, 1 3, 4 3, 5 3)), POLYGON ((1 9, 4 9, 4 3, 1 3, 1 9)))");
        }

        [Test]
        public void TestMergeOverlap2()
        {
            CheckCleanSnap(ReadArray(
                "POLYGON ((5 9, 9 9, 9 1, 5 1, 5 9))",
                "POLYGON ((1 5, 5 5, 5 2, 1 2, 1 5))",
                "POLYGON ((2 7, 5 7, 5 4, 2 4, 2 7))"),
                0.1,
                ReadArray(
                    "POLYGON ((5 1, 5 2, 5 4, 5 5, 5 7, 5 9, 9 9, 9 1, 5 1))",
                    "POLYGON ((5 2, 1 2, 1 5, 2 5, 5 5, 5 4, 5 2))",
                    "POLYGON ((5 5, 2 5, 2 7, 5 7, 5 5))"));
        }

        [Test]
        public void TestMergeOverlap()
        {
            CheckCleanOverlapMerge(
                "GEOMETRYCOLLECTION (POLYGON ((5 9, 9 9, 9 1, 5 1, 5 9)), POLYGON ((1 5, 5 5, 5 2, 1 2, 1 5)), POLYGON ((2 7, 5 7, 5 4, 2 4, 2 7)))",
                CoverageCleaner.MergeLongestBorder,
                "GEOMETRYCOLLECTION (POLYGON ((5 7, 5 9, 9 9, 9 1, 5 1, 5 2, 5 4, 5 5, 5 7)), POLYGON ((5 2, 1 2, 1 5, 2 5, 5 5, 5 4, 5 2)), POLYGON ((2 5, 2 7, 5 7, 5 5, 2 5)))");
        }

        //-------------------------------------------

        //-- a duplicate coverage element is assigned to the lowest result index
        [Test]
        public void TestDuplicateItems()
        {
            CheckClean(
                "GEOMETRYCOLLECTION (POLYGON ((1 9, 9 1, 1 1, 1 9)), POLYGON ((1 9, 9 1, 1 1, 1 9)))",
                "GEOMETRYCOLLECTION (POLYGON ((1 9, 9 1, 1 1, 1 9)), POLYGON EMPTY)");
        }

        [Test]
        public void TestCoveredItem()
        {
            CheckClean(
                "GEOMETRYCOLLECTION (POLYGON ((1 9, 9 9, 9 4, 1 4, 1 9)), POLYGON ((2 5, 2 8, 8 8, 8 5, 2 5)))",
                "GEOMETRYCOLLECTION (POLYGON ((9 9, 9 4, 1 4, 1 9, 9 9)), POLYGON EMPTY)");
        }

        [Test]
        public void TestCoveredItemMultiPolygon()
        {
            CheckClean(
                "GEOMETRYCOLLECTION (MULTIPOLYGON (((1 1, 1 5, 5 5, 5 1, 1 1)), ((6 5, 6 1, 9 1, 6 5))), POLYGON ((6 1, 6 5, 9 1, 6 1)))",
                "GEOMETRYCOLLECTION (MULTIPOLYGON (((1 5, 5 5, 5 1, 1 1, 1 5)), ((6 5, 9 1, 6 1, 6 5))), POLYGON EMPTY)");
        }

        //=========================================================

        private void CheckClean(string wkt, string wktExpected)
        {
            var geom = Read(wkt);
            var cov = ToArray(geom);
            var actual = CoverageCleaner.CleanGapWidth(cov, 0);
            var covExpected = ToArray(Read(wktExpected));
            CheckEqual(covExpected, actual);
        }

        private void CheckCleanGapWidth(string wkt, double gapWidth, string wktExpected)
        {
            var geom = Read(wkt);
            var cov = ToArray(geom);
            var actual = CoverageCleaner.CleanGapWidth(cov, gapWidth);
            var covExpected = ToArray(Read(wktExpected));
            CheckEqual(covExpected, actual);
        }

        private void CheckCleanOverlapMerge(string wkt, int mergeStrategy, string wktExpected)
        {
            var geom = Read(wkt);
            var cov = ToArray(geom);
            var actual = CoverageCleaner.CleanOverlapGap(cov, mergeStrategy, 0);
            var covExpected = ToArray(Read(wktExpected));
            CheckEqual(covExpected, actual);
        }

        private static Geometry[] ToArray(Geometry geom)
        {
            var list = geom.GetPolygonals<List<Geometry>>();
            return GeometryFactory.ToGeometryArray(list);
        }

        private void CheckCleanSnap(Geometry[] cov, double snapDist)
        {
            var covClean = CoverageCleaner.Clean(cov, snapDist, 0);
            CheckValidCoverage(covClean, snapDist);
        }

        private void CheckCleanSnap(Geometry[] cov, double snapDist, Geometry[] expected)
        {
            var actual = CoverageCleaner.Clean(cov, snapDist, 0);
            CheckValidCoverage(actual, snapDist);
            CheckEqual(expected, actual);
        }

        private static void CheckValidCoverage(Geometry[] coverage, double tolerance)
        {
            foreach (var geom in coverage)
            {
                Assert.That(geom.IsValid, Is.True);
            }
            bool isValid = CoverageValidator.IsValid(coverage, tolerance);
            Assert.That(isValid, Is.True);
        }
    }
}
