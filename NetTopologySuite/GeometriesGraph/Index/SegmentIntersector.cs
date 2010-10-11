using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Noding;
using NPack.Interfaces;

namespace NetTopologySuite.GeometriesGraph.Index
{
    /// <summary>
    /// Computes the intersections of segments of <see cref="Edge{TCoordinate}"/>s.
    /// </summary>
    /// <typeparam name="TCoordinate">The coordinate type.</typeparam>
    public class SegmentIntersector<TCoordinate> : ISegmentIntersector<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        /*
         * These variables keep track of what types of intersections were
         * found during ALL edges that have been intersected.
         */
        private readonly Boolean _includeProper;
        private readonly LineIntersector<TCoordinate> _lineIntersector;
        private readonly Boolean _recordIsolated;
        private IEnumerable<Node<TCoordinate>> _boundaryNodes0;
        private IEnumerable<Node<TCoordinate>> _boundaryNodes1;
        private Boolean _hasIntersection;
        private Boolean _hasProper;
        private Boolean _hasProperInterior;

        // the proper intersection point found
        private Int32 _intersectionCount;

        // Testing only.
        private Int32 _numTests;
        private TCoordinate _properIntersectionPoint;

        /// <summary>
        /// Creates a new <see cref="SegmentIntersector{TCoordinate}"/>
        /// with the given <see cref="LineIntersector{TCoordinate}"/>
        /// and values indicating whether proper intersections should be included
        /// in the edges and if edges should be marked as not isolated 
        /// when adding intersections.
        /// </summary>
        /// <param name="li">
        /// The <see cref="LineIntersector{TCoordinate}"/> to use to compute 
        /// intersections.
        /// </param>
        /// <param name="includeProper">
        /// A flag indicating whether proper intersections should be included
        /// in the edges' intersection lists.
        /// </param>
        /// <param name="recordIsolated">
        /// A flag indicating whether edges should be marked as not isolated
        /// when adding intersections.
        /// </param>
        public SegmentIntersector(LineIntersector<TCoordinate> li,
                                  Boolean includeProper,
                                  Boolean recordIsolated)
        {
            _lineIntersector = li;
            _includeProper = includeProper;
            _recordIsolated = recordIsolated;
        }

        /// <summary> 
        /// Gets the proper intersection point, or an empty
        /// <typeparamref name="TCoordinate"/> if none was found.
        /// </summary>
        public TCoordinate ProperIntersectionPoint
        {
            get { return _properIntersectionPoint; }
        }

        public Boolean HasIntersection
        {
            get { return _hasIntersection; }
        }

        /// <summary>
        /// Gets whether the line segments have a proper intersection.
        /// </summary>
        /// <remarks>
        /// A proper intersection is an intersection which is interior to at least two
        /// line segments.  Note that a proper intersection is not necessarily
        /// in the interior of the entire Geometry, since another edge may have
        /// an endpoint equal to the intersection, which according to SFS semantics
        /// can result in the point being on the Boundary of the Geometry.
        /// </remarks>
        public Boolean HasProperIntersection
        {
            get { return _hasProper; }
        }

        /// <summary> 
        /// A proper interior intersection is a proper intersection which is not
        /// contained in the set of boundary nodes set for this 
        /// <see cref="SegmentIntersector{TCoordinate}"/>.
        /// </summary>
        public Boolean HasProperInteriorIntersection
        {
            get { return _hasProperInterior; }
        }

        /// <summary>
        /// Computes if two segments, referred to by their indexes, 
        /// are sequential in either direction.
        /// </summary>
        /// <param name="i1">The index of the first segment to test.</param>
        /// <param name="i2">The index of the second segment to test.</param>
        /// <returns>
        /// <see langword="true"/> if the indexes differ by 1 or -1.
        /// </returns>
        public static Boolean AreAdjacentSegments(Int32 i1, Int32 i2)
        {
            return Math.Abs(i1 - i2) == 1;
        }

        public void SetBoundaryNodes(IEnumerable<Node<TCoordinate>> boundaryNodes0,
                                     IEnumerable<Node<TCoordinate>> boundaryNodes1)
        {
            _boundaryNodes0 = boundaryNodes0;
            _boundaryNodes1 = boundaryNodes1;
        }

