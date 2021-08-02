using System;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Noding.Snap
{
    /// <summary>
    /// Finds intersections between line segments which are being snapped,
    /// and adds them as nodes.
    /// </summary>
    /// <version>1.17</version>
    public sealed class SnappingIntersectionAdder : ISegmentIntersector
    {
        private readonly LineIntersector _li = new RobustLineIntersector();
        private readonly double _snapTolerance;
        private readonly SnappingPointIndex _snapPointIndex;

        /// <summary>
        /// Creates an intersector which finds all snapped intersections,
        /// and adds them as nodes.
        /// </summary>
        /// <param name="snapTolerance">The snapping tolerance distance</param>
        /// <param name="snapPointIndex">A snap index to use</param>
        public SnappingIntersectionAdder(double snapTolerance, SnappingPointIndex snapPointIndex)
        {
            _snapPointIndex = snapPointIndex;
            _snapTolerance = snapTolerance;
        }

        /// <summary>
        /// This method is called by clients
        /// of the <see cref="ISegmentIntersector"/> class to process
        /// intersections for two segments of the <see cref="ISegmentString"/>s being intersected.
        /// Note that some clients (such as <c>MonotoneChain</c>s) may optimize away
        /// this call for segment pairs which they have determined do not intersect
        /// (e.g. by an disjoint envelope test).
        /// </summary>
        public void ProcessIntersections(
            ISegmentString seg0, int segIndex0,
            ISegmentString seg1, int segIndex1
        )
        {
            // don't bother intersecting a segment with itself
            if (seg0 == seg1 && segIndex0 == segIndex1) return;

            var coordinates = seg0.Coordinates;
            var p00 = coordinates[segIndex0];
            var p01 = coordinates[segIndex0 + 1];
            coordinates = seg1.Coordinates;
            var p10 = coordinates[segIndex1];
            var p11 = coordinates[segIndex1 + 1];

            /*
             * Don't node intersections which are just 
             * due to the shared vertex of adjacent segments.
             */
            if (!IsAdjacent(seg0, segIndex0, seg1, segIndex1))
            {
                _li.ComputeIntersection(p00, p01, p10, p11);
                //if (_li.HasIntersection && _li.IsProper) System.Diagnostics.Debug.WriteLine(_li);

                /*
                 * Process single point intersections only.
                 * Two-point (colinear) ones will be handled by the near-vertex code
                 */
                if (_li.HasIntersection && _li.IntersectionNum == 1)
                {
                    var intPt = _li.GetIntersection(0);
                    var snapPt = _snapPointIndex.Snap(intPt);

                    ((NodedSegmentString)seg0).AddIntersection(snapPt, segIndex0);
                    ((NodedSegmentString)seg1).AddIntersection(snapPt, segIndex1);
                }
            }

            /*
             * The segments must also be snapped to the other segment endpoints.
             */
            ProcessNearVertex(seg0, segIndex0, p00, seg1, segIndex1, p10, p11);
            ProcessNearVertex(seg0, segIndex0, p01, seg1, segIndex1, p10, p11);
            ProcessNearVertex(seg1, segIndex1, p10, seg0, segIndex0, p00, p01);
            ProcessNearVertex(seg1, segIndex1, p11, seg0, segIndex0, p00, p01);
        }

        /// <summary>
        /// If an endpoint of one segment is near
        /// the <i>interior</i> of the other segment, add it as an intersection.
        /// EXCEPT if the endpoint is also close to a segment endpoint
        /// (since this can introduce "zigs" in the linework).
        /// <para/>
        /// This resolves situations where
        /// a segment A endpoint is extremely close to another segment B,
        /// but is not quite crossing.  Due to robustness issues
        /// in orientation detection, this can
        /// result in the snapped segment A crossing segment B
        /// without a node being introduced.
        /// </summary>
        private void ProcessNearVertex(ISegmentString srcSS, int srcIndex, Coordinate p, ISegmentString ss, int segIndex, Coordinate p0, Coordinate p1)
        {
            /*
             * Don't add intersection if candidate vertex is near endpoints of segment.
             * This avoids creating "zig-zag" linework
             * (since the vertex could actually be outside the segment envelope).
             * Also, this should have already been snapped.
             */
            if (p.Distance(p0) < _snapTolerance) return;
            if (p.Distance(p1) < _snapTolerance) return;

            double distSeg = DistanceComputer.PointToSegment(p, p0, p1);
            if (distSeg < _snapTolerance)
            {
                // add vertex to target segment
                ((NodedSegmentString)ss).AddIntersection(p, segIndex);
                // add node at vertex to source SS
                ((NodedSegmentString)srcSS).AddIntersection(p, srcIndex);
            }
        }

        /// <summary>
        /// Test if two segments are adjacent segments on the same SegmentString.
        /// Note that closed edges require a special check for the point shared by the beginning
        /// and end segments.
        /// </summary>
        private static bool IsAdjacent(ISegmentString e0, int segIndex0, ISegmentString e1, int segIndex1)
        {
            if (e0 != e1) return false;

            bool isAdjacent = Math.Abs(segIndex0 - segIndex1) == 1;
            if (isAdjacent)
                return true;
            if (e0.IsClosed)
            {
                int maxSegIndex = e0.Count - 1;
                if ((segIndex0 == 0 && segIndex1 == maxSegIndex)
                    || (segIndex1 == 0 && segIndex0 == maxSegIndex))
                {
                    return true;
                }
            }

            return false;
        }

        /// <inheritdoc cref="ISegmentIntersector.IsDone"/>>
        /// <remarks>Always process all intersections</remarks>>
        /// <returns><c>false</c></returns>
        public bool IsDone
        {
            get => false;
        }

    }
}
