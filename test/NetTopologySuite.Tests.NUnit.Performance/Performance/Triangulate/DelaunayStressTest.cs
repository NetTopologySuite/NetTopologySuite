using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.OverlayNG;
using NetTopologySuite.Triangulate;
using NetTopologySuite.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace NetTopologySuite.Tests.NUnit.Performance.Triangulate
{
    /**
     * Test correctness of Delaunay computation with 
     * synthetic random datasets.
     * 
     * @author Martin Davis
     *
     */
    public class DelaunayStressTest
    {
        private const int N_PTS = 50;
        private const int RUN_COUNT = 10000;
        private const double SIDE_LEN = 1000.0;
        private const double BASE_OFFSET = 0;

        private static readonly Random Random = new Random();

        private static readonly GeometryFactory geomFact = new GeometryFactory();
        private const double WIDTH = 100;
        private const double HEIGHT = 100;

        [Test]
        public void Run()
        {
            for (int i = 0; i < RUN_COUNT; i++)
            {
                TestContext.WriteLine($"Run # {i}");
                Run(N_PTS);
            }
        }

        public void Run(int nPts)
        {
            var pts = RandomPointsInGrid(nPts, BASE_OFFSET, BASE_OFFSET, WIDTH, HEIGHT, 1);
            Run(pts);
        }

        public void Run(IList<Coordinate> pts)
        {
            TestContext.WriteLine($"Base offset: {BASE_OFFSET}");
            TestContext.WriteLine($"# pts: {pts.Count}");
            var sw = new Stopwatch();
            var builder = new DelaunayTriangulationBuilder();
            builder.SetSites(pts);

            Geometry tris = builder.GetTriangles(geomFact);
            CheckDelaunay(tris);

            CheckVoronoi(pts);

            TestContext.WriteLine($"  --  Time: {sw.ElapsedMilliseconds}ms  Mem: {Memory.TotalString}");
        }

        private static void CheckVoronoi(IList<Coordinate> pts)
        {
            var vdb = new VoronoiDiagramBuilder();
            vdb.SetSites(pts);
            vdb.GetDiagram(geomFact);

            //-- for now simply confirm the Voronoi is computed with no failure
        }

        private static void CheckDelaunay(Geometry tris)
        {
            //TODO: check all elements are triangles

            //-- check triangulation is a coverage
            //-- this will error if triangulation is not a valid coverage
            var union = CoverageUnion.Union(tris);

            CheckConvex(tris, union);
        }

        private static void CheckConvex(Geometry tris, Geometry triHull)
        {
            var convexHull = ConvexHull(tris);
            bool isEqual = triHull.EqualsTopologically(convexHull);

            bool isConvex = IsConvex((Polygon)triHull);

            if (!isConvex)
            {
                TestContext.WriteLine("Tris:");
                TestContext.WriteLine(tris);
                TestContext.WriteLine("Convex Hull:");
                TestContext.WriteLine(convexHull);
                throw new InvalidOperationException("Delaunay triangulation is not convex");
            }
        }

        private static Geometry ConvexHull(Geometry tris)
        {
            var hull = new ConvexHull(tris);
            return hull.GetConvexHull();
        }

        private static bool IsConvex(Polygon poly)
        {
            var pts = poly.Coordinates;
            for (int i = 0; i < pts.Length - 1; i++)
            {
                int iprev = i - 1;
                if (iprev < 0) iprev = pts.Length - 2;
                int inext = i + 1;
                //-- orientation must be CLOCKWISE or COLLINEAR
                bool isConvex = OrientationIndex.CounterClockwise != Orientation.Index(pts[iprev], pts[i], pts[inext]);
                if (!isConvex)
                    return false;
            }
            return true;
        }

        private static List<Coordinate> RandomPointsInGrid(int nPts, double basex, double basey, double width, double height, double scale)
        {
            PrecisionModel pm = null;
            if (scale > 0)
            {
                pm = new PrecisionModel(scale);
            }
            var pts = new List<Coordinate>(nPts);

            int nSide = (int)Math.Sqrt(nPts) + 1;

            for (int i = 0; i < nSide; i++)
            {
                for (int j = 0; j < nSide; j++)
                {
                    double x = basex + i * width + width * Random.NextDouble();
                    double y = basey + j * height + height * Random.NextDouble();
                    var p = new Coordinate(x, y);
                    Round(p, pm);
                    pts.Add(p);
                }
            }
            return pts;
        }

        private static void Round(Coordinate p, PrecisionModel pm)
        {
            if (pm == null)
                return;
            pm.MakePrecise(p);
        }

        //private static IList<Coordinate> RandomPoints(int nPts, double sideLen)
        //{
        //    var pts = new List<Coordinate>();

        //    for (int i = 0; i < nPts; i++)
        //    {
        //        double x = sideLen * Random.NextDouble();
        //        double y = sideLen * Random.NextDouble();
        //        pts.Add(new Coordinate(x, y));
        //    }
        //    return pts;
        //}
    }
}
