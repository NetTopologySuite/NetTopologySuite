using System.Globalization;
using NetTopologySuite.EdgeRay;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.GeometriesGraph;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.EdgeRay
{
    public class EdgeRayPerformanceTest : PerformanceTestCase
    {
        bool verbose = true;
        private Geometry geom1;
        private Geometry geom2;

        public EdgeRayPerformanceTest() : base(nameof(EdgeRayPerformanceTest))
        {
            RunSize = new[] {100, 1000, 2000};
            RunIterations = 1;
        }

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(EdgeRayPerformanceTest));
        }

        public override void StartRun(int size)
        {
            TestContext.WriteLine("\n---  Running with size " + size + "  -----------");
            iter = 0;
            geom1 = CreateSineStar(size, 0);
            geom2 = CreateSineStar(size, 10);

        }

        private int iter = 0;

        public void RunEdgeRayArea()
        {
            //System.out.println("Test 1 : Iter # " + iter++);
            double area = EdgeRayIntersectionArea.GetArea(geom1, geom2);
            TestContext.WriteLine("EdgeRay area = {0}", area.ToString(NumberFormatInfo.InvariantInfo));
        }

        public void RunIntersectionArea()
        {
            double area = geom1.Intersection(geom2).Area;
            TestContext.WriteLine("Overlay area = {0}", area.ToString(NumberFormatInfo.InvariantInfo));
        }

        private static Geometry CreateSineStar(int nPts, double offset)
        {
            var gsf = new SineStarFactory();
            gsf.Centre = new Coordinate(0, offset);
            gsf.Size = 100;
            gsf.NumPoints = nPts;

            var g = gsf.CreateSineStar();

            return g;

        }
    }

}
