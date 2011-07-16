using System;
using GeoAPI.Coordinates;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;
using NUnit.Framework;
using Coordinate = NetTopologySuite.Coordinates.Simple.Coordinate;

namespace NetTopologySuite.Tests.NUnit.Noding
{
    [TestFixture]
    public class SegmentPointComparatorFullTest
    {
        [Test]
        public void TestQuadrant0()
        {
            CheckSegment(100, 0);
            CheckSegment(100, 50);
            CheckSegment(100, 100);
            CheckSegment(100, 150);
            CheckSegment(0, 100);
        }

        [Test]
        public void TestQuadrant4()
        {
            CheckSegment(100, -50);
            CheckSegment(100, -100);
            CheckSegment(100, -150);
            CheckSegment(0, -100);
        }

        [Test]
        public void TestQuadrant1()
        {
            CheckSegment(-100, 0);
            CheckSegment(-100, 50);
            CheckSegment(-100, 100);
            CheckSegment(-100, 150);
        }

        [Test]
        public void TestQuadrant2()
        {
            CheckSegment(-100, 0);
            CheckSegment(-100, -50);
            CheckSegment(-100, -100);
            CheckSegment(-100, -150);
        }

        private void CheckSegment(double x, double y)
        {
            ICoordinateFactory<Coordinate> factory = GeometryUtils.GetScaledFactory(1d).CoordinateFactory;

            Coordinate seg0 = factory.Create(0, 0);
            Coordinate seg1 = factory.Create(x, y);
            LineSegment<Coordinate> seg = new LineSegment<Coordinate>(seg0, seg1);

            for (int i = 0; i < 4; i++)
            {
                double dist = i;

                double gridSize = 1 / factory.PrecisionModel.Scale;

                CheckPointsAtDistance(seg, dist, dist + 1.0 * gridSize);
                CheckPointsAtDistance(seg, dist, dist + 2.0 * gridSize);
                CheckPointsAtDistance(seg, dist, dist + 3.0 * gridSize);
                CheckPointsAtDistance(seg, dist, dist + 4.0 * gridSize);
            }
        }

        private Coordinate ComputePoint(LineSegment<Coordinate> seg, double dist)
        {
            ICoordinateFactory<Coordinate> factory = GeometryUtils.GetScaledFactory(1d).CoordinateFactory;
            double dx = seg.P1.X - seg.P0.X;
            double dy = seg.P1.Y - seg.P0.Y;
            double len = seg.Length;
            Coordinate pt = factory.Create(dist * dx / len, dist * dy / len);
            pt = factory.PrecisionModel.MakePrecise(pt);
            return pt;
        }

        private void CheckPointsAtDistance(LineSegment<Coordinate> seg, double dist0, double dist1)
        {
            Coordinate p0 = ComputePoint(seg, dist0);
            Coordinate p1 = ComputePoint(seg, dist1);
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

        private void CheckNodePosition(LineSegment<Coordinate> seg, Coordinate p0, Coordinate p1, int expectedPositionValue)
        {
            Octants octant = Octant.GetOctant(seg.P0, seg.P1);
            int posValue = SegmentPointComparator.Compare(octant, p0, p1);
            Console.WriteLine(octant + " " + p0 + " " + p1 + " " + posValue);
            Assert.IsTrue(posValue == expectedPositionValue);
        }
    }
}