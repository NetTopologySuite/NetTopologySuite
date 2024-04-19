using NetTopologySuite.Geometries;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.Distance
{
    [TestFixture]
    internal class PointPointDistancePerfTest : PerformanceTestCase
    {
        Point[] _grid;
        public PointPointDistancePerfTest()
            : base(nameof(PointPointDistancePerfTest))
        {
            RunSize = new[] { 10000 };
            RunIterations = 1;
        }

        public override void StartRun(int size)
        {
            TestContext.WriteLine($"\n-------  Running with # pts = {size}");
            _grid = CreatePointGrid(new Envelope(0, 10, 0, 10), size);
        }

        private static Point[] CreatePointGrid(Envelope envelope, int npts)
        {
            var pts = new Point[npts];
            var fact = NtsGeometryServices.Instance.CreateGeometryFactory();
            int nSide = (int)Math.Sqrt(npts);
            double xInc = envelope.Width / nSide;
            double yInc = envelope.Height / nSide;
            for (int i = 0, k = 0; i < nSide; i++)
            {
                for (int j = 0; j < nSide; j++)
                {
                    double x = envelope.MinX + i * xInc;
                    double y = envelope.MinY + i * yInc;
                    pts[k++] = fact.CreatePoint(new Coordinate(x, y));
                }
            }
            return pts;
        }

        public void RunPoints()
        {
            foreach(Geometry p1 in _grid)
            {
                foreach (Geometry p2 in _grid)
                {
                    double dist = p1.Distance(p2);
                }
            }
        }
    }
}
