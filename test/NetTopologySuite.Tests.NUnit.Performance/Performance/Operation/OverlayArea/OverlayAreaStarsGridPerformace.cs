using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using System;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.OverlayArea
{
    public class OverlayAreaStarsGridPerfTest : PerformanceTestCase
    {

        bool verbose = true;
        private Geometry geom;
        private Geometry grid;

        public OverlayAreaStarsGridPerfTest() : base("OverlayAreaStarsGridPerfTest")
        {
            RunSize = new int[] { 100, 1000, 2000, 10000, 20000 };
            RunIterations = 1;
        }

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(OverlayAreaStarsGridPerfTest));
        }

        public override void StartRun(int size)
        {
            geom = CreateSineStar(size, 0);
            grid = Grid(geom, 100_00);

            TestContext.WriteLine("\n---  Running with Polygon size {0}, grid # = {1} -------------\n",
                geom.NumPoints, grid.NumGeometries);
        }

        public void RunIntersectionArea()
        {
            double area = 0.0;
            var intArea = new NetTopologySuite.Operation.OverlayArea.OverlayArea(geom);
            //System.out.println("Test 1 : Iter # " + iter++);
            for (int i = 0; i < grid.NumGeometries; i++)
            {
                var cell = grid.GetGeometryN(i);
                area += intArea.IntersectionArea(cell);
            }
            TestContext.WriteLine(">>> IntersectionArea = {0:R}", area);
        }

        public void RunFullIntersection()
        {
            double area = 0.0;
            //System.out.println("Test 1 : Iter # " + iter++);
            for (int i = 0; i < grid.NumGeometries; i++)
            {
                var cell = grid.GetGeometryN(i);
                area += geom.Intersection(cell).Area;
            }
            TestContext.WriteLine(">>> Overlay area = {0:R}", area);
        }

        public static Geometry CreateSineStar(int nPts, double offset)
        {
            var gsf = new SineStarFactory();
            gsf.Centre = new Coordinate(0, offset);
            gsf.Size = 100;
            gsf.NumPoints = nPts;

            var g = gsf.CreateSineStar();

            return g;
        }

        public static Geometry Grid(Geometry g, int nCells)
        {
            var env = g.EnvelopeInternal;
            var geomFact = g.Factory;

            int nCellsOnSideY = (int)Math.Sqrt(nCells);
            int nCellsOnSideX = nCells / nCellsOnSideY;

            // alternate: make square cells, with varying grid width/height
            //double extent = env.minExtent();
            //double nCellsOnSide = Math.max(nCellsOnSideY, nCellsOnSideX);

            double cellSizeX = env.Width / nCellsOnSideX;
            double cellSizeY = env.Height / nCellsOnSideY;

            var geoms = new System.Collections.Generic.List<Geometry>();

            for (int i = 0; i < nCellsOnSideX; i++)
            {
                for (int j = 0; j < nCellsOnSideY; j++)
                {
                    double x = env.MinX + i * cellSizeX;
                    double y = env.MinY + j * cellSizeY;
                    double x2 = env.MinX + (i + 1) * cellSizeX;
                    double y2 = env.MinY + (j + 1) * cellSizeY;

                    var cellEnv = new Envelope(x, x2, y, y2);
                    geoms.Add(geomFact.ToGeometry(cellEnv));
                }
            }
            return geomFact.BuildGeometry(geoms);
        }
    }

}
