using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Algorithm;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph.Index
{
    public class SegmentIntersector<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        public static Boolean IsAdjacentSegments(Int32 i1, Int32 i2)
        {
            return Math.Abs(i1 - i2) == 1;
        }

        /*
         * These variables keep track of what types of intersections were
         * found during ALL edges that have been intersected.
         */
        private Boolean _hasIntersection = false;
        private Boolean _hasProper = false;
        private Boolean _hasProperInterior = false;

        // the proper intersection point found
        private TCoordinate _properIntersectionPoint;

        private readonly LineIntersector<TCoordinate> _lineIntersector;
        private readonly Boolean _includeProper;
        private readonly Boolean _recordIsolated;
        private Int32 _intersectionCount = 0;

        /// <summary>
        /// Testing only.
        /// </summary>
        private Int32 _numTests = 0;

        private IEnumerable<Node<TCoordinate>> _boundaryNodes0;
        private IEnumerable<Node<TCoordinate>> _boundaryNodes1;

        public SegmentIntersector(LineIntersector<TCoordinate> li, Boolean includeProper, Boolean recordIsolated)
        {
            _lineIntersector = li;
            _includeProper = includeProper;
            _recordIsolated = recordIsolated;
        }

        public void SetBoundaryNodes(IEnumerable<Node<TCoordinate>> boundaryNodes0, IEnumerable<Node<TCoordinate>> boundaryNodes1)
        {
            _boundaryNodes0 = boundaryNodes0;
            _boundaryNodes1 = boundaryNodes1;
        }

        /// <returns> 
        /// The proper intersection point, or a default  
        /// <typeparamref name="TCoordinate"/> if none was found.
        /// </returns>
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
        /// contained in the set of boundary nodes set for this SegmentIntersector.
        /// </summary>
        public Boolean HasProperInteriorIntersection
        {
            get { return _hasProperInterior; }
        }

        /// <summary> 
        /// This method is called by clients of the EdgeIntersector class to test for and add
        /// intersections for two segments of the edges being intersected.
        /// Note that clients (such as MonotoneChainEdges) may choose not to intersect
        /// certain pairs of segments for efficiency reasons.
        /// </summary>
        public void AddIntersections(Edge<TCoordinate> e0, Int32 segIndex0, Edge<TCoordinate> e1, Int32 segIndex1)
        {
            // if (e0 == e1 && segIndex0 == segIndex1) 
            if (ReferenceEquals(e0, e1) && segIndex0 == segIndex1)
            {
                return;
                // Diego Guidi says: Avoid overload equality, 
                // i use references equality, otherwise TOPOLOGY ERROR!
            }

            _numTests++;
            Pair<TCoordinate> edge0Coordinates = Slice.GetPairAt(e0.Coordinates, segIndex0).Value;
            Pair<TCoordinate> edge1Coordinates = Slice.GetPairAt(e1.Coordinates, segIndex1).Value;

            Intersection<TCoordinate> intersection = _lineIntersector.ComputeIntersection(
                edge0Coordinates.First, edge0Coordinates.Second,
                edge1Coordinates.First, edge1Coordinates.Second);

            /*
             *  Always record any non-proper intersections.
             *  If includeProper is true, record any proper intersections as well.
             */
            if (intersection.HasIntersection)
            {
                if (_recordIsolated)
                {
                    e0.Isolated = false;
                    e1.Isolated = false;
                }

                _intersectionCount++;

                // if the segments are adjacent they have at least one trivial intersection,
                // the shared endpoint.  Don't bother adding it if it is the
                // only intersection.
                if (!isTrivialIntersection(intersection, e0, segIndex0, e1, segIndex1))
                {
                    _hasIntersection = true;

                    if (_includeProper || !intersection.IsProper)
                    {
                        e0.AddIntersections(intersection, segIndex0, 0);
                        e1.AddIntersections(intersection, segIndex1, 1);
                    }

                    if (intersection.IsProper)
                    {
                        _properIntersectionPoint = (TCoordinate)intersection.GetIntersectionPoint(0).Clone();
                        
                        _hasProper = true;

                        if (!isBoundaryPoint(intersection, _boundaryNodes0, _boundaryNodes1))
                        {
                            _hasProperInterior = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// A trivial intersection is an apparent self-intersection which in fact
        /// is simply the point shared by adjacent line segments.
        /// Note that closed edges require a special check for the point shared by the beginning
        /// and end segments.
        /// </summary>
        private static Boolean isTrivialIntersection(Intersection<TCoordinate> intersection, Edge<TCoordinate> e0, Int32 segIndex0, Edge<TCoordinate> e1, Int32 segIndex1)
        {
            if (ReferenceEquals(e0, e1))
            {
                if (intersection.IntersectionType == LineIntersectionType.Intersects)
                {
                    if (IsAdjacentSegments(segIndex0, segIndex1))
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

        private static Boolean isBoundaryPoint(Intersection<TCoordinate> Intersection, params IEnumerable<Node<TCoordinate>>[] boundaryNodes)
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

        private static Boolean isBoundaryPoint(Intersection<TCoordinate> intersection, IEnumerable<Node<TCoordinate>> bdyNodes)
        {
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
    }
}