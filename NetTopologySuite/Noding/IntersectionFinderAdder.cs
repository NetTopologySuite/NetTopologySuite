using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Index.Chain;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Finds proper and interior intersections in a set of 
    /// <see cref="NodedSegmentString{TCoordinate}" />s,
    /// and adds them as nodes.
    /// </summary>
    public class IntersectionFinderAdder<TCoordinate> : ISegmentIntersector<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly List<TCoordinate> _interiorIntersections = new List<TCoordinate>();
        private readonly LineIntersector<TCoordinate> _li;

        /// <summary>
        /// Creates an intersection finder which finds all proper intersections.
        /// </summary>
        /// <param name="li">The <see cref="LineIntersector{TCoordinate}" /> to use.</param>
        public IntersectionFinderAdder(LineIntersector<TCoordinate> li)
        {
            _li = li;
        }

        public IList<TCoordinate> InteriorIntersections
        {
            get { return _interiorIntersections; }
        }

        #region ISegmentIntersector<TCoordinate> Members

        /// <summary>
        /// This method is called by clients
        /// of the <see cref="ISegmentIntersector{TCoordinate}" /> class to process
        /// intersections for two segments of the <see cref="NodedSegmentString{TCoordinate}" />s 
        /// being intersected.
        /// </summary>
        /// <remarks>
        /// Note that some clients (such as <see cref="MonotoneChain{TCoordinate}" />s) 
        /// may optimize away this call for segment pairs which they have determined 
        /// do not intersect (e.g. by an disjoint envelope test).
        /// </remarks>
        public void ProcessIntersections(ISegmentString<TCoordinate> e0, Int32 segIndex0,
                                         ISegmentString<TCoordinate> e1, Int32 segIndex1)
        {
            // don't bother intersecting a segment with itself
            if (e0 == e1 && segIndex0 == segIndex1)
            {
                return;
            }

            LineSegment<TCoordinate> p0 = e0[segIndex0];
            LineSegment<TCoordinate> p1 = e1[segIndex1];

            Intersection<TCoordinate> intersection = _li.ComputeIntersection(p0, p1);

            if (intersection.HasIntersection)
            {
                if (intersection.IsInteriorIntersection())
                {
                    Int32 intersectionCount = (Int32) intersection.IntersectionDegree;

                    for (Int32 intersectionIndex = 0; intersectionIndex < intersectionCount; intersectionIndex++)
                    {
                        TCoordinate intersectionPoint = intersection.GetIntersectionPoint(intersectionIndex);
                        _interiorIntersections.Add(intersectionPoint);
                    }

                    ((NodedSegmentString<TCoordinate>)e0).AddIntersections(intersection, segIndex0, 0);
                    ((NodedSegmentString<TCoordinate>)e1).AddIntersections(intersection, segIndex1, 1);
                }
            }
        }

        #endregion

        #region ISegmentIntersector<TCoordinate> Member

        public bool IsDone
        {
            get { return false; }
        }

        #endregion
    }
}