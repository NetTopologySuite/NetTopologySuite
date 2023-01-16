using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.OverlayArea;
using NetTopologySuite.Operation.OverlayNG;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.OverlayArea
{
    public class SimpleOverlayAreaPerfTest : PerformanceTestCase
    {
        private Polygon quadA;
        private Polygon quadB;

        public SimpleOverlayAreaPerfTest() : base("SimpleOverlayAreaPerfTest")
        {
            RunSize = new int[] { 100 };
            RunIterations = 10000;
        }

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(SimpleOverlayAreaPerfTest));
        }

        public override void StartRun(int size)
        {
            var rdr = new WKTReader();
            quadA = (Polygon)rdr.Read("POLYGON ((60 80, 9 45, 52.5 5, 80 45, 60 80))");
            quadB = (Polygon)rdr.Read("POLYGON ((13.5 60, 72 18, 79.5 65.5, 41.5 75.5, 13.5 60))");
        }

        public void RunSimpleOverlayArea()
        {
            double area = SimpleOverlayArea.IntersectionArea(quadA, quadB);
        }

        public void RunOverlayArea()
        {
            double area = NetTopologySuite.Operation.OverlayArea.OverlayArea.IntersectionArea(quadA, quadB);
        }

        public void RunFullOverlayArea()
        {
            double area = quadA.Intersection(quadB).Area;
        }

        public void RunOverlayNGArea()
        {
            double area = OverlayNGRobust.Overlay(quadB, quadA, NetTopologySuite.Operation.OverlayNG.OverlayNG.INTERSECTION).Area;
        }
    }

}
