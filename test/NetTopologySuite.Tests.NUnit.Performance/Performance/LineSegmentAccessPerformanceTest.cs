using System;
using System.Collections.Generic;
using System.Diagnostics;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NUnit.Framework;


namespace NetTopologySuite.Tests.NUnit.Performance
{
    public class LineSegmentAccessPerformanceTest
    {
        private const int MaxIter = 1;

        private readonly GeometryFactory _fact;

        public LineSegmentAccessPerformanceTest()
        {
            _fact = new GeometryFactory(new PrecisionModel(), 0);
        }

        [Test]
        public void Test()
        {
            Test(50);
            Test(100);
            Test(200);
            Test(500);
            Test(1000);
        }

        public void Test(int gridLength)
        {
            var gsf = new NetTopologySuite.Utilities.GeometricShapeFactory();

            var geo = new List<LineSegment>();

            Console.WriteLine("Creating grid of " + gridLength);

            for (int gx = 0; gx < gridLength * 10; gx += 10)
            {
                for (int gy = 0; gy < gridLength * 10; gy += 10)
                {
                    geo.Add(new LineSegment(gx, gy, gx + 10, gy));
                    geo.Add(new LineSegment(gx, gy, gx, gy + 10));
                }
            }

            Test(geo, gridLength);
        }

        public void Test(IList<LineSegment> gridSquares, int gridLength)
        {
            Console.WriteLine("Length of grid: " + gridLength
                              + "      # squares: " + gridSquares.Count);

            var sw = new Stopwatch();
            sw.Start();
            double count = 0;
            for (int i = 0; i < MaxIter; i++)
            {
                count += testOriginal(gridSquares);
            }
            sw.Stop();

            Console.WriteLine("Sum of weights = " + count);
            Console.WriteLine("Finished in " + sw.ElapsedMilliseconds + " ms");
        }

        public static double testOriginal(IEnumerable<LineSegment> lines)
        {
            Console.WriteLine("Using given lines");

            var random = new Random(0);
            var weights = new Dictionary<LineSegment, double>();

            double count = 0;
            // build a collection that stores data against edges
            foreach (var line in lines)
            {
                weights.Add(line, random.NextDouble());
            }

            // pull data from the collection for a given edge
            foreach (var line in lines)
            {
                count += weights[line];
            }

            return count;
        }

    }


}
