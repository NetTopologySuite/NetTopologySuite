using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.OverlayNG;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.OverlayNG
{
    /**
     * 
     * @author Martin Davis
     *
     */
    [Category("LongRunning")]
    public class OverlayNGSnapRoundingPerfTest : PerformanceTestCase
    {

        private const int N_ITER = 1;

        static double ORG_X = 100;
        static double ORG_Y = 100;
        static double SIZE = 100;
        static int N_ARMS = 20;
        static double ARM_RATIO = 0.3;


        private Geometry sineStar;

        private PrecisionModel pm;

        private Geometry sineStar2;


        public OverlayNGSnapRoundingPerfTest()
            : base(nameof(OverlayNGSnapRoundingPerfTest))
        {
            RunSize = new[] {100, 200, 400, 1000, 2000, 4000, 8000, 10000, 100_000, 200_000, 400_000, 1000_000};
            RunIterations = N_ITER;
        }

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(OverlayNGSnapRoundingPerfTest));
        }

        public override void SetUp()
        {
            TestContext.WriteLine("OverlayNG Snap-Rounding perf test");
            TestContext.WriteLine("SineStar: origin: ("
                                  + ORG_X + ", " + ORG_Y + ")  size: " + SIZE
                                  + "  # arms: " + N_ARMS + "  arm ratio: " + ARM_RATIO);
            TestContext.WriteLine("# Iterations: " + N_ITER);
        }

        public override void StartRun(int npts)
        {
            sineStar = SineStarFactory.Create(new Coordinate(ORG_X, ORG_Y), SIZE, npts, N_ARMS, ARM_RATIO);
            sineStar2 = SineStarFactory.Create(new Coordinate(ORG_X + SIZE / 8, ORG_Y + SIZE / 8), SIZE, npts, N_ARMS,
                ARM_RATIO);

            double scale = npts / SIZE;
            pm = new PrecisionModel(scale);
            TestContext.WriteLine("\n# pts = {0}, Scale = {1}\n", npts, scale);

            if (npts <= 1000) TestContext.WriteLine(sineStar);
            TestContext.Out.Flush();
        }


        public void RunSR()
        {
            var result =
                NetTopologySuite.Operation.OverlayNG.OverlayNG.Overlay(sineStar, sineStar2,
                    SpatialFunction.Intersection, pm);
        }

        public void xRunRobust()
        {
            var result = OverlayNGRobust.Overlay(sineStar, sineStar2, SpatialFunction.Intersection);
        }

        public void xRunClassic()
        {
            var result = sineStar.Intersection(sineStar2);
        }

    }

}
