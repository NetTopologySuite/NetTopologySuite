using System;
using System.Collections.Generic;
using System.Globalization;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Precision;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Algorithm
{
    /**

     * 
     * @author
     *
     */
    /// <summary>
    /// Stress test for accuracy of various line intersection implementations.
    /// The test is to compute the intersection point of pairs of line segments
    /// with realistically large ordinate values,
    /// for angles of incidence which become increasingly close to parallel.
    /// The measure of accuracy is the sum of the distances of the computed point from the two lines.
    /// <para/>
    /// The intersection algorithms are:
    /// <list type="Bullet">
    /// <item><term>DP-Basic</term><description>a basic double-precision(DP) implementation, with no attempt at reducing the effects of numerical round-off</description></item>
    /// <item><term>DP-Cond</term><description>a DP implementation in which the inputs are conditioned by translating them to around the origin</description></item>
    /// <item><term>DP-CB</term><description>a DP implementation using the <see cref="CommonBitsRemover"/> functionality</description></item>
    /// <item><term>DD</term><description>an implementation using extended-precision {@link DoubleDouble} arithmetic</description></item>
    /// </list>
    /// <h2>Results</h2>
    /// <list type="Bullet">
    /// <item><description>DP-Basic is the least accurate</description></item>
    /// <item><description>DP-Cond has similar accuracy to DD</description></item>
    /// <item><description>DP-CB accuracy is better than DP, but degrades significantly as angle becomes closer to parallel</description></item>
    /// <item><description>DD is (presumably) the most accurate</description></item>
    /// </list>
    /// </summary>
    /// <author>Martin Davis</author>
    public class IntersectionStressTest
    {

        /**
         * 1 is fully parallel
         */
        private const double PARALLEL_FACTOR = 0.9999999999;

        private const int MAX_ITER = 1000;
        private const double ORDINATE_MAGNITUDE = 1000000;
        private const double SEG_LEN = 100;
        // make results reproducible
        static readonly Random RandGen = new Random(123456);

        private readonly IDictionary<string, double> _distMap = new Dictionary<string, double>();
        private const bool Verbose = false;

        [Test, Category("Stress")]
        public void Run()
        {
            Run(0.9);
            Run(0.999);
            Run(0.999999);
            Run(0.99999999);
        }

        /// <summary>Run tests for a given incident angle factor.
        /// The angle between the segments is <code>PI * incidentAngleFactor</code>.
        /// A factor closer to 1 means the segments are more nearly parallel.
        /// A factor of 1 means they are parallel.
        /// </summary>
        /// <param name="incidentAngleFactor">The factor of PI between the two segments</param>
        private void Run(double incidentAngleFactor)
        {
            for (int i = 0; i < MAX_ITER; i++)
            {
                DoIntersectionTest(i, incidentAngleFactor);
            }
            Console.WriteLine($"\nIncident angle factor = {incidentAngleFactor.ToString(NumberFormatInfo.InvariantInfo)}");
            PrintAverage();
        }

        private void DoIntersectionTest(int i, double incidentAngleFactor)
        {
            var basePt = RandomCoordinate();

            double baseAngle = 2 * Math.PI * RandGen.NextDouble();

            var p1 = ComputeVector(basePt, baseAngle, 0.1 * SEG_LEN);
            var p2 = ComputeVector(basePt, baseAngle, 1.1 * SEG_LEN);

            double angleBetween = baseAngle + incidentAngleFactor * Math.PI;

            var q1 = ComputeVector(basePt, angleBetween, 0.1 * SEG_LEN);
            var q2 = ComputeVector(basePt, angleBetween, 1.1 * SEG_LEN);

            var intPt = IntersectionAlgorithms.IntersectionBasic(p1, p2, q1, q2);
            var intPtDD = CGAlgorithmsDD.Intersection(p1, p2, q1, q2);
            var intPtCB = IntersectionAlgorithms.IntersectionCB(p1, p2, q1, q2);
            var intPtCond = IntersectionComputer.Intersection(p1, p2, q1, q2);

            if (Verbose)
            {
                Console.WriteLine(i + ":  Lines: "
                                    + WKTWriter.ToLineString(p1, p2) + "  -  "
                                    + WKTWriter.ToLineString(q1, q2));
            }

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
            if (Verbose)
            {
                Console.WriteLine(tag + " : "
                                      + WKTWriter.ToPoint(intPt)
                                      + " -- Dist P = " + distP + "    Dist Q = " + distQ);
            }
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
            double x = ORDINATE_MAGNITUDE * RandGen.NextDouble();
            double y = ORDINATE_MAGNITUDE * RandGen.NextDouble();
            return new Coordinate(x, y);
        }
    }

}
