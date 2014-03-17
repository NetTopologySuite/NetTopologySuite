using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixtureAttribute]
    public class AngleTest
    {
        private const double Tolerance = 1E-5;

        [TestAttribute]
        public void TestAngle()
        {
            Assert.AreEqual(AngleUtility.Angle(new Coordinate(10, 0)), 0.0, Tolerance);
            Assert.AreEqual(AngleUtility.Angle(new Coordinate(10, 10)), Math.PI / 4, Tolerance);
            Assert.AreEqual(AngleUtility.Angle(new Coordinate(0, 10)), Math.PI / 2, Tolerance);
            Assert.AreEqual(AngleUtility.Angle(new Coordinate(-10, 10)), 0.75 * Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.Angle(new Coordinate(-10, 0)), Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.Angle(new Coordinate(-10, -0.1)), -3.131592986903128, Tolerance);
            Assert.AreEqual(AngleUtility.Angle(new Coordinate(-10, -10)), -0.75 * Math.PI, Tolerance);
        }
        [TestAttribute]
        public void TestIsAcute()
        {
            Assert.AreEqual(AngleUtility.IsAcute(new Coordinate(10, 0), new Coordinate(0, 0), new Coordinate(5, 10)), true);
            Assert.AreEqual(AngleUtility.IsAcute(new Coordinate(10, 0), new Coordinate(0, 0), new Coordinate(5, -10)), true);
            // angle of 0
            Assert.AreEqual(AngleUtility.IsAcute(new Coordinate(10, 0), new Coordinate(0, 0), new Coordinate(10, 0)), true);

            Assert.AreEqual(AngleUtility.IsAcute(new Coordinate(10, 0), new Coordinate(0, 0), new Coordinate(-5, 10)), false);
            Assert.AreEqual(AngleUtility.IsAcute(new Coordinate(10, 0), new Coordinate(0, 0), new Coordinate(-5, -10)), false);


        }
        [TestAttribute]
        public void TestNormalizePositive()
        {
            Assert.AreEqual(AngleUtility.NormalizePositive(0.0), 0.0, Tolerance);

            Assert.AreEqual(AngleUtility.NormalizePositive(-0.5 * Math.PI), 1.5 * Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.NormalizePositive(-Math.PI), Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.NormalizePositive(-1.5 * Math.PI), .5 * Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.NormalizePositive(-2 * Math.PI), 0.0, Tolerance);
            Assert.AreEqual(AngleUtility.NormalizePositive(-2.5 * Math.PI), 1.5 * Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.NormalizePositive(-3 * Math.PI), Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.NormalizePositive(-4 * Math.PI), 0.0, Tolerance);

            Assert.AreEqual(AngleUtility.NormalizePositive(0.5 * Math.PI), 0.5 * Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.NormalizePositive(Math.PI), Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.NormalizePositive(1.5 * Math.PI), 1.5 * Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.NormalizePositive(2 * Math.PI), 0.0, Tolerance);
            Assert.AreEqual(AngleUtility.NormalizePositive(2.5 * Math.PI), 0.5 * Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.NormalizePositive(3 * Math.PI), Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.NormalizePositive(4 * Math.PI), 0.0, Tolerance);

        }

        [TestAttribute]
        public void TestNormalize()
        {
            Assert.AreEqual(AngleUtility.Normalize(0.0), 0.0, Tolerance);

            Assert.AreEqual(AngleUtility.Normalize(-0.5 * Math.PI), -0.5 * Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.Normalize(-Math.PI), Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.Normalize(-1.5 * Math.PI), .5 * Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.Normalize(-2 * Math.PI), 0.0, Tolerance);
            Assert.AreEqual(AngleUtility.Normalize(-2.5 * Math.PI), -0.5 * Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.Normalize(-3 * Math.PI), Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.Normalize(-4 * Math.PI), 0.0, Tolerance);

            Assert.AreEqual(AngleUtility.Normalize(0.5 * Math.PI), 0.5 * Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.Normalize(Math.PI), Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.Normalize(1.5 * Math.PI), -0.5 * Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.Normalize(2 * Math.PI), 0.0, Tolerance);
            Assert.AreEqual(AngleUtility.Normalize(2.5 * Math.PI), 0.5 * Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.Normalize(3 * Math.PI), Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.Normalize(4 * Math.PI), 0.0, Tolerance);


        }
    }
}
