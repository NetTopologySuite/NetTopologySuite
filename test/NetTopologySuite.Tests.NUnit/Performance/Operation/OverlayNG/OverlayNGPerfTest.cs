using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.OverlayNg;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.Overlay
{
    public class OverlayNGPerfTest : PerformanceTestCase
    {
        private const int PREC_SCALE_FACTOR = 1000000;

        private const int N_ITER = 1;

        static double ORG_X = 100;
        static double ORG_Y = ORG_X;
        static double SIZE = 2 * ORG_X;
        static int N_ARMS = 6;
        static double ARM_RATIO = 0.3;

        static int GRID_SIZE = 20;
        static double GRID_CELL_SIZE = SIZE / GRID_SIZE;

        static int NUM_CASES = GRID_SIZE * GRID_SIZE;

        private Geometry geomA;
        private Geometry[] geomB;

        private PrecisionModel precisionModel;


        public OverlayNGPerfTest() : base("OverlayNGPerfTest")
        {
            RunSize = new[] {100, 1000, 10000, 100000, 200000};
            //setRunSize(new int[] { 200000 });
            RunIterations = N_ITER;
        }

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(OverlayNGPerfTest));
        }

        public override void SetUp()
        {
            TestContext.WriteLine("OverlaySR perf test");
            TestContext.WriteLine("SineStar: origin: ("
                                  + ORG_X + ", " + ORG_Y + ")  size: " + SIZE
                                  + "  # arms: " + N_ARMS + "  arm ratio: " + ARM_RATIO);
            TestContext.WriteLine("# Iterations: " + N_ITER);
            TestContext.WriteLine("# B geoms: " + NUM_CASES);
            TestContext.WriteLine("Precision scale: " + PREC_SCALE_FACTOR);
        }

        public override void StartRun(int npts)
        {
            iter = 0;
            precisionModel = new PrecisionModel(PREC_SCALE_FACTOR);

            geomA = SineStarFactory.Create(new Coordinate(ORG_X, ORG_Y), SIZE, npts, N_ARMS, ARM_RATIO);

            int nptsB = npts / NUM_CASES;
            if (nptsB < 10) nptsB = 10;

            geomB = createTestGeoms(NUM_CASES, nptsB);

            TestContext.WriteLine("\n-------  Running with A: # pts = " + npts + "   B # pts = " + nptsB);

            if (npts == 999)
            {
                TestContext.WriteLine(geomA);

                foreach (var g in geomB)
                {
                    TestContext.WriteLine(g);
                }
            }

        }

        private Geometry[] createTestGeoms(int nGeoms, int npts)
        {
            var geoms = new Geometry[NUM_CASES];
            int index = 0;
            for (int i = 0; i < GRID_SIZE; i++)
            {
                for (int j = 0; j < GRID_SIZE; j++)
                {
                    double x = GRID_CELL_SIZE / 2 + i * GRID_CELL_SIZE;
                    double y = GRID_CELL_SIZE / 2 + j * GRID_CELL_SIZE;
                    var geom = SineStarFactory.Create(new Coordinate(x, y), GRID_CELL_SIZE, npts, N_ARMS, ARM_RATIO);
                    geoms[index++] = geom;
                }
            }

            return geoms;
        }

        private int iter = 0;

        public void RunIntersectionOLD()
        {
            foreach (var b in geomB)
            {
                geomA.Intersection(b);
            }
        }

        public void xRunUnionNG()
        {
            foreach (var b in geomB)
            {
                OverlayNG.Overlay(geomA, b, OverlayNG.UNION, precisionModel);
            }
        }

        public void xRunUnionOLD()
        {
            foreach (var b in geomB)
            {
                geomA.Union(b);
            }
        }

        public void RunIntersectionOLDOpt()
        {
            foreach (var b in geomB)
            {
                intersectionOpt(geomA, b);
            }
        }

        public void RunIntersectionNG()
        {
            foreach (var b in geomB)
            {
                OverlayNG.Overlay(geomA, b, OverlayNG.INTERSECTION, precisionModel);
            }
        }

        public void RunIntersectionNGFloating()
        {
            foreach (var b in geomB)
            {
                intersectionNGFloating(geomA, b);
            }
        }

        public void RunIntersectionNGOpt()
        {
            foreach (var b in geomB)
            {
                intersectionNGOpt(geomA, b);
            }
        }

        public void xRunIntersectionNGNoClip()
        {
            foreach (var b in geomB)
            {
                intersectionNGNoClip(geomA, b);
            }
        }

        public void xRunIntersectionNGPrepNoCache()
        {
            foreach (var b in geomB)
            {
                intersectionNGPrepNoCache(geomA, b);
            }
        }

        /**
 * Switching input order doesn't make much difference.
 * Update: actually it looks like having the smaller geometry
 * as the prepared one is faster (by a variable amount)
 */
        public void xRunIntersectionNGPrepNoCacheBA()
        {
            foreach (var b in geomB)
            {
                intersectionNGPrepNoCache(b, geomA);
            }
        }

        public Geometry intersectionNGOpt(Geometry a, Geometry b)
        {
            var intFast = fastIntersect(a, b);
            if (intFast != null) return intFast;
            return OverlayNG.Overlay(a, b, OverlayNG.INTERSECTION, precisionModel);
        }

        public Geometry intersectionNGNoClip(Geometry a, Geometry b)
        {
            var overlay = new OverlayNG(a, b, precisionModel, OverlayNG.INTERSECTION);
            overlay.Optimized = false;
            return overlay.GetResult();
        }

        public Geometry intersectionNGFloating(Geometry a, Geometry b)
        {
            var overlay = new OverlayNG(a, b, OverlayNG.INTERSECTION);
            overlay.Optimized = false;
            return overlay.GetResult();
        }

        public Geometry intersectionNGPrep(Geometry a, Geometry b)
        {
            var pg = cacheFetch(a);
            if (!pg.Intersects(b)) return null;
            if (pg.Covers(b)) return b.Copy();
            return OverlayNG.Overlay(a, b, OverlayNG.INTERSECTION, precisionModel);
        }

        public Geometry intersectionNGPrepNoCache(Geometry a, Geometry b)
        {
            var intFast = fastintersectsPrepNoCache(a, b);
            if (intFast != null) return intFast;

            return OverlayNG.Overlay(a, b, OverlayNG.INTERSECTION, precisionModel);
        }

        private Geometry fastintersectsPrepNoCache(Geometry a, Geometry b)
        {
            var aPG = (new PreparedGeometryFactory()).Create(a);

            if (!aPG.Intersects(b))
            {
                return a.Factory.CreateEmpty(a.Dimension);
            }

            if (aPG.Covers(b))
            {
                return b.Copy();
            }

            if (b.Covers(a))
            {
                return a.Copy();
            }

            // null indicates full overlay required
            return null;
        }

        private static Geometry fastIntersect(Geometry a, Geometry b)
        {
            var im = a.Relate(b);
            if (!im.IsIntersects())
                return a.Factory.CreateEmpty(a.Dimension);
            if (im.IsCovers())
                return b.Copy();
            if (im.IsCoveredBy())
                return a.Copy();
            // null indicates full overlay required
            return null;
        }

        /**
 * Use spatial predicates as a filter
 * in front of intersection.
 * 
 * @param a a geometry
 * @param b a geometry
 * @return the intersection of the geometries
 */
        public static Geometry intersectionOpt(Geometry a, Geometry b)
        {
            var intFast = fastIntersect(a, b);
            if (intFast != null) return intFast;
            return a.Intersection(b);
        }

        public Geometry intersectionOptPrepNoCache(Geometry a, Geometry b)
        {
            var intFast = fastintersectsPrepNoCache(a, b);
            if (intFast != null) return intFast;
            return a.Intersection(b);
        }

        /**
 * Use prepared geometry spatial predicates as a filter
 * in front of intersection,
 * with the first operand prepared.
 * 
 * @param a a geometry to prepare
 * @param b a geometry
 * @return the intersection of the geometries
 */
        public static Geometry intersectionOptPrep(Geometry a, Geometry b)
        {
            var pg = cacheFetch(a);
            if (!pg.Intersects(b)) return null;
            if (pg.Covers(b)) return b.Copy();
            return a.Intersection(b);
        }

        private static Geometry cacheKey = null;
        private static IPreparedGeometry cache = null;


        private static IPreparedGeometry cacheFetch(Geometry g)
        {
            if (g != cacheKey)
            {
                cacheKey = g;
                cache = (new PreparedGeometryFactory()).Create(g);
            }

            return cache;
        }
    }
}
