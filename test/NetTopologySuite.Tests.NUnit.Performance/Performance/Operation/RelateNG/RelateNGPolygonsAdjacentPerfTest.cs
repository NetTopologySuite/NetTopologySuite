using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.RelateNG;
using NUnit.Framework;

using System.Collections.Generic;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.RelateNG
{
    internal class RelateNGPolygonsAdjacentPerfTest : PerformanceTestCase
    {
        private readonly WKTReader rdr = new WKTReader();

        private const int N_ITER = 10;

        private IList<Geometry> polygons;

        public RelateNGPolygonsAdjacentPerfTest() : base(nameof(RelateNGPolygonsAdjacentPerfTest))
        {
            RunSize = new int[] { 1 };
            //setRunSize(new int[] { 20 });
            RunIterations = N_ITER;
        }

        public override void SetUp()
        {
            string resource = "europe.wkt";
            //String resource = "world.wkt";
            LoadPolygons(resource);

            TestContext.WriteLine("RelateNG Performance Test - Adjacent Polygons ");
            TestContext.WriteLine("Dataset: " + resource);

            TestContext.WriteLine("# geometries: " + polygons.Count
                + "   # pts: " + NumPoints(polygons));
            TestContext.WriteLine("----------------------------------");
        }

        private static int NumPoints(IEnumerable<Geometry> geoms)
        {
            int n = 0;
            foreach (var g in geoms)
            {
                n += g.NumPoints;
            }
            return n;
        }

        private void LoadPolygons(string resourceName)
        {
            var stream = TestData.EmbeddedResourceManager.GetResourceStream($"NetTopologySuite.Tests.NUnit.TestData.{resourceName}");
            var wktFileRdr = new WKTFileReader(stream, rdr);
            polygons = wktFileRdr.Read();
        }


        public void RunIntersectsOld()
        {
            foreach (var a in polygons)
            {
                foreach (var b in polygons)
                {
                    a.Intersects(b);
                }
            }
        }

        public void RunIntersectsOldPrep()
        {
            foreach (var a in polygons)
            {
                var pgA = PreparedGeometryFactory.Prepare(a);
                foreach (var b in polygons)
                {
                    pgA.Intersects(b);
                }
            }
        }

        public void RunIntersectsNG()
        {
            foreach (var a in polygons)
            {
                foreach (var b in polygons)
                {
                    NetTopologySuite.Operation.RelateNG.RelateNG.Relate(a, b, RelatePredicateFactory.Intersects());
                }
            }
        }

        public void RunIntersectsNGPrep()
        {
            foreach (var a in polygons)
            {
                var rng = NetTopologySuite.Operation.RelateNG.RelateNG.Prepare(a);
                foreach (var b in polygons)
                {
                    rng.Evaluate(b, RelatePredicateFactory.Intersects());
                }
            }
        }

        public void RunTouchesOld()
        {
            foreach (var a in polygons)
            {
                foreach (var b in polygons)
                {
                    a.Touches(b);
                }
            }
        }

        public void RunTouchesNG()
        {
            foreach (var a in polygons)
            {
                foreach (var b in polygons)
                {
                    NetTopologySuite.Operation.RelateNG.RelateNG.Relate(a, b, RelatePredicateFactory.Touches());
                }
            }
        }

        public void RunTouchesNGPrep()
        {
            foreach (var a in polygons)
            {
                var rng = NetTopologySuite.Operation.RelateNG.RelateNG.Prepare(a);
                foreach (var b in polygons)
                {
                    rng.Evaluate(b, RelatePredicateFactory.Touches());
                }
            }
        }

        public void RunAdjacentOld()
        {
            foreach (var a in polygons)
            {
                foreach (var b in polygons)
                {
                    a.Relate(b, IntersectionMatrixPattern.Adjacent);
                }
            }
        }

        public void RunAdjacentNG()
        {
            foreach (var a in polygons)
            {
                foreach (var b in polygons)
                {
                    NetTopologySuite.Operation.RelateNG.RelateNG.Relate(a, b, RelatePredicateFactory.Matches(IntersectionMatrixPattern.Adjacent));
                }
            }
        }

        public void RunAdjacentNGPrep()
        {
            foreach (var a in polygons)
            {
                var rng = NetTopologySuite.Operation.RelateNG.RelateNG.Prepare(a);
                foreach (var b in polygons)
                {
                    rng.Evaluate(b, RelatePredicateFactory.Matches(IntersectionMatrixPattern.Adjacent));
                }
            }
        }

        public void RunInteriorIntersectsOld()
        {
            foreach (var a in polygons)
            {
                foreach (var b in polygons)
                {
                    a.Relate(b, IntersectionMatrixPattern.InteriorIntersects);
                }
            }
        }

        public void RunInteriorIntersectsNG()
        {
            foreach (var a in polygons)
            {
                foreach (var b in polygons)
                {
                    NetTopologySuite.Operation.RelateNG.RelateNG.Relate(a, b, RelatePredicateFactory.Matches(IntersectionMatrixPattern.InteriorIntersects));
                }
            }
        }

        public void RunInteriorIntersectsNGPrep()
        {
            foreach (var a in polygons)
            {
                var rng = NetTopologySuite.Operation.RelateNG.RelateNG.Prepare(a);
                foreach (var b in polygons)
                {
                    rng.Evaluate(b, RelatePredicateFactory.Matches(IntersectionMatrixPattern.InteriorIntersects));
                }
            }
        }
    }
}
