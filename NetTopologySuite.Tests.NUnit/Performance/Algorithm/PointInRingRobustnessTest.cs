using System;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Tests.NUnit.Algorithm;
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
    /// The stress test reveals that the original <see cref="RayCrossingCounter"/>
    /// has inconsistencies with the <see cref="CGAlgorithmsDD.OrientationIndex"/> orientation index computation
    /// (which is now the standard in JTS, due to improved robustness).
    /// The {@link RayCrossingCounterDD} implementation is consistent,
    /// as expected.
    /// <para/>
    /// Note that the inconsistency does not indicate which algorithm is
    /// "more correct" - just that they produce different results.
    /// However, it is highly likely that the original RCC algorithm
    /// is <b>not</b> as robust as RCCDD, since it involves a series
    /// of double-precision arithmetic operations.
    /// </summary>
    /// <author>Martin Davis</author>
    public class PointInRingRobustnessTest
    {
        private bool _isAllConsistent = true;
        private int _testCount;
        private int _failureCount;

        private void Init()
        {
            _testCount = 0;
            _failureCount = 0;
            _isAllConsistent = true;
        }

        [Test, Category("Stress")]
        public void TestRightTriangles()
        {
            Init();
            CheckRightTriangles(200, 100, 1000);
            Console.WriteLine("Tests: " + _testCount + "   Failures: " + _failureCount);
            Assert.IsTrue(_isAllConsistent);
        }

        [Test, Category("Stress"), Ignore("Known to fail occasionally")]
        public void TestRandomTriangles()
        {
            CheckRandomTriangles(1000, 100, 100);
            Console.WriteLine("Tests: " + _testCount + "   Failures: " + _failureCount);
            Assert.IsTrue(_isAllConsistent);
        }

        private void CheckRandomTriangles(int sideLen, int numTris, int numEdgePts)
        {
            for (int i = 0; i < numTris; i++)
            {
                var start = new Coordinate(RandomInt(sideLen), RandomInt(sideLen));
                var triPts = new Coordinate[] {
                    start,
                    new Coordinate(RandomInt(sideLen), RandomInt(sideLen)),
                    new Coordinate(RandomInt(sideLen), RandomInt(sideLen)),
                    new Coordinate(start)
                };
                CheckTriangleEdgePoints(triPts, numEdgePts);
            }
        }

        private static readonly Random Rnd = new Random();

        private static int RandomInt(int max)
        {
            return Rnd.Next(max);
        }

        private void CheckRightTriangles(double maxHeight, double width, int numEdgePts)
        {
            for (int height = 0; height < maxHeight; height++)
            {
                var triPts = new Coordinate[] {
                    new Coordinate(0,0),
                    new Coordinate(0, height),
                    new Coordinate(width, 0),
                    new Coordinate(0,0)
                };
                CheckTriangleEdgePoints(triPts, numEdgePts);
            }
        }

        public void CheckTriangleEdgePoints(Coordinate[] triPts, int numEdgePts)
        {
            for (int i = 0; i < numEdgePts; i++)
            {
                double lenFrac = i / (double)(numEdgePts + 1);
                CheckTriangleConsistent(triPts, lenFrac);
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
            var triPts = new Coordinate[]
            {
                new Coordinate(0, 0),
                new Coordinate(0, height),
                new Coordinate(width, 0),
                new Coordinate(0, 0)
            };
            var seg = new LineSegment(0, height, width, 0);
            var pt = seg.PointAlong(lenFraction);

            return CheckTriangleConsistent(triPts, lenFraction);
        }

        bool CheckTriangleConsistent(Coordinate[] triPts, double lenFraction)
        {
            _testCount++;

            var seg = new LineSegment(triPts[1], triPts[2]);
            var pt = seg.PointAlong(lenFraction);

            //var isPointInRing = PointLocation.IsInRing(pt, triPts);
            //var isPointInRing = PointInRingWindingNumber(pt, triPts);
            //var isPointInRing = Location.Interior == RayCrossingCounter.LocatePointInRing(pt, triPts);
            bool isPointInRing = Location.Interior == NonRobustRayCrossingCounter.LocatePointInRing(pt, triPts);

            var orientation = Orientation.Index(triPts[1], triPts[2], pt);
            if (Orientation.IsCCW(triPts))
                orientation = Orientation.ReOrient(orientation);

            // if collinear can't determine a failure
            if (orientation == OrientationIndex.Collinear) return true;

            bool bothOutside = !isPointInRing && orientation == OrientationIndex.Left;
            bool bothInside = isPointInRing && orientation == OrientationIndex.Right;
            bool isConsistent = bothOutside || bothInside;

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
                for (int i = 0; i < coord.Length; i++)
                {
                    if (i > 0)
                        buf.Append(", ");
                    buf.Append(coord[i].X + " " + coord[i].Y);
                }

                buf.Append("))");
            }

            return buf.ToString();
        }

        /// <summary>
        /// Winding Number algorithm included for comparison purposes.
        /// This is almost equivalent to the RayCrossing algorithm,
        /// except that it checks all crossing segments
        /// instead of just the ones to the right of the point.
        /// <para/>
        /// This produces the same results as CGAlgorithmsDD
        /// </summary>
        /// <param name="p">point to test</param>
        /// <param name="ring">coordinates of ring (with same first and last point)</param>
        /// <returns>true if the point lies inside the ring</returns>
        private static bool PointInRingWindingNumber(Coordinate p, Coordinate[] ring)
        {
            int winding = 0; // the winding number counter

            int n = ring.Length;

            for (int i = 0; i < n - 1; i++)
            {
                if (ring[i].Y <= p.Y)
                {
                    // detect an upward crossing
                    if (ring[i + 1].Y > p.Y)
                    {
                        if (OrientationIndex.Left == Orientation.Index(ring[i], ring[i + 1], p))
                            winding++;
                    }
                }
                else
                {
                    // detect a downward crossing
                    if (ring[i + 1].Y <= p.Y)
                        if (OrientationIndex.Right == Orientation.Index(ring[i], ring[i + 1], p))
                            winding--;
                }
            }
            return winding != 0;
        }
    }
}
