using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using System.Runtime;
using System.Diagnostics;
using NetTopologySuite.Coordinates;
using Xunit;
#if unbuffered
using coord = NetTopologySuite.Coordinates.Simple.Coordinate;
using coordFac = NetTopologySuite.Coordinates.Simple.CoordinateFactory;
using coordSeqFac = NetTopologySuite.Coordinates.Simple.CoordinateSequenceFactory;

#else
using coord = NetTopologySuite.Coordinates.BufferedCoordinate;
using coordFac = NetTopologySuite.Coordinates.BufferedCoordinateFactory;
using coordSeqFac = NetTopologySuite.Coordinates.BufferedCoordinateSequenceFactory;
#endif

namespace NetTopologySuite.Tests.Geometries
{
    ///<summary>
    /// derived from a performance test by ...@soloplan.de
    /// FObermaier
    /// </summary>
    public class PolygonPerformanceTest
    {
        static readonly string CLRVersion =
          FileVersionInfo.GetVersionInfo(typeof(string).Assembly.Location).ProductName;

        static StreamWriter outfile = new StreamWriter(CLRVersion + "_" + GCSettings.IsServerGC + "_" + GCSettings.LatencyMode + ".txt");

        private static readonly coordFac _coordFact = new coordFac(100000);
        private static readonly GeometryFactory<coord> _geomFact =
            new GeometryFactory<coord>(new coordSeqFac(_coordFact));


        public const int OuterRingPoints = 500; // Increase here
        public const int InnerRingPoints = 100; // Increase here

        private static void LogLine(string format, params object[] data)
        {
            string line = string.Format(format, data);
            Console.WriteLine(line);
            outfile.WriteLine(line);
            outfile.Flush();
        }

        [Fact]
        public void Test()
        {
            Console.WriteLine(GCSettings.IsServerGC + ", " + GCSettings.LatencyMode);

            LogLine("              \tRings \tpoints \tvalid \tseconds");

            BenchmarkPolygons();

            //Console.Write("Press any key to continue . . . ");
            //Console.ReadKey(true);
        }

        private static void Benchmark(IPolygon<coord> poly)
        {
            BenchmarkIsValid(poly);
            BenchmarkIntersection(poly);
        }


        private static void BenchmarkIsValid(IPolygon<coord> poly)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
            bool valid = poly.IsValid;
            sw.Stop();
            LogLine("IsValid     : \t{0} \t{1} \t{2} \t{3}", poly.InteriorRingsCount, poly.Coordinates.Count, valid, sw.Elapsed);
        }

        private static void BenchmarkIntersection(IPolygon<coord> poly)
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
            List<ILinearRing<coord>> holes = new List<ILinearRing<coord>>();
            ILinearRing<coord> shell = CreateRing(0, 0, 20, OuterRingPoints);
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

        static ILinearRing<coord> CreateRing(Double x, Double y, Double radius, Int32 points)
        {
            IPoint<coord> point = _geomFact.CreatePoint(_coordFact.Create(x, y));
            IPolygon<coord> poly = (IPolygon<coord>)point.Buffer(radius, points, GeoAPI.Operations.Buffer.BufferStyle.Round);
            return (ILinearRing<coord>)poly.ExteriorRing;
        }

        public static IPolygon<coord> CreateBox(double x, double y, double w, double h)
        {
            ICoordinateSequence<coord> seq = _geomFact.CoordinateSequenceFactory.Create(
                _coordFact.Create(x,y),
                _coordFact.Create(x+w,y),
                _coordFact.Create(x+w,y+h),
                _coordFact.Create(x,y+h),
                _coordFact.Create(x,y));
            return _geomFact.CreatePolygon(_geomFact.CreateLinearRing(seq), null);
        }
    }
}
