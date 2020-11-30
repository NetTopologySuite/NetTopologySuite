using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Precision;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Precision
{
    /**
     * This test revealed a scaling issue with the {@link SnapRoundingNoder}:
     * the {@link HotPixelIndex} could not handle very large numbers
     * of points due to kdTree becoming unbalanced.
     * 
     * @author Martin Davis
     *
     */
    [Category("LongRunning")]
    public class GeometryPrecisionReducerPerfTest : PerformanceTestCase
    {

    private const int N_ITER = 1;

    private const double ORG_X = 100;
    private const double ORG_Y = 100;
    private const double SIZE = 100;
    private const int N_ARMS = 20;
    private const double ARM_RATIO = 0.3;


    private Geometry _sineStar;

    private PrecisionModel _pm;


    public GeometryPrecisionReducerPerfTest()
        : base(nameof(GeometryPrecisionReducerPerfTest))
    {
        RunSize = new [] { 100, 200, 400, 1000, 2000, 4000, 8000, 10000, 100_000, 200_000, 400_000, 1000_000, 2000_000 };
        RunIterations = N_ITER;
    }

    public override void TestInternal()
    {
        PerformanceTestRunner.Run(typeof(GeometryPrecisionReducerPerfTest));
    }
    public override void SetUp()
    {
        TestContext.WriteLine("Geometry Precision Reducer perf test");
        TestContext.WriteLine("SineStar: origin: ("
                               + ORG_X + ", " + ORG_Y + ")  size: " + SIZE
                               + "  # arms: " + N_ARMS + "  arm ratio: " + ARM_RATIO);
        TestContext.WriteLine("# Iterations: " + N_ITER);
    }

    public override void StartRun(int npts)
    {
        iter = 0;
        _sineStar = SineStarFactory.Create(new Coordinate(ORG_X, ORG_Y), SIZE, npts, N_ARMS, ARM_RATIO);

        double scale = npts / SIZE;
        _pm = new PrecisionModel(scale);
        TestContext.WriteLine("\n# pts = %d, Scale = %f\n", npts, scale);

        if (npts <= 1000) TestContext.WriteLine(_sineStar);
        TestContext.Out.Flush();
    }

        private int iter = 0;

    public void RunReduce()
    {
        var sinePolyCrinkly = GeometryPrecisionReducer.Reduce(_sineStar, _pm);
    }

    }

}
