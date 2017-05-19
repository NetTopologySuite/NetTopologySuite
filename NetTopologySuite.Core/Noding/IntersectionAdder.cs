using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Noding
{
    /// <summary>
    ///     Computes the possible intersections between two line segments in <see cref="ISegmentString" />s
    ///     and adds them to each string
    ///     using <see cref="NodedSegmentString.AddIntersection(NetTopologySuite.Algorithm.LineIntersector,int,int,int)" />.
    /// </summary>
    public class IntersectionAdder : ISegmentIntersector
    {
        /// <summary>
        /// </summary>
        public int NumInteriorIntersections;

        /**
         * These variables keep track of what types of intersections were
         * found during ALL edges that have been intersected.
         */

        // the proper intersection point found

        /// <summary>
        /// </summary>
        public int NumIntersections;

        /// <summary>
        /// </summary>
        public int NumProperIntersections;

        /// <summary>
        /// </summary>
        public int NumTests;

        /// <summary>
        ///     Initializes a new instance of the <see cref="IntersectionAdder" /> class.
        /// </summary>
        /// <param name="li"></param>
        public IntersectionAdder(LineIntersector li)
        {
            LineIntersector = li;
        }

        /// <summary>
        /// </summary>
        public LineIntersector LineIntersector { get; }

        /// <summary>
        ///     Returns the proper intersection point, or <c>null</c> if none was found.
        /// </summary>
        public Coordinate ProperIntersectionPoint { get; } = null;

        /// <summary>
        /// </summary>
        public bool HasIntersection { get; private set; }

        /// <summary>
        ///     A proper intersection is an intersection which is interior to at least two
        ///     line segments.  Note that a proper intersection is not necessarily
        ///     in the interior of the entire <see cref="Geometry" />, since another edge may have
        ///     an endpoint equal to the intersection, which according to SFS semantics
        ///     can result in the point being on the Boundary of the <see cref="Geometry" />.
        /// </summary>
        public bool HasProperIntersection { get; private set; }

        /// <summary>
        ///     A proper interior intersection is a proper intersection which is not
        ///     contained in the set of boundary nodes set for this <see cref="ISegmentIntersector" />.
        /// </summary>
        public bool HasProperInteriorIntersection { get; private set; }

        /// <summary>
        ///     An interior intersection is an intersection which is
        ///     in the interior of some segment.
        /// </summary>
        public bool HasInteriorIntersection { get; private set; }

        /// <summary>
        ///     This method is called by clients
        ///     of the <see cref="ISegmentIntersector" /> class to process
        ///     intersections for two segments of the <see cref="ISegmentString" /> being intersected.<br />
        ///     Note that some clients (such as <c>MonotoneChain</c>") may optimize away
        ///     this call for segment pairs which they have determined do not intersect
        ///     (e.g. by an disjoint envelope test).
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="segIndex0"></param>
        /// <param name="e1"></param>
        /// <param name="segIndex1"></param>
        public void ProcessIntersections(ISegmentString e0, int segIndex0, ISegmentString e1, int segIndex1)
        {
            if ((e0 == e1) && (segIndex0 == segIndex1))
                return;

            NumTests++;
            var coordinates0 = e0.Coordinates;
            var p00 = coordinates0[segIndex0];
            var p01 = coordinates0[segIndex0 + 1];
            var coordinates1 = e1.Coordinates;
            var p10 = coordinates1[segIndex1];
            var p11 = coordinates1[segIndex1 + 1];

            LineIntersector.ComputeIntersection(p00, p01, p10, p11);
            if (LineIntersector.HasIntersection)
            {
                NumIntersections++;
                if (LineIntersector.IsInteriorIntersection())
                {
                    NumInteriorIntersections++;
                    HasInteriorIntersection = true;
                }
                // if the segments are adjacent they have at least one trivial intersection,
                // the shared endpoint.  Don't bother adding it if it is the
                // only intersection.
                if (!IsTrivialIntersection(e0, segIndex0, e1, segIndex1))
                {
                    HasIntersection = true;
                    ((NodedSegmentString) e0).AddIntersections(LineIntersector, segIndex0, 0);
                    ((NodedSegmentString) e1).AddIntersections(LineIntersector, segIndex1, 1);
                    if (LineIntersector.IsProper)
                    {
                        NumProperIntersections++;
                        HasProperIntersection = true;
                        HasProperInteriorIntersection = true;
                    }
                }
            }
        }

        /// <summary>
        ///     Always process all intersections
        /// </summary>
        public bool IsDone => false;

        /// <summary>
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        /// <returns></returns>
        public static bool IsAdjacentSegments(int i1, int i2)
        {
            return Math.Abs(i1 - i2) == 1;
        }

        /// <summary>
        ///     A trivial intersection is an apparent self-intersection which in fact
        ///     is simply the point shared by adjacent line segments.
        ///     Note that closed edges require a special check for the point shared by the beginning and end segments.
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="segIndex0"></param>
        /// <param name="e1"></param>
        /// <param name="segIndex1"></param>
        /// <returns></returns>
        private bool IsTrivialIntersection(ISegmentString e0, int segIndex0, ISegmentString e1, int segIndex1)
        {
            if (e0 == e1)
                if (LineIntersector.IntersectionNum == 1)
                {
                    if (IsAdjacentSegments(segIndex0, segIndex1))
                        return true;
                    if (e0.IsClosed)
                    {
                        var maxSegIndex = e0.Count - 1;
                        if (((segIndex0 == 0) && (segIndex1 == maxSegIndex)) ||
                            ((segIndex1 == 0) && (segIndex0 == maxSegIndex)))
                            return true;
                    }
                }
            return false;
        }
    }
}