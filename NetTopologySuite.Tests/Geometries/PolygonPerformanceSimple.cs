#define simple
using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using System.Runtime;
using System.Diagnostics;
using NetTopologySuite.Coordinates.Simple;
using NUnit.Framework;

namespace NetTopologySuite.Tests.Geometries
{
    ///<summary>
    /// derived from a performance test by ...@soloplan.de
    /// FObermaier
    /// </summary>
    [TestFixture]
    public class PolygonPerformanceSimple
    {
        static readonly string CLRVersion =
          FileVersionInfo.GetVersionInfo(typeof(string).Assembly.Location).ProductName;

        static StreamWriter outfile = new StreamWriter("Simple_" + CLRVersion + "_" + GCSettings.IsServerGC + "_" + GCSettings.LatencyMode + ".txt");

        private static readonly CoordinateFactory _coordFact = new CoordinateFactory(100000);
        private static readonly GeometryFactory<Coordinate> _geomFact =
            new GeometryFactory<Coordinate>(new CoordinateSequenceFactory(_coordFact));


        public const int OuterRingPoints = 500; // Increase here
        public const int InnerRingPoints = 100; // Increase here

        private static void LogLine(string format, params object[] data)
        {
            string line = string.Format(format, data);
            Console.WriteLine(line);
            outfile.WriteLine(line);
            outfile.Flush();
        }

        [Test]
        public void Test()
        {
            Console.WriteLine(GCSettings.IsServerGC + ", " + GCSettings.LatencyMode);

            LogLine("              \tRings \tpoints \tvalid \tseconds");

            BenchmarkPolygons();

            //Console.Write("Press any key to continue . . . ");
            //Console.ReadKey(true);
        }

        private static void Benchmark(IPolygon<Coordinate> poly)
        {
            BenchmarkIsValid(poly);
            BenchmarkIntersection(poly);
        }


        private static void BenchmarkIsValid(IPolygon<Coordinate> poly)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool valid = poly.IsValid;
            sw.Stop();
            LogLine("IsValid     : \t{0} \t{1} \t{2} \t{3}", poly.InteriorRingsCount, poly.Coordinates.Count, valid, sw.Elapsed);
        }

        private static void BenchmarkIntersection(IPolygon<Coordinate> poly)
        {
            for (double w = 1; w <= 100; w *= 10)
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                int points = poly.Intersection(CreateBox(0, 0, w, w)).Coordinates.Count;
                sw.Stop();
                LogLine("Intersection: \t{0} \t{1} \t{2} \t{3} \t{4}", poly.InteriorRingsCount, poly.Coordinates.Count, points, sw.Elapsed, w);
            }
        }

        public static void BenchmarkPolygons()
        {
            List<ILinearRing<Coordinate>> holes = new List<ILinearRing<Coordinate>>();
            ILinearRing<Coordinate> shell = CreateRing(0, 0, 20, OuterRingPoints);
            Benchmark(_geomFact.CreatePolygon(shell, holes));
            for (int i = 0; i <= 100; i += 1)
            {
                holes.Add(CreateRing((i % 10) - 5, (i / 10) - 5, 0.4, InnerRingPoints));
                if (i % 5 == 0)
                {
                    Benchmark(_geomFact.CreatePolygon(shell, holes));
                }
            }
        }

        static ILinearRing<Coordinate> CreateRing(Double x, Double y, Double radius, Int32 points)
        {
            IPoint<Coordinate> point = _geomFact.CreatePoint(_coordFact.Create(x, y));
            IPolygon<Coordinate> poly = (IPolygon<Coordinate>)point.Buffer(radius, points, GeoAPI.Operations.Buffer.BufferStyle.Round);
            return (ILinearRing<Coordinate>)poly.ExteriorRing;
        }

        public static IPolygon<Coordinate> CreateBox(double x, double y, double w, double h)
        {
            ICoordinateSequence<Coordinate> seq = _geomFact.CoordinateSequenceFactory.Create(
                _coordFact.Create(x, y),
                _coordFact.Create(x + w, y),
                _coordFact.Create(x + w, y + h),
                _coordFact.Create(x, y + h),
                _coordFact.Create(x, y));
            return _geomFact.CreatePolygon(_geomFact.CreateLinearRing(seq), null);
        }
    }
}
