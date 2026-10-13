using System;
using System.Collections.Generic;
using NetTopologySuite.Coverage;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Coverage
{
    /// <summary>
    /// Edge cases and novel angles for <see cref="CoverageCleaner"/> that the JTS test class does not exercise.
    /// Some of these document the current contract; others document desired-but-untested behaviour
    /// and may go red until the underlying contract is decided.
    /// </summary>
    public class CoverageCleanerEdgeCasesTest : GeometryTestCase
    {
        // A. Empty coverage array
        // Expectation: calling Clean on an empty array returns an empty array (not a crash).
        [Test]
        public void TestEmptyCoverageArray()
        {
            var actual = CoverageCleaner.CleanGapWidth(Array.Empty<Geometry>(), 0);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.Length, Is.EqualTo(0));
        }

        // B. Null entry in coverage array
        // Expectation: a null slot should produce an empty output for that slot, not throw.
        [Test]
        public void TestNullEntryInCoverage()
        {
            var cov = new Geometry[]
            {
                Read("POLYGON ((1 1, 1 5, 5 5, 5 1, 1 1))"),
                null,
                Read("POLYGON ((6 1, 6 5, 10 5, 10 1, 6 1))"),
            };
            var actual = CoverageCleaner.CleanGapWidth(cov, 0);
            Assert.That(actual.Length, Is.EqualTo(3));
            Assert.That(actual[1].IsEmpty, Is.True, "Null slot should yield an empty output, not crash.");
        }

        // C. All-LineString input (no polygonal entries)
        // Expectation: returns an array of empty geometries of the input length, doesn't throw.
        [Test]
        public void TestAllNonPolygonalInput()
        {
            var cov = new Geometry[]
            {
                Read("LINESTRING (0 0, 1 1)"),
                Read("LINESTRING (2 2, 3 3)"),
            };
            var actual = CoverageCleaner.CleanGapWidth(cov, 0);
            Assert.That(actual.Length, Is.EqualTo(2));
            foreach (var g in actual)
                Assert.That(g.IsEmpty, Is.True);
        }

        // D. Single-polygon input
        // Expectation: round-trips as the same polygon (or normalized equivalent), result is a valid coverage.
        [Test]
        public void TestSinglePolygonInput()
        {
            var cov = ReadArray("POLYGON ((1 1, 1 5, 5 5, 5 1, 1 1))");
            var actual = CoverageCleaner.CleanGapWidth(cov, 0);
            Assert.That(actual.Length, Is.EqualTo(1));
            Assert.That(actual[0].IsEmpty, Is.False, "Single polygon should not vanish");
            Assert.That(actual[0].IsValid, Is.True);
            // Areas should match (up to normalization).
            Assert.That(actual[0].Area, Is.EqualTo(cov[0].Area).Within(1e-9));
        }

        // E. Polygon with a hole containing another input polygon (a "filled hole")
        // Expectation: outer polygon's hole is preserved, inner polygon is preserved separately, valid coverage.
        [Test]
        public void TestFilledHoleIsPreserved()
        {
            var cov = ReadArray(
                "POLYGON ((0 0, 0 10, 10 10, 10 0, 0 0), (3 3, 7 3, 7 7, 3 7, 3 3))",
                "POLYGON ((3 3, 3 7, 7 7, 7 3, 3 3))");
            var actual = CoverageCleaner.CleanGapWidth(cov, 0);
            Assert.That(actual.Length, Is.EqualTo(2));
            Assert.That(actual[0].IsEmpty, Is.False, "Outer polygon with hole should be preserved");
            Assert.That(actual[1].IsEmpty, Is.False, "Inner polygon should be preserved");
            // Total area should be the unioned area (outer with hole + inner = full 10x10 square).
            double totalArea = actual[0].Area + actual[1].Area;
            Assert.That(totalArea, Is.EqualTo(100.0).Within(1e-9));
            // And the result should be a valid coverage.
            Assert.That(CoverageValidator.IsValid(actual), Is.True);
        }

        // F. (removed) Inlet-gap testing turns out to be untestable at this level:
        // Polygonizer only produces enclosed faces, so unenclosed "inlets" are never even
        // candidate gaps in the classifier. JTS's doc claim is a tautology in the pipeline.

        // G. Z-coordinate preservation
        // Expectation (probably aspirational): a clean coverage preserves Z on output coordinates.
        // Current implementation routes through SnappingNoder/LineDissolver/Polygonizer, which likely drop Z.
        [Test]
        public void TestZCoordinatesPreserved()
        {
            var cov = ReadArray(
                "POLYGON Z ((0 0 10, 0 5 10, 5 5 10, 5 0 10, 0 0 10))",
                "POLYGON Z ((5 0 20, 5 5 20, 10 5 20, 10 0 20, 5 0 20))");
            var actual = CoverageCleaner.CleanGapWidth(cov, 0);
            Assert.That(actual.Length, Is.EqualTo(2));
            // Check that at least one output vertex carries a non-NaN Z.
            bool anyZ = false;
            foreach (var g in actual)
            {
                foreach (var c in g.Coordinates)
                {
                    if (!double.IsNaN(c.Z))
                    {
                        anyZ = true;
                        break;
                    }
                }
                if (anyZ) break;
            }
            Assert.That(anyZ, Is.True,
                "At least one output coordinate should carry a Z value (NaN would mean Z was dropped).");
        }

        // H. Triple overlap with MergeMaxArea
        // Three polygons all cover the same central square. With MergeMaxArea, the overlap should be
        // attached to the largest of the three (poly0, the 10x10 frame).
        [Test]
        public void TestTripleOverlapMaxArea()
        {
            var cov = ReadArray(
                "POLYGON ((0 0, 0 10, 10 10, 10 0, 0 0))",       // big 100
                "POLYGON ((2 2, 2 8, 8 8, 8 2, 2 2))",            // medium 36
                "POLYGON ((3 3, 3 7, 7 7, 7 3, 3 3))");           // small 16
            var actual = CoverageCleaner.CleanOverlapGap(cov, CoverageCleaner.MergeMaxArea, 0);
            Assert.That(actual.Length, Is.EqualTo(3));
            // After cleaning with MergeMaxArea, all overlap area should end up attached to the biggest
            // parent (poly0). Poly1 and poly2 should be emptied because they are fully covered.
            Assert.That(CoverageValidator.IsValid(actual), Is.True,
                "Triple-overlap merge must yield a valid coverage");
            Assert.That(actual[0].Area, Is.EqualTo(100.0).Within(1e-9),
                "MergeMaxArea should attach the whole overlap to the largest polygon");
            Assert.That(actual[1].IsEmpty || actual[1].Area < 1e-9, Is.True,
                "Medium polygon is fully covered; should be emptied");
            Assert.That(actual[2].IsEmpty || actual[2].Area < 1e-9, Is.True,
                "Small polygon is fully covered; should be emptied");
        }

        // (I/I' removed) Both threshold tests were constructed wrong: their "gap" touched
        // y=0 (the southern outer boundary), making it an unenclosed inlet rather than a
        // gap the classifier ever considers. JTS's own covWithGaps tests already verify
        // enclosed-gap threshold behaviour at widths 0/1/2.

        // J. Calling Clean() twice — idempotency / re-entrancy
        // Expectation: the second call either produces the same result, or throws meaningfully.
        // Internal state (_overlaps, _gaps, _overlapParentMap) is accumulated in instance fields,
        // so a second invocation may double-count.
        [Test]
        public void TestCallCleanTwiceIsIdempotent()
        {
            var cov = ReadArray(
                "POLYGON ((1 3, 5 3, 4 1, 1 1, 1 3))",
                "POLYGON ((1 3, 1 9, 4 9, 4 3, 3 1.9, 1 3))");
            var cc = new CoverageCleaner(cov);
            cc.Clean();
            var firstResult = cc.Result;
            // Second invocation
            cc.Clean();
            var secondResult = cc.Result;
            Assert.That(secondResult.Length, Is.EqualTo(firstResult.Length));
            for (int i = 0; i < firstResult.Length; i++)
            {
                Assert.That(secondResult[i].Area, Is.EqualTo(firstResult[i].Area).Within(1e-9),
                    $"Calling Clean() twice should be idempotent (element {i}).");
            }
        }

        // K. OverlapMergeStrategy property validates input
        [Test]
        public void TestOverlapMergeStrategyRejectsInvalidCode()
        {
            var cc = new CoverageCleaner(ReadArray("POLYGON ((0 0, 0 1, 1 1, 1 0, 0 0))"));
            Assert.Throws<ArgumentOutOfRangeException>(() => cc.OverlapMergeStrategy = 99);
            Assert.Throws<ArgumentOutOfRangeException>(() => cc.OverlapMergeStrategy = -1);
        }
    }
}
