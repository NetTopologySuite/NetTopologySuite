using System;
using System.Collections.Generic;
using System.Diagnostics;
using NetTopologySuite.Algorithm.Locate;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Geometries.Prepared
{
    public class PreparedPolygonCoversPerfTest : PerformanceTestCase
    {
        private const int NUM_ITER = 10_000;

        private const int NUM_PTS = 2000;

        private readonly GeometryFactory fact = new GeometryFactory(PrecisionModel.Floating.Value, 0);

        Stopwatch sw = new Stopwatch();


        bool testFailed = false;

        private IPreparedGeometry prepGeom;

        private List<Point> testPoints;

        private Geometry sinePoly;

        public PreparedPolygonCoversPerfTest()
            : base(nameof(PreparedPolygonCoversPerfTest))
        {
            RunSize = new[] {1000};
            RunIterations = 1;
        }

        public override void StartRun(int nPts)
        {
            TestContext.WriteLine("Running with size " + nPts);
            TestContext.WriteLine("Iterations per run = " + NUM_ITER);

            //  	Geometry poly = createCircle(new Coordinate(0, 0), 100, nPts);
            sinePoly = createSineStar(new Coordinate(0, 0), 100, nPts);
            //  	System.out.println(poly);
            //  	Geometry target = sinePoly.getBoundary();
            prepGeom = (new PreparedGeometryFactory()).Create(sinePoly);

            testPoints = CreatePoints(sinePoly.EnvelopeInternal, NUM_PTS);
        }

        Geometry createSineStar(Coordinate origin, double size, int nPts)
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

        List<Point> CreatePoints(Envelope env, int nPts)
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

        public void RunPreparedPolygon()
        {
            for (int i = 0; i < NUM_ITER; i++)
            {
                prepGeom = (new PreparedGeometryFactory()).Create(sinePoly);
                foreach (var pt in testPoints)
                {
                    prepGeom.Covers(pt);
                    //prepGeom.contains(pt);
                }
            }
        }

        public void RunIndexPointInAreaLocator()
        {
            for (int i = 0; i < NUM_ITER; i++)
            {
                var ipa = new IndexedPointInAreaLocator(sinePoly);
                foreach (var pt in testPoints)
                {
                    ipa.Locate(pt.Coordinate);
                }
            }
        }

        public override void TestInternal()
        {
            PerformanceTestRunner.Run(typeof(PreparedPolygonCoversPerfTest));
        }
    }

}
