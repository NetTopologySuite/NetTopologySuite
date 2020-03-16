using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Tests.NUnit.Performance.Algorithm
{
    public class IntersectionPerfTest : PerformanceTestCase
    {
        private const int N_ITER = 1000000;


        public IntersectionPerfTest() : base(nameof(IntersectionPerfTest))
        {
            RunSize = new[] {1};
            RunIterations = N_ITER;
        }

        Coordinate a0 = new Coordinate(0, 0);
        Coordinate a1 = new Coordinate(10, 0);
        Coordinate b0 = new Coordinate(20, 10);
        Coordinate b1 = new Coordinate(20, 20);

        Coordinate p0;
        Coordinate p1;
        Coordinate q0;
        Coordinate q1;

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(IntersectionPerfTest));
        }

        public override void StartRun(int npts)
        {
            p0 = new Coordinate(35613471.6165017, 4257145.3061322933);
            p1 = new Coordinate(35613477.7705378, 4257160.5282227108);
            q0 = new Coordinate(35613477.775057241, 4257160.5396535359);
            q1 = new Coordinate(35613479.856073894, 4257165.9236917039);
        }

        public void RunDP()
        {
            var intPt = IntersectionAlgorithms.IntersectionBasic(p0, p1, q0, q1);
        }

        public void RunDD()
        {
            var intPt = CGAlgorithmsDD.Intersection(p0, p1, q0, q1);
        }

        public void RunDDWithFilter()
        {
            var intPt = IntersectionAlgorithms.IntersectionDDWithFilter(p0, p1, q0, q1);
        }

        public void RunCB()
        {
            var intPt = IntersectionAlgorithms.IntersectionCB(p0, p1, q0, q1);
        }

        public void RunCond()
        {
            var intPt = IntersectionComputer.Intersection(p0, p1, q0, q1);
        }

        public void RunDP_easy()
        {
            var intPt = IntersectionAlgorithms.IntersectionBasic(a0, a1, b0, b1);
        }

        public void RunCond_easy()
        {
            var intPt = IntersectionComputer.Intersection(a0, a1, b0, b1);
        }

        public void RunDD_easy()
        {
            var intPt = CGAlgorithmsDD.Intersection(a0, a1, b0, b1);
        }

        public void RunDDWithFilter_easy()
        {
            var intPt = IntersectionAlgorithms.IntersectionDDWithFilter(a0, a1, b0, b1);
        }


    }

}
