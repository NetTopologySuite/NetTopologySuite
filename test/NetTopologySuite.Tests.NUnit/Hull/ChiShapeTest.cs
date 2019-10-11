﻿using NetTopologySuite.Hull;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Hull
{
    [TestFixture]
    public class ChiShapeTest : GeometryTestCase
    {

        [Test]
        public void TestSimple()
        {
            CheckHull(
                "POLYGON ((100 200, 200 180, 300 200, 200 190, 100 200))",
                150,
                "POLYGON ((100 200, 200 180, 300 200, 200 190, 100 200))"
                );
        }

        [Test(Author ="Jeroen Bloemscheer",
            Description ="Test found on turf.js website uses angular degrees for measure")]
        public void TestTurf()
        {
            CheckHull(
                "LINESTRING (-63.601226 44.642643, -63.591442 44.651436, -63.580799 44.648749, -63.573589 44.641788, -63.587665 44.64533, -63.595218 44.64765)",
                0.0145,
                "POLYGON ((-63.601226 44.642643, -63.587665 44.64533, -63.573589 44.641788, -63.580799 44.648749, -63.591442 44.651436, -63.601226 44.642643))"
                );
        }


        private void CheckHull(string inputWKT, double tolerance, string expectedWKT)
        {
            var input = Read(inputWKT);
            var expected = Read(expectedWKT);
            var hull = new ChiShape(input, tolerance);
            var actual = hull.GetResult();
            CheckEqual(expected, actual);
        }
    }
}
