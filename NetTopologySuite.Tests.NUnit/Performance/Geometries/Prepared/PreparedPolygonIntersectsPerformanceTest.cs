using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Geometries.Utilities;

namespace NetTopologySuite.Tests.NUnit.Performance.Geometries.Prepared
{
    [TestFixtureAttribute]
    public class PreparedPolygonIntersectsPerformanceTest
    {
        private const int MaxIter = 10;

        private static readonly int NumAoiPts = 2000;
        private const int NumLines = 10000;
        private const int NumLinePts = 100;

        private static readonly IPrecisionModel Pm = new PrecisionModel();
        private static readonly IGeometryFactory Fact = new GeometryFactory(Pm, 0);

        [TestAttribute, CategoryAttribute("LongRunning"), Explicit("takes ages to complete")]
        public void Test()
        {
            Test(5);
            Test(10);
            Test(500);
            Test(1000);
            Test(2000);
            Test(4000);
            /*
            Test(4000);
            Test(8000);
            */
        }

        public void Test(int nPts)
        {
            //var poly = createCircle(new Coordinate(0, 0), 100, nPts);
            var sinePoly = CreateSineStar(new Coordinate(0, 0), 100, nPts);
            //Console.WriteLine(poly);
            //var target = sinePoly.getBoundary();
            var target = sinePoly;

            var lines = CreateLines(target.EnvelopeInternal, NumLines, 1.0, NumLinePts);
            Test(target, lines);
        }

        private static IGeometry CreateCircle(Coordinate origin, double size, int nPts)
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

        private static IGeometry CreateSineStar(Coordinate origin, double size, int nPts)
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

        private static IList<IGeometry> CreateLines(Envelope env, int nItems, double size, int nPts)
        {
            int nCells = (int) Math.Sqrt(nItems);

            var geoms = new List<IGeometry>();
            double width = env.Width;
            double xInc = width/nCells;
            double yInc = width/nCells;
            for (int i = 0; i < nCells; i++)
            {
                for (int j = 0; j < nCells; j++)
                {
                    var @base = new Coordinate(
                        env.MinX + i*xInc,
                        env.MinY + j*yInc);
                    var line = CreateLine(@base, size, nPts);
                    geoms.Add(line);
                }
            }
            return geoms;
        }

        private static IGeometry CreateLine(Coordinate @base, double size, int nPts)
        {
            var gsf = new SineStarFactory();
            gsf.Centre = @base;
            gsf.Size = size;
            gsf.NumPoints = nPts;
            var circle = gsf.CreateSineStar();
            //    System.out.println(circle);
            return circle.Boundary;
        }

        private static void Test(IGeometry g, ICollection<IGeometry> lines)
        {
            Console.WriteLine("AOI # pts: " + g.NumPoints
                              + "      # lines: " + lines.Count
                              + "   # pts in line: " + NumLinePts
                );

            var sw = new Stopwatch();
            int count = 0;
            long time1 = 0L;
            long time2 = 0L;
            long time3 = 0L;
            for (int i = 0; i < MaxIter; i++)
            {
                Console.WriteLine(string.Format("\nIteration {0}", i));
                sw.Start();
                int count1 = TestPrepGeomNotCached(g, lines);
                sw.Stop();
                time1 += sw.ElapsedMilliseconds;

                sw.Restart();
                int count2 = TestPrepGeomCached(g, lines);
                sw.Stop();
                time2 += sw.ElapsedMilliseconds;

                sw.Restart();
                int count3 = TestOriginal(g, lines);
                sw.Stop();
                time3 += sw.ElapsedMilliseconds;
                sw.Reset();

                Assert.AreEqual(count1, count2);
                Assert.AreEqual(count2, count3);
                count = count3;
            }
            Console.WriteLine("Count of intersections = " + count);
            Console.WriteLine("Finished in \n\tPG NonCached: {0}\n\tPG Cached   : {1}\n\told JTS Algo: {2}", time1, time2, time3);
        }

        private static int TestOriginal(IGeometry g, IEnumerable<IGeometry> lines)
        {
            Console.WriteLine("Using original JTS algorithm");
            int count = 0;
            foreach (var line in lines)
            {
                if (g.Intersects(line))
                    count++;
            }
            return count;
        }

        private static int TestPrepGeomCached(IGeometry g, IEnumerable<IGeometry> lines)
        {
            Console.WriteLine("Using cached Prepared Geometry");
            var pgFact = new PreparedGeometryFactory();
            var prepGeom = pgFact.Create(g);

            int count = 0;
            foreach (var line in lines)
            {
                if (prepGeom.Intersects(line))
                    count++;
            }
            return count;
        }

        /// <summary>
        /// Tests using PreparedGeometry, but creating a new
        /// PreparedGeometry object each time.
        /// This tests whether there is a penalty for using
        /// the PG algorithm as a complete replacement for
        /// the original algorithm.
        /// </summary>
        /// <param name="g">The polygonal test geometry</param>
        /// <param name="lines">The lines to test for intersection with <paramref name="g"/></param>
        /// <returns>The count</returns>
        private static int TestPrepGeomNotCached(IGeometry g, IEnumerable<IGeometry> lines)
        {
            Console.WriteLine("Using NON-CACHED Prepared Geometry");
            var pgFact = new PreparedGeometryFactory();
            //    PreparedGeometry prepGeom = pgFact.create(g);

            int count = 0;
            foreach (var line in lines)
            {
                // test performance of creating the prepared geometry each time
                var prepGeom = pgFact.Create(g);

                if (prepGeom.Intersects(line))
                    count++;
            }
            return count;
        }

    }
}
