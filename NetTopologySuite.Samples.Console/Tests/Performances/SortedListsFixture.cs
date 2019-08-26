using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Buffer;
using NUnit.Framework;

namespace NetTopologySuite.Tests.Performances
{
    [TestFixture]
    public class SortedListsFixture
    {
        public static StreamWriter OutFile = new StreamWriter("log.txt");

        public static void LogLine(string format, params object[] data)
        {
            string line = string.Format(format, data);
            Console.WriteLine(line);
            OutFile.WriteLine(line);
            OutFile.Flush();
        }

        [Test]
        public void Performances()
        {
            LogLine("ServerGC: {0}, LatencyMode: {1}", GCSettings.IsServerGC, GCSettings.LatencyMode);
            LogLine("CLR Implementation: {0}", FileVersionInfo.GetVersionInfo(typeof(string).Assembly.Location).FileDescription);

            LogLine("Rings \tpoints \tvalid \tseconds");

            BenchmarkPolygons();

            //Console.Write("Press any key to continue . . . ");
            //Console.ReadKey(true);
        }

        private void Benchmark(Polygon poly)
        {
            var start = DateTime.Now;
            bool valid = poly.IsValid;
            var end = DateTime.Now;
            var diff = end - start;
            double td = diff.TotalSeconds;
            LogLine("{0} \t{1} \t{2} \t{3}", poly.NumInteriorRings, poly.NumPoints, valid, td);
        }

        public void BenchmarkPolygons()
        {
            var factory = GeometryFactory.Default;
            var holes = new List<LinearRing>(100);
            var shell = CreateRing(0, 0, 20, 10000);
            Benchmark(factory.CreatePolygon(shell, holes.ToArray()));
            for (int i = 0; i < 100; i += 5)
            {
                holes.Add(CreateRing((i % 10) - 5, (i / 10) - 5, 0.4, 500));
                Benchmark(factory.CreatePolygon(shell, holes.ToArray()));
            }
        }

        public LinearRing CreateRing(double x, double y, double radius, int points)
        {
            var factory = GeometryFactory.Default;
            var point = factory.CreatePoint(new Coordinate(x, y));
            Polygon poly = (Polygon) point.Buffer(radius, points, EndCapStyle.Round);
            return poly.Shell;
        }
    }
}
