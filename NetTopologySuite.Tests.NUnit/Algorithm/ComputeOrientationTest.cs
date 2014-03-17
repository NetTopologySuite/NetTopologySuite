using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixtureAttribute]
    public class ComputeOrientationTest
    {
        private readonly WKTReader _reader = new WKTReader();

        [TestAttribute]
        public void TestCCW()
        {
            Assert.IsTrue(IsAllOrientationsEqual(GetCoordinates("LINESTRING ( 0 0, 0 1, 1 1)")));

            // experimental case - can't make it fail
            Coordinate[] pts2 = {
            new Coordinate(1.0000000000004998, -7.989685402102996),
            new Coordinate(10.0, -7.004368924503866),
            new Coordinate(1.0000000000005, -7.989685402102996),
            };
            Assert.IsTrue(IsAllOrientationsEqual(pts2));
        }
  
        // MD - deliberately disabled
        [TestAttribute]
        [IgnoreAttribute("This case fails because subtraction of small from large loses precision")]
        public void TestBadCCW()
        {
            // this case fails because subtraction of small from large loses precision
            Coordinate[] pts1 = {
            new Coordinate(1.4540766091864998, -7.989685402102996),
            new Coordinate(23.131039116367354, -7.004368924503866),
            new Coordinate(1.4540766091865, -7.989685402102996),
            };
            Assert.IsTrue(IsAllOrientationsEqual(pts1));
        }

        private static bool IsAllOrientationsEqual(Coordinate[] pts)
        {
            int[] orient = new int[3];
            orient[0] = CGAlgorithms.ComputeOrientation(pts[0], pts[1], pts[2]);
            orient[1] = CGAlgorithms.ComputeOrientation(pts[1], pts[2], pts[0]);
            orient[2] = CGAlgorithms.ComputeOrientation(pts[2], pts[0], pts[1]);
            return orient[0] == orient[1] && orient[0] == orient[2];
        }

        private Coordinate[] GetCoordinates(String wkt)
        {
            IGeometry geom = _reader.Read(wkt);
            return geom.Coordinates;
        }
    }
}
