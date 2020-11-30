using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Distance;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.Distance
{
    [Category("Stress")]
    public class DistanceGeomPairPerfTest : PerformanceTestCase
    {

        const int MAX_ITER = 100;


        bool _testFailed = false;
        bool _verbose = true;

        public DistanceGeomPairPerfTest()
            : base(nameof(DistanceGeomPairPerfTest))
        {
            RunSize = new int[] {10, 20, 50, 100, 200, 500, 1000, 2000, 5000, 10_000, 20_000, 50_000};
            RunIterations = 1000;
        }

        const double SIZE = 100;
        const double OFFSET = SIZE * 10;

        private Geometry geom1;
        private Geometry geom2;
        private Point pt2;

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(DistanceGeomPairPerfTest));
        }

        public override void StartRun(int nPts)
        {
            //int nPts2 = nPts;
            int nPts2 = 100;

            TestContext.WriteLine("\nRunning with " + nPts + " points (size-product = " + nPts * nPts2);

            geom1 = CreateSineStar(nPts, 0);
            geom2 = CreateSineStar(nPts2, OFFSET);

            pt2 = geom2.Centroid;
        }

        public void RunSimpleLines()
        {
            double dist = DistanceOp.Distance(geom1, geom2);
        }

        public void RunIndexedLines()
        {
            double dist = IndexedFacetDistance.Distance(geom1, geom2);
        }


        public void RunSimpleLinePoint()
        {
            double dist = DistanceOp.Distance(geom1, pt2);
        }

        public void RunIndexedLinePoint()
        {
            double dist = IndexedFacetDistance.Distance(geom1, pt2);
        }

        public void RunCachedLinePoint()
        {
            double dist = CachedFastDistance.Distance(geom1, pt2);
        }

        Geometry CreateSineStar(int nPts, double offset)
        {
            var gsf = new SineStarFactory();
            //gsf.Centre = new Coordinate(0, 0);
            gsf.Size = SIZE;
            gsf.NumPoints = nPts;
            gsf.Centre = new Coordinate(0, offset);

            var g2 = gsf.CreateSineStar().Boundary;

            return g2;
        }
    }
}
