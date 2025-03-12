﻿using NetTopologySuite.Coverage;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Coverage
{
    public class CoverageValidatorTest : GeometryTestCase
    {

        //========  Invalid cases   =============================

        [Test]
        public void TestCollinearUnmatchedEdge()
        {
            CheckInvalid(ReadArray(
                "POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))",
                "POLYGON ((100 300, 180 300, 180 200, 100 200, 100 300))"),
                ReadArray(
                    "LINESTRING (100 200, 200 200)",
                    "LINESTRING (100 200, 180 200, 180 300)")
                    );
        }

        [Test]
        public void TestOverlappingSquares()
        {
            CheckInvalid(ReadArray(
                "POLYGON ((1 9, 6 9, 6 4, 1 4, 1 9))",
                "POLYGON ((9 1, 4 1, 4 6, 9 6, 9 1))"),
                ReadArray(
                    "LINESTRING (6 9, 6 4, 1 4)",
                    "LINESTRING (4 1, 4 6, 9 6)")
                    );
        }

        [Test]
        public void TestFullyCoveredTriangles()
        {
            CheckInvalid(ReadArray(
                "POLYGON ((1 9, 9 1, 1 1, 1 9))",
                "POLYGON ((9 9, 1 9, 9 1, 9 9))",
                "POLYGON ((9 9, 9 1, 1 1, 1 9, 9 9))"
                ),
                ReadArray(
                    "LINESTRING (9 1, 1 1, 1 9)",
                    "LINESTRING (9 1, 9 9, 1 9)",
                    "LINESTRING (9 9, 9 1, 1 1, 1 9, 9 9)")
                    );
        }


        //========  Gap cases   =============================

        [Test]
        public void TestGap()
        {
            CheckInvalidWithGaps(ReadArray(
                "POLYGON ((1 5, 9 5, 9 1, 1 1, 1 5))",
                "POLYGON ((1 9, 5 9, 5 5.1, 1 5, 1 9))",
                "POLYGON ((5 9, 9 9, 9 5, 5.5 5.1, 5 9))"),
                0.5,
                ReadArray(
                    "LINESTRING (1 5, 9 5)",
                    "LINESTRING (1 5, 5 5.1, 5 9)",
                    "LINESTRING (5 9, 5.5 5.1, 9 5)")
                    );
        }

        [Test]
        public void TestGapDisjoint()
        {
            CheckInvalidWithGaps(ReadArray(
                "POLYGON ((1 5, 9 5, 9 1, 1 1, 1 5))",
                "POLYGON ((1 9, 5 9, 5 5.1, 1 5.1, 1 9))",
                "POLYGON ((5 9, 9 9, 9 5.1, 5 5.1, 5 9))"),
                0.5,
                ReadArray(
                    "LINESTRING (1 5, 9 5)",
                    "LINESTRING (5 5.1, 1 5.1)",
                    "LINESTRING (9 5.1, 5 5.1)")
                    );
        }

        [Test]
        public void TestGore()
        {
            CheckInvalidWithGaps(ReadArray(
                "POLYGON ((1 5, 5 5, 9 5, 9 1, 1 1, 1 5))",
                "POLYGON ((1 9, 5 9, 5 5, 1 5.1, 1 9))",
                "POLYGON ((5 9, 9 9, 9 5, 5 5, 5 9))"),
                0.5,
                ReadArray(
                    "LINESTRING (1 5, 5 5)",
                    "LINESTRING (1 5.1, 5 5)",
                    null)
                    );
        }

        //========  Valid cases   =============================

        [Test]
        public void TestGrid()
        {
            CheckValid(ReadArray(
                "POLYGON ((1 9, 5 9, 5 5, 1 5, 1 9))",
                "POLYGON ((9 9, 9 5, 5 5, 5 9, 9 9))",
                "POLYGON ((1 1, 1 5, 5 5, 5 1, 1 1))",
                "POLYGON ((9 1, 5 1, 5 5, 9 5, 9 1))"));
        }

        [Test]
        public void TestMultiPolygon()
        {
            CheckValid(ReadArray(
                "MULTIPOLYGON (((1 9, 5 9, 5 5, 1 5, 1 9)), ((9 1, 5 1, 5 5, 9 5, 9 1)))",
                "MULTIPOLYGON (((1 1, 1 5, 5 5, 5 1, 1 1)), ((9 9, 9 5, 5 5, 5 9, 9 9)))"));
        }

        [Test]
        public void TestValidDuplicatePoints()
        {
            CheckValid(ReadArray(
                "POLYGON ((1 9, 5 9, 5 5, 1 5, 1 5, 1 5, 1 9))",
                "POLYGON ((9 9, 9 5, 5 5, 5 9, 9 9))",
                "POLYGON ((1 1, 1 5, 5 5, 5 1, 1 1))",
                "POLYGON ((9 1, 5 1, 5 5, 9 5, 9 1))"));
        }

        [Test]
        public void TestRingCollapse()
        {
            CheckValid(ReadArray(
                "POLYGON ((1 9, 5 9, 1 9))",
                "POLYGON ((9 9, 9 5, 5 5, 5 9, 9 9))",
                "POLYGON ((1 1, 1 5, 5 5, 5 1, 1 1))",
                "POLYGON ((9 1, 5 1, 5 5, 9 5, 9 1))"));
        }

        //========  Valid cases with EMPTY  =============================

        [Test]
        public void TestPolygonEmpty()
        {
            CheckValid(ReadArray(
                "POLYGON ((1 9, 5 9, 5 5, 1 5, 1 9))",
                "POLYGON ((9 9, 9 5, 5 5, 5 9, 9 9))",
                "POLYGON ((1 1, 1 5, 5 5, 5 1, 1 1))",
                "POLYGON EMPTY"));
        }

        [Test]
        public void TestMultiPolygonWithEmptyRing()
        {
            CheckValid(ReadArray(
                "MULTIPOLYGON (((9 9, 9 1, 1 1, 2 4, 7 7, 9 9)), EMPTY)"));
        }

        //------------------------------------------------------------

        private void CheckValid(Geometry[] coverage)
        {
            Assert.That(CoverageValidator.IsValid(coverage));
        }

        private void CheckInvalid(Geometry[] coverage, Geometry[] expected)
        {
            var actual = CoverageValidator.Validate(coverage);
            CheckEqual(expected, actual);
        }

        private void CheckInvalidWithGaps(Geometry[] coverage, double gapWidth, Geometry[] expected)
        {
            var actual = CoverageValidator.Validate(coverage, gapWidth);
            CheckEqual(expected, actual);
        }
    }

}
