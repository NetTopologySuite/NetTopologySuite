using NetTopologySuite.Geometries;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetTopologySuite.Tests.NUnit.Performance.Algorithm
{
    internal class ConvexHullPerfTest : PerformanceTestCase
    {
        private MultiPoint _geom;

        public ConvexHullPerfTest() : base(nameof(ConvexHullPerfTest))
        {
            RunSize = new int[] { 1000, 10_000, 100_000, 1_000_000 };
            RunIterations = 100;
        }

        public override void StartRun(int num)
        {
            TestContext.WriteLine($"Running with size {num}");
            _geom = createRandomMultiPoint(num);
        }

        private static MultiPoint createRandomMultiPoint(int num)
        {
            var pts = new Coordinate[num];
            var rand = new Random(1324);
            for (int i = 0; i < num; i++)
            {
                pts[i] = new Coordinate(rand.NextDouble() * 100, rand.NextDouble() * 100);
            }
            var fact = NtsGeometryServices.Instance.CreateGeometryFactory();
            return fact.CreateMultiPointFromCoords(pts);
        }

        public void RunConvexHull()
        {
            var convextHull = _geom.ConvexHull();
        }

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(ConvexHullPerfTest));
        }
    }
}
