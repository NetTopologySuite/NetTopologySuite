using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Algorithm
{
    public class IndexedPointInAreaPerfTest : PerformanceTestCase
    {

        public IndexedPointInAreaPerfTest() : base(nameof(IndexedPointInAreaPerfTest))
        {
            RunSize = new[] {100_000};
            RunIterations = 1;
        }

        private IList<Coordinate> _coords;
        private Polygon _polygon;

        public override void StartRun(int num)
        {
            TestContext.WriteLine("Running with size " + num);
            var factory = new GeometricShapeFactory();
            factory.Size = 100;
            _polygon = factory.CreateCircle();

            _coords = new List<Coordinate>();
            var rand = new Random(1324);
            for (int i = 0; i < num; i++)
            {
                _coords.Add(new Coordinate(rand.NextDouble() * 100, rand.NextDouble() * 100));
            }
        }

        public void RunParallel()
        {
            for (int i = 0; i < 1000; i++)
            {
                var locator = new IndexedPointInAreaLocator(_polygon);
                Parallel.ForEach(_coords, c => IsInside(locator, c));
            }
        }

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(IndexedPointInAreaPerfTest));
        }


        private static bool IsInside(IndexedPointInAreaLocator locator, Coordinate coord)
        {
            return locator.Locate(coord) == Location.Interior;
        }
    }
}
