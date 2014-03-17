using System;
using System.Diagnostics;
using GeoAPI.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Mathematics
{
    /**
     * Test performance of evaluating Triangle predicate computations
     * using 
     * various extended precision APIs.
     * 
     * @author Martin Davis
     *
     */

    [CategoryAttribute("Stress")]
    public class InCirclePerf
    {


        private readonly Coordinate _pa = new Coordinate(687958.05, 7460725.97);
        private readonly Coordinate _pb = new Coordinate(687957.43, 7460725.93);
        private readonly Coordinate _pc = new Coordinate(687957.58, 7460721);
        private readonly Coordinate _pp = new Coordinate(687958.13, 7460720.99);

        [TestAttribute]
        public void Test()
        {
            Console.WriteLine("InCircle perf");
            int n = 1000000;
            double doubleTime = RunDouble(n);
            double ddSelfTime = RunDDSelf(n);
            double ddSelf2Time = runDDSelf2(n);
            double ddTime = RunDD(n);
            //		double ddSelfTime = runDoubleDoubleSelf(10000000);

            Console.WriteLine("DD VS double performance factor      = " + ddTime/doubleTime);
            Console.WriteLine("DDSelf VS double performance factor  = " + ddSelfTime/doubleTime);
            Console.WriteLine("DDSelf2 VS double performance factor = " + ddSelf2Time/doubleTime);
        }

        public double RunDouble(int nIter)
        {
            var sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < nIter; i++)
            {
                TriPredicate.IsInCircle(_pa, _pb, _pc, _pp);
            }
            sw.Stop();
            Console.WriteLine("double:   nIter = " + nIter
                              + "   time = " + sw.ElapsedMilliseconds);
            return sw.ElapsedMilliseconds/(double) nIter;
        }

        public double RunDD(int nIter)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < nIter; i++)
            {
                TriPredicate.IsInCircleDD(_pa, _pb, _pc, _pp);
            }
            sw.Stop();
            Console.WriteLine("DD:       nIter = " + nIter
                              + "   time = " + sw.ElapsedMilliseconds);
            return sw.ElapsedMilliseconds/(double) nIter;
        }

        public double RunDDSelf(int nIter)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < nIter; i++)
            {
                TriPredicate.IsInCircleDD2(_pa, _pb, _pc, _pp);
            }
            sw.Stop();
            Console.WriteLine("DD-Self:  nIter = " + nIter
                              + "   time = " + sw.ElapsedMilliseconds);
            return sw.ElapsedMilliseconds/(double) nIter;
        }

        public double runDDSelf2(int nIter)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            for (int i = 0; i < nIter; i++)
            {
                TriPredicate.IsInCircleDD3(_pa, _pb, _pc, _pp);
            }
            sw.Stop();
            Console.WriteLine("DD-Self2: nIter = " + nIter
                              + "   time = " + sw.ElapsedMilliseconds);
            return sw.ElapsedMilliseconds/(double) nIter;
        }
    }
    /**
     * Algorithms for computing values and predicates
     * associated with triangles.
     * For some algorithms extended-precision
     * versions are provided, which are more robust
     * (i.e. they produce correct answers in more cases).
     * These are used in triangulation algorithms.
     * 
     * @author Martin Davis
     *
     */
}