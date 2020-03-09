#nullable disable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Geometries.Prepared
{
    public class PreparedLineIntersectsPerformanceTest
    {
        private const int MaxIter = 1;

        private const int NumLines = 50000;
        private const int NumLinePts = 10;

        private readonly GeometryFactory _fact;
        private readonly TestDataBuilder _builder;

        public PreparedLineIntersectsPerformanceTest()
        {
            _fact = new GeometryFactory(new PrecisionModel(), 0);
            _builder = new TestDataBuilder(_fact);
        }

        [Test]
        public void Test()
        {
            Test(5);
            Test(10);
            Test(500);
            Test(1000);
            Test(2000);
            /*
            test(100);
            test(1000);
            test(2000);
            test(4000);
            test(8000);
            */
        }

        public void Test(int nPts)
        {
            _builder.TestDimension = 1;
            var target = _builder.CreateSineStar(nPts).Boundary;

            var lines = _builder.CreateTestGeoms(target.EnvelopeInternal,
                                                 NumLines, 1.0, NumLinePts);

            // Console.WriteLine("Running with " + nPts + " points");
            Test(target, lines);
        }

        public void Test(Geometry g, IList<Geometry> lines)
        {
            Console.WriteLine("AOI # pts: " + g.NumPoints
                              + "      # lines: " + lines.Count
                              + "   # pts in line: " + NumLinePts);

            var sw = new Stopwatch();
            sw.Start();
            int count = 0;
            for (int i = 0; i < MaxIter; i++)
            {

                // count = testPrepGeomNotCached(g, lines);
                count = testPrepGeomCached(g, lines);
                // count = testOriginal(g, lines);

            }
            sw.Stop();

            Console.WriteLine("Count of intersections = " + count);
            Console.WriteLine("Finished in " + sw.ElapsedMilliseconds);
        }

        public static int testOriginal(Geometry g, IEnumerable<Geometry> lines)
        {
            Console.WriteLine("Using original JTS algorithm");
            int count = 0;
            foreach (var line in lines)
            {
                if (g.Intersects(line))
                    count++;
            }
            return count;
        }

        public static int testPrepGeomCached(Geometry g, IEnumerable<Geometry> lines)
        {
            Console.WriteLine("Using cached Prepared Geometry");
            var pgFact = new PreparedGeometryFactory();
            var prepGeom = pgFact.Create(g);

            int count = 0;
            foreach (var line in lines)
            {

                if (prepGeom.Intersects(line))
                    count++;
            }
            return count;
        }

        /**
         * Tests using PreparedGeometry, but creating a new
         * PreparedGeometry object each time.
         * This tests whether there is a penalty for using
         * the PG algorithm as a complete replacement for
         * the original algorithm.
         *
         * @param g
         * @param lines
         * @return the count
         */

        public static int testPrepGeomNotCached(Geometry g, IEnumerable<Geometry> lines)
        {
            Console.WriteLine("Using NON-CACHED Prepared Geometry");
            var pgFact = new PreparedGeometryFactory();
            // PreparedGeometry prepGeom = pgFact.create(g);

            int count = 0;
            foreach (var line in lines)
            {

                // test performance of creating the prepared geometry each time
                var prepGeom = pgFact.Create(g);

                if (prepGeom.Intersects(line))
                    count++;
            }
            return count;
        }

    }
}
