using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Geometries.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace NetTopologySuite.Tests.NUnit.Performance.Geometries.Prepared
{
    public class PreparedPolygonPointsPerfTest : PerformanceTestCase
    {
        private const int NUM_ITER = 1;

        private const int NUM_PTS = 2000;

        private readonly GeometryFactory fact = new GeometryFactory(PrecisionModel.Floating.Value, 0);


        private IPreparedGeometry _prepGeom;

        private IList<Point> _testPoints;

        private Geometry _sinePoly;
        private IndexedPointInAreaLocator _ipa;

        public PreparedPolygonPointsPerfTest()
            : base(nameof(PreparedPolygonPointsPerfTest))
        {
            RunSize = new[] {1000};
            RunIterations = NUM_ITER;
        }

        public override void StartRun(int nPts)
        {
            //  	Geometry poly = createCircle(new Coordinate(0, 0), 100, nPts);
            _sinePoly = CreateSineStar(new Coordinate(0, 0), 100, nPts);
            //  	System.out.println(poly);
            //  	Geometry target = sinePoly.getBoundary();
            _prepGeom = (new PreparedGeometryFactory()).Create(_sinePoly);
            _ipa = new IndexedPointInAreaLocator(_sinePoly);

            _testPoints = CreatePoints(_sinePoly.EnvelopeInternal, NUM_PTS);

            TestContext.WriteLine($"\n-------  Running with polygon size = {nPts}");
        }

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(PreparedPolygonPointsPerfTest));
        }

        private Geometry CreateSineStar(Coordinate origin, double size, int nPts)
        {
            var gsf = new SineStarFactory(fact);
            gsf.Centre = origin;
            gsf.Size = size;
            gsf.NumPoints = nPts;
            gsf.ArmLengthRatio = 0.1;
            gsf.NumArms = 50;
            var poly = gsf.CreateSineStar();
            return poly;
        }

        private IList<Point> CreatePoints(Envelope env, int nPts)
        {
            int nCells = (int) Math.Sqrt(nPts);

            var geoms = new List<Point>();
            double width = env.Width;
            double xInc = width / nCells;
            double yInc = width / nCells;
            for (int i = 0; i < nCells; i++)
            {
                for (int j = 0; j < nCells; j++)
                {
                    var @base = new Coordinate(
                        env.MinX + i * xInc,
                        env.MinY + j * yInc);
                    var pt = fact.CreatePoint(@base);
                    geoms.Add(pt);
                }
            }

            return geoms;
        }

        public void RunCoversNonPrep()
        {
            foreach (var pt in _testPoints)
            {
                _sinePoly.Covers(pt);
            }
        }

        public void RunCoversPrepared()
        {
            foreach (var pt in _testPoints)
            {
                _prepGeom.Covers(pt);
            }
        }

        public void RunCoversPrepNoCache()
        {
            foreach (var pt in _testPoints)
            {
                var pg = new PreparedGeometryFactory().Create(_sinePoly);
                pg.Covers(pt);
            }
        }

        public void RunIndexPointInAreaLocator()
        {
            foreach (var pt in _testPoints)
            {
                _ipa.Locate(pt.Coordinate);
            }
        }

        public void RunIntersectsNonPrep()
        {
            foreach (var pt in _testPoints)
            {
                _sinePoly.Intersects(pt);
            }
        }

        public void RunIntersectsPrepared()
        {
            foreach (var pt in _testPoints)
            {
                _prepGeom.Intersects(pt);
            }
        }

        public void RunIntersectsPrepNoCache()
        {
            foreach (var pt in _testPoints)
            {
                var pg = new PreparedGeometryFactory().Create(_sinePoly);
                pg.Intersects(pt);
            }
        }
    }
}
