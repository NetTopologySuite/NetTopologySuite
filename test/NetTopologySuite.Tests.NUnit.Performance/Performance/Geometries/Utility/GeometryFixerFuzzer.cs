using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Utilities;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Geometries.Utility
{
    public class GeometryFixerFuzzer
    {

        private const int NUM_ITER = 10000;
        private readonly Random _rnd = new Random(13);

        [Test, Explicit]
        public void Run()
        {
            var fuzzer = new GeometryFixerFuzzer();
            fuzzer.Run(NUM_ITER);
        }

        private readonly GeometryFactory _factory = new GeometryFactory();


        private void Run(int numIter)
        {
            for (int i = 0; i < numIter; i++)
            {
                int numHoles = _rnd.Next(0, 10);
                var invalidPoly = CreateRandomPoly(100, numHoles);
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

        private Geometry CreateRandomPoly(int numPoints, int numHoles)
        {
            int numRingPoints = numPoints / (numHoles + 1);
            var shell = CreateRandomRing(numRingPoints);
            var holes = new LinearRing[numHoles];
            for (int i = 0; i < numHoles; i++)
            {
                holes[i] = CreateRandomRing(numRingPoints);
            }
            return _factory.CreatePolygon(shell, holes);
        }

        private LinearRing CreateRandomRing(int numPoints)
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
            double ord = 100 * _rnd.NextDouble();
            return ord;
        }
    }

}
