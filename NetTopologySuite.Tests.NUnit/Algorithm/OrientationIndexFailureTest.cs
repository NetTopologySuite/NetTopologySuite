using System;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Mathematics;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    /// <summary>
    /// Tests failure cases of CGAlgorithms.computeOrientation
    /// </summary>
    [TestFixtureAttribute]
    public class OrientationIndexFailureTest
    {

        /// <summary>
        /// This is included to confirm this test is operating correctly
        /// </summary>
        [TestAttribute]
        public void TestSanity()
        {
            Assert.IsTrue(OrientationIndexTest.IsAllOrientationsEqual(
                OrientationIndexTest.GetCoordinates("LINESTRING ( 0 0, 0 1, 1 1)")));
        }

        [TestAttribute/*, ExpectedException(typeof(AssertionException))*/]
        public void TestBadCCW()
        {
            // this case fails because subtraction of small from large loses precision
            Coordinate[] pts = {
                                   new Coordinate(1.4540766091864998, -7.989685402102996),
                                   new Coordinate(23.131039116367354, -7.004368924503866),
                                   new Coordinate(1.4540766091865, -7.989685402102996)
                               };
            CheckOrientation(pts);
        }

        [TestAttribute/*, ExpectedException(typeof(AssertionException))*/]
        public void TestBadCCW2()
        {
            // this case fails because subtraction of small from large loses precision
            Coordinate[] pts = {
                                   new Coordinate(219.3649559090992, 140.84159161824724),
                                   new Coordinate(168.9018919682399, -5.713787599646864),
                                   new Coordinate(186.80814046338352, 46.28973405831556)
                               };
            CheckOrientation(pts);
        }

        [TestAttribute/*, ExpectedException(typeof(AssertionException))*/]
        public void TestBadCCW3()
        {
            // this case fails because subtraction of small from large loses precision
            Coordinate[] pts = {
                                   new Coordinate(279.56857838488514, -186.3790522565901),
                                   new Coordinate(-20.43142161511487, 13.620947743409914),
                                   new Coordinate(0, 0)
                               };
            CheckOrientation(pts);
        }

        [TestAttribute]
        public void TestBadCCW4()
        {
            // from JTS list - 5/15/2012  strange case for the GeometryNoder
            Coordinate[] pts = {
                                   new Coordinate(-26.2, 188.7),
                                   new Coordinate(37.0, 290.7),
                                   new Coordinate(21.2, 265.2)
                               };
            CheckOrientation(pts);
        }

        [TestAttribute]
        public void TestBadCCW5()
        {
            // from JTS list - 6/15/2012  another case from Tomas Fa
            Coordinate[] pts = {
                                   new Coordinate(-5.9, 163.1),
                                   new Coordinate(76.1, 250.7),
                                   new Coordinate(14.6, 185)
                                   //new Coordinate(96.6, 272.6)
                               };
            CheckOrientation(pts);
        }
        [TestAttribute]
        public void TestBadCCW7()
  {
    // from JTS list - 6/26/2012  another case from Tomas Fa
    Coordinate[] pts = {
        new Coordinate(-0.9575, 0.4511),
        new Coordinate(-0.9295, 0.3291),
        new Coordinate(-0.8945, 0.1766)
    };
    CheckDD(pts, true);
    CheckShewchuk(pts, false);
    CheckOriginalJTS(pts, false);
  }

  [TestAttribute]
        public void TestBadCCW7_2()
  {
    // from JTS list - 6/26/2012  another case from Tomas Fa
    // scale to integers - all methods work on this
    Coordinate[] pts = {
        new Coordinate(-9575, 4511),
        new Coordinate(-9295, 3291),
        new Coordinate(-8945, 1766)
    };
    CheckDD(pts, true);
    CheckShewchuk(pts, true);
    CheckOriginalJTS(pts, true);
  }


  public void TestBadCCW6()
  {
    // from JTS Convex Hull "Almost collinear" unit test
    Coordinate[] pts = {
        new Coordinate(-140.8859438214298, 140.88594382142983),
        new Coordinate(-57.309236848216706, 57.30923684821671),
        new Coordinate(-190.9188309203678, 190.91883092036784)
    };
    CheckOrientation(pts);
  }

        private static void CheckOrientation(Coordinate[] pts)
        {
            // this should succeed
            CheckDD(pts, true);
            CheckShewchuk(pts, true);

            // this is expected to fail
            CheckOriginalJTS(pts, false);
        }

        private static void CheckShewchuk(Coordinate[] pts, bool expected)
        {
            Assert.IsTrue(expected == IsAllOrientationsEqualSD(pts), "Shewchuk");
        }

        private static void CheckOriginalJTS(Coordinate[] pts, bool expected)
        {
            Assert.IsTrue(expected == OrientationIndexTest.IsAllOrientationsEqual(pts), "NTS Robust FAIL");
        }

        private static void CheckDD(Coordinate[] pts, bool expected)
        {
            Assert.IsTrue(expected == IsAllOrientationsEqualDD(pts), "DD");
        }
  


        public static bool IsAllOrientationsEqual(
            double p0x, double p0y,
            double p1x, double p1y,
            double p2x, double p2y)
        {
            Coordinate[] pts = {
                                   new Coordinate(p0x, p0y),
                                   new Coordinate(p1x, p1y),
                                   new Coordinate(p2x, p2y)
                               };
            if (!IsAllOrientationsEqualDD(pts))
                throw new InvalidOperationException("High-precision orientation computation FAILED");
            return OrientationIndexTest.IsAllOrientationsEqual(pts);
        }

        public static bool IsAllOrientationsEqualDD(Coordinate[] pts)
        {
            var orient = new int[3];
            orient[0] = NetTopologySuite.Algorithm.CGAlgorithmsDD.OrientationIndex(pts[0], pts[1], pts[2]);
            orient[1] = NetTopologySuite.Algorithm.CGAlgorithmsDD.OrientationIndex(pts[1], pts[2], pts[0]);
            orient[2] = NetTopologySuite.Algorithm.CGAlgorithmsDD.OrientationIndex(pts[2], pts[0], pts[1]);
            return orient[0] == orient[1] && orient[0] == orient[2];
        }

        private static int OrientationIndexDD(Coordinate p1, Coordinate p2, Coordinate q)
        {
            DD dx1 = DD.ValueOf(p2.X) - p1.X;
            DD dy1 = DD.ValueOf(p2.Y) - p1.Y;
            DD dx2 = DD.ValueOf(q.X) - p2.X;
            DD dy2 = DD.ValueOf(q.Y) - p2.Y;

            return SignOfDet2x2DD(dx1, dy1, dx2, dy2);
        }

        private static int SignOfDet2x2DD(DD x1, DD y1, DD x2, DD y2)
        {
            DD det = x1 * y2 - y1 * x2;
            if (det.IsZero)
                return 0;
            if (det.IsNegative)
                return -1;
            return 1;

        }

        public static bool IsAllOrientationsEqualSD(Coordinate[] pts)
        {
            int orient0 = ShewchuksDeterminant.OrientationIndex(pts[0], pts[1], pts[2]);
            int orient1 = ShewchuksDeterminant.OrientationIndex(pts[1], pts[2], pts[0]);
            int orient2 = ShewchuksDeterminant.OrientationIndex(pts[2], pts[0], pts[1]);
            return orient0 == orient1 && orient0 == orient2;
        }

    }
}