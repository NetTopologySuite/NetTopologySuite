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
                var n = random.Next(0, 10000);
                Run(n);
            }
        }
        private void Run(int nPts)
        {
            var randPts = CreateRandomPoints(nPts);
            IGeometry mp = _geomFact.CreateMultiPoint(randPts);
            var mbc = new MinimumBoundingCircle(mp);
            var centre = mbc.GetCentre();
            var radius = mbc.GetRadius();
            Console.WriteLine("Testing " + nPts + " random points.  Radius = " + radius);
            checkWithinCircle(randPts, centre, radius, 0.0001);
        }
        private void checkWithinCircle(Coordinate[] pts, Coordinate centre, double radius, double tolerance)
        {
            for (var i = 0; i < pts.Length; i++)
            {
                var p = pts[i];
                var ptRadius = centre.Distance(p);
                var error = ptRadius - radius;
                Assert.LessOrEqual(error, tolerance);
            }
        }
        private Coordinate[] CreateRandomPoints(int n)
        {
            var pts = new Coordinate[n];
            for (var i = 0; i < n; i++)
            {
                var x = 100 * random.NextDouble();
                var y = 100*random.NextDouble();
                pts[i] = new Coordinate(x, y);
            }
            return pts;
        }
    }
}
