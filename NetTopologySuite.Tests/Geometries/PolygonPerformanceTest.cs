
using System;
using System.Collections.Generic;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using System.Runtime;
using System.Diagnostics;
using NetTopologySuite.Coordinates;
using Xunit;
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

        private static readonly BufferedCoordinateFactory _coordFact = new BufferedCoordinateFactory(100000);

        private static readonly GeometryFactory<BufferedCoordinate> _geomFact =
            new GeometryFactory<BufferedCoordinate>(new BufferedCoordinateSequenceFactory(_coordFact));

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

        private static void Benchmark(IPolygon<BufferedCoordinate> poly)
        {
            BenchmarkIsValid(poly);
            BenchmarkIntersection(poly);
        }

        private static void BenchmarkIsValid(IPolygon<BufferedCoordinate> poly)
        {
            DateTime start = DateTime.Now;
            bool valid = poly.IsValid;
            DateTime end = DateTime.Now;
            var diff = end - start;
            double td = diff.TotalSeconds;
            LogLine("IsValid     : \t{0} \t{1} \t{2} \t{3}", poly.InteriorRingsCount, poly.Coordinates.Count, valid, td);
        }

        private static void BenchmarkIntersection(IPolygon<BufferedCoordinate> poly)
        {
            for (double w = 1; w <= 100; w *= 10)
            {
                DateTime start = DateTime.Now;
                int points = poly.Intersection(CreateBox(0, 0, w, w)).Coordinates.Count;
                DateTime end = DateTime.Now;
                var diff = end - start;
                double td = diff.TotalSeconds;
                LogLine("Intersection: \t{0} \t{1} \t{2} \t{3} \t{4}", poly.InteriorRingsCount, poly.Coordinates.Count, points, td, w);
            }
        }

        public static void BenchmarkPolygons()
        {
            List<ILinearRing<BufferedCoordinate>> holes = new List<ILinearRing<BufferedCoordinate>>(100);
            ILinearRing<BufferedCoordinate> shell = CreateRing(0, 0, 20, OuterRingPoints);
            Benchmark(_geomFact.CreatePolygon(shell, holes.ToArray()));
            for (int i = 0; i <= 100; i += 1)
            {
                holes.Add(CreateRing((i % 10) - 5, (i / 10) - 5, 0.4, InnerRingPoints));
                if (i % 5 == 0)
                {
                    Benchmark(_geomFact.CreatePolygon(shell, holes.ToArray()));
                }
            }
        }

        static ILinearRing<BufferedCoordinate> CreateRing(Double x, Double y, Double radius, Int32 points)
        {
            IPoint<BufferedCoordinate> point = _geomFact.CreatePoint(_coordFact.Create(x, y));
            IPolygon<BufferedCoordinate> poly = (IPolygon<BufferedCoordinate>)point.Buffer(radius, points, GeoAPI.Operations.Buffer.BufferStyle.Round);
            return (ILinearRing<BufferedCoordinate>)poly.ExteriorRing;
        }

        public static IGeometry CreateBox(double x, double y, double w, double h)
        {
            ICoordinateSequence<BufferedCoordinate> seq = _geomFact.CoordinateSequenceFactory.Create(
                _coordFact.Create(x,y),
                _coordFact.Create(x+w,y),
                _coordFact.Create(x+w,y+h),
                _coordFact.Create(x,y+h),
                _coordFact.Create(x,y));
            return _geomFact.CreatePolygon(_geomFact.CreateLinearRing(seq), null);
        }
    }
}
