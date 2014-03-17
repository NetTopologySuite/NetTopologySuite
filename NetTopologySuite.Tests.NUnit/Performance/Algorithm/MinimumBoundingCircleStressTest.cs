using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Algorithm
{
    [TestFixture]
    public class MinimumBoundingCircleStressTest
    {
        private readonly GeometryFactory _geomFact = new GeometryFactory();
        private readonly Random random = new Random();

        [TestAttribute]
        [CategoryAttribute("Stress")]
        public void TestStressRun()
        {
            var count = 100;
            while (count-- > 0)
            {
                int n = random.Next(0, 10000);
                Run(n);
            }
        }

        private void Run(int nPts)
        {
            Coordinate[] randPts = CreateRandomPoints(nPts);
            IGeometry mp = _geomFact.CreateMultiPoint(randPts);
            MinimumBoundingCircle mbc = new MinimumBoundingCircle(mp);
            Coordinate centre = mbc.GetCentre();
            double radius = mbc.GetRadius();
            Console.WriteLine("Testing " + nPts + " random points.  Radius = " + radius);

            checkWithinCircle(randPts, centre, radius, 0.0001);
        }

        private void checkWithinCircle(Coordinate[] pts, Coordinate centre, double radius, double tolerance)
        {
            for (int i = 0; i < pts.Length; i++)
            {
                Coordinate p = pts[i];
                double ptRadius = centre.Distance(p);
                double error = ptRadius - radius;
                Assert.LessOrEqual(error, tolerance);
            }
        }

        private Coordinate[] CreateRandomPoints(int n)
        {
            Coordinate[] pts = new Coordinate[n];
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
