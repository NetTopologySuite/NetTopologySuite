using System;
using GeoAPI.Geometries;
using GeoAPI.Geometries.Prepared;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.IO;
using NetTopologySuite.Utilities;
using NUnit.Framework;
namespace NetTopologySuite.Tests.NUnit.Geometries.Prepared
{
    ///<summary>
    /// Stress tests <see cref="PreparedPolygon.Intersects(IGeometry)"/>
    /// to confirm it finds intersections correctly.
    ///</summary>
    /// <author>Martin Davis</author>
    public class PreparedPolygonIntersectsStressTest
    {
        const int MAX_ITER = 10000;
        static PrecisionModel pm = new PrecisionModel();
        static GeometryFactory fact = new GeometryFactory(pm, 0);
        static WKTReader wktRdr = new WKTReader(fact);
        static WKTWriter wktWriter = new WKTWriter();
        bool testFailed = false;
        [TestAttribute]
        [CategoryAttribute("Stress")]
        public void Test()
        {
            Run(1000);
        }
        public void Run(int nPts)
        {
            //  	Geometry poly = createCircle(new Coordinate(0, 0), 100, nPts);
            var poly = CreateSineStar(new Coordinate(0, 0), 100, nPts);
            //System.Console.WriteLine(poly);
            //System.Console.WriteLine("Running with " + nPts + " points");
            Test(poly);
        }
        static IGeometry CreateCircle(Coordinate origin, double size, int nPts)
        {
            var gsf = new GeometricShapeFactory();
            gsf.Centre = origin;
            gsf.Size = size;
            gsf.NumPoints = nPts;
            IGeometry circle = gsf.CreateCircle();
            // Polygon gRect = gsf.createRectangle();
            // Geometry g = gRect.getExteriorRing();
            return circle;
        }
        static IGeometry CreateSineStar(Coordinate origin, double size, int nPts)
        {
            var gsf = new SineStarFactory();
            gsf.Centre = origin;
            gsf.Size = size;
            gsf.NumPoints = nPts;
            gsf.ArmLengthRatio = 0.1;
            gsf.NumArms = 20;
            var poly = gsf.CreateSineStar();
            return poly;
        }
        static ILineString CreateTestLine(Envelope env, double size, int nPts)
        {
            var rnd = new Random();
            var width = env.Width;
            var xOffset = width * rnd.NextDouble();
            var yOffset = env.Height * rnd.NextDouble();
            var basePt = new Coordinate(
                            env.MinX + xOffset,
                            env.MinY + yOffset);
            var line = CreateTestLine(basePt, size, nPts);
            return line;
        }
        static ILineString CreateTestLine(Coordinate basePt, double size, int nPts)
        {
            var gsf = new GeometricShapeFactory();
            gsf.Centre = basePt;
            gsf.Size = size;
            gsf.NumPoints = nPts;
            IGeometry circle = gsf.CreateCircle();
            //    System.out.println(circle);
            return (ILineString)circle.Boundary;
        }
        public void Test(IGeometry g)
        {
            var count = 0;
            while (count < MAX_ITER)
            {
                count++;
                var line = CreateTestLine(g.EnvelopeInternal, 10, 20);
                //      System.out.println("Test # " + count);
                //  		System.out.println(line);
                TestResultsEqual(g, line);
            }
        }
        public void TestResultsEqual(IGeometry g, ILineString line)
        {
            var slowIntersects = g.Intersects(line);
            var pgFact = new PreparedGeometryFactory();
            var prepGeom = pgFact.Create(g);
            var fastIntersects = prepGeom.Intersects(line);
            if (slowIntersects != fastIntersects)
            {
                Console.WriteLine(line);
                Console.WriteLine("Slow = " + slowIntersects + ", Fast = " + fastIntersects);
                throw new Exception("Different results found for intersects() !");
            }
        }
    }
}
