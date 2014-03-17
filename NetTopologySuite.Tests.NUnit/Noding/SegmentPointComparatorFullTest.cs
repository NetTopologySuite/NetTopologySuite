using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Noding;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Noding
{
    /// <summary>
    /// Test IntersectionSegment#compareNodePosition using an exhaustive set
    /// of test cases
    /// </summary>
    [TestFixtureAttribute]
    public class SegmentPointComparatorFullTest
    {
        private PrecisionModel pm = new PrecisionModel(1.0);

        [TestAttribute]
        public void TestQuadrant0()
        {
            CheckSegment(100, 0);
            CheckSegment(100, 50);
            CheckSegment(100, 100);
            CheckSegment(100, 150);
            CheckSegment(0, 100);
        }

        [TestAttribute]
        public void TestQuadrant4()
        {
            CheckSegment(100, -50);
            CheckSegment(100, -100);
            CheckSegment(100, -150);
            CheckSegment(0, -100);
        }

        [TestAttribute]
        public void TestQuadrant1()
        {
            CheckSegment(-100, 0);
            CheckSegment(-100, 50);
            CheckSegment(-100, 100);
            CheckSegment(-100, 150);
        }

        [TestAttribute]
        public void TestQuadrant2()
        {
            CheckSegment(-100, 0);
            CheckSegment(-100, -50);
            CheckSegment(-100, -100);
            CheckSegment(-100, -150);
        }

        private void CheckSegment(double x, double y)
        {
            Coordinate seg0 = new Coordinate(0, 0);
            Coordinate seg1 = new Coordinate(x, y);
            LineSegment seg = new LineSegment(seg0, seg1);

            for (int i = 0; i < 4; i++)
            {
                double dist = i;

                double gridSize = 1 / pm.Scale;

                CheckPointsAtDistance(seg, dist, dist + 1.0 * gridSize);
                CheckPointsAtDistance(seg, dist, dist + 2.0 * gridSize);
                CheckPointsAtDistance(seg, dist, dist + 3.0 * gridSize);
                CheckPointsAtDistance(seg, dist, dist + 4.0 * gridSize);
            }
        }

        private Coordinate computePoint(LineSegment seg, double dist)
        {
            double dx = seg.P1.X - seg.P0.X;
            double dy = seg.P1.Y - seg.P0.Y;
            double len = seg.Length;
            Coordinate pt = new Coordinate(dist * dx / len, dist * dy / len);
            pm.MakePrecise(pt);
            return pt;
        }

        private void CheckPointsAtDistance(LineSegment seg, double dist0, double dist1)
        {
            Coordinate p0 = computePoint(seg, dist0);
            Coordinate p1 = computePoint(seg, dist1);
            if (p0.Equals(p1))
            {
                CheckNodePosition(seg, p0, p1, 0);
            }
            else
            {
                CheckNodePosition(seg, p0, p1, -1);
                CheckNodePosition(seg, p1, p0, 1);
            }
        }

        private void CheckNodePosition(LineSegment seg, Coordinate p0, Coordinate p1, int expectedPositionValue)
        {
            Octants octant = Octant.GetOctant(seg.P0, seg.P1);
            int posValue = SegmentPointComparator.Compare(octant, p0, p1);
            Console.WriteLine(octant + " " + p0 + " " + p1 + " " + posValue);
            Assert.IsTrue(posValue == expectedPositionValue);
        }
    }
}
