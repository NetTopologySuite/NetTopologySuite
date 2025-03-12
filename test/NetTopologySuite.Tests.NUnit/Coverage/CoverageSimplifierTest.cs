﻿using NetTopologySuite.Coverage;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Coverage
{
    public class CoverageSimplifierTest : GeometryTestCase
    {
        [Test]
        public void TestNoopSimple2()
        {
            CheckNoop(ReadArray(
                "POLYGON ((100 100, 200 200, 300 100, 200 101, 100 100))",
                "POLYGON ((150 0, 100 100, 200 101, 300 100, 250 0, 150 0))")
            );
        }

        [Test]
        public void TestNoopSimple3()
        {
            CheckNoop(ReadArray(
                "POLYGON ((100 300, 200 200, 100 200, 100 300))",
                "POLYGON ((100 200, 200 200, 200 100, 100 100, 100 200))",
                "POLYGON ((100 100, 200 100, 150 50, 100 100))")
            );
        }

        [Test]
        public void TestNoopHole()
        {
            CheckNoop(ReadArray(
                "POLYGON ((10 90, 90 90, 90 10, 10 10, 10 90), (20 80, 80 80, 80 20, 20 20, 20 80))",
                "POLYGON ((80 20, 20 20, 20 80, 80 80, 80 20))")
            );
        }

        [Test]
        public void TestNoopMulti()
        {
            CheckNoop(ReadArray(
                "MULTIPOLYGON (((10 10, 10 50, 50 50, 50 10, 10 10)), ((90 90, 90 50, 50 50, 50 90, 90 90)))",
                "MULTIPOLYGON (((10 90, 50 90, 50 50, 10 50, 10 90)), ((90 10, 50 10, 50 50, 90 50, 90 10)))")
            );
        }

        //---------------------------------------------

        [Test]
        public void TestRepeatedPointRemoved()
        {
            CheckResult(ReadArray(
                "POLYGON ((2 9, 7 6, 9 1, 2 1, 2 1, 3 6, 2 9))"),
                2,
                ReadArray(
                    "POLYGON ((2 1, 2 9, 7 6, 9 1, 2 1))")
            );
        }

        [Test]
        public void TestRepeatedPointCollapseToLine()
        {
            CheckResult(ReadArray(
                "MULTIPOLYGON (((10 10, 10 20, 20 19, 30 20, 30 10, 10 10)), ((10 30, 20 29, 30 30, 30 20, 20 19, 10 20, 10 30)), ((10 20, 20 19, 20 19, 10 20)))"),
                5,
                ReadArray(
                    "MULTIPOLYGON (((10 20, 20 19, 30 20, 30 10, 10 10, 10 20)), ((30 20, 20 19, 10 20, 10 30, 30 30, 30 20)), ((10 20, 20 19, 10 20)))")
            );
        }

        [Test]
        public void TestRepeatedPointCollapseToPoint()
        {
            CheckResult(ReadArray(
                "MULTIPOLYGON (((10 10, 10 20, 20 19, 30 20, 30 10, 10 10)), ((10 30, 20 29, 30 30, 30 20, 20 19, 10 20, 10 30)), ((20 19, 20 19, 20 19)))"),
                5,
                ReadArray(
                    "MULTIPOLYGON (((10 10, 10 20, 20 19, 30 20, 30 10, 10 10)), ((10 20, 10 30, 30 30, 30 20, 20 19, 10 20)), ((20 19, 20 19, 20 19)))")
            );
        }

        [Test]
        public void TestRepeatedPointCollapseToPoint2()
        {
            CheckResult(ReadArray(
                "MULTIPOLYGON (((100 200, 150 195, 200 200, 200 100, 100 100, 100 200)), ((150 195, 150 195, 150 195, 150 195)))"),
                40,
                ReadArray(
                    "MULTIPOLYGON (((150 195, 200 200, 200 100, 100 100, 100 200, 150 195)), ((150 195, 150 195, 150 195, 150 195)))")
            );
        }

        //---------------------------------------------

        [Test]
        public void TestSimple2()
        {
            CheckResult(ReadArray(
                "POLYGON ((100 100, 200 200, 300 100, 200 101, 100 100))",
                "POLYGON ((150 0, 100 100, 200 101, 300 100, 250 0, 150 0))"),
                10,
                ReadArray(
                    "POLYGON ((100 100, 200 200, 300 100, 100 100))",
                    "POLYGON ((150 0, 100 100, 300 100, 250 0, 150 0))")
            );
        }

        [Test]
        public void TestMultiPolygons()
        {
            CheckResult(ReadArray(
                "MULTIPOLYGON (((5 9, 2.5 7.5, 1 5, 5 5, 5 9)), ((5 5, 9 5, 7.5 2.5, 5 1, 5 5)))",
                "MULTIPOLYGON (((5 9, 6.5 6.5, 9 5, 5 5, 5 9)), ((1 5, 5 5, 5 1, 3.5 3.5, 1 5)))"),
                3,
                ReadArray(
                    "MULTIPOLYGON (((1 5, 5 9, 5 5, 1 5)), ((5 1, 5 5, 9 5, 5 1))))",
                    "MULTIPOLYGON (((1 5, 5 5, 5 1, 1 5)), ((5 5, 5 9, 9 5, 5 5)))")
            );
        }


        [Test]
        public void TestSingleRingNoCollapse()
        {
            CheckResult(ReadArray(
                    "POLYGON ((10 50, 50 90, 60 90, 70 50, 60 10, 10 50))"),
                100000,
                ReadArray(
                    "POLYGON ((10 50, 60 90, 70 50, 60 10, 10 50))")
            );
        }

        /**
         * Checks that a polygon on the edge of the coverage does not collapse 
         * under maximal simplification
         */
        [Test]
        public void TestMultiEdgeRingNoCollapse()
        {
            CheckResult(ReadArray(
                "POLYGON ((50 250, 200 200, 180 170, 200 150, 50 50, 50 250))",
                "POLYGON ((200 200, 180 170, 200 150, 200 200))"),
                40,
                ReadArray(
                    "POLYGON ((50 250, 200 200, 180 170, 200 150, 50 50, 50 250))",
                    "POLYGON ((200 200, 180 170, 200 150, 200 200))")
            );
        }

        [Test]
        public void TestFilledHole()
        {
            CheckResult(ReadArray(
                "POLYGON ((20 30, 20 80, 60 50, 80 20, 50 20, 20 30))",
                "POLYGON ((10 90, 90 90, 90 10, 10 10, 10 90), (50 20, 20 30, 20 80, 60 50, 80 20, 50 20))"),
                28,
                ReadArray(
                    "POLYGON ((20 30, 20 80, 60 50, 80 20, 20 30))",
                    "POLYGON ((10 90, 90 90, 90 10, 10 10, 10 90), (20 30, 20 80, 60 50, 80 20, 20 30))")
            );
        }

        [Test]
        public void TestTouchingHoles()
        {
            CheckResult(ReadArray(
                    "POLYGON (( 0 0, 0 11, 19 11, 19 0, 0 0 ), ( 4 5, 12 5, 12 6, 10 6, 10 8, 9 8, 9 9, 7 9, 7 8, 6 8, 6 6, 4 6, 4 5 ), ( 12 6, 14 6, 14 9, 13 9, 13 7, 12 7, 12 6 ))",
                    "POLYGON (( 12 6, 12 5, 4 5, 4 6, 6 6, 6 8, 7 8, 7 9, 9 9, 9 8, 10 8, 10 6, 12 6 ))",
                    "POLYGON (( 12 6, 12 7, 13 7, 13 9, 14 9, 14 6, 12 6 ))"),
                1.0,
                ReadArray(
                    "POLYGON ((0 0, 0 11, 19 11, 19 0, 0 0), (4 5, 12 5, 12 6, 10 6, 9 9, 6 8, 6 6, 4 5), (12 6, 14 6, 14 9, 13 7, 12 6))",
                    "POLYGON ((4 5, 6 6, 6 8, 9 9, 10 6, 12 6, 12 5, 4 5))",
                    "POLYGON ((12 6, 14 6, 14 9, 13 7, 12 6))")
            );
        }

        [Test]
        public void TestInnerHoleTouchingShell()
        {
            CheckResultInner(ReadArray(
                    "POLYGON ((200 300, 300 300, 300 100, 100 100, 100 300, 200 300), (170 220, 170 160, 200 140, 210 200, 200 250, 170 220), (170 250, 200 250, 200 300, 180 280, 170 250))",
                    "POLYGON ((170 220, 200 250, 210 200, 200 140, 170 160, 170 220))",
                    "POLYGON ((170 250, 180 280, 200 300, 200 250, 170 250))"),
                70.0,
                ReadArray(
                    "POLYGON ((100 100, 100 300, 200 300, 300 300, 300 100, 100 100), (170 160, 200 140, 200 250, 170 220, 170 160), (170 250, 200 250, 200 300, 170 250))",
                    "POLYGON ((170 160, 170 220, 200 250, 200 140, 170 160))",
                    "POLYGON ((170 250, 200 300, 200 250, 170 250))")
            );
        }

        [Test]
        public void TestInnerHolesTouchingHolesAndShell()
        {
            CheckResultInner(ReadArray(
                    "POLYGON (( 8 5, 9 4, 9 2, 1 2, 1 4, 2 4, 2 5, 1 5, 1 8, 9 8, 9 6, 8 5 ), ( 8 5, 7 6, 6 6, 6 4, 7 4, 8 5 ), ( 7 6, 8 6, 7 7, 7 6 ), ( 6 6, 6 7, 5 6, 6 6 ), ( 6 4, 5 4, 6 3, 6 4 ), ( 7 4, 7 3, 8 4, 7 4 ))"),
                4.0,
                ReadArray(
                    "POLYGON (( 8 5, 9 4, 9 2, 1 2, 1 4, 2 4, 2 5, 1 5, 1 8, 9 8, 9 6, 8 5 ), ( 8 5, 7 6, 6 6, 6 4, 7 4, 8 5 ), ( 7 6, 8 6, 7 7, 7 6 ), ( 6 6, 6 7, 5 6, 6 6 ), ( 6 4, 5 4, 6 3, 6 4 ), ( 7 4, 7 3, 8 4, 7 4 ))")
            );
        }

        [Test]
        public void TestHolesTouchingHolesAndShell()
        {
            CheckResult(ReadArray(
                    "POLYGON (( 8 5, 9 4, 9 2, 1 2, 1 4, 2 4, 2 5, 1 5, 1 8, 9 8, 9 6, 8 5 ), ( 8 5, 7 6, 6 6, 6 4, 7 4, 8 5 ), ( 7 6, 8 6, 7 7, 7 6 ), ( 6 6, 6 7, 5 6, 6 6 ), ( 6 4, 5 4, 6 3, 6 4 ), ( 7 4, 7 3, 8 4, 7 4 ))"),
                4.0,
                ReadArray(
                    "POLYGON ((8 5, 9 2, 1 2, 1 8, 9 8, 8 5), (8 5, 7 6, 6 6, 6 4, 7 4, 8 5))")
            );
        }

        [Test]
        public void TestInnerMultiPolygonWithTouchingShells()
        {
            CheckResultInner(
                ReadArray(
                    "MULTIPOLYGON ((( 2 7, 2 8, 3 8, 3 7, 2 7 )), (( 1 6, 1 7, 2 7, 2 6, 1 6 )), (( 0 7, 0 8, 1 8, 1 7, 0 7 )), (( 0 5, 0 6, 1 6, 1 5, 0 5 )), (( 2 5, 2 6, 3 6, 3 5, 2 5 )))"),
                1.0,
                ReadArray(
                    "MULTIPOLYGON ((( 2 7, 2 8, 3 8, 3 7, 2 7 )), (( 1 6, 1 7, 2 7, 2 6, 1 6 )), (( 0 7, 0 8, 1 8, 1 7, 0 7 )), (( 0 5, 0 6, 1 6, 1 5, 0 5 )), (( 2 5, 2 6, 3 6, 3 5, 2 5 )))")
                );
        }

        [Test]
        public void TestMultiPolygonWithTouchingShells()
        {
            CheckResult(
                ReadArray(
                    "MULTIPOLYGON (((1 6, 1 7, 2 7, 2 6, 1 6)), ((0 7, 0 8, 1 8, 1.2 7.5, 1 7, 0 7)), ((0 5, 0 6, 1 6, 1.2 5.5, 1 5, 0 5)))"),
                1.0,
                ReadArray(
                    "MULTIPOLYGON (((0 5, 0 6, 1 6, 1 5, 0 5)), ((0 7, 0 8, 1 8, 1 7, 0 7)), ((1 6, 1 7, 2 6, 1 6)))")
            );
        }

        [Test]
        public void TestInnerTouchingShells()
        {
            CheckResultInner(ReadArray(
                    "POLYGON ((0 0, 0 5, 5 6, 10 5, 10 0, 0 0))",
                    "POLYGON ((0 10, 5 6, 10 10, 0 10))"),
                4.0,
                ReadArray(
                    "POLYGON ((0 0, 0 5, 5 6, 10 5, 10 0, 0 0))",
                    "POLYGON ((0 10, 5 6, 10 10, 0 10))")
            );
        }

        [Test]
        public void TestShellSimplificationAtStartingNode()
        {
            CheckResult(ReadArray(
                    "POLYGON (( 1 5, 1 7, 5 7, 5 3, 2 3, 1 5 ))"),
                1.5,
                ReadArray(
                    "POLYGON ((1 7, 5 7, 5 3, 2 3, 1 7))")
            );
        }

        [Test]
        public void TestInnerAtStartingNode()
        {
            CheckResultInner(
                ReadArray(
                    "POLYGON (( 0 5, 0 9, 6 9, 6 2, 1 2, 0 5 ), ( 1 5, 2 3, 5 3, 5 7, 1 7, 1 5 ))",
                    "POLYGON (( 1 5, 1 7, 5 7, 5 3, 2 3, 1 5 ))"),
                1.5,
                ReadArray(
                    "POLYGON ((0 5, 0 9, 6 9, 6 2, 1 2, 0 5), (1 7, 2 3, 5 3, 5 7, 1 7))",
                    "POLYGON ((1 7, 5 7, 5 3, 2 3, 1 7))")
            );
        }

        [Test]
        public void TestAllAtStartingNode()
        {
            CheckResult(ReadArray(
                    "POLYGON (( 0 5, 0 9, 6 9, 6 2, 1 2, 0 5 ), ( 1 5, 2 3, 5 3, 5 7, 1 7, 1 5 ))",
                    "POLYGON (( 1 5, 1 7, 5 7, 5 3, 2 3, 1 5 ))"),
                1.5,
                ReadArray(
                    "POLYGON ((0 9, 6 9, 6 2, 1 2, 0 9), (1 7, 2 3, 5 3, 5 7, 1 7))",
                    "POLYGON ((1 7, 5 7, 5 3, 2 3, 1 7))")
            );
        }

        //---------------------------------

        [Test]
        public void TestInnerSimple()
        {
            CheckResultInner(ReadArray(
                "POLYGON ((50 50, 50 150, 100 190, 100 200, 200 200, 160 150, 120 120, 90 80, 50 50))",
                "POLYGON ((100 0, 50 50, 90 80, 120 120, 160 150, 200 200, 250 100, 170 50, 100 0))"),
                100,
                ReadArray(
                    "POLYGON ((50 50, 50 150, 100 190, 100 200, 200 200, 50 50))",
                    "POLYGON ((200 200, 50 50, 100 0, 170 50, 250 100, 200 200))")
            );

        }

        //---------------------------------

        [Test]
        public void TestAllEmpty()
        {
            CheckResult(ReadArray(
                "POLYGON EMPTY",
                "POLYGON EMPTY"),
                1,
                ReadArray(
                    "POLYGON EMPTY",
                    "POLYGON EMPTY")
            );
        }

        [Test]
        public void TestOneEmpty()
        {
            CheckResult(ReadArray(
                "POLYGON ((1 9, 5 9.1, 9 9, 9 1, 1 1, 1 9))",
                "POLYGON EMPTY"),
                1,
                ReadArray(
                    "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))",
                    "POLYGON EMPTY")
            );
        }

        [Test]
        public void TestEmptyHole()
        {
            CheckResult(ReadArray(
                "POLYGON ((1 9, 5 9.1, 9 9, 9 1, 1 1, 1 9), EMPTY)",
                "POLYGON EMPTY"),
                1,
                ReadArray(
                    "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9), EMPTY)",
                    "POLYGON EMPTY")
            );
        }

        //==============  Test with removed rings  =======================

        // A Polygon with a small hole containing another Polygon - small Polygon is primary so is not removed
        [Test]
        public void TestPolygonInHoleNotRemoved()
        {
            CheckResult(ReadArray(
                "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9), (2 2, 2 3, 3 3, 2 2))",
                "POLYGON ((2 2, 2 3, 3 3, 2 2))"
                  ),
                1,
                ReadArray(
                    "POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9), (2 2, 2 3, 3 3, 2 2))",
                    "POLYGON ((2 2, 2 3, 3 3, 2 2))")
            );
        }

        [Test]
        public void TestMultiPolygonWithSmallPartRemoved()
        {
            CheckResult(ReadArray(
                "MULTIPOLYGON (((11 9, 15 9, 15 5, 11 5, 11 9)), ((11 2, 12 2, 11 1, 11 2)))",
                "POLYGON ((15 9, 18 9, 19 4, 14 1, 15 5, 15 9))"
                  ),
                1,
                ReadArray(
                    "POLYGON ((11 5, 11 9, 15 9, 15 5, 11 5))",
                    "POLYGON ((15 9, 18 9, 19 4, 14 1, 15 5, 15 9))")
            );
        }

        [Test]
        public void TestMultiPolygonWithTouchingSmallPartsRemoved()
        {
            CheckResult(ReadArray(
                "MULTIPOLYGON (((1 5, 5 5, 5 1, 1 1, 1 5)), ((6 3, 7 2, 6 2, 6 3)), ((8 2, 7 2, 8 3, 8 2)))"
                  ),
                1,
                ReadArray(
                    "POLYGON ((1 5, 5 5, 5 1, 1 1, 1 5)))")
            );
        }

        [Test]
        public void TestMultiPolygonHolesSmallPartRemoved()
        {
            CheckResult(ReadArray(
                "POLYGON ((0 9, 9 9, 9 0, 0 0, 0 9), (2 5, 2 4, 3 5, 2 5), (4 5, 4 3, 6 3, 6 5, 4 5))",
                "MULTIPOLYGON (((2 5, 3 5, 2 4, 2 5)), ((4 5, 6 5, 6 3, 4 3, 4 5)))"
                  ),
                1,
                ReadArray(
                    "POLYGON ((0 9, 9 9, 9 0, 0 0, 0 9), (4 5, 4 3, 6 3, 6 5, 4 5))",
                    "POLYGON ((4 5, 6 5, 6 3, 4 3, 4 5))")
            );
        }

        [Test]
        public void TestMultiPolygonHolesSmallPart()
        {
            CheckResultRemovalSize(ReadArray(
                "POLYGON ((0 9, 9 9, 9 0, 0 0, 0 9), (2 5, 2 4, 3 5, 2 5), (4 5, 4 3, 6 3, 6 5, 4 5))",
                "MULTIPOLYGON (((2 5, 3 5, 2 4, 2 5)), ((4 5, 6 5, 6 3, 4 3, 4 5)))"
                  ),
                1, 0,
                ReadArray(
                    "POLYGON ((0 9, 9 9, 9 0, 0 0, 0 9), (2 5, 2 4, 3 5, 2 5), (4 5, 4 3, 6 3, 6 5, 4 5))",
                    "MULTIPOLYGON (((2 5, 3 5, 2 4, 2 5)), ((4 5, 6 5, 6 3, 4 3, 4 5)))")
            );
        }

        [Test]
        public void TestTolerances()
        {
            CheckResult(ReadArray(
                "POLYGON ((1 19, 6 19, 7 11, 6 1, 1 1, 1 19))",
                "POLYGON ((6 19, 12 19, 11 15, 12 1, 6 1, 7 11, 6 19))",
                "POLYGON ((12 19, 19 19, 22 10, 19 1, 12 1, 11 15, 12 19))"
                  ),
                new double[] { 0, 3, 6 },
                ReadArray(
                    "POLYGON ((6 19, 7 11, 6 1, 1 1, 1 19, 6 19))",
                    "POLYGON  ((6 19, 12 19, 12 1, 6 1, 7 11, 6 19))",
                    "POLYGON  ((12 19, 19 19, 19 1, 12 1, 12 19))")
            );
        }
        //=================================


        private void CheckNoop(Geometry[] input)
        {
            var actual = CoverageSimplifier.Simplify(input, 0);
            CheckEqual(input, actual);
        }

        private void CheckResult(Geometry[] input, double[] tolerances, Geometry[] expected)
        {
            var actual = CoverageSimplifier.Simplify(input, tolerances);
            CheckEqual(expected, actual);
        }

        private void CheckResult(Geometry[] input, double tolerance, Geometry[] expected)
        {
            var actual = CoverageSimplifier.Simplify(input, tolerance);
            CheckEqual(expected, actual);
        }

        private void CheckResultRemovalSize(Geometry[] input, double tolerance, double removalFactor, Geometry[] expected)
        {
            var simplifier = new CoverageSimplifier(input);
            simplifier.RemovableRingSizeFactor = removalFactor;
            var actual = simplifier.Simplify(tolerance);
            CheckEqual(expected, actual);
        }

        private void CheckResultInner(Geometry[] input, double tolerance, Geometry[] expected)
        {
            var actual = CoverageSimplifier.SimplifyInner(input, tolerance);
            CheckEqual(expected, actual);
        }
    }
}
