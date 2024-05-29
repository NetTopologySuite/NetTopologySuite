using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Operation.RelateNG;
using NUnit.Framework;
using System.Xml.Linq;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.RelateNG
{
    internal class RelateNGPolygonPointsPerfTest : PerformanceTestCase
    {
        private const int N_ITER = 1;

        static double ORG_X = 100;
        static double ORG_Y = ORG_X;
        static double SIZE = 2 * ORG_X;
        static int N_ARMS = 6;
        static double ARM_RATIO = 0.3;

        static int GRID_SIZE = 100;

        private static GeometryFactory geomFact = NtsGeometryServices.Instance.CreateGeometryFactory();

        private Geometry geomA;
        private Geometry[] geomB;

        public RelateNGPolygonPointsPerfTest() : base(nameof(RelateNGPolygonPointsPerfTest))
        {
            RunSize = new int[] { 100, 1000, 10000, 100000 };
            RunIterations = N_ITER;
        }

        public override void SetUp()
        {
            TestContext.WriteLine("RelateNG perf test");
            TestContext.WriteLine("SineStar: origin: ("
                + ORG_X + ", " + ORG_Y + ")  size: " + SIZE
                + "  # arms: " + N_ARMS + "  arm ratio: " + ARM_RATIO);
            TestContext.WriteLine("# Iterations: " + N_ITER);
        }

        public override void StartRun(int npts)
        {
            var sineStar = SineStarFactory.Create(new Coordinate(ORG_X, ORG_Y), SIZE, npts, N_ARMS, ARM_RATIO);
            geomA = sineStar;

            geomB = CreateTestPoints(geomA.EnvelopeInternal, GRID_SIZE);

            TestContext.WriteLine("\n-------  Running with A: # pts = " + npts
                + "   B: " + geomB.Length + " points");

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
                NetTopologySuite.Operation.RelateNG.RelateNG.Relate(geomA, b, RelatePredicate.Intersects());
            }
        }

        public void RunIntersectsNGPrep()
        {
            var rng = NetTopologySuite.Operation.RelateNG.RelateNG.Prepare(geomA);
            foreach (var b in geomB)
            {
                rng.Evaluate(b, RelatePredicate.Intersects());
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
                NetTopologySuite.Operation.RelateNG.RelateNG.Relate(geomA, b, RelatePredicate.Contains());
            }
        }

        public void RunContainsNGPrep()
        {
            var rng = NetTopologySuite.Operation.RelateNG.RelateNG.Prepare(geomA);
            foreach (var b in geomB)
            {
                rng.Evaluate(b, RelatePredicate.Contains());
            }
        }

        public void xRunContainsNGPrepValidate()
        {
            var rng = NetTopologySuite.Operation.RelateNG.RelateNG.Prepare(geomA);
            foreach (var b in geomB)
            {
                bool resultNG = rng.Evaluate(b, RelatePredicate.Contains());
                bool resultOld = geomA.Contains(b);
                Assert.That(resultNG, Is.EqualTo(resultOld));
            }
        }

        private static Geometry[] CreateTestPoints(Envelope env, int nPtsOnSide)
        {
            var geoms = new Geometry[nPtsOnSide * nPtsOnSide];
            double baseX = env.MinX;
            double deltaX = env.Width / nPtsOnSide;
            double baseY = env.MinY;
            double deltaY = env.Height / nPtsOnSide;
            int index = 0;
            for (int i = 0; i < nPtsOnSide; i++)
            {
                for (int j = 0; j < nPtsOnSide; j++)
                {
                    double x = baseX + i * deltaX;
                    double y = baseY + i * deltaY;
                    Geometry geom = geomFact.CreatePoint(new Coordinate(x, y));
                    geoms[index++] = geom;
                }
            }
            return geoms;
        }
    }
}
