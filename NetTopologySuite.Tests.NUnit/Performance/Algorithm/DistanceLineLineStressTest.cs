using System;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Tests.NUnit.Performance.Algorithm
{
    public class DistanceLineLineStressTest
    {
        [TestAttribute]
        [CategoryAttribute("Stress")]
        public void TestRandomDisjointCollinearSegments()
        {
            int n = 1000000;
            int failCount = 0;
            for (int i = 0; i < n; i++)
            {
                //System.out.println(i);
                Coordinate[] seg = RandomDisjointCollinearSegments();
                if (0 == CGAlgorithms.DistanceLineLine(seg[0], seg[1], seg[2], seg[3]))
                {
                    /*
            System.out.println("FAILED! - "
                + WKTWriter.toLineString(seg[0], seg[1]) + "  -  "
                + WKTWriter.toLineString(seg[2], seg[3]));
                */
                    failCount++;
                }
            }
            Console.WriteLine("# failed = " + failCount + " out of " + n);
        }

        // make results reproducible
        private static readonly Random Rnd = new Random(123456);

        private static Coordinate[] RandomDisjointCollinearSegments()
        {
            var slope = Rnd.NextDouble();
            var seg = new Coordinate[4];

            double gap = 1;
            double x1 = 10;
            double x2 = x1 + gap;
            double x3 = x1 + gap + 10;
            seg[0] = new Coordinate(0, 0);
            seg[1] = new Coordinate(x1, slope*x1);
            seg[2] = new Coordinate(x2, slope*x2);
            seg[3] = new Coordinate(x3, slope*x3);

            return seg;
        }

    }
}