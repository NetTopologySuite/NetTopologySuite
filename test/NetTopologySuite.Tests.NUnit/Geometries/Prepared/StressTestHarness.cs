using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Utilities;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Prepared
{
    public abstract class StressTestHarness
    {
        private int _numTargetPts = 1000;

        protected StressTestHarness()
        {
        }

        public int TargetSize
        {
            get => _numTargetPts;
            set => _numTargetPts = value;
        }

        public void Run(int nIter)
        {
            // TestContext.WriteLine("Running " + nIter + " tests");
            // Geometry poly = createCircle(new Coordinate(0, 0), 100, nPts);
            var poly = CreateSineStar(new Coordinate(0, 0), 100, _numTargetPts);
            TestContext.WriteLine(poly);

            // TestContext.WriteLine();
            // System.out.TestContext.WriteLine("Running with " + nPts + " points");
            Run(nIter, poly);
        }

        static Geometry CreateCircle(Coordinate origin, double size, int nPts)
        {
            var gsf = new GeometricShapeFactory();
            gsf.Centre = origin;
            gsf.Size = size;
            gsf.NumPoints = nPts;
            Geometry circle = gsf.CreateCircle();
            // Polygon gRect = gsf.createRectangle();
            // Geometry g = gRect.getExteriorRing();
            return circle;
        }

        static Geometry CreateSineStar(Coordinate origin, double size, int nPts)
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

        static Geometry CreateRandomTestGeometry(Envelope env, double size, int nPts)
        {
            var rnd = new Random(1);
            double width = env.Width;
            double xOffset = width * rnd.NextDouble();
            double yOffset = env.Height * rnd.NextDouble();
            var basePt = new Coordinate(
                            env.MinX + xOffset,
                            env.MinY + yOffset);
            var test = CreateTestCircle(basePt, size, nPts);
            if (test is Polygon && rnd.NextDouble() > 0.5)
            {
                test = test.Boundary;
            }
            return test;
        }

        static Geometry CreateTestCircle(Coordinate origin, double size, int nPts)
        {
            var gsf = new GeometricShapeFactory();
            gsf.Centre = origin;
            gsf.Size = size;
            gsf.NumPoints = nPts;
            Geometry circle = gsf.CreateCircle();
            // System.out.println(circle);
            return circle;
        }

        public void Run(int nIter, Geometry target)
        {
            int count = 0;
            while (count < nIter)
            {
                count++;
                var test = CreateRandomTestGeometry(target.EnvelopeInternal, 10, 20);

                // TestContext.WriteLine("Test # " + count);
                // TestContext.WriteLine(line);
                // TestContext.WriteLine("Test[" + count + "] " + target.GetType().Name + "/" + test.GetType().Name);
                bool isResultCorrect = CheckResult(target, test);
                if (!isResultCorrect)
                {
                    throw new Exception("Invalid result found");
                }
            }
        }

        public abstract bool CheckResult(Geometry target, Geometry test);
    }
}
