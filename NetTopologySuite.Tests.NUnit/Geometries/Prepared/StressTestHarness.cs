using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Utilities;
using NetTopologySuite.Geometries.Utilities;
namespace NetTopologySuite.Tests.NUnit.Geometries.Prepared
{
    public abstract class StressTestHarness
    {
        const int MAX_ITER = 10000;
        static readonly PrecisionModel pm = new PrecisionModel();
        static readonly GeometryFactory fact = new GeometryFactory(pm, 0);
        static WKTReader _wktRdr = new WKTReader(fact);
        static WKTWriter _wktWriter = new WKTWriter();
        protected StressTestHarness()
        {
        }
        public int TargetSize { get; set; } = 1000;
        public void Run(int nIter)
        {
            //System.Console.WriteLine("Running " + nIter + " tests");
            //  	Geometry poly = createCircle(new Coordinate(0, 0), 100, nPts);
            var poly = CreateSineStar(new Coordinate(0, 0), 100, TargetSize);
            Console.WriteLine(poly);
            //System.Console.WriteLine();
            //System.out.Console.WriteLine("Running with " + nPts + " points");
            Run(nIter, poly);
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
        static IGeometry CreateRandomTestGeometry(Envelope env, double size, int nPts)
        {
            var rnd = new Random(1);
            var width = env.Width;
            var xOffset = width * rnd.NextDouble();
            var yOffset = env.Height * rnd.NextDouble();
            var basePt = new Coordinate(
                            env.MinX + xOffset,
                            env.MinY + yOffset);
            var test = CreateTestCircle(basePt, size, nPts);
            if (test is IPolygon && rnd.NextDouble() > 0.5)
            {
                test = test.Boundary;
            }
            return test;
        }
        static IGeometry CreateTestCircle(Coordinate origin, double size, int nPts)
        {
            var gsf = new GeometricShapeFactory();
            gsf.Centre = origin;
            gsf.Size = size;
            gsf.NumPoints = nPts;
            IGeometry circle = gsf.CreateCircle();
            //    System.out.println(circle);
            return circle;
        }
        public void Run(int nIter, IGeometry target)
        {
            var count = 0;
            while (count < nIter)
            {
                count++;
                var test = CreateRandomTestGeometry(target.EnvelopeInternal, 10, 20);
                //Console.WriteLine("Test # " + count);
                //Console.WriteLine(line);
                //Console.WriteLine("Test[" + count + "] " + target.GetType().Name + "/" + test.GetType().Name);
                var isResultCorrect = CheckResult(target, test);
                if (!isResultCorrect)
                {
                    throw new Exception("Invalid result found");
                }
            }
        }
        public abstract bool CheckResult(IGeometry target, IGeometry test);
    }
}
