﻿using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snap;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.OverlayNG;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    /// <summary>
    /// Tests <see cref="OverlayNG"/> using the <see cref="SnappingNoder"/>
    /// </summary>
    public class OverlayNGSnappingNoderTest : GeometryTestCase
    {

        [Test]
        public void TestRectanglesOneAjarUnion()
        {
            var a = Read("POLYGON ((10 10, 10 5, 5 5, 5 10, 10 10))");
            var b = Read("POLYGON ((10 15, 15 15, 15 7, 10.01 7, 10 15))");
            var expected = Read("POLYGON ((5 5, 5 10, 10 10, 10 15, 15 15, 15 7, 10.01 7, 10 5, 5 5))");
            CheckEqual(expected, Union(a, b, 1));
        }

        [Test]
        public void TestRectanglesBothAjarUnion()
        {
            var a = Read("POLYGON ((10.01 10, 10 5, 5 5, 5 10, 10.01 10))");
            var b = Read("POLYGON ((10 15, 15 15, 15 7, 10.01 7, 10 15))");
            var expected = Read("POLYGON ((5 5, 5 10, 10.01 10, 10 15, 15 15, 15 7, 10.01 7, 10 5, 5 5))");
            CheckEqual(expected, Union(a, b, 1));
        }

        [Test]
        public void TestRandomUnion()
        {
            var a = Read(
                "POLYGON ((85.55954154387994 100, 92.87214039753759 100, 94.7254728121147 100, 98.69765702432045 96.38825885127041, 85.55954154387994 100))");
            var b = Read(
                "POLYGON ((80.20688423699171 99.99999999999999, 100.00000000000003 99.99999999999997, 100.00000000000003 88.87471526860915, 80.20688423699171 99.99999999999999))");
            var expected =
                Read(
                    "POLYGON ((80.20688423699171 99.99999999999999, 85.55954154387994 100, 92.87214039753759 100, 94.7254728121147 100, 100.00000000000003 99.99999999999997, 100.00000000000003 88.87471526860915, 80.20688423699171 99.99999999999999))");
            CheckEqual(expected, Union(a, b, 0.00000001));
        }

        [Test]
        public void TestTrianglesBSegmentsDisplacedSmallTolUnion()
        {
            var a = Read("POLYGON ((100 200, 200 0, 300 200, 100 200))");
            var b = Read("POLYGON ((150 200.01, 200 200.01, 260 200.01, 200 100, 150 200.01))");
            var expected = Read("POLYGON ((150 200.01, 200 200.01, 260 200.01, 300 200, 200 0, 100 200, 150 200.01))");
            CheckEqual(expected, Union(a, b, 0.01));
        }

        [Test]
        public void TestTrianglesBSegmentsDisplacedUnion()
        {
            var a = Read("POLYGON ((100 200, 200 0, 300 200, 100 200))");
            var b = Read("POLYGON ((150 200.01, 200 200.01, 260 200.01, 200 100, 150 200.01))");
            var expected = Read("POLYGON ((100 200, 150 200.01, 200 200.01, 260 200.01, 300 200, 200 0, 100 200))");
            CheckEqual(expected, Union(a, b, 0.1));
        }

        /**
         * Failing due to OverlayUtil#isResultAreaConsistent
         * See https://github.com/locationtech/jts/issues/951
         */
        [Test]
        public void TestRotatedVerticesDifference()
        {
            var a = Read("POLYGON ((0.37676311 2.57570853, 7.28652472 0.00028375, 7.60034931 0.81686059, 0.50229292 3.4551325, 0.37676311 2.57570853))");
            var b = Read("POLYGON ((0.50229292 3.4551325, 7.60034931 0.81686059, 7.28652472 0.00028375, 0.37676311 2.57570853, 0.50229292 3.4551325))");
            var expected = Read("POLYGON EMPTY");
            CheckEqual(expected, difference(a, b, 0.00001));
        }


        public static Geometry Union(Geometry a, Geometry b, double tolerance)
        {
            var noder = GetNoder(tolerance);
            return NetTopologySuite.Operation.OverlayNG.OverlayNG.Overlay(a, b, SpatialFunction.Union, null, noder);
        }

        public static Geometry difference(Geometry a, Geometry b, double tolerance)
        {
            var noder = GetNoder(tolerance);
            return NetTopologySuite.Operation.OverlayNG.OverlayNG.Overlay(a, b, SpatialFunction.Difference, null, noder);
        }

        private static INoder GetNoder(double tolerance)
        {
            var snapNoder = new SnappingNoder(tolerance);
            return new ValidatingNoder(snapNoder);
        }

    }
}
