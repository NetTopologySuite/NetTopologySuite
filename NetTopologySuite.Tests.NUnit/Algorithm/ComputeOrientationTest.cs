using System;
using NetTopologySuite.Algorithm;
using NUnit.Framework;
using Coord = NetTopologySuite.Coordinates.Simple.Coordinate;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public class ComputeOrientationTest
    {
        [Test]
        public void TestCCW()
        {
            Assert.IsTrue(
                IsAllOrientationsEqual(GeometryUtils.Reader.Read("LINESTRING ( 0 0, 0 1, 1 1)").Coordinates.ToArray()));

            // experimental case - can't make it fail
            Coord[] pts2 = {
                               GeometryUtils.CoordFac.Create(1.0000000000004998, -7.989685402102996),
                               GeometryUtils.CoordFac.Create(10.0, -7.004368924503866),
                               GeometryUtils.CoordFac.Create(1.0000000000005, -7.989685402102996),
                           };
            Assert.IsTrue(IsAllOrientationsEqual(pts2));
        }

        // MD - deliberately disabled
        //[Test]
        public void XtestBadCCW()
        {
            // this case fails because subtraction of small from large loses precision
            Coord[] pts1 = {
                               GeometryUtils.CoordFac.Create(1.4540766091864998, -7.989685402102996),
                               GeometryUtils.CoordFac.Create(23.131039116367354, -7.004368924503866),
                               GeometryUtils.CoordFac.Create(1.4540766091865, -7.989685402102996),
                           };
            Assert.IsTrue(IsAllOrientationsEqual(pts1));
        }

        private Boolean IsAllOrientationsEqual(Coord[] pts)
        {
            Orientation[] orient = new Orientation[]
                                 {
                                     CGAlgorithms<Coord>.ComputeOrientation(pts[0], pts[1], pts[2]),
                                     CGAlgorithms<Coord>.ComputeOrientation(pts[1], pts[2], pts[0]),
                                     CGAlgorithms<Coord>.ComputeOrientation(pts[2], pts[0], pts[1])
                                 };
            return orient[0] == orient[1] && orient[0] == orient[2];
        }

    }
}