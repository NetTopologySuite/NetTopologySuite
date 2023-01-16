using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Geometries;
using NetTopologySuite.Tests.NUnit.Utilities;
using NUnit.Framework;
using System;
using NetTopologySuite.Tests.NUnit.Performance.Geometries.Prepared;
using NetTopologySuite.Tests.NUnit.TestData;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.OverlayArea
{
    public class OverlayAreaGridsPerfTest : PerformanceTestCase
    {

        bool verbose = true;
        private Geometry geom;
        private Geometry grid;

        public OverlayAreaGridsPerfTest()
            : base("OverlayAreaGridsPerfTest")
        {
            RunSize = new int[] { 100, 200, 1000, 2000, 10_000, 20_000, 40_000, 100_000, 200_000, 400_000, 1000_000 };
            //setRunSize(new int[] { 100, 200, 20_000, 40_000, 400_000, 1000_000 });
            RunIterations = 1;
        }

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(OverlayAreaGridsPerfTest));
        }

        public override void StartRun(int size)
        {
            //geom = createSineStar(10_000, 0);
            //geom = (Geometry) IOUtil.readWKTFile("D:/proj/jts/testing/intersectionarea/dvg_nw.wkt").toArray()[0];
            using var file = EmbeddedResourceManager.GetResourceStream("NetTopologySuite.Tests.NUnit.TestData.africa.wkt");
            geom = IOUtil.ReadWKTFile(new System.IO.StreamReader(file))[0];
            grid = Grid(geom, size);

            TestContext.WriteLine("\n---  Running with Polygon size {0}, grid # = {1} -------------\n",
                geom.NumPoints, grid.NumGeometries);
    }

    public void RunOverlayArea()
    {
        double area = 0.0;
        var intArea = new NetTopologySuite.Operation.OverlayArea.OverlayArea(geom);
        //System.out.println("Test 1 : Iter # " + iter++);
        for (int i = 0; i < grid.NumGeometries; i++)
        {
            var cell = grid.GetGeometryN(i);
            area += intArea.IntersectionArea(cell);
            //checkOrigArea(geom, cell);
        }
        TestContext.WriteLine(">>> OverlayArea = {0:R}", area);
    }

    private void CheckOrigArea(Geometry geom0, Geometry geom1)
    {
        double intArea = NetTopologySuite.Operation.OverlayArea.OverlayArea.IntersectionArea(geom0, geom1);
        double origArea = geom0.Intersection(geom1).Area;
        if (!IsEqual(intArea, origArea, 0.1))
                TestContext.WriteLine("********************   Areas are different! OA = {0:R}  Orig = {1:R}", intArea, origArea);
    }

    private static bool IsEqual(double v1, double v2, double tol)
    {
        if (v1 == v2) return true;
        double diff = Math.Abs((v1 - v2) / (v1 + v2));
        return diff < tol;
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
        TestContext.WriteLine(">>> Full Intersection area = {0:R}", area);
    }

    public void RunFullIntersectionPrep()
    {
        double area = 0.0;
        var geomPrep = PreparedGeometryFactory.Prepare(geom);
        //System.out.println("Test 1 : Iter # " + iter++);
        for (int i = 0; i < grid.NumGeometries; i++)
        {
            var cell = grid.GetGeometryN(i);
            area += IntAreaFullPrep(geom, geomPrep, cell);
        }
            TestContext.WriteLine(">>> Full Intersection area = {0:R}", area);
    }

    private static double IntAreaFullPrep(Geometry geom, IPreparedGeometry geomPrep, Geometry geom1)
    {
        if (!geomPrep.Intersects(geom1)) return 0.0;
        if (geomPrep.Contains(geom1)) return geom1.Area;

        double intArea = geom.Intersection(geom1).Area;
        return intArea;
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

    private static Geometry Grid(Geometry g, int nCells)
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
