using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Tests.NUnit.Performance.Algorithm
{
    /// <summary>
    /// Performance test for various line intersection implementations.
    /// <para/>
    /// These include:
    /// <list type="Bullet">
    /// <item><term>DP-Basic</term><description>a basic double-precision (DP) implementation, with no attempt at reducing the effects of numerical round-off</description></item>
    /// <item><term>DP-Cond</term><description>a DP implementation in which the inputs are conditioned by translating them to around the origin</description></item>
    /// <item><term>DP-CB</term><description>a DP implementation using the <see cref="CommonBitsRemover"/> functionality</description></item>
    /// <item><term>DD</term><description>an implementation using extended-precision <see cref="DoubleDouble"/> arithmetic</description></item>
    /// <item><term>DDFilter</term><description>an experimental implementation using extended-precision <see cref="DoubleDouble"/> arithmetic
    /// along with a filter that uses DP if the accuracy is sufficient</description></item>
    /// </list>
    /// <h2>Results</h2>
    /// <list type="Bullet">
    /// <item><description>DP-Basic is the fastest but least accurate</description></item>
    /// <item><description>DP-Cond is fairly fast</description></item>
    /// <item><description>DP-CB is similar in performance to DP-Cond (but less accurate)</description></item>
    /// <item><description>DD is the slowest implementation</description></item>
    /// <item><description>the performance of DP-Filter is similar to DP or DD, depending on which method is chosen by the filter</description></item>
    /// </list>
    /// This test is evaluated together with the accuracy results from <see cref="IntersectionStressTest"/>.
    /// The conclusion is that the best combination of accuracy and performance
    /// is provided by DP-Cond.
    /// </summary>
    /// <seealso cref="IntersectionStressTest"/>
    /// <author>mdavis</author>
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
