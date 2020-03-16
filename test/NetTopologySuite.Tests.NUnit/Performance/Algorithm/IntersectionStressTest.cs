using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Algorithm
{
    public class IntersectionStressTest
    {

        /**
         * 1 is fully parallel
         */
        private const double PARALLEL_FACTOR = 0.9999999999;

        private const int MAX_ITER = 1000;
        private const double MAX_ORD = 1000000;
        private const double SEG_LEN = 100;
        // make results reproducible
        static readonly Random RandGen = new Random(123456);

        private readonly IDictionary<string, double> _distMap = new Dictionary<string, double>();

        [Test, Category("Stress")]
        public void Run()
        {
            for (int i = 0; i < MAX_ITER; i++)
            {
                DoIntersectionTest(i);
            }
            PrintAverage();
        }

        private void DoIntersectionTest(int i)
        {
            var basePt = RandomCoordinate();

            double baseAngle = 2 * Math.PI * RandGen.NextDouble();

            var p1 = ComputeVector(basePt, baseAngle, 0.1 * SEG_LEN);
            var p2 = ComputeVector(basePt, baseAngle, 1.1 * SEG_LEN);

            double angleTest = baseAngle + PARALLEL_FACTOR * Math.PI;

            var q1 = ComputeVector(basePt, angleTest, 0.1 * SEG_LEN);
            var q2 = ComputeVector(basePt, angleTest, 1.1 * SEG_LEN);

            Console.WriteLine(i + ":  Lines: "
                + WKTWriter.ToLineString(p1, p2) + "  -  "
                + WKTWriter.ToLineString(q1, q2));

            var intPt = IntersectionAlgorithms.IntersectionBasic(p1, p2, q1, q2);
            var intPtDD = CGAlgorithmsDD.Intersection(p1, p2, q1, q2);
            var intPtCB = IntersectionAlgorithms.IntersectionCB(p1, p2, q1, q2);
            var intPtCond = IntersectionComputer.Intersection(p1, p2, q1, q2);

            PrintStats("DP    ", intPt, p1, p2, q1, q2);
            PrintStats("CB    ", intPtCB, p1, p2, q1, q2);
            PrintStats("Cond  ", intPtCond, p1, p2, q1, q2);
            PrintStats("DD    ", intPtDD, p1, p2, q1, q2);
        }

        private void PrintStats(string tag, Coordinate intPt, Coordinate p1, Coordinate p2, Coordinate q1, Coordinate q2)
        {
            double distP = DistanceComputer.PointToLinePerpendicular(intPt, p1, p2);
            double distQ = DistanceComputer.PointToLinePerpendicular(intPt, q1, q2);
            AddStat(tag, distP);
            AddStat(tag, distQ);
            Console.WriteLine(tag + " : "
                + WKTWriter.ToPoint(intPt)
                + " -- Dist P = " + distP + "    Dist Q = " + distQ);
        }

        private void AddStat(string tag, double dist)
        {
            double distTotal = 0.0;
            if (_distMap.ContainsKey(tag))
            {
                distTotal = _distMap[tag];
            }
            distTotal += dist;
            _distMap[tag] = distTotal;
        }

        private void PrintAverage()
        {
            Console.WriteLine("Average distance from lines");
            foreach (string key in _distMap.Keys)
            {
                double distTotal = _distMap[key];
                double avg = distTotal / MAX_ITER;
                Console.WriteLine(key + " : " + avg);
            }
        }
        private static Coordinate ComputeVector(Coordinate basePt, double angle, double len)
        {
            double x = basePt.X + len * Math.Cos(angle);
            double y = basePt.Y + len * Math.Sin(angle);
            return new Coordinate(x, y);
        }

        private static Coordinate RandomCoordinate()
        {
            double x = MAX_ORD * RandGen.NextDouble();
            double y = MAX_ORD * RandGen.NextDouble();
            return new Coordinate(x, y);
        }
    }

}
