using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Diagnostics;
using NetTopologySuite.Triangulate;
using NUnit.Framework;
using GeoAPI.Algorithms;
#if unbuffered
using coord = NetTopologySuite.Coordinates.Simple.Coordinate;
#else
using coord = NetTopologySuite.Coordinates.BufferedCoordinate;
#endif
namespace NetTopologySuite.Tests.Triangulate
{
    [TestFixture]
    public class DelaunayPerformanceTest
    {
        [Test]
        public void Run()
        {
            run(10);
            run(10);
            run(100);
            run(1000);
            run(10000);
            run(20000);
            run(30000);
            run(100000);
            run(200000);
            run(300000);
            //the following do not work reasonably
            //run(1000000);
            //run(2000000);
            //run(3000000);
        }

        const double SideLen = 10.0;

        public void run(int nPts)
        {
            List<coord> pts = randomPoints(nPts);
            Console.WriteLine("# pts: " + pts.Count);
            Stopwatch sw = new Stopwatch();
            sw.Start();
            DelaunayTriangulationBuilder<coord> builder = new DelaunayTriangulationBuilder<coord>(TestFactories.GeometryFactory);
            builder.SetSites(pts);

            //		Geometry g = builder.getEdges(geomFact);
            // don't actually form output geometry, to save time and memory
            builder.GetSubdivision();

            Console.WriteLine("  --  Time: " + sw.ElapsedMilliseconds
                    + "  Mem: " + Memory.TotalString);
            //		System.out.println(g);
        }

        List<coord> randomPointsInGrid(int nPts)
        {
            List<coord> pts = new List<coord>(nPts);

            int nSide = (int)Math.Sqrt(nPts) + 1;
            Random r = new Random(0);
            for (int i = 0; i < nSide; i++)
            {
                for (int j = 0; j < nSide; j++)
                {
                    double x = i * SideLen + SideLen * r.NextDouble();
                    double y = j * SideLen + SideLen * r.NextDouble();
                    pts.Add(TestFactories.CoordFactory.Create(x, y));
                }
            }
            return pts;
        }

        List<coord> randomPoints(int nPts)
        {
            List<coord> pts = new List<coord>(nPts);
            Random r = new Random(1);
            for (int i = 0; i < nPts; i++)
            {
                double x = SideLen * r.NextDouble();
                double y = SideLen * r.NextDouble();
                pts.Add(TestFactories.CoordFactory.Create(x, y));
            }
            return pts;
        }
    }
}
