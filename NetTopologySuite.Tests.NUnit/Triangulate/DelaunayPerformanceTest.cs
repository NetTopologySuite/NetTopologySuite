using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Tests.NUnit.Triangulate
{
    public class DelaunayPerformanceTest
    {
        [Test]
        public void RunTests()
        {
            Run(10);
            Run(10);
            Run(100);
            Run(1000);
            Run(10000);
            Run(20000);
            Run(30000);
            Run(100000);
            Run(200000);
            Run(300000);
        }

        [Test, Ignore("These take very long ... If you have time, go ahead!")]
        public void RunLongTests()
        {
            Run(1000000);
            Run(2000000);
            Run(3000000);
        }


        //static readonly IGeometryFactory GeomFact = new GeometryFactory();

        private const double SideLen = 10.0;

        public void Run(int nPts)
        {
            var pts = RandomPoints(nPts);
            Console.WriteLine("# pts: " + pts.Count);
            var sw = new Stopwatch();
            sw.Start();
            var builder = new DelaunayTriangulationBuilder();
            builder.SetSites(pts);

            // don't actually form output geometry, to save time and memory
            //var g = builder.GetEdges(GeomFact);
            builder.GetSubdivision();

            Console.WriteLine("  --  Time: " + sw.ElapsedMilliseconds
                              + "  Mem: " + Memory.TotalString);
            //Console.WriteLine(g);
        }

        private static readonly Random RND = new Random(998715632);

        private static ICollection<Coordinate> RandomPointsInGrid(int nPts)
        {
            var pts = new List<Coordinate>();

            int nSide = (int) Math.Sqrt(nPts) + 1;

            for (int i = 0; i < nSide; i++)
            {
                for (int j = 0; j < nSide; j++)
                {
                    double x = i*SideLen + SideLen*RND.NextDouble();
                    double y = j*SideLen + SideLen*RND.NextDouble();
                    pts.Add(new Coordinate(x, y));
                }
            }
            return pts;
        }

        private static ICollection<Coordinate> RandomPoints(int nPts)
        {
            var pts = new List<Coordinate>();

            for (int i = 0; i < nPts; i++)
            {
                double x = SideLen*RND.NextDouble();
                double y = SideLen*RND.NextDouble();
                pts.Add(new Coordinate(x, y));
            }
            return pts;
        }
    }
}