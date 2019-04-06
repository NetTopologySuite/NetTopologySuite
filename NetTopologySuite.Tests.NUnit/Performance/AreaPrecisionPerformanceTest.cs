using System;
using System.Diagnostics;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance
{
    public class AreaPrecisionPerfTest
    {
        [Test]
        [Category("Stress")]
        public void TestAreaPrecisionPerformance()
        {

            const double originX = 1000000;
            const double originY = 5000000;
            var sw = new Stopwatch();
            var sw1 = new Stopwatch();
            var sw2 = new Stopwatch();
            var sw3 = new Stopwatch();

            //-2,23057128323489E-11

            sw.Start();
            for (int nrVertices = 4; nrVertices <= 5000000; nrVertices *= 2)
            {
                var coordinates = new Coordinate[nrVertices + 1];

                for (int i = 0; i <= nrVertices; i++)
                {
                    var vertex = new Coordinate(originX + (1d + Math.Sin( i/(double) nrVertices*2*Math.PI)),
                                                originY + (1d + Math.Cos( i/(double) nrVertices*2*Math.PI)));
                    coordinates[i] = vertex;
                }
                // close ring
                coordinates[nrVertices] = coordinates[0];

                var g1 = new GeometryFactory().CreateLinearRing(coordinates);
                var holes = new ILinearRing[] {};
                var polygon = (Polygon) new GeometryFactory().CreatePolygon(g1, holes);
                //Console.WriteLine(polygon);

                sw1.Start();
                double area = polygon.Area;
                sw1.Stop();
                sw2.Start();
                double area2 = AccurateSignedArea(coordinates);
                sw2.Stop();
                sw3.Start();
                double areaOld = OriginalSignedArea(coordinates);
                sw3.Stop();

                double exactArea = 0.5 * nrVertices * Math.Sin(2 * Math.PI / nrVertices);
                double eps1 = exactArea - area;
                double eps2 = exactArea - area2;
                double eps3 = exactArea - areaOld;

                //Assert.IsTrue(Math.Abs(eps2) <= Math.Abs(eps3));

                Console.WriteLine(string.Format("{0,10},\tnow err: {1,23},\tacc err: {2,23},\told err: {3,23}", nrVertices ,eps1, eps2 ,eps3));
            }

            sw.Stop();

            Console.WriteLine("\n\nTime: " + sw.Elapsed);
            Console.WriteLine("Time Now: " + sw1.ElapsedTicks);
            Console.WriteLine("Time Acc: " + sw2.ElapsedTicks);
            Console.WriteLine("Time Old: " + sw3.ElapsedTicks);

            Assert.IsTrue(true);
        }

        private static double OriginalSignedArea(Coordinate[] ring)
        {
            if (ring.Length < 3)
                return 0.0;
            double sum = 0.0;
            for (int i = 0; i < ring.Length - 1; i++)
            {
                double bx = ring[i].X;
                double by = ring[i].Y;
                double cx = ring[i + 1].X;
                double cy = ring[i + 1].Y;
                sum += (bx + cx)*(cy - by);
            }
            return -sum/2.0;
        }

        private static double AccurateSignedArea(Coordinate[] ring)
        {
            if (ring.Length < 3)
                return 0.0;
            double sum = 0.0;
            // http://en.wikipedia.org/wiki/Shoelace_formula
            double x0 = ring[0].X;
            for (int i = 1; i < ring.Length - 1; i++)
            {
                double x = ring[i].X - x0;
                double y1 = ring[i + 1].Y;
                double y2 = ring[i == 0 ? ring.Length - 1 : i - 1].Y;
                sum += x*(y2 - y1);
            }
            return sum/2.0;
        }
    }
}