        /// <summary> 
        /// This method is called by clients of the 
        /// <see cref="SegmentIntersector{TCoordinate}"/> class 
        /// to test for and add intersections for two segments of the edges 
        /// being intersected.
        /// Note that clients (such as MonotoneChainEdges) may choose not to intersect
        /// certain pairs of segments for efficiency reasons.
        /// </summary>
        public void AddIntersections(Edge<TCoordinate> e0, Int32 segIndex0,
                                     Edge<TCoordinate> e1, Int32 segIndex1)
        {
            // Need to check if this is the same edge and index, not if the edges
            // are equal, so ReferenceEquals() is used.
            if (ReferenceEquals(e0, e1) && segIndex0 == segIndex1)
            {
                return;
            }

            _numTests++;

            // get segments from edge coordinates
            Pair<TCoordinate> edge0Segment = e0.Coordinates.SegmentAt(segIndex0);
            Pair<TCoordinate> edge1Segment = e1.Coordinates.SegmentAt(segIndex1);

            // compute intersection
            Intersection<TCoordinate> intersection
                = _lineIntersector.ComputeIntersection(edge0Segment, edge1Segment);

            // If there is no intersection, we're done.
            if (!intersection.HasIntersection)
            {
                return;
            }

            /*
             *  Always record any non-proper intersections.
             *  If includeProper is true, record any proper intersections as well.
             */
            if (_recordIsolated)
            {
                e0.Isolated = false;
                e1.Isolated = false;
            }

            _intersectionCount++;

            // if the segments are adjacent they have at least one trivial 
            // intersection: the shared endpoint.  Don't bother adding it 
            // if it is the only intersection.
            if (isTrivialIntersection(intersection, e0, segIndex0, e1, segIndex1))
            {
                return;
            }

            _hasIntersection = true;

            if (_includeProper || !intersection.IsProper)
            {
                e0.AddIntersections(intersection, segIndex0, 0);
                e1.AddIntersections(intersection, segIndex1, 1);
            }

            if (intersection.IsProper)
            {
                //_properIntersectionPoint
                //    = (TCoordinate)intersection.GetIntersectionPoint(0).Clone();

                // this works fine when TCoordinate is a value type.... but
                // what if it isn't?
                // TODO: evaluate a struct constraint on TCoordinate
                _properIntersectionPoint = intersection.GetIntersectionPoint(0);

                _hasProper = true;

                if (!isBoundaryPoint(intersection, _boundaryNodes0, _boundaryNodes1))
                {
                    _hasProperInterior = true;
                }
            }
        }

        // A trivial intersection is an apparent self-intersection which in fact
        // is simply the point shared by adjacent line segments.
        // Note that closed edges require a special check for the point 
        // shared by the beginning and end segments.
        private static Boolean isTrivialIntersection(Intersection<TCoordinate> intersection,
                                                     Edge<TCoordinate> e0,
                                                     Int32 segIndex0,
                                                     Edge<TCoordinate> e1,
                                                     Int32 segIndex1)
        {
            if (ReferenceEquals(e0, e1))
            {
                if (intersection.IntersectionDegree == LineIntersectionDegrees.Intersects)
                {
                    if (AreAdjacentSegments(segIndex0, segIndex1))
                    {
                        return true;
                    }

                    if (e0.IsClosed)
                    {
                        Int32 maxSegIndex = e0.PointCount - 1;

                        if ((segIndex0 == 0 && segIndex1 == maxSegIndex) ||
                            (segIndex1 == 0 && segIndex0 == maxSegIndex))
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static Boolean isBoundaryPoint(Intersection<TCoordinate> Intersection,
                                               params IEnumerable<Node<TCoordinate>>[] boundaryNodes)
        {
            if (boundaryNodes == null || boundaryNodes.Length < 2)
            {
                return false;
            }

            if (isBoundaryPoint(Intersection, boundaryNodes[0]))
            {
                return true;
            }

            if (isBoundaryPoint(Intersection, boundaryNodes[1]))
            {
                return true;
            }

            return false;
        }

        private static Boolean isBoundaryPoint(Intersection<TCoordinate> intersection,
                                               IEnumerable<Node<TCoordinate>> bdyNodes)
        {
            if (bdyNodes == null)
                return false;

            foreach (Node<TCoordinate> node in bdyNodes)
            {
                TCoordinate pt = node.Coordinate;

                if (intersection.IsIntersection(pt))
                {
                    return true;
                }
            }

            return false;
        }

        #region ISegmentIntersector<TCoordinate> Member

        public void ProcessIntersections(ISegmentString<TCoordinate> e0, int segIndex0,
                                         ISegmentString<TCoordinate> e1, int segIndex1)
        {
            //nothing to do here since all
        }

        public bool IsDone
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}