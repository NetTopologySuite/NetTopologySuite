using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Polygonize;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.Polygonize
{

    /// <summary>
    /// Test performance of {@link Polygonizer}.
    /// Force large number of hole-in-shell tests,
    /// as well as testing against large ring
    /// for point-in-polygon computation.
    /// </summary>
    /// <author>mdavis</author>
    public class PolygonizerPerfTest : PerformanceTestCase
    {
        private const int BufferSegments = 10;

        private Geometry _testCircles;


        public PolygonizerPerfTest()
            : base(typeof(PolygonizerPerfTest).Name)
        {
            RunSize = new [] { 10, /* 100, 200, 300, 400, 500, */ 1000, 2000 };
            RunIterations = 1;
        }

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(PolygonizerPerfTest));
        }

        public override void StartRun(int num)
        {
            Console.WriteLine($"Running with size {num}");

            double size = 100;
            var polys = CreateCircleGrid(num, size, BufferSegments);

            var surround = CreateAnnulus(size / 2, size / 2, 2 * size, size, 1000 * BufferSegments);
            polys.Add(surround);
            _testCircles = GeometryFactory.Default.CreateMultiPolygon(GeometryFactory.ToPolygonArray(polys));
            //_testCircles = CreateCircles(num);
        }

        private static IList<Polygon> CreateCircleGrid(int num, double size, int bufferSegs)
        {
            var polys = new List<Polygon>(num);

            int nOnSide = (int)Math.Sqrt(num) + 1;
            double radius = size / nOnSide / 4;
            double gap = 4 * radius;

            for (int index = 0; index < num; index++)
            {
                int iy = index / nOnSide;
                int ix = index % nOnSide;
                double x = ix * gap;
                double y = iy * gap;

                var poly = CreateAnnulus(x, y, radius, radius / 2, bufferSegs);
                polys.Add(poly);
            }
            return polys;
        }

        private static Polygon CreateAnnulus(double x, double y, double radius, double innerRadius, int bufferSegs)
        {
            var pt = Geometry.DefaultFactory.CreatePoint(new Coordinate(x, y));
            var shell = BufferRing(pt, radius, bufferSegs);
            var hole = BufferRing(pt, innerRadius, bufferSegs);
            return pt.Factory.CreatePolygon(shell, new LinearRing[] { hole });
        }

        private static LinearRing BufferRing(Point pt, double radius, int bufferSegs)
        {
            var buf = (Polygon)pt.Buffer(radius, bufferSegs);
            return (LinearRing)buf.ExteriorRing;
        }

        public void RunDisjointCircles()
        {
            const bool extractOnlyPolygonal = false;
            var polygonizer = new Polygonizer(extractOnlyPolygonal);
            polygonizer.Add(_testCircles);
            var output = polygonizer.GetPolygons();
        }
    
    }
}
