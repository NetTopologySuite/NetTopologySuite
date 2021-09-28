using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.GeometriesGraph.Index
{
    /// <summary>
    /// Computes the intersection of line segments,
    /// and adds the intersection to the edges containing the segments.
    /// </summary>
    public class SegmentIntersector
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="i1"></param>
        /// <param name="i2"></param>
        /// <returns></returns>
        public static bool IsAdjacentSegments(int i1, int i2)
        {
            return Math.Abs(i1 - i2) == 1;
        }

        /*
         * These variables keep track of what types of intersections were
         * found during ALL edges that have been intersected.
         */
        private bool _hasIntersection;
        private bool _hasProper;
        private bool _hasProperInterior;

        // the proper intersection point found
        private Coordinate _properIntersectionPoint;

        private readonly LineIntersector _li;
        private readonly bool _includeProper;
        private readonly bool _recordIsolated;
        private int _numIntersections;

        /// <summary>
        /// Testing only.
        /// </summary>
        public int NumTests;

        private IList<Node>[] _bdyNodes;
        private bool _isDone;
        private bool _isDoneWhenProperInt;

        /// <summary>
        ///
        /// </summary>
        /// <param name="li"></param>
        /// <param name="includeProper"></param>
        /// <param name="recordIsolated"></param>
        public SegmentIntersector(LineIntersector li, bool includeProper, bool recordIsolated)
        {
            _li = li;
            _includeProper = includeProper;
            _recordIsolated = recordIsolated;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="bdyNodes0"></param>
        /// <param name="bdyNodes1"></param>
        public void SetBoundaryNodes(IList<Node> bdyNodes0, IList<Node> bdyNodes1)
        {
            _bdyNodes = new IList<Node>[2];
            _bdyNodes[0] = bdyNodes0;
            _bdyNodes[1] = bdyNodes1;
        }

        public bool IsDoneIfProperInt
        {
            set => _isDoneWhenProperInt = value;
        }

        public bool IsDone => _isDone;

        /// <returns>
        /// The proper intersection point, or <c>null</c> if none was found.
        /// </returns>
        public Coordinate ProperIntersectionPoint => _properIntersectionPoint;

        /// <summary>
        ///
        /// </summary>
        public bool HasIntersection => _hasIntersection;

        /// <summary>
        /// A proper intersection is an intersection which is interior to at least two
        /// line segments.  Note that a proper intersection is not necessarily
        /// in the interior of the entire Geometry, since another edge may have
        /// an endpoint equal to the intersection, which according to SFS semantics
        /// can result in the point being on the Boundary of the Geometry.
        /// </summary>
        /// <returns>Indicates a proper intersection with an interior to at least two line segments</returns>
        public bool HasProperIntersection => _hasProper;

        /// <summary>
        /// A proper interior intersection is a proper intersection which is not
        /// contained in the set of boundary nodes set for this SegmentIntersector.
        /// </summary>
        /// <returns>Indicates a proper interior intersection</returns>
        public bool HasProperInteriorIntersection => _hasProperInterior;

        /// <summary>
        /// A trivial intersection is an apparent self-intersection which in fact
        /// is simply the point shared by adjacent line segments.
        /// Note that closed edges require a special check for the point shared by the beginning
        /// and end segments.
        /// </summary>
        /// <param name="e0">An Edge</param>
        /// <param name="segIndex0">The segment index of <paramref name="e0"/></param>
        /// <param name="e1">Another Edge</param>
        /// <param name="segIndex1">The segment index of <paramref name="e1"/></param>
        private bool IsTrivialIntersection(Edge e0, int segIndex0, Edge e1, int segIndex1)
        {
            if (ReferenceEquals(e0, e1))
            {
                if (_li.IntersectionNum == 1)
                {
                    if (IsAdjacentSegments(segIndex0, segIndex1))
                        return true;
                    if (e0.IsClosed)
                    {
                        int maxSegIndex = e0.NumPoints - 1;
                        if ((segIndex0 == 0 && segIndex1 == maxSegIndex) ||
                            (segIndex1 == 0 && segIndex0 == maxSegIndex))
                                return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// This method is called by clients of the EdgeIntersector class to test for and add
        /// intersections for two segments of the edges being intersected.
        /// Note that clients (such as MonotoneChainEdges) may choose not to intersect
        /// certain pairs of segments for efficiency reasons.
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="segIndex0"></param>
        /// <param name="e1"></param>
        /// <param name="segIndex1"></param>
        public void AddIntersections(Edge e0, int segIndex0, Edge e1, int segIndex1)
        {
            // if (e0 == e1 && segIndex0 == segIndex1)
            if (ReferenceEquals(e0, e1) && segIndex0 == segIndex1)
                return;             // Diego Guidi say's: Avoid overload equality, i use references equality, otherwise TOPOLOGY ERROR!

            NumTests++;
            var coordinates = e0.Coordinates;
            var p00 = coordinates[segIndex0];
            var p01 = coordinates[segIndex0 + 1];
            coordinates = e1.Coordinates;
            var p10 = coordinates[segIndex1];
            var p11 = coordinates[segIndex1 + 1];
            _li.ComputeIntersection(p00, p01, p10, p11);
            /*
             *  Always record any non-proper intersections.
             *  If includeProper is true, record any proper intersections as well.
             */
            if (_li.HasIntersection)
            {
                if (_recordIsolated)
                {
                    e0.Isolated = false;
                    e1.Isolated = false;
                }
                _numIntersections++;
                // if the segments are adjacent they have at least one trivial intersection,
                // the shared endpoint.  Don't bother adding it if it is the
                // only intersection.
                if (!IsTrivialIntersection(e0, segIndex0, e1, segIndex1))
                {
                    _hasIntersection = true;
                    if (_includeProper || !_li.IsProper)
                    {
                        e0.AddIntersections(_li, segIndex0, 0);
                        e1.AddIntersections(_li, segIndex1, 1);
                    }
                    if (_li.IsProper)
                    {
                        _properIntersectionPoint = (Coordinate) _li.GetIntersection(0).Copy();
                        _hasProper = true;
                        if (_isDoneWhenProperInt) _isDone = true;
                        if (!IsBoundaryPoint(_li, _bdyNodes))
                            _hasProperInterior = true;
                    }
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="li"></param>
        /// <param name="bdyNodes"></param>
        /// <returns></returns>
        private static bool IsBoundaryPoint(LineIntersector li, IList<Node>[] bdyNodes)
        {
            if (bdyNodes == null)
                return false;
            if (IsBoundaryPointInternal(li, bdyNodes[0]))
                return true;
            if (IsBoundaryPointInternal(li, bdyNodes[1]))
                return true;
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="li"></param>
        /// <param name="bdyNodes"></param>
        /// <returns></returns>
        private static bool IsBoundaryPointInternal(LineIntersector li, IEnumerable<Node> bdyNodes)
        {
            foreach (var node in bdyNodes)
            {
                var pt = node.Coordinate;
                if (li.IsIntersection(pt))
                    return true;
            }
            return false;
        }
    }
}
