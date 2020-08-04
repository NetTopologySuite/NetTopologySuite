using NetTopologySuite.Geometries;
using NetTopologySuite.Tests.NUnit.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.Valid
{
    /**
     * Intended to test out an optimization introduced in GEOS
     * (https://github.com/libgeos/geos/pull/255/commits/1bf16cdf5a4827b483a1f712e0597ccb243f58cb)
     * 
     * This test doesn't show a clear benefit, so not changing the code at the moment (2020/03/11)
     * 
     * @author mdavis
     *
     */
    public class IsValidNestedHolesPerformanceTest : PerformanceTestCase
    {


        const int N_ITER = 10;

        public IsValidNestedHolesPerformanceTest() : base(nameof(IsValidNestedHolesPerformanceTest))
        {
            RunSize = new[] {1000, 10_000, 100_000, 1000_000, 2000_000};
            RunIterations = N_ITER;
        }

        Geometry geom;

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(IsValidNestedHolesPerformanceTest));
        }

        public override void StartRun(int npts)
        {
            geom = CreateSlantHoles(npts);
        }

        static int NUM_GEOMS = 100;

        private Geometry CreateSlantHoles(int npts)
        {
            var ellipses = TestShapeFactory.CreateSlantedEllipses(new Coordinate(0, 0), 100, 10, NUM_GEOMS, npts);
            var geom = TestShapeFactory.CreateExtentWithHoles(ellipses);
            TestContext.WriteLine($"\nRunning Slanted Ellipses: # geoms = {NUM_GEOMS}, # pts {npts}");
            return geom;
        }

        public void RunValidate()
        {
            bool isValid = geom.IsValid;
        }
    }
}
