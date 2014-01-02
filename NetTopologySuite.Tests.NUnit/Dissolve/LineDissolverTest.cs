using System;
using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Dissolve;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Dissolve
{
    [TestFixture]
    public class LineDissolverTest
    {
        [Test]
        public void TestDebug()
        {
            //TestSingleLine();
            TestIsolatedRing();
        }

        [Test]
        public void TestSingleSegmentLine()
        {
            CheckDissolve(
                "LINESTRING (0 0, 1 1)", 
                "LINESTRING (0 0, 1 1)");
        }

        [Test]
        public void TestTwoSegmentLine()
        {
            CheckDissolve(
                "LINESTRING (0 0, 1 1, 2 2)", 
                "LINESTRING (0 0, 1 1, 2 2)");
        }

        [Test]
        public void TestOverlappingTwoSegmentLines()
        {
            CheckDissolve(
                new[] { "LINESTRING (0 0, 1 1, 2 2)", "LINESTRING (1 1, 2 2, 3 3)" }, 
                "LINESTRING (0 0, 1 1, 2 2, 3 3)");
        }

        [Test]
        public void TestOverlappingLines3()
        {
            CheckDissolve(
                new[] 
                {
                    "LINESTRING (0 0, 1 1, 2 2)", 
                    "LINESTRING (1 1, 2 2, 3 3)",
                    "LINESTRING (1 1, 2 2, 2 0)" 
                },
                "MULTILINESTRING ((0 0, 1 1, 2 2), (2 0, 2 2), (2 2, 3 3))");
        }

        [Test]
        public void TestDivergingLines()
        {
            CheckDissolve(
                "MULTILINESTRING ((0 0, 1 0, 2 1), (0 0, 1 0, 2 0), (1 0, 2 1, 2 0, 3 0))",
                "MULTILINESTRING ((0 0, 1 0), (1 0, 2 0), (1 0, 2 1, 2 0), (2 0, 3 0))");
        }

        [Test]
        public void TestLollipop()
        {
            CheckDissolve(
                "LINESTRING (0 0, 1 0, 2 0, 2 1, 1 0, 0 0)",
                "MULTILINESTRING ((0 0, 1 0), (1 0, 2 0, 2 1, 1 0))");
        }

        [Test]
        public void TestDisjointLines()
        {
            CheckDissolve(
                "MULTILINESTRING ((0 0, 1 0, 2 1), (10 0, 11 0, 12 0))",
                "MULTILINESTRING ((0 0, 1 0, 2 1), (10 0, 11 0, 12 0))");
        }

        [Test]
        public void TestSingleLine()
        {
            CheckDissolve(
                "MULTILINESTRING ((0 0, 1 0, 2 1))",
                "LINESTRING (0 0, 1 0, 2 1)");
        }

        [Test]
        public void TestOneSegmentY()
        {
            CheckDissolve(
                "MULTILINESTRING ((0 0, 1 1, 2 2), (1 1, 1 2))",
                "MULTILINESTRING ((0 0, 1 1), (1 1, 2 2), (1 1, 1 2))");
        }

        [Test]
        public void TestTwoSegmentY()
        {
            CheckDissolve(
                "MULTILINESTRING ((0 0, 9 9, 10 10, 11 11, 20 20), (10 10, 10 20))",
                "MULTILINESTRING ((10 20, 10 10), (10 10, 9 9, 0 0), (10 10, 11 11, 20 20))");
        }

        [Test]
        public void TestIsolatedRing()
        {
            CheckDissolve(
                "LINESTRING (0 0, 1 1, 1 0, 0 0)",
                "LINESTRING (0 0, 1 1, 1 0, 0 0)");
        }

        [Test]
        public void TestIsolateRingFromMultipleLineStrings()
        {
            CheckDissolve(
                "MULTILINESTRING ((0 0, 1 0, 1 1), (0 0, 0 1, 1 1))",
                "LINESTRING (0 0, 0 1, 1 1, 1 0, 0 0)");
        }

        // Shows that rings with incident lines are created with the correct node point.
        [Test]
        public void TestRingWithTail()
        {
            CheckDissolve(
                "MULTILINESTRING ((0 0, 1 0, 1 1), (0 0, 0 1, 1 1), (1 0, 2 0))",
                "MULTILINESTRING ((1 0, 0 0, 0 1, 1 1, 1 0), (1 0, 2 0))");
        }

        private void CheckDissolve(string wkt, string expectedWkt)
        {
            CheckDissolve(new[] { wkt }, expectedWkt);
        }

        private void CheckDissolve(string[] wkt, string expectedWkt)
        {
            IList<IGeometry> geoms = GeometryUtils.ReadWKT(wkt);
            IGeometry expected = GeometryUtils.ReadWKT(expectedWkt);
            CheckDissolve(geoms, expected);
        }

        private void CheckDissolve(IEnumerable<IGeometry> geoms, IGeometry expected)
        {
            LineDissolver d = new LineDissolver();
            d.Add(geoms);
            IGeometry result = d.GetResult();
            IGeometry rnorm = result.Normalized();
            IGeometry enorm = expected.Normalized();
            bool equal = rnorm.EqualsExact(enorm);
            Assert.IsTrue(equal, String.Format("Expected = {0} actual = {1}", expected, rnorm));
        }
    }
}
