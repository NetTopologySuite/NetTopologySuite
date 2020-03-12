using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Polygonize;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.Polygonize
{
    public class PolygonizerPerfTest : PerformanceTestCase
    {

        private Geometry _circles;


        public PolygonizerPerfTest()
            : base(typeof(PolygonizerPerfTest).Name)
        {
            RunSize = new [] {1000, 10000, 20000};
            RunIterations = 1;
        }

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(PolygonizerPerfTest));
        }

        public override void StartRun(int size)
        {
            Console.WriteLine($"Running with size {size}");

            _circles = CreateCircles(size);
        }

        private static Geometry CreateCircles(int size)
        {
            var polys = new Polygon[size];

            double radius = 10;
            double gap = 4 * radius;
            int nOnSide = (int) Math.Sqrt(size) + 1;
            for (int index = 0; index < size; index++)
            {
                int i = index % nOnSide;
                int j = index - (nOnSide * i);
                double x = i * gap;
                double y = j * gap;

                var pt = GeometryFactory.Default.CreatePoint(new Coordinate(x, y));
                polys[index] = (Polygon) pt.Buffer(radius);
            }

            return GeometryFactory.Default.CreateMultiPolygon(polys);
        }

        
        public void RunDisjointCircles()
        {
            const bool extractOnlyPolygonal = false;
            var polygonizer = new Polygonizer(extractOnlyPolygonal);
            polygonizer.Add(_circles);
            var output = polygonizer.GetPolygons();
        }
    
    }
}
