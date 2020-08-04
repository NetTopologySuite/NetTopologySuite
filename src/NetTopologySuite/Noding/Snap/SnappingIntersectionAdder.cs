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
    public class SnappingIntersectionAdder : ISegmentIntersector
    {
        private readonly LineIntersector _li;
        private readonly double _snapTolerance;
        private readonly SnapPointIndex _snapIndex;

        /// <summary>
        /// Creates an intersector which finds all snapped interior intersections,
        /// and adds them as nodes.
        /// </summary>
        /// <param name="snapIndex">A snap index to use</param>
        public SnappingIntersectionAdder(SnapPointIndex snapIndex)
        {
            _snapIndex = snapIndex;
            _snapTolerance = snapIndex.Tolerance;

            /*
             * Intersections are detected and computed using full precision.
             */
            _li = new RobustLineIntersector();
        }

        /// <summary>
        /// A trivial intersection is an apparent self-intersection which in fact
        /// is the point shared by adjacent segments of a SegmentString.
        /// Note that closed edges require a special check for the point shared by the beginning
        /// and end segments.
        /// </summary>
        private bool IsAdjacentIntersection(ISegmentString e0, int segIndex0, ISegmentString e1, int segIndex1)
        {
            if (e0 == e1)
            {
                if (_li.IntersectionNum == 1)
                {
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
                }
            }

            return false;
        }

        /// <summary>
        /// This method is called by clients
        /// of the <see cref="ISegmentIntersector"/> class to process
        /// intersections for two segments of the <see cref="ISegmentString"/>s being intersected.
        /// Note that some clients (such as <code>MonotoneChain</code>s) may optimize away
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

            var p00 = seg0.Coordinates[segIndex0];
            var p01 = seg0.Coordinates[segIndex0 + 1];
            var p10 = seg1.Coordinates[segIndex1];
            var p11 = seg1.Coordinates[segIndex1 + 1];

            _li.ComputeIntersection(p00, p01, p10, p11);
            //if (li.hasIntersection() && li.isProper()) Debug.println(li);

            /*
             * Process single point intersections only.
             * Two-point ones will be handled by the near-vertex code
             */
            if (_li.HasIntersection && _li.IntersectionNum == 1)
            {
                /*
                if (li.isInteriorIntersection()) {
                  ((NodedSegmentString) e0).addIntersections(li, segIndex0, 0);
                  ((NodedSegmentString) e1).addIntersections(li, segIndex1, 1);
                  return;
                }
                */
                if (!IsAdjacentIntersection(seg0, segIndex0, seg1, segIndex1))
                {
                    var intPt = _li.GetIntersection(0);
                    var snapPt = _snapIndex.Snap(intPt);

                    ((NodedSegmentString) seg0).AddIntersection(snapPt, segIndex0);
                    ((NodedSegmentString) seg1).AddIntersection(snapPt, segIndex1);
                }
            }

            /*
             * Segments do not actually intersect, within the limits of orientation index robustness.
             * 
             * To avoid certain robustness issues in snapping, 
             * also treat very near vertex-segment situations as intersections.
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
        private void ProcessNearVertex(ISegmentString srcSS, int srcIndex, Coordinate p, ISegmentString edge, int segIndex, Coordinate p0, Coordinate p1)
        {
            /*
             * Don't add intersection if candidate vertex is near endpoints of segment.
             * This avoids creating "zig-zag" linework
             * (since the vertex could actually be outside the segment envelope).
             */
            if (p.Distance(p0) < _snapTolerance) return;
            if (p.Distance(p1) < _snapTolerance) return;

            double distSeg = DistanceComputer.PointToSegment(p, p0, p1);
            if (distSeg < _snapTolerance)
            {
                // add vertex to target segment
                ((NodedSegmentString) edge).AddIntersection(p, segIndex);
                // add node at vertext to source SS
                ((NodedSegmentString)srcSS).AddIntersection(p, srcIndex);
            }
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
