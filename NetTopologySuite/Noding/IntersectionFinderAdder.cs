using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Index.Chain;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Finds proper and interior intersections in a set of <see cref="SegmentString{TCoordinate}" />s,
    /// and adds them as nodes.
    /// </summary>
    public class IntersectionFinderAdder<TCoordinate> : ISegmentIntersector<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private readonly LineIntersector<TCoordinate> _li = null;
        private readonly List<TCoordinate> _interiorIntersections = new List<TCoordinate>();

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

        /// <summary>
        /// This method is called by clients
        /// of the <see cref="ISegmentIntersector{TCoordinate}" /> class to process
        /// intersections for two segments of the <see cref="SegmentString{TCoordinate}" />s 
        /// being intersected.
        /// Note that some clients (such as <see cref="MonotoneChain" />s) may optimize away
        /// this call for segment pairs which they have determined do not intersect
        /// (e.g. by an disjoint envelope test).
        /// </summary>
        public void ProcessIntersections(SegmentString<TCoordinate> e0, Int32 segIndex0, SegmentString<TCoordinate> e1, Int32 segIndex1)
        {
            // don't bother intersecting a segment with itself
            if (e0 == e1 && segIndex0 == segIndex1)
            {
                return;
            }

            Pair<TCoordinate> p0 = Slice.GetPairAt(e0.Coordinates, segIndex0);
            Pair<TCoordinate> p1 = Slice.GetPairAt(e1.Coordinates, segIndex1);

            _li.ComputeIntersection(p0.First, p0.Second, p1.First, p1.Second);

            if (_li.HasIntersection)
            {
                if (_li.IsInteriorIntersection())
                {
                    for (Int32 intIndex = 0; intIndex < (Int32)_li.IntersectionType; intIndex++)
                    {
                        _interiorIntersections.Add(_li.GetIntersection(intIndex));
                    }

                    e0.AddIntersections(_li, segIndex0, 0);
                    e1.AddIntersections(_li, segIndex1, 1);
                }
            }
        }
    }
}