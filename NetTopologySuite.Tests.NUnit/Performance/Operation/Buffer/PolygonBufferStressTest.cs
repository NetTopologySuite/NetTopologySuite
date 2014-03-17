using System;
using System.Diagnostics;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

namespace NetTopologySuite.Tests.NUnit.Performance.Operation.Buffer
{
    /// <summary>
    /// Stress-tests buffering by repeatedly buffering a geometry
    /// using alternate positive and negative distances.
    /// <para/>
    /// In older versions of JTS this used to quickly cause failure due to robustness
    /// issues (bad noding causing topology failures).
    /// However by ver 1.13 (at least) this test should pass perfectly.
    /// This is due to the many heuristics introduced to improve buffer
    /// </summary>
    public class PolygonBufferStressTest
    {
        private const int MaxIter = 50;

        private static readonly IPrecisionModel PrecModel = new PrecisionModel();
        //private static IPrecisionModel PrecModel = new PrecisionModel(10);

        private static readonly IGeometryFactory Factory = new GeometryFactory(PrecModel, 0);
        private static readonly WKTReader WktReader = new WKTReader(Factory);
        //private static WKTWriter wktWriter = new WKTWriter();

        private readonly Stopwatch _sw = new Stopwatch();

        private bool _testFailed;

        [TestAttribute]
        [CategoryAttribute("Stress")]
        public void Test()
        {
            //String geomStr;
            //var shapeFact = new GeometricShapeFactory(Factory);

            var g = GetSampleGeometry();

            //    Geometry g = GeometricShapeFactory.createArc(fact, 0, 0, 200.0, 0.0, 6.0, 100);

            //Geometry circle = GeometricShapeFactory.createCircle(fact, 0, 0, 200, 100);
            //Geometry g = circle;

            //    Geometry sq = GeometricShapeFactory.createBox(fact, 0, 0, 1, 120);
            //    Geometry g = sq.difference(circle);

            //    Geometry handle = GeometricShapeFactory.createRectangle(fact, 0, 0, 400, 20, 1);
            //    Geometry g = circle.union(handle);

            Console.WriteLine(g);
            Test(g);
        }

        private IGeometry GetSampleGeometry()
        {
            String wkt;
            // triangle
            //wkt ="POLYGON (( 233 221, 210 172,  262 181, 233 221  ))";

            //star polygon with hole
            wkt =
                "POLYGON ((260 400, 220 300, 80 300, 180 220, 40 200, 180 160, 60 20, 200 80, 280 20, 260 140, 440 20, 340 180, 520 160, 280 220, 460 340, 300 300, 260 400), (260 320, 240 260, 220 220, 160 180, 220 160, 200 100, 260 160, 300 140, 320 180, 260 200, 260 320))";

            //star polygon with NO hole
            // wkt ="POLYGON ((260 400, 220 300, 80 300, 180 220, 40 200, 180 160, 60 20, 200 80, 280 20, 260 140, 440 20, 340 180, 520 160, 280 220, 460 340, 300 300, 260 400))";

            //star polygon with NO hole, 10x size
            // wkt ="POLYGON ((2600 4000, 2200 3000, 800 3000, 1800 2200, 400 2000, 1800 1600, 600 200, 2000 800, 2800 200, 2600 1400, 4400 200, 3400 1800, 5200 1600, 2800 2200, 4600 3400, 3000 3000, 2600 4000))";

            IGeometry g = null;
            try
            {
                g = WktReader.Read(wkt);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                _testFailed = true;
            }
            return g;
        }

        public void Test(IGeometry g)
        {
            var maxCount = MaxIter;
            //doIteratedBuffer(g, 1, -120.01, maxCount);
            //doIteratedBuffer(g, 1, 2, maxCount);
            DoAlternatingIteratedBuffer(g, 1, maxCount);
            if (_testFailed)
            {
                Console.WriteLine("FAILED!");
            }
        }

        public void DoIteratedBuffer(IGeometry g, double initDist, double distanceInc, int maxCount)
        {
            int i = 0;
            double dist = initDist;
            while (i < maxCount)
            {
                i++;
                Console.WriteLine("Iter: " + i + " --------------------------------------------------------");

                dist += distanceInc;
                Console.WriteLine("Buffer (" + dist + ")");
                g = GetBuffer(g, dist);
                //if (((Polygon) g).getNumInteriorRing() > 0)
                //  return;
            }
        }

        public void DoAlternatingIteratedBuffer(IGeometry g, double dist, int maxCount)
        {
            int i = 0;
            while (i < maxCount)
            {
                i++;
                Console.WriteLine("Iter: " + i + " --------------------------------------------------------");

                dist += 1.0;
                Console.WriteLine("Pos Buffer (" + dist + ")");
                g = GetBuffer(g, dist);
                Console.WriteLine("Neg Buffer (" + -dist + ")");
                g = GetBuffer(g, -dist);
            }
        }

        private IGeometry GetBuffer(IGeometry geom, double dist)
        {
            var buf = geom.Buffer(dist);
            //System.out.println(buf);
            Console.WriteLine(_sw.Elapsed);
            if (!buf.IsValid) throw new Exception("buffer not valid!");
            return buf;
        }
    }
}