using System;
using GeoAPI.Coordinates;
using GisSharpBlog.NetTopologySuite.Algorithm;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    ///<summary>
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public class InteriorIntersectionFinder<TCoordinate> : ISegmentIntersector<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {

        private Boolean _isCheckEndSegmentsOnly;
        private readonly LineIntersector<TCoordinate> _li;
        private TCoordinate _interiorIntersection;
        private TCoordinate[] _intSegments;

        ///<summary>
        /// Creates an intersection finder which finds an interior intersection if one exists
        ///</summary>
        ///<param name="li">the LineIntersector to use</param>
        public InteriorIntersectionFinder(LineIntersector<TCoordinate> li)
        {
            _li = li;
            _interiorIntersection = default(TCoordinate);
        }

        ///<summary>
        /// Gets/Sets whether only end segments should be tested for interior intersection.
        /// This is a performance optimization that may be used if
        /// the segments have been previously noded by an appropriate algorithm.
        /// It may be known that any potential noding failures will occur only in
        /// end segments.
        ///</summary>
        public Boolean CheckEndSegmentsOnly
        {
            get { return _isCheckEndSegmentsOnly;}
            set { _isCheckEndSegmentsOnly = value;}
        }

        ///<summary>
        /// Tests whether an intersection was found.
        ///</summary>
        public Boolean HasIntersection
        {
            get
            {
                if (typeof(TCoordinate).IsValueType)
                    return !_interiorIntersection.Equals(default(TCoordinate));
                return _interiorIntersection != null;
            }
        }

        ///<summary>
        /// Gets the computed location of the intersection.
        /// Due to round-off, the location may not be exact.
        ///</summary>
        public TCoordinate InteriorIntersection
        {
            get { return _interiorIntersection; }
        }

        ///<summary>
        /// Gets the endpoints of the intersecting segments.
        ///</summary>
        public TCoordinate[] IntersectionSegments
        {
            get { return _intSegments; }
        }

        ///<summary>
        /// This method is called by clients of the <see cref="ISegmentIntersector{TCoordinate}"/> class to process
        /// intersections for two segments of the <see cref="ISegmentString{TCoordinate}"/>s being intersected.
        /// Note that some clients (such as <see cref="MonotoneChain{TCoordinate}"/>}s) may optimize away
        /// this call for segment pairs which they have determined do not intersect
        /// (e.g. by an disjoint envelope test).
        ///</summary>
        ///<param name="e0"></param>
        ///<param name="segIndex0"></param>
        ///<param name="e1"></param>
        ///<param name="segIndex1"></param>
        public void ProcessIntersections(
            ISegmentString<TCoordinate> e0, int segIndex0,
            ISegmentString<TCoordinate> e1, int segIndex1
            )
        {
            // short-circuit if intersection already found
            if (HasIntersection)
                return;

            // don't bother intersecting a segment with itself
            if (e0 == e1 && segIndex0 == segIndex1) return;

            /**
             * If enabled, only test end segments (on either segString).
             * 
             */
            if (_isCheckEndSegmentsOnly)
            {
                Boolean isEndSegPresent = IsEndSegment(e0, segIndex0) || IsEndSegment(e1, segIndex1);
                if (!isEndSegPresent)
                    return;
            }

            TCoordinate p00 = e0.Coordinates[segIndex0];
            TCoordinate p01 = e0.Coordinates[segIndex0 + 1];
            TCoordinate p10 = e1.Coordinates[segIndex1];
            TCoordinate p11 = e1.Coordinates[segIndex1 + 1];

            var res = _li.ComputeIntersection(p00, p01, p10, p11);
            if (res.HasIntersection)
            {
                if (res.IsInteriorIntersection())
                {
                    _intSegments = new TCoordinate[4];
                    _intSegments[0] = p00;
                    _intSegments[1] = p01;
                    _intSegments[2] = p10;
                    _intSegments[3] = p11;

                    _interiorIntersection = res.GetIntersectionPoint(0);
                }
            }
        }

        ///<summary>
        /// Tests whether a segment in a {@link SegmentString} is an end segment.
        /// (either the first or last).
        ///</summary>
        ///<param name="segStr">a segment string</param>
        ///<param name="index">the index of a segment in the segment string</param>
        ///<returns>true if the segment is an end segment</returns>
        private static Boolean IsEndSegment(ISegmentString<TCoordinate>  segStr, int index)
        {
            if (index == 0) return true;
            if (index >= segStr.Count - 2) return true;
            return false;
        }

        public Boolean IsDone
        {
            get
            {
                if ( typeof(TCoordinate).IsValueType)
                    return !_interiorIntersection.Equals(default(TCoordinate));
                return _interiorIntersection != null;
            }
        }

    }
}
