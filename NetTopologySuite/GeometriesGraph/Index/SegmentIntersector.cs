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
        private Boolean hasIntersection = false;
        private Boolean hasProper = false;
        private Boolean hasProperInterior = false;

        // the proper intersection point found
        private TCoordinate properIntersectionPoint;

        private readonly LineIntersector<TCoordinate> _lineIntersector;
        private readonly Boolean _includeProper;
        private readonly Boolean _recordIsolated;
        private Int32 _intersectionCount = 0;

        /// <summary>
        /// Testing only.
        /// </summary>
        public Int32 numTests = 0;

        private IEnumerable<Node<TCoordinate>> _boundaryNodes1;
        private IEnumerable<Node<TCoordinate>> _boundaryNodes2;

        public SegmentIntersector(LineIntersector<TCoordinate> li, Boolean includeProper, Boolean recordIsolated)
        {
            _lineIntersector = li;
            _includeProper = includeProper;
            _recordIsolated = recordIsolated;
        }

        public void SetBoundaryNodes(IEnumerable<Node<TCoordinate>> boundaryNodes0, IEnumerable<Node<TCoordinate>> boundaryNodes1)
        {
            _boundaryNodes1 = boundaryNodes0;
            _boundaryNodes2 = boundaryNodes1;
        }

        /// <returns> 
        /// The proper intersection point, or a default  
        /// <typeparamref name="TCoordinate"/> if none was found.
        /// </returns>
        public TCoordinate ProperIntersectionPoint
        {
            get { return properIntersectionPoint; }
        }

        public Boolean HasIntersection
        {
            get { return hasIntersection; }
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
            get { return hasProper; }
        }

        /// <summary> 
        /// A proper interior intersection is a proper intersection which is not
        /// contained in the set of boundary nodes set for this SegmentIntersector.
        /// </summary>
        public Boolean HasProperInteriorIntersection
        {
            get { return hasProperInterior; }
        }

        /// <summary>
        /// A trivial intersection is an apparent self-intersection which in fact
        /// is simply the point shared by adjacent line segments.
        /// Note that closed edges require a special check for the point shared by the beginning
        /// and end segments.
        /// </summary>
        private Boolean IsTrivialIntersection(Edge<TCoordinate> e0, Int32 segIndex0, Edge<TCoordinate> e1, Int32 segIndex1)
        {
            if (ReferenceEquals(e0, e1))
            {
                if (_lineIntersector.IntersectionType == LineIntersectionType.Intersects)
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

            numTests++;
            Pair<TCoordinate> edge0Coordinates = Slice.GetPairAt(e0.Coordinates, segIndex0);
            Pair<TCoordinate> edge1Coordinates = Slice.GetPairAt(e1.Coordinates, segIndex1);

            _lineIntersector.ComputeIntersection(
                edge0Coordinates.First, edge0Coordinates.Second,
                edge1Coordinates.First, edge1Coordinates.Second);

            /*
             *  Always record any non-proper intersections.
             *  If includeProper is true, record any proper intersections as well.
             */
            if (_lineIntersector.HasIntersection)
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
                if (!IsTrivialIntersection(e0, segIndex0, e1, segIndex1))
                {
                    hasIntersection = true;
                    if (_includeProper || !_lineIntersector.IsProper)
                    {
                        e0.AddIntersections(_lineIntersector, segIndex0, 0);
                        e1.AddIntersections(_lineIntersector, segIndex1, 1);
                    }
                    if (_lineIntersector.IsProper)
                    {
                        properIntersectionPoint = (TCoordinate)_lineIntersector.GetIntersection(0).Clone();
                        
                        hasProper = true;
                        
                        if (!isBoundaryPoint(_lineIntersector, _boundaryNodes1, _boundaryNodes2))
                        {
                            hasProperInterior = true;
                        }
                    }
                }
            }
        }

        private static Boolean isBoundaryPoint(LineIntersector<TCoordinate> li, params IEnumerable<Node<TCoordinate>>[] boundaryNodes)
        {
            if (boundaryNodes == null || boundaryNodes.Length < 2)
            {
                return false;
            }

            if (isBoundaryPoint(li, boundaryNodes[0]))
            {
                return true;
            }

            if (isBoundaryPoint(li, boundaryNodes[1]))
            {
                return true;
            }

            return false;
        }

        private static Boolean isBoundaryPoint(LineIntersector<TCoordinate> li, IEnumerable<Node<TCoordinate>> bdyNodes)
        {
            foreach (Node<TCoordinate> node in bdyNodes)
            {
                TCoordinate pt = node.Coordinate;

                if (li.IsIntersection(pt))
                {
                    return true;
                }
            }

            return false;
        }
    }
}