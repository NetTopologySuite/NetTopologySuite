using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Geometries.Utility
{
    public class GeometryFixerFuzzer
    {

        private const int GEOM_EXTENT_SIZE = 100;
        private const int NUM_ITER = 10000;
        private readonly Random _rnd = new Random(13);
        private const bool IS_VERBOSE = false;

        [Test, Explicit]
        public void Run()
        {
            var fuzzer = new GeometryFixerFuzzer();
            fuzzer.Run(NUM_ITER);
        }

        private readonly GeometryFactory _factory = new GeometryFactory();


        private void Run(int numIter)
        {
            TestContext.WriteLine("GeometryFixer fuzzer: iterations = " + numIter);
            for (int i = 0; i < numIter; i++)
            {
                int numHoles = _rnd.Next(0, 10);
                //var invalidPoly = CreateRandomLinePoly(100, numHoles);
                var invalidPoly = CreateRandomCirclePoly(100, numHoles);
                var result = GeometryFixer.Fix(invalidPoly);
                bool isValid = result.IsValid;
                string status = isValid ? "valid" : "INVALID";
                string msg = string.Format("{0}: Pts - input {1}, output {2} - {3}",
                    i, invalidPoly.NumPoints, result.NumPoints, status);
                //System.out.println(invalidPoly);

                if (!isValid)
                {
                    TestContext.WriteLine(msg);
                    TestContext.WriteLine(invalidPoly);
                }
            }
        }

        private void report(int i, Geometry invalidPoly, Geometry result, bool isValid)
        {
            string status = isValid ? "valid" : "INVALID";
            string msg = string.Format("{0:D}: Pts - input {1:D}, output {2:D} - {3}",
                i, invalidPoly.NumPoints, result.NumPoints, status);
            if (IS_VERBOSE || !isValid)
            {
                TestContext.WriteLine(msg);
                TestContext.WriteLine(invalidPoly);
            }
        }


        private Geometry CreateRandomLinePoly(int numPoints, int numHoles)
        {
            int numRingPoints = numPoints / (numHoles + 1);
            var shell = CreateRandomLineRing(numRingPoints);
            var holes = new LinearRing[numHoles];
            for (int i = 0; i < numHoles; i++)
            {
                holes[i] = CreateRandomLineRing(numRingPoints);
            }
            return _factory.CreatePolygon(shell, holes);
        }

        private LinearRing CreateRandomLineRing(int numPoints)
        {
            return _factory.CreateLinearRing(CreateRandomPoints(numPoints));
        }

        private Coordinate[] CreateRandomPoints(int numPoints)
        {
            var pts = new Coordinate[numPoints + 1];
            for (int i = 0; i < numPoints; i++)
            {
                var p = new Coordinate(RandOrd(), RandOrd());
                pts[i] = p;
            }
            pts[pts.Length - 1] = pts[0].Copy();
            return pts;
        }

        private double RandOrd()
        {
            double ord = GEOM_EXTENT_SIZE * _rnd.NextDouble();
            return ord;
        }

        private Geometry CreateRandomCirclePoly(int numPoints, int numHoles)
        {
            int numRingPoints = numPoints / (numHoles + 1);
            var shell = CreateRandomCircleRing(numRingPoints);
            var holes = new LinearRing[numHoles];
            for (int i = 0; i < numHoles; i++)
            {
                holes[i] = CreateRandomCircleRing(numRingPoints);
            }
            return _factory.CreatePolygon(shell, holes);
        }

        private LinearRing CreateRandomCircleRing(int numPoints)
        {
            int numQuadSegs = (numPoints / 4) + 1;
            if (numQuadSegs < 3) numQuadSegs = 3;

            var p = new Coordinate(RandOrd(), RandOrd());
            var pt = _factory.CreatePoint(p);
            double radius = GEOM_EXTENT_SIZE * _rnd.NextDouble() / 2;
            var buffer = (Polygon)pt.Buffer(radius, numQuadSegs);
            return (LinearRing)buffer.ExteriorRing;
        }
    }

}
