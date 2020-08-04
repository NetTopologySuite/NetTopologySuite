using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Noding.Snapround
{
    /**
     * Finds intersections between line segments which will be snap-rounded,
     * and adds them as nodes.
     * <p>
     * Intersections are detected and computed using full precision.
     * Snapping takes place in a subsequent phase.
     * To avoid robustness issues with vertices which lie very close to line segments,
     * the following heuristic is used:
     * nodes are created if a vertex lies within a tolerance distance
     * of the interior of a segment.
     * The tolerance distance is chosen to be significantly below the snap-rounding grid size.
     * This has empirically proven to eliminate noding failures.
     *
     * @version 1.17
     */
    public class SnapRoundingIntersectionAdder : ISegmentIntersector
    {
        /**
   * The division factor used to determine
   * nearness distance tolerance for interior intersection detection.
   */
        private const int NearnessFactor = 100;

        private readonly LineIntersector _li;
        private readonly List<Coordinate> _intersections;
        private readonly PrecisionModel _precModel;
        private readonly double _nearnessTol;


        /**
     * Creates an intersector which finds all snapped interior intersections,
     * and adds them as nodes.
     *
     * @param pm the precision mode to use
     */
        public SnapRoundingIntersectionAdder(PrecisionModel pm)
        {
            _precModel = pm;
            /**
         * Nearness distance tolerance is a small fraction of the snap grid size
         */
            double snapGridSize = 1.0 / _precModel.Scale;
            _nearnessTol = snapGridSize / NearnessFactor;

            /**
         * Intersections are detected and computed using full precision.
         * They are snapped in a subsequent phase.
         */
            _li = new RobustLineIntersector();
            _intersections = new List<Coordinate>();
        }

        /**
     * Gets the created intersection nodes, 
     * so they can be processed as hot pixels.
     * 
     * @return a list of the intersection points
     */
        public List<Coordinate> Intersections { get => _intersections; }

        /**
     * This method is called by clients
     * of the {@link SegmentIntersector} class to process
     * intersections for two segments of the {@link SegmentString}s being intersected.
     * Note that some clients (such as <code>MonotoneChain</code>s) may optimize away
     * this call for segment pairs which they have determined do not intersect
     * (e.g. by an disjoint envelope test).
     */
        public void ProcessIntersections(
            ISegmentString e0, int segIndex0,
            ISegmentString e1, int segIndex1
        )
        {
            // don't bother intersecting a segment with itself
            if (e0 == e1 && segIndex0 == segIndex1) return;

            var p00 = e0.Coordinates[segIndex0];
            var p01 = e0.Coordinates[segIndex0 + 1];
            var p10 = e1.Coordinates[segIndex1];
            var p11 = e1.Coordinates[segIndex1 + 1];

            _li.ComputeIntersection(p00, p01, p10, p11);
            //if (li.hasIntersection() && li.isProper()) Debug.println(li);

            if (_li.HasIntersection)
            {
                if (_li.IsInteriorIntersection())
                {
                    for (int intIndex = 0; intIndex < _li.IntersectionNum; intIndex++)
                    {
                        _intersections.Add(_li.GetIntersection(intIndex));
                    }
                    ((NodedSegmentString)e0).AddIntersections(_li, segIndex0, 0);
                    ((NodedSegmentString)e1).AddIntersections(_li, segIndex1, 1);
                    return;
                }
            }

            /**
         * Segments did not actually intersect, within the limits of orientation index robustness.
         * 
         * To avoid certain robustness issues in snap-rounding, 
         * also treat very near vertex-segment situations as intersections.
         */
            ProcessNearVertex(p00, e1, segIndex1, p10, p11);
            ProcessNearVertex(p01, e1, segIndex1, p10, p11);
            ProcessNearVertex(p10, e0, segIndex0, p00, p01);
            ProcessNearVertex(p11, e0, segIndex0, p00, p01);
        }

        /**
     * If an endpoint of one segment is near 
     * the <i>interior</i> of the other segment, add it as an intersection.
     * EXCEPT if the endpoint is also close to a segment endpoint
     * (since this can introduce "zigs" in the linework).
     * <p>
     * This resolves situations where
     * a segment A endpoint is extremely close to another segment B,
     * but is not quite crossing.  Due to robustness issues
     * in orientation detection, this can 
     * result in the snapped segment A crossing segment B
     * without a node being introduced.
     * 
     * @param p
     * @param edge
     * @param segIndex
     * @param p0
     * @param p1
     */
        private void ProcessNearVertex(Coordinate p, ISegmentString edge, int segIndex, Coordinate p0, Coordinate p1)
        {

            /**
         * Don't add intersection if candidate vertex is near endpoints of segment.
         * This avoids creating "zig-zag" linework
         * (since the vertex could actually be outside the segment envelope).
         */
            if (p.Distance(p0) < _nearnessTol) return;
            if (p.Distance(p1) < _nearnessTol) return;

            double distSeg = DistanceComputer.PointToSegment(p, p0, p1);
            if (distSeg < _nearnessTol)
            {
                _intersections.Add(p);
                ((NodedSegmentString)edge).AddIntersection(p, segIndex);
            }
        }

        /**
     * Always process all intersections
     * 
     * @return false always
     */
        public bool IsDone { get => false; }

    }
}
