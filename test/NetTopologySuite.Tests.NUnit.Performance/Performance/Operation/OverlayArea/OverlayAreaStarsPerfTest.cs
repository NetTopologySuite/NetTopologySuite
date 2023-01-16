using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.OverlayArea;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.OverlayArea
{
    public class OverlayAreaStarsPerfTest : PerformanceTestCase
    {
        bool verbose = true;
        private Geometry star1;
        private Geometry star2;

        public OverlayAreaStarsPerfTest() : base("OverlayAreaStarsPerfTest")
        {
            RunSize = new int[] { 100, 1000, 2000, 10000, 20000 };
            RunIterations = 1;
        }

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(OverlayAreaStarsPerfTest));
        }

        public override void StartRun(int size)
        {
            TestContext.WriteLine($"\n---  Running with size {size}  -----------");
            star1 = CreateSineStar(size, 0);
            star2 = CreateSineStar(size, 10);
        }

        public void RunOverlayArea()
        {
            //System.out.println("Test 1 : Iter # " + iter++);
            double area = NetTopologySuite.Operation.OverlayArea.OverlayArea.IntersectionArea(star1, star2);
            TestContext.WriteLine(">>> OverlayArea = {0:R}", area);
        }

        public void RunFullIntersection()
        {
            double area = star1.Intersection(star2).Area;
            TestContext.WriteLine(">>> Full Intersection area = {0:R}", area);
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
