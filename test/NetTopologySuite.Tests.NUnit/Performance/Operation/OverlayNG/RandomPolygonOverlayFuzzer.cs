using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Noding;
using NetTopologySuite.Noding.Snap;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Operation.OverlayNg;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.OverlayNG
{
    /// <summary>
    /// Runs overlay operations on pairs of random polygonal geometries
    /// to see if errors occur.
    /// <para/>
    /// For OverlayNG a spectrum of scale factors is used.
    /// Using a Floating precision model can be optionally specified.
    /// </summary>
    /// <author>Martin Davis</author>
    public class RandomPolygonOverlayFuzzer
    {

        private void Overlay(Geometry poly1, Geometry poly2)
        {
            //overlayOrig(poly1, poly2);
            //overlayOrigNoSnap(poly1, poly2);
            //overlayNGFloat(poly1, poly2);
            OverlayNGSnapIfNeeded(poly1, poly2);
            //overlayNG(poly1, poly2);
            //OverlayNGSnapping(poly1, poly2);
        }

        const bool IsVerbose = false;

        const bool IsSameVoronoi = false;

        const int N_PTS = 100;

        const int N_TESTS = 10000;

        static double SCALE = 100000000;

        static readonly double[] SCALES = new[]
        {
            // 0, // floating PM
            1, 100, 10000, 1000000, 100000000, 1e12
            // , 1e15
        };

        static void Log(string msg)
        {
            if (IsVerbose)
            {
                TestContext.WriteLine(msg);
            }
        }

        private bool UseSameBase
        {
            get => 0 == testIndex % 2;
        }


        private int testIndex = 0;
        private int errCount = 0;
        private string testDesc = "";

        [Test]
        public void Run()
        {
            TestContext.WriteLine("Running {0:D} tests", N_TESTS);
            for (int i = 1; i <= N_TESTS; i++)
            {
                testIndex = i;
                OverlayPolys();

                if ((i + 1) % 100 == 0)
                {
                    TestContext.Write(".");
                    if ((i + 1) % 10000 == 0)
                    {
                        TestContext.WriteLine();
                    }
                }
            }

            TestContext.WriteLine("\n============================");
            TestContext.WriteLine("Tests: {0:D}  Errors: {1:D}\n", N_TESTS, errCount);
        }

        private void OverlayPolys()
        {
            bool isUseSameBase = UseSameBase;
            var poly = CreatePolygons(N_PTS, isUseSameBase);
            Process(poly[0], poly[1]);
        }

        private void Process(Geometry poly1, Geometry poly2)
        {
            try
            {
                Overlay(poly1, poly2);
            }
            catch (TopologyException ex)
            {
                errCount++;
                TestContext.WriteLine(Stats());
                TestContext.Write("ERROR - %s\n", ex.Message);
                TestContext.WriteLine(poly1);
                TestContext.WriteLine(poly2);
            }
        }

        private string Stats()
        {
            return string.Format("\nTest {0:D}: {1}   (# errs: {2:D} = {3:D}%)\n", testIndex, testDesc,
                errCount, (int) (100 * errCount) / testIndex);
        }

        private void OverlayNG(Geometry poly1, Geometry poly2)
        {
            Log("Test: " + testIndex + "  --------------------");
            foreach (double scale in SCALES)
            {
                OverlayNG(poly1, poly2, scale);
            }
        }

        private void OverlayNG(Geometry poly1, Geometry poly2, double scale)
        {
            testDesc = string.Format("OverlayNG  scale: {0:F}", scale);
            Log(testDesc);
            var pm = PrecModel(scale);

            var inter = NetTopologySuite.Operation.OverlayNg.OverlayNG.Overlay(poly1, poly2,
                NetTopologySuite.Operation.OverlayNg.OverlayNG.INTERSECTION,
                pm);
            var symDiff = NetTopologySuite.Operation.OverlayNg.OverlayNG.Overlay(poly1, poly2,
                NetTopologySuite.Operation.OverlayNg.OverlayNG.SYMDIFFERENCE,
                pm);
            var union = NetTopologySuite.Operation.OverlayNg.OverlayNG.Overlay(OnlyPolys(inter), OnlyPolys(symDiff),
                NetTopologySuite.Operation.OverlayNg.OverlayNG.UNION,
                pm);
        }

        private static Geometry OnlyPolys(Geometry geom)
        {
            var polyList = PolygonExtracter.GetPolygons(geom);
            return geom.Factory.CreateMultiPolygon(GeometryFactory.ToPolygonArray(polyList));
        }

        private static PrecisionModel PrecModel(double scale)
        {
            // floating PM
            if (scale <= 0) return new PrecisionModel();

            return new PrecisionModel(scale);
        }

        private void OverlayNGFloat(Geometry poly1, Geometry poly2)
        {
            NetTopologySuite.Operation.OverlayNg.OverlayNG.Overlay(poly1, poly2,
                NetTopologySuite.Operation.OverlayNg.OverlayNG.INTERSECTION);
            //Geometry diff1 = poly1.difference(poly2);
            //Geometry diff2 = poly2.difference(poly1);
            //Geometry union = inter.union(diff1).union(diff2);
        }

        private void OverlayNGSnapIfNeeded(Geometry poly1, Geometry poly2)
        {
            NetTopologySuite.Operation.OverlayNg.OverlayNGSnapIfNeeded.Intersection(poly1, poly2);
            poly1.Intersection(poly2);
            //Geometry diff1 = poly1.difference(poly2);
            //Geometry diff2 = poly2.difference(poly1);
            //Geometry union = inter.union(diff1).union(diff2);
        }



        private void OverlayOrig(Geometry poly1, Geometry poly2)
        {
            poly1.Intersection(poly2);
            //Geometry diff1 = poly1.difference(poly2);
            //Geometry diff2 = poly2.difference(poly1);
            //Geometry union = inter.union(diff1).union(diff2);
        }

        private void OverlayOrigNoSnap(Geometry a, Geometry b)
        {
            OverlayOp.Overlay(a, b, SpatialFunction.Intersection);
        }

        private void OverlayNGSnapping(Geometry a, Geometry b)
        {
            UnionSnap(a, b, 0.00001);
        }

        public static Geometry UnionSnap(Geometry a, Geometry b, double tolerance)
        {
            var noder = GetNoder(tolerance);
            return NetTopologySuite.Operation.OverlayNg.OverlayNG.Overlay(a, b, SpatialFunction.Union, null, noder);
        }

        private static INoder GetNoder(double tolerance)
        {
            var snapNoder = new SnappingNoder(tolerance);
            return new ValidatingNoder(snapNoder);
        }
        //=======================================

        private static Geometry[] CreatePolygons(int npts, bool isUseSameBase)
        {
            var builder = new RandomPolygonBuilder(npts);
            var poly1 = builder.CreatePolygon();

            var builder2 = builder;
            if (!isUseSameBase)
            {
                builder2 = new RandomPolygonBuilder(npts);
            }

            var poly2 = builder2.CreatePolygon();
            poly2 = PerturbByRotation(poly2);

            //System.out.println(poly1);
            //System.out.println(poly2);

            //checkValid(poly1);
            //checkValid(poly2);

            return new[] {poly1, poly2};
        }

        private static Geometry PerturbByRotation(Geometry geom)
        {
            var rot = AffineTransformation.RotationInstance(2 * Math.PI);
            var geomRot = geom.Copy();
            geomRot.Apply(rot);
            return geomRot;
        }

        private void CheckValid(Geometry poly)
        {
            if (poly.IsValid) return;
            TestContext.WriteLine("INVALID!");
            TestContext.WriteLine(poly);
        }
    }

}
