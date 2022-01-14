using System;
using System.Collections.ObjectModel;

using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// Finds intersections between line segments which will be snap-rounded,
    /// and adds them as nodes to the segments.
    /// <para/>
    /// Intersections are detected and computed using full precision.
    /// Snapping takes place in a subsequent phase.
    /// <para/>
    /// The intersection points are recorded, so that HotPixels can be created for them.
    /// <para/>
    /// To avoid robustness issues with vertices which lie very close to line segments
    /// a heuristic is used:
    /// nodes are created if a vertex lies within a tolerance distance
    /// of the interior of a segment.
    /// The tolerance distance is chosen to be significantly below the snap-rounding grid size.
    /// This has empirically proven to eliminate noding failures.
    /// </summary>
    public sealed class SnapRoundingIntersectionAdder : ISegmentIntersector
    {

        private readonly LineIntersector _li;
        private readonly double _nearnessTol;

        /// <summary>
        /// Creates an intersector which finds all snapped interior intersections,
        /// and adds them as nodes.
        /// </summary>
        /// <param name="pm">The precision model to use</param>
        [Obsolete]
        public SnapRoundingIntersectionAdder(PrecisionModel pm)
            :this(CalculateNearnessTol(pm))
        {
        }

        /// <summary>
        /// Creates an intersector which finds all snapped interior intersections,
        /// and adds them as nodes.
        /// </summary>
        /// <param name="nearnessTol">the intersection distance tolerance</param>
        public SnapRoundingIntersectionAdder(double nearnessTol)
        {
            _nearnessTol = nearnessTol;

            /*
             * Intersections are detected and computed using full precision.
             * They are snapped in a subsequent phase.
             */
            _li = new RobustLineIntersector();
            Intersections = new Collection<Coordinate>();
        }

        /// <summary>
        /// Gets the created intersection nodes,
        /// so they can be processed as hot pixels.
        /// </summary>
        /// <returns>A list of intersection points</returns>
        public Collection<Coordinate> Intersections { get; }

        /// <summary>
        /// This method is called by clients
        /// of the <see cref="ISegmentIntersector"/> class to process
        /// intersections for two segments of the <see cref="ISegmentString"/>
        /// s being intersected.
        /// Note that some clients (such as <c>MonotoneChain</c> s) may optimize away
        /// this call for segment pairs which they have determined do not intersect
        /// (e.g.by an disjoint envelope test).
        /// </summary>
        public void ProcessIntersections(
            ISegmentString e0, int segIndex0,
            ISegmentString e1, int segIndex1
        )
        {
            // don't bother intersecting a segment with itself
            if (e0 == e1 && segIndex0 == segIndex1) return;

            var coordinates = e0.Coordinates;
            var p00 = coordinates[segIndex0];
            var p01 = coordinates[segIndex0 + 1];
            coordinates = e1.Coordinates;
            var p10 = coordinates[segIndex1];
            var p11 = coordinates[segIndex1 + 1];

            _li.ComputeIntersection(p00, p01, p10, p11);
            //if (li.hasIntersection() && li.isProper()) Debug.println(li);

            if (_li.HasIntersection)
            {
                if (_li.IsInteriorIntersection())
                {
                    for (int intIndex = 0; intIndex < _li.IntersectionNum; intIndex++)
                    {
                        Intersections.Add(_li.GetIntersection(intIndex));
                    }
                    ((NodedSegmentString)e0).AddIntersections(_li, segIndex0, 0);
                    ((NodedSegmentString)e1).AddIntersections(_li, segIndex1, 1);
                    return;
                }
            }

            /*
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

        /// <summary>
        /// If an endpoint of one segment is near
        /// the <i>interior</i> of the other segment, add it as an intersection.
        /// EXCEPT if the endpoint is also close to a segment endpoint
        /// (since this can introduce "zigs" in the linework).
        /// <para/>
        /// This resolves situations where
        /// a segment A endpoint is extremely close to another segment B,
        /// but is not quite crossing.Due to robustness issues
        /// in orientation detection, this can
        /// result in the snapped segment A crossing segment B
        /// without a node being introduced.
        /// </summary>
        private void ProcessNearVertex(Coordinate p, ISegmentString edge, int segIndex, Coordinate p0, Coordinate p1)
        {
            /*
             * Don't add intersection if candidate vertex is near endpoints of segment.
             * This avoids creating "zig-zag" linework
             * (since the vertex could actually be outside the segment envelope).
             */
            if (p.Distance(p0) < _nearnessTol) return;
            if (p.Distance(p1) < _nearnessTol) return;

            double distSeg = DistanceComputer.PointToSegment(p, p0, p1);
            if (distSeg < _nearnessTol)
            {
                Intersections.Add(p);
                ((NodedSegmentString)edge).AddIntersection(p, segIndex);
            }
        }

        /// <summary>
        /// Always process all intersections
        /// </summary>
        /// <returns>Always <c>false</c></returns>
        public bool IsDone { get => false; }


        [Obsolete]
        private static double CalculateNearnessTol(PrecisionModel precModel)
        {
            const double NEARNESS_FACTOR = 100d;
            /*
             * Nearness distance tolerance is a small fraction of the snap grid size
             */
            double snapGridSize = 1.0 / precModel.Scale;
            return snapGridSize / NEARNESS_FACTOR;
        }
    }
}
