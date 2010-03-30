using System;
using GisSharpBlog.NetTopologySuite.Algorithm;
using NUnit.Framework;
using Coord = NetTopologySuite.Coordinates.Simple.Coordinate;
namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public class AngleTest
    {
        private const double Tolerance = 1E-5;

        [Test]
        public void TestAngle()
        {
            Assert.AreEqual(Angle<Coord>.CalculateAngle(GeometryUtils.CoordFac.Create(10, 0)), 0.0, Tolerance);
            Assert.AreEqual(Angle<Coord>.CalculateAngle(GeometryUtils.CoordFac.Create(10, 10)), Math.PI / 4, Tolerance);
            Assert.AreEqual(Angle<Coord>.CalculateAngle(GeometryUtils.CoordFac.Create(0, 10)), Math.PI / 2, Tolerance);
            Assert.AreEqual(Angle<Coord>.CalculateAngle(GeometryUtils.CoordFac.Create(-10, 10)), 0.75 * Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.CalculateAngle(GeometryUtils.CoordFac.Create(-10, 0)), Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.CalculateAngle(GeometryUtils.CoordFac.Create(-10, -0.1)), -3.131592986903128, Tolerance);
            Assert.AreEqual(Angle<Coord>.CalculateAngle(GeometryUtils.CoordFac.Create(-10, -10)), -0.75 * Math.PI, Tolerance);
        }

        [Test]
        public void TestIsAcute()
        {
            Assert.AreEqual(Angle<Coord>.IsAcute(GeometryUtils.CoordFac.Create(10, 0), GeometryUtils.CoordFac.Create(0, 0), GeometryUtils.CoordFac.Create(5, 10)), true);
            Assert.AreEqual(Angle<Coord>.IsAcute(GeometryUtils.CoordFac.Create(10, 0), GeometryUtils.CoordFac.Create(0, 0), GeometryUtils.CoordFac.Create(5, -10)), true);
            // angle of 0
            Assert.AreEqual(Angle<Coord>.IsAcute(GeometryUtils.CoordFac.Create(10, 0), GeometryUtils.CoordFac.Create(0, 0), GeometryUtils.CoordFac.Create(10, 0)), true);

            Assert.AreEqual(Angle<Coord>.IsAcute(GeometryUtils.CoordFac.Create(10, 0), GeometryUtils.CoordFac.Create(0, 0), GeometryUtils.CoordFac.Create(-5, 10)), false);
            Assert.AreEqual(Angle<Coord>.IsAcute(GeometryUtils.CoordFac.Create(10, 0), GeometryUtils.CoordFac.Create(0, 0), GeometryUtils.CoordFac.Create(-5, -10)), false);


        }
        [Test]
        public void TestNormalizePositive()
        {
            Assert.AreEqual(Angle<Coord>.NormalizePositive(0.0), 0.0, Tolerance);

            Assert.AreEqual(Angle<Coord>.NormalizePositive(-0.5 * Math.PI), 1.5 * Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.NormalizePositive(-Math.PI), Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.NormalizePositive(-1.5 * Math.PI), .5 * Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.NormalizePositive(-2 * Math.PI), 0.0, Tolerance);
            Assert.AreEqual(Angle<Coord>.NormalizePositive(-2.5 * Math.PI), 1.5 * Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.NormalizePositive(-3 * Math.PI), Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.NormalizePositive(-4 * Math.PI), 0.0, Tolerance);

            Assert.AreEqual(Angle<Coord>.NormalizePositive(0.5 * Math.PI), 0.5 * Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.NormalizePositive(Math.PI), Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.NormalizePositive(1.5 * Math.PI), 1.5 * Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.NormalizePositive(2 * Math.PI), 0.0, Tolerance);
            Assert.AreEqual(Angle<Coord>.NormalizePositive(2.5 * Math.PI), 0.5 * Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.NormalizePositive(3 * Math.PI), Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.NormalizePositive(4 * Math.PI), 0.0, Tolerance);

        }
        [Test]
        public void TestNormalize()
        {
            Assert.AreEqual(Angle<Coord>.Normalize(0.0), 0.0, Tolerance);

            Assert.AreEqual(Angle<Coord>.Normalize(-0.5 * Math.PI), -0.5 * Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.Normalize(-Math.PI), Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.Normalize(-1.5 * Math.PI), .5 * Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.Normalize(-2 * Math.PI), 0.0, Tolerance);
            Assert.AreEqual(Angle<Coord>.Normalize(-2.5 * Math.PI), -0.5 * Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.Normalize(-3 * Math.PI), Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.Normalize(-4 * Math.PI), 0.0, Tolerance);

            Assert.AreEqual(Angle<Coord>.Normalize(0.5 * Math.PI), 0.5 * Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.Normalize(Math.PI), Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.Normalize(1.5 * Math.PI), -0.5 * Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.Normalize(2 * Math.PI), 0.0, Tolerance);
            Assert.AreEqual(Angle<Coord>.Normalize(2.5 * Math.PI), 0.5 * Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.Normalize(3 * Math.PI), Math.PI, Tolerance);
            Assert.AreEqual(Angle<Coord>.Normalize(4 * Math.PI), 0.0, Tolerance);


        }


    }
}
