using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Operation.RelateNG;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.RelateNG
{
    internal class RelateNGPolygonsOverlappingPerfTest : PerformanceTestCase
    {
        private const int N_ITER = 1;

        static double ORG_X = 100;
        static double ORG_Y = ORG_X;
        static double SIZE = 2 * ORG_X;
        static int N_ARMS = 6;
        static double ARM_RATIO = 0.3;

        static int GRID_SIZE = 100;
        static double GRID_CELL_SIZE = SIZE / GRID_SIZE;

        static int NUM_CASES = GRID_SIZE * GRID_SIZE;

        private const int B_SIZE_FACTOR = 20;
        private static readonly GeometryFactory factory = NtsGeometryServices.Instance.CreateGeometryFactory();

        private Geometry geomA;

        private Geometry[] geomB;

        public RelateNGPolygonsOverlappingPerfTest() : base(nameof(RelateNGPolygonsOverlappingPerfTest))
        {
            RunSize = new int[] { 100, 1000, 10000, 100000, 200000 };
            //setRunSize(new int[] { 200000 });
            RunIterations = N_ITER;
        }

        public override void SetUp()
        {
            TestContext.WriteLine("RelateNG perf test");
            TestContext.WriteLine("SineStar: origin: ("
                + ORG_X + ", " + ORG_Y + ")  size: " + SIZE
                + "  # arms: " + N_ARMS + "  arm ratio: " + ARM_RATIO);
            TestContext.WriteLine("# Iterations: " + N_ITER);
            TestContext.WriteLine("# B geoms: " + NUM_CASES);
        }

        public override void StartRun(int npts)
        {
            var sineStar = NetTopologySuite.Geometries.Utilities.SineStarFactory.Create(new Coordinate(ORG_X, ORG_Y), SIZE, npts, N_ARMS, ARM_RATIO);
            geomA = sineStar;

            int nptsB = npts * B_SIZE_FACTOR / NUM_CASES;
            if (nptsB < 10) nptsB = 10;

            geomB = createSineStarGrid(NUM_CASES, nptsB);
            //geomB =  createCircleGrid(NUM_CASES, nptsB);

            TestContext.WriteLine("\n-------  Running with A: polygon # pts = " + npts
                + "   B # pts = " + nptsB + "  x " + NUM_CASES + " polygons");

            /*
            if (npts == 999) {
              TestContext.WriteLine(geomA);

              for (Geometry g : geomB) {
                TestContext.WriteLine(g);
              }
            }
        */
        }

        public void RunIntersectsOld()
        {
            foreach (var b in geomB)
            {
                geomA.Intersects(b);
            }
        }

        public void RunIntersectsOldPrep()
        {
            var pgA = PreparedGeometryFactory.Prepare(geomA);
            foreach (var b in geomB)
            {
                pgA.Intersects(b);
            }
        }

        public void RunIntersectsNG()
        {
            foreach (var b in geomB)
            {
                NetTopologySuite.Operation.RelateNG.RelateNG.Relate(geomA, b, RelatePredicateFactory.Intersects());
            }
        }

        public void RunIntersectsNGPrep()
        {
            var rng = NetTopologySuite.Operation.RelateNG.RelateNG.Prepare(geomA);
            foreach (var b in geomB)
            {
                rng.Evaluate(b, RelatePredicateFactory.Intersects());
            }
        }

        public void RunContainsOld()
        {
            foreach (var b in geomB)
            {
                geomA.Contains(b);
            }
        }

        public void RunContainsOldPrep()
        {
            var pgA = PreparedGeometryFactory.Prepare(geomA);
            foreach (var b in geomB)
            {
                pgA.Contains(b);
            }
        }

        public void RunContainsNG()
        {
            foreach (var b in geomB)
            {
                NetTopologySuite.Operation.RelateNG.RelateNG.Relate(geomA, b, RelatePredicateFactory.Contains());
            }
        }

        public void RunContainsNGPrep()
        {
            var rng = NetTopologySuite.Operation.RelateNG.RelateNG.Prepare(geomA);
            foreach (var b in geomB)
            {
                rng.Evaluate(b, RelatePredicateFactory.Contains());
            }
        }

        public void xRunContainsNGPrepValidate()
        {
            var rng = NetTopologySuite.Operation.RelateNG.RelateNG.Prepare(geomA);
            foreach (var b in geomB)
            {
                bool resultNG = rng.Evaluate(b, RelatePredicateFactory.Contains());
                bool resultOld = geomA.Contains(b);
                Assert.That(resultNG, Is.EqualTo(resultOld));
            }
        }

        private Geometry[] createSineStarGrid(int nGeoms, int npts)
        {
            var geoms = new Geometry[NUM_CASES];
            int index = 0;
            for (int i = 0; i < GRID_SIZE; i++)
            {
                for (int j = 0; j < GRID_SIZE; j++)
                {
                    double x = GRID_CELL_SIZE / 2 + i * GRID_CELL_SIZE;
                    double y = GRID_CELL_SIZE / 2 + j * GRID_CELL_SIZE;
                    var geom = NetTopologySuite.Geometries.Utilities.SineStarFactory.Create(
                        new Coordinate(x, y), GRID_CELL_SIZE, npts, N_ARMS, ARM_RATIO);
                    geoms[index++] = geom;
                }
            }
            return geoms;
        }

        private Geometry[] CreateCircleGrid(int nGeoms, int npts)
        {
            var geoms = new Geometry[NUM_CASES];
            int index = 0;
            for (int i = 0; i < GRID_SIZE; i++)
            {
                for (int j = 0; j < GRID_SIZE; j++)
                {
                    double x = GRID_CELL_SIZE / 2 + i * GRID_CELL_SIZE;
                    double y = GRID_CELL_SIZE / 2 + j * GRID_CELL_SIZE;
                    var p = new Coordinate(x, y);
                    var geom = factory.CreatePoint(p).Buffer(GRID_CELL_SIZE / 2.0);
                    geoms[index++] = geom;
                }
            }
            return geoms;
        }

    }
}
