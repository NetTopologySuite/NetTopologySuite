using System;
using System.Linq;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Shape.Random;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public class AngleTest
    {
        private const double Tolerance = 1E-5;

        [Test]
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
        [Test]
        public void TestIsAcute()
        {
            Assert.AreEqual(AngleUtility.IsAcute(new Coordinate(10, 0), new Coordinate(0, 0), new Coordinate(5, 10)), true);
            Assert.AreEqual(AngleUtility.IsAcute(new Coordinate(10, 0), new Coordinate(0, 0), new Coordinate(5, -10)), true);
            // angle of 0
            Assert.AreEqual(AngleUtility.IsAcute(new Coordinate(10, 0), new Coordinate(0, 0), new Coordinate(10, 0)), true);

            Assert.AreEqual(AngleUtility.IsAcute(new Coordinate(10, 0), new Coordinate(0, 0), new Coordinate(-5, 10)), false);
            Assert.AreEqual(AngleUtility.IsAcute(new Coordinate(10, 0), new Coordinate(0, 0), new Coordinate(-5, -10)), false);

        }
        [Test]
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

        [Test]
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

        [Test]
        public void TestInteriorAngle()
        {
            var p1 = new Coordinate(1, 2);
            var p2 = new Coordinate(3, 2);
            var p3 = new Coordinate(2, 1);

            // Tests all interior angles of a triangle "POLYGON ((1 2, 3 2, 2 1, 1 2))"
            Assert.AreEqual(45, AngleUtility.ToDegrees(AngleUtility.InteriorAngle(p1, p2, p3)), 0.01);
            Assert.AreEqual(90, AngleUtility.ToDegrees(AngleUtility.InteriorAngle(p2, p3, p1)), 0.01);
            Assert.AreEqual(45, AngleUtility.ToDegrees(AngleUtility.InteriorAngle(p3, p1, p2)), 0.01);
            // Tests interior angles greater than 180 degrees
            Assert.AreEqual(315, AngleUtility.ToDegrees(AngleUtility.InteriorAngle(p3, p2, p1)), 0.01);
            Assert.AreEqual(270, AngleUtility.ToDegrees(AngleUtility.InteriorAngle(p1, p3, p2)), 0.01);
            Assert.AreEqual(315, AngleUtility.ToDegrees(AngleUtility.InteriorAngle(p2, p1, p3)), 0.01);
        }

        /// <summary>
        /// Tests interior angle calculation using a number of random triangles
        /// </summary>
        [Test]
        public void TestInteriorAngle_randomTriangles()
        {
            var geometryFactory = new GeometryFactory();
            var coordinateSequenceFactory = geometryFactory.CoordinateSequenceFactory;
            for (int i = 0; i < 100; i++)
            {
                var builder = new RandomPointsBuilder();
                builder.NumPoints = 3;
                var threeRandomPoints = builder.GetGeometry();
                var triangle = geometryFactory.CreatePolygon(
                        CoordinateSequences.EnsureValidRing(
                                coordinateSequenceFactory,
                                coordinateSequenceFactory.Create(threeRandomPoints.Coordinates)
                        )
                );
                // Triangle coordinates in clockwise order
                var c = Orientation.IsCCW(triangle.Coordinates)
                        ? triangle.Reverse().Coordinates
                        : triangle.Coordinates;
                double sumOfInteriorAngles = AngleUtility.InteriorAngle(c[0], c[1], c[2])
                        + AngleUtility.InteriorAngle(c[1], c[2], c[0])
                        + AngleUtility.InteriorAngle(c[2], c[0], c[1]);
                Assert.AreEqual(
                        Math.PI,
                        sumOfInteriorAngles,
                        0.01,
                        i + ": The sum of the angles of a triangle is not equal to two right angles for points: " + c.Select(i => i.ToString())
                );
            }
        }

    }
}
