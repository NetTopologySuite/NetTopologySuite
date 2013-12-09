using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Noding
{
    ///<summary>
    ///</summary>
    public class InteriorIntersectionFinder : ISegmentIntersector
    {
        private readonly LineIntersector _li;
        private Coordinate _interiorIntersection;
        private Coordinate[] _intSegments;
        private readonly List<Coordinate> _intersections = new List<Coordinate>();

        ///<summary>
        /// Creates an intersection finder which finds an interior intersection if one exists
        ///</summary>
        ///<param name="li">the LineIntersector to use</param>
        public InteriorIntersectionFinder(LineIntersector li)
        {
            _li = li;
            _interiorIntersection = null;
        }

        /// <summary>
        /// Gets or sets whether all intersections should be found
        /// </summary>
        public bool FindAllIntersections { get; set; }

        ///<summary>
        /// Gets/Sets whether only end segments should be tested for interior intersection.
        /// This is a performance optimization that may be used if
        /// the segments have been previously noded by an appropriate algorithm.
        /// It may be known that any potential noding failures will occur only in
        /// end segments.
        ///</summary>
        public bool CheckEndSegmentsOnly { get; set; }

        ///<summary>
        /// Tests whether an intersection was found.
        ///</summary>
        public Boolean HasIntersection
        {
            get
            {
                return _interiorIntersection != null;
            }
        }

        /// <summary>
        /// Gets the list of intersection points
        /// </summary>
        public IList<Coordinate> Intersections
        {
            get
            {
                return new ReadOnlyCollection<Coordinate>(_intersections);
            }
        }

        ///<summary>
        /// Gets the computed location of the intersection.
        /// Due to round-off, the location may not be exact.
        ///</summary>
        public Coordinate InteriorIntersection
        {
            get { return _interiorIntersection; }
        }

        ///<summary>
        /// Gets the endpoints of the intersecting segments.
        ///</summary>
        public Coordinate[] IntersectionSegments
        {
            get { return _intSegments; }
        }

        ///<summary>
        /// This method is called by clients of the <see cref="ISegmentIntersector"/> class to process
        /// intersections for two segments of the <see cref="ISegmentString"/>s being intersected.<br/>
        /// Note that some clients (such as <c>MonotoneChain</c>s) may optimize away
        /// this call for segment pairs which they have determined do not intersect
        /// (e.g. by an disjoint envelope test).
        ///</summary>
        ///<param name="e0"></param>
        ///<param name="segIndex0"></param>
        ///<param name="e1"></param>
        ///<param name="segIndex1"></param>
        public void ProcessIntersections(
            ISegmentString e0, int segIndex0,
            ISegmentString e1, int segIndex1
            )
        {
            // short-circuit if intersection already found
            if (HasIntersection)
                return;

            // don't bother intersecting a segment with itself
            if (e0 == e1 && segIndex0 == segIndex1) return;

            /*
             * If enabled, only test end segments (on either segString).
             *
             */
            if (CheckEndSegmentsOnly)
            {
                Boolean isEndSegPresent = IsEndSegment(e0, segIndex0) || IsEndSegment(e1, segIndex1);
                if (!isEndSegPresent)
                    return;
            }

            Coordinate p00 = e0.Coordinates[segIndex0];
            Coordinate p01 = e0.Coordinates[segIndex0 + 1];
            Coordinate p10 = e1.Coordinates[segIndex1];
            Coordinate p11 = e1.Coordinates[segIndex1 + 1];

            /*var res = */
            _li.ComputeIntersection(p00, p01, p10, p11);
            if (_li.HasIntersection /*res.HasIntersection*/)
            {
                if (_li.IsInteriorIntersection() /* res.IsInteriorIntersection()*/)
                {
                    _intSegments = new Coordinate[4];
                    _intSegments[0] = p00;
                    _intSegments[1] = p01;
                    _intSegments[2] = p10;
                    _intSegments[3] = p11;

                    _interiorIntersection = _li.GetIntersection(0) /*res.GetIntersectionPoint(0)*/;
                    _intersections.Add(_interiorIntersection);
                }
            }
        }

        ///<summary>
        /// Tests whether a segment in a <see cref="ISegmentString" /> is an end segment.
        /// (either the first or last).
        ///</summary>
        ///<param name="segStr">a segment string</param>
        ///<param name="index">the index of a segment in the segment string</param>
        ///<returns>true if the segment is an end segment</returns>
        private static Boolean IsEndSegment(ISegmentString segStr, int index)
        {
            if (index == 0) return true;
            if (index >= segStr.Count - 2) return true;
            return false;
        }

        public Boolean IsDone
        {
            get
            {
                if (FindAllIntersections) return false;
                return _interiorIntersection != null;
            }
        }
    }
}