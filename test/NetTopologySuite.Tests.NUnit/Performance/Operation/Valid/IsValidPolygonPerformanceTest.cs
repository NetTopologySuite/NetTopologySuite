using NetTopologySuite.Geometries;
using NetTopologySuite.Tests.NUnit.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.Valid
{
    /**
     * Used to test performance enhancement in IsValidOp.checkHolesInShell.
     * 
     * @author mdavis
     *
     */
    public class IsValidPolygonPerformanceTest : PerformanceTestCase
    {

        const int N_ITER = 10;

        public IsValidPolygonPerformanceTest() : base(nameof(IsValidNestedHolesPerformanceTest))
        {
            RunSize = new[] {1000, 10_000, 100_000, 1000_000, 2000_000};
            RunIterations = N_ITER;
        }

        Geometry geom;

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(IsValidPolygonPerformanceTest));
        }

        public override void StartRun(int npts)
        {
            geom = CreateSineStar(npts);
        }

        private Geometry CreateSineStar(int npts)
        {
            var sineStar = TestShapeFactory.CreateSineStar(new Coordinate(0, 0), 100, npts);
            TestContext.WriteLine($"\nRunning with # pts {sineStar.NumPoints}");
            return sineStar;
        }


        public void runValidate()
        {
            bool isValid = geom.IsValid;
        }
    }
}
