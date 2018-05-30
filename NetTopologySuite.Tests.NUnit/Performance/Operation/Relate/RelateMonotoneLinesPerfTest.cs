using System;
using System.IO;
using GeoAPI.Geometries;
using NetTopologySuite.Densify;
using NetTopologySuite.Geometries;
using NetTopologySuite.Tests.NUnit.Utilities;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.Relate
{
    /**
     * Tests the performance of {@link RelateOp} (via {@link Geometry#intersects(Geometry)}
     * on monotone linestrings, to confirm that the Monotone Chain comparison logic
     * is working as expected.
     * (In particular, Monotone Chains can be tested for intersections very efficiently,
     * since the monotone property allows subchain envelopes to be computed dynamically,
     * and thus binary search can be used to determine if two monotone chains intersect).
     * This should result in roughly linear performance for testing intersection of
     * chains (since the construction of the chain dominates the computation).
     * This test demonstrates that this occurs in practice.
     *
     * @author mdavis
     *
     */

    public class RelateMonotoneLinesPerfTest : PerformanceTestCase
    {
        private const int DENSIFY_FACTOR = 1000;

        public RelateMonotoneLinesPerfTest()
            : base("RelateMonotoneLinesPerfTest")
        {
            RunSize = new int[] {2, 4, 8, 16, 32, 64, 128, 256, 512};
            RunIterations = 1;
        }

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(RelateMonotoneLinesPerfTest));
        }

        private ILineString _line1;
        private ILineString _line2;

        public override void StartRun(int runSize)
        {
            int nVertices = runSize*DENSIFY_FACTOR;
            _line1 = CreateLine("LINESTRING (0 0, 100 100)", nVertices);
            _line2 = CreateLine("LINESTRING (0 1, 100 99)", nVertices);

            // force compilation of intersects code
            _line1.Intersects(_line2);
        }

        private static ILineString CreateLine(string wkt, int nVertices)
        {
            var distanceTolerance = 100.0/nVertices;
            var line = IOUtil.Read(wkt);
            var lineDense = (ILineString) Densifier.Densify(line, distanceTolerance);
            return lineDense;
        }

        public void RunIntersects()
        {
            Console.WriteLine("Line size: " + _line2.NumPoints);
            //@SuppressWarnings("unused")
            var isIntersects = _line1.Intersects(_line2);
        }

        public override void TearDown()
        {
            var timeFactor = ComputeTimeFactors();
            Console.Write("Time factors: ");
            PrintArray(timeFactor, Console.Out);
            Console.WriteLine();
        }

        private void PrintArray(double[] timeFactor, TextWriter @out)
        {
            foreach (var tf in timeFactor)
            {
                @out.Write(tf + " ");
            }
        }

        private double[] ComputeTimeFactors()
        {
            var runTime = RunTime;
            double[] timeFactor = new double[runTime.Length - 1];
            for (int i = 0; i < runTime.Length - 1; i++)
            {
                timeFactor[i] = (double) runTime[i + 1]/(double) runTime[i];
            }
            return timeFactor;
        }
    }
}