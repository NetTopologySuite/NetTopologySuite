using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Geometries.Utilities;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace NetTopologySuite.Tests.NUnit.Performance.Geometries.Prepared
{
    [TestFixture]
    public class PreparedPolygonLinesPerfTest : PerformanceTestCase
    {
        private const int MaxIter = 10;

        private const int NumLines = 1000;
        private const int NumLinePts = 100;

        private static readonly PrecisionModel Pm = new PrecisionModel();
        private static readonly GeometryFactory Fact = new GeometryFactory(Pm, 0);

        private Geometry _target;
        private IList<LineString> _lines;
        private IPreparedGeometry prepGeom;

        public PreparedPolygonLinesPerfTest()
            : base(nameof(PreparedPolygonLinesPerfTest))
        {
            RunSize = new int[] { 10, 100, 1000, 2000 };
            RunIterations = MaxIter;
        }

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(PreparedPolygonLinesPerfTest));
        }

        public override void StartRun(int nPts)
        {
            //var poly = createCircle(new Coordinate(0, 0), 100, nPts);
            var sinePoly = CreateSineStar(new Coordinate(0, 0), 100, nPts);
            //Console.WriteLine(poly);
            //var target = sinePoly.getBoundary();
            _target = sinePoly;

            var pgFact = new PreparedGeometryFactory();
            prepGeom = pgFact.Create(_target);

            _lines = CreateLines(_target.EnvelopeInternal, NumLines, 1.0, NumLinePts);
        }

        private static Geometry CreateCircle(Coordinate origin, double size, int nPts)
        {
            var gsf = new NetTopologySuite.Utilities.GeometricShapeFactory();
            gsf.Centre = origin;
            gsf.Size = size;
            gsf.NumPoints = nPts;
            var circle = gsf.CreateCircle();
            // Polygon gRect = gsf.createRectangle();
            // Geometry g = gRect.getExteriorRing();
            return circle;
        }

        private static Geometry CreateSineStar(Coordinate origin, double size, int nPts)
        {
            var gsf = new SineStarFactory();
            gsf.Centre = origin;
            gsf.Size = size;
            gsf.NumPoints = nPts;
            gsf.ArmLengthRatio = 0.1;
            gsf.NumArms = 50;
            var poly = gsf.CreateSineStar();
            return poly;
        }

        private static IList<LineString> CreateLines(Envelope env, int nItems, double size, int nPts)
        {
            int nCells = (int)Math.Sqrt(nItems);

            var geoms = new List<LineString>();
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
                    var line = CreateLine(@base, size, nPts);
                    geoms.Add(line);
                }
            }
            return geoms;
        }

        private static LineString CreateLine(Coordinate @base, double size, int nPts)
        {
            var gsf = new SineStarFactory();
            gsf.Centre = @base;
            gsf.Size = size;
            gsf.NumPoints = nPts;
            var circle = gsf.CreateSineStar();
            //    System.out.println(circle);
            return (LineString)circle.Boundary;
        }

        public void RunIntersectsNonPrep()
        {
            foreach (var line in _lines)
            {
                bool result = _target.Intersects(line);
            }
        }

        public void RunIntersectsPrepCached()
        {
            foreach (var line in _lines)
            {
                bool result = prepGeom.Intersects(line);
            }
        }

        public void RunIntersectsPrepNotCached()
        {
            foreach (var line in _lines)
            {
                var pg = new PreparedGeometryFactory().Create(_target);
                bool result = pg.Intersects(line);
            }
        }

        public void RunCoversNonPrep()
        {
            foreach (var line in _lines)
            {
                bool result = _target.Covers(line);
            }
        }

        public void RunCoverPrepCached()
        {
            foreach (var line in _lines)
            {
                bool result = prepGeom.Covers(line);
            }
        }

        public void RunCoverPrepNotCached()
        {
            foreach (var line in _lines)
            {
                var pg = new PreparedGeometryFactory().Create(_target);
                bool result = pg.Covers(line);
            }
        }
    }
}
