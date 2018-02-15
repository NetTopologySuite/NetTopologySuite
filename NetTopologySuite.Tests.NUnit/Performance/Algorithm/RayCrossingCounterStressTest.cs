using System;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Performance.Algorithm
{
    ///<summary>
    /// Stress test of <see cref="RayCrossingCounter"/> Point-In-Ring algorithm.
    /// The input to each test is a triangle with a slanted side, 
    /// and a point interpolated along the side. 
    /// Almost always the point will not lie exactly on the side.
    /// The test consists of comparing the result of computing Point-In-Ring and the result of
    /// determining the orientation of the point relative to the side.
    /// The test fails if these are not consistent.
    /// <para/>
    /// The stress test reveals that the original RayCrossingCounter
    /// has inconsistencies with the DD orientation index computation
    /// (which is now the standard in JTS, due to improved robustness).
    /// The {@link RayCrossingCounterDD} implementation is consistent,
    /// as expected.
    /// <para/>
    /// Note that the inconsistency does not indicate which algorithm is 
    /// "more correct" - just that they produce different results.
    /// However, it is highly likely that the original RCC algorithm
    /// is not robust, since it involves significant arithmetic calcuation.
    /// </summary>
    /// <author>Martin Davis</author>
    public class RayCrossingCounterStressTest
    {
        private readonly PrecisionModel pmFixed_1 = new PrecisionModel(1.0);
        private bool _isAllConsistent = true;
        private int _testCount;
        private int _failureCount;


        [Test, Category("Stress")]
        public void TestTriangles()
        {
            CheckTriangles(500, 100, 1000);
            Console.WriteLine("Tests: " + _testCount + "   Failures: " + _failureCount);
            Assert.IsTrue(_isAllConsistent);
        }

        private void CheckTriangles(double maxHeight, double width, int numPts)
        {
            for (int i = 0; i < maxHeight; i++)
            {
                CheckTriangleEdge(i, width, numPts);
            }
        }

        public void CheckTriangleEdge(double height, double width, int numPts)
        {
            for (int i = 0; i < numPts; i++)
            {
                double lenFrac = i / (double) (numPts + 1);
                CheckTriangle(height, width, lenFrac);
            }
        }

        private bool CheckTriangle(double height, double width, double lenFraction)
        {
            Coordinate[] triPts = new Coordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(0, height),
                new Coordinate(width, 0),
                new Coordinate(0, 0)
            };
            LineSegment seg = new LineSegment(0, height, width, 0);
            Coordinate pt = seg.PointAlong(lenFraction);

            return CheckTriangleConsistent(triPts, pt);
        }

        bool CheckTriangleConsistent(Coordinate[] triPts, Coordinate pt)
        {
            _testCount++;
            //boolean isPointInRing = CGAlgorithms.isPointInRing(pt, triPts);
            var isPointInRing = Location.Interior == RayCrossingCounter.LocatePointInRing(pt, triPts);
            //boolean isPointInRing = Location.INTERIOR == RayCrossingCounterDD.locatePointInRing(pt, triPts);

            int orientation = CGAlgorithms.OrientationIndex(triPts[1], triPts[2], pt);

            // if collinear can't determine a failure
            if (orientation == CGAlgorithms.Collinear) return true;

            var bothOutside = !isPointInRing && orientation == CGAlgorithms.CounterClockwise;
            var bothInside = isPointInRing && orientation == CGAlgorithms.Clockwise;
            var isConsistent = bothOutside || bothInside;

            if (!isConsistent)
            {
                _isAllConsistent = false;
                _failureCount++;
                Console.WriteLine("Inconsistent: "
                                  + "PIR=" + isPointInRing + " Orient=" + orientation
                                  + "  Pt: " + WKTWriter.ToPoint(pt)
                                  + "  seg: " + WKTWriter.ToLineString(triPts[1], triPts[2])
                                  + "  tri: " + ToPolygon(triPts));
            }

            return isConsistent;

        }

        public static string ToPolygon(Coordinate[] coord)
        {
            var buf = new StringBuilder();
            buf.Append("POLYGON ");
            if (coord.Length == 0)
                buf.Append(" EMPTY");
            else
            {
                buf.Append("((");
                for (var i = 0; i < coord.Length; i++)
                {
                    if (i > 0)
                        buf.Append(", ");
                    buf.Append(coord[i].X + " " + coord[i].Y);
                }

                buf.Append("))");
            }

            return buf.ToString();
        }

    }
}