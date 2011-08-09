using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Algorithm
{
    [TestFixture]
    public class MinimumBoundingCircleStressTest
    {
        private readonly GeometryFactory _geomFact = new GeometryFactory();
        private readonly Random random = new Random();

        [Test]
        public void TestStressRun()
        {
            while (true)
            {
                int n = random.Next(0, 10000);
                Run(n);
            }
        }

        private void Run(int nPts)
        {
            ICoordinate[] randPts = CreateRandomPoints(nPts);
            IGeometry mp = _geomFact.CreateMultiPoint(randPts);
            MinimumBoundingCircle mbc = new MinimumBoundingCircle(mp);
            ICoordinate centre = mbc.GetCentre();
            double radius = mbc.GetRadius();
            Console.WriteLine("Testing " + nPts + " random points.  Radius = " + radius);

            checkWithinCircle(randPts, centre, radius, 0.0001);
        }

        private void checkWithinCircle(ICoordinate[] pts, ICoordinate centre, double radius, double tolerance)
        {
            for (int i = 0; i < pts.Length; i++)
            {
                ICoordinate p = pts[i];
                double ptRadius = centre.Distance(p);
                double error = ptRadius - radius;
                Assert.LessOrEqual(error, tolerance);
            }
        }

        private ICoordinate[] CreateRandomPoints(int n)
        {
            ICoordinate[] pts = new ICoordinate[n];
            for (int i = 0; i < n; i++)
            {
                double x = 100 * random.NextDouble();
                double y = 100*random.NextDouble();
                pts[i] = new Coordinate(x, y);
            }
            return pts;
        }

    }
}
