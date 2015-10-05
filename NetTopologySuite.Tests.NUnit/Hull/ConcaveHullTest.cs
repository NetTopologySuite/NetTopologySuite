using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Hull;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Hull
{
    public class ConcaveHullTest : GeometryTestCase
    {

        [Test, Ignore("Incomplete")]
        public void TestSimple() {
            CheckHull(
                "POLYGON ((100 200, 200 180, 300 200, 200 190, 100 200))",
                150,
                "POLYGON ((100 200, 200 180, 300 200, 200 190, 100 200))"
                );
        }

        private void CheckHull(string inputWKT, double tolerance, string expectedWKT) {
            var input = Read(inputWKT);
            var expected = Read(expectedWKT);
            var hull = new ConcaveHull(input, tolerance);
            var actual = hull.GetResult();
            CheckEqual(expected, actual);
        }
    }
}