using NetTopologySuite.Coverage;
using NetTopologySuite.Geometries;
using NetTopologySuite.Tests.NUnit.Coverage;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

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
        public void TestSingleRingNoCollapse()
        {
            CheckResult(ReadArray(
                "POLYGON ((10 50, 60 90, 70 50, 60 10, 10 50))"),
                100000,
                ReadArray(
                    "POLYGON ((10 50, 60 90, 60 10, 10 50))")
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
                    "POLYGON ((20 30, 20 80, 80 20, 50 20, 20 30))",
                    "POLYGON ((10 10, 10 90, 90 90, 90 10, 10 10), (20 30, 50 20, 80 20, 20 80, 20 30))")
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
        //=================================


        private void CheckNoop(Geometry[] input)
        {
            var actual = CoverageSimplifier.Simplify(input, 0);
            CheckEqual(input, actual);
        }

        private void CheckResult(Geometry[] input, double tolerance, Geometry[] expected)
        {
            var actual = CoverageSimplifier.Simplify(input, tolerance);
            CheckEqual(expected, actual);
        }

        private void CheckResultInner(Geometry[] input, double tolerance, Geometry[] expected)
        {
            var actual = CoverageSimplifier.SimplifyInner(input, tolerance);
            CheckEqual(expected, actual);
        }
    }
}
