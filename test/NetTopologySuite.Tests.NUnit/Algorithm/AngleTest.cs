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
            Assert.AreEqual(AngleUtility.Angle(p(10, 0)), 0.0, Tolerance);
            Assert.AreEqual(AngleUtility.Angle(p(10, 10)), Math.PI / 4, Tolerance);
            Assert.AreEqual(AngleUtility.Angle(p(0, 10)), Math.PI / 2, Tolerance);
            Assert.AreEqual(AngleUtility.Angle(p(-10, 10)), 0.75 * Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.Angle(p(-10, 0)), Math.PI, Tolerance);
            Assert.AreEqual(AngleUtility.Angle(p(-10, -0.1)), -3.131592986903128, Tolerance);
            Assert.AreEqual(AngleUtility.Angle(p(-10, -10)), -0.75 * Math.PI, Tolerance);
        }
        [Test]
        public void TestIsAcute()
        {
            Assert.AreEqual(AngleUtility.IsAcute(p(10, 0), p(0, 0), p(5, 10)), true);
            Assert.AreEqual(AngleUtility.IsAcute(p(10, 0), p(0, 0), p(5, -10)), true);
            // angle of 0
            Assert.AreEqual(AngleUtility.IsAcute(p(10, 0), p(0, 0), p(10, 0)), true);

            Assert.AreEqual(AngleUtility.IsAcute(p(10, 0), p(0, 0), p(-5, 10)), false);
            Assert.AreEqual(AngleUtility.IsAcute(p(10, 0), p(0, 0), p(-5, -10)), false);

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
            var p1 = p(1, 2);
            var p2 = p(3, 2);
            var p3 = p(2, 1);

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

        [Test]
        public void TestAngleBisector()
        {
            Assert.AreEqual(45, AngleUtility.ToDegrees(AngleUtility.Bisector(p(0, 1), p(0, 0), p(1, 0))), 0.01);
            Assert.AreEqual(22.5, AngleUtility.ToDegrees(AngleUtility.Bisector(p(1, 1), p(0, 0), p(1, 0))), 0.01);
            Assert.AreEqual(67.5, AngleUtility.ToDegrees(AngleUtility.Bisector(p(-1, 1), p(0, 0), p(1, 0))), 0.01);
            Assert.AreEqual(-45, AngleUtility.ToDegrees(AngleUtility.Bisector(p(0, -1), p(0, 0), p(1, 0))), 0.01);
            Assert.AreEqual(180, AngleUtility.ToDegrees(AngleUtility.Bisector(p(-1, -1), p(0, 0), p(-1, 1))), 0.01);

            Assert.AreEqual(45, AngleUtility.ToDegrees(AngleUtility.Bisector(p(13, 10), p(10, 10), p(10, 20))), 0.01);
        }

        [Test]
        public void TestSinCosSnap()
        {

            // -720 to 720 degrees with 1 degree increments
            for (int angdeg = -720; angdeg <= 720; angdeg++)
            {
                double ang = AngleUtility.ToRadians(angdeg);

                double rSin = AngleUtility.SinSnap(ang);
                double rCos = AngleUtility.CosSnap(ang);

                double cSin = Math.Sin(ang);
                double cCos = Math.Cos(ang);
                if ((angdeg % 90) == 0)
                {
                    // not always the same for multiples of 90 degrees
                    Assert.That(Math.Abs(rSin - cSin) < 1e-15);
                    Assert.That(Math.Abs(rCos - cCos) < 1e-15);
                }
                else
                {
                    Assert.That(rSin, Is.EqualTo(cSin));
                    Assert.That(rCos, Is.EqualTo(cCos));
                }

            }

            // use radian increments that don't snap to exact degrees or zero
            for (double angrad = -6.3; angrad < 6.3; angrad += 0.013)
            {

                double rSin = AngleUtility.SinSnap(angrad);
                double rCos = AngleUtility.CosSnap(angrad);

                Assert.That(rSin, Is.EqualTo(Math.Sin(angrad)));
                Assert.That(rCos, Is.EqualTo(Math.Cos(angrad)));

            }
        }


        private static Coordinate p(double x, double y) => new Coordinate(x, y);
    }
}
