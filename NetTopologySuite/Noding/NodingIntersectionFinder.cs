using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;

namespace NetTopologySuite.Noding
{
    ///<summary>
    ///</summary>
    public class NodingIntersectionFinder : ISegmentIntersector
    {
        ///<summary>
        /// Creates an intersection finder which tests if there is at least one interior intersection.
        /// Uses short-circuiting for efficient performance.
        /// The intersection found is recorded.
        ///</summary>
        /// <param name="li">A line intersector.</param>
        /// <returns>A intersection finder which tests if there is at least one interior intersection.</returns>
        public static NodingIntersectionFinder CreateAnyIntersectionFinder(LineIntersector li)
        {
            return new NodingIntersectionFinder(li);
        }

        ///<summary>
        /// Creates an intersection finder which finds all interior intersections.
        /// The intersections are recorded for later inspection.
        ///</summary>
        /// <param name="li">A line intersector.</param>
        /// <returns>a intersection finder which finds all interior intersections.</returns>
        public static NodingIntersectionFinder CreateAllIntersectionsFinder(LineIntersector li)
        {
            var finder = new NodingIntersectionFinder(li);
            finder.FindAllIntersections = true;
            return finder;
        }

        ///<summary>
        /// Creates an intersection finder which counts all interior intersections.
        /// The intersections are note recorded to reduce memory usage.
        ///</summary>
        /// <param name="li">A line intersector.</param>
        /// <returns>a intersection finder which counts all interior intersections.</returns>
        public static NodingIntersectionFinder CreateIntersectionCounter(LineIntersector li)
        {
            var finder = new NodingIntersectionFinder(li);
            finder.FindAllIntersections = true;
            finder.KeepIntersections = false;
            return finder;
        }

        private readonly LineIntersector _li;
        private Coordinate _interiorIntersection;
        private Coordinate[] _intSegments;
        private readonly List<Coordinate> _intersections = new List<Coordinate>();

        private int intersectionCount = 0;
        private bool _keepIntersections = true;

        ///<summary>
        /// Creates an intersection finder which finds an interior intersection if one exists
        ///</summary>
        ///<param name="li">the LineIntersector to use</param>
        public NodingIntersectionFinder(LineIntersector li)
        {
            _li = li;
            _interiorIntersection = null;
        }

        /// <summary>
        /// Gets/Sets whether all intersections should be computed.
        /// <remarks>
        /// When this is <c>false</c> (the default value), the value of <see cref="IsDone"/>
        /// is <c>true</c> after the first intersection is found.
        /// Default is <c>false</c>.
        /// </remarks>
        /// </summary>
        public bool FindAllIntersections { get; set; }

        /// <summary>
        /// Gets/Sets whether intersection points are recorded.
        /// <remarks>
        /// If the only need is to count intersection points, this can be set to <c>false</c>.
        /// Default is <c>true</c>.
        /// </remarks>
        /// </summary>
        public bool KeepIntersections
        {
            get => _keepIntersections;
            set => _keepIntersections = value;
        }

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
        public bool HasIntersection => _interiorIntersection != null;

        /// <summary>
        /// Gets the intersections found.
        /// </summary>
        /// <returns>A list of <see cref="Coordinate"/>.</returns>
        public IList<Coordinate> Intersections => new ReadOnlyCollection<Coordinate>(_intersections);

        /// <summary>
        /// Gets the count of intersections found.
        /// </summary>
        /// <returns>The intersection count.</returns>
        public int Count => intersectionCount;

        ///<summary>
        /// Gets the computed location of the intersection.
        /// Due to round-off, the location may not be exact.
        ///</summary>
        public Coordinate InteriorIntersection => _interiorIntersection;

        ///<summary>
        /// Gets the endpoints of the intersecting segments.
        ///</summary>
        public Coordinate[] IntersectionSegments => _intSegments;

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
            if (!FindAllIntersections && HasIntersection)
                return;

            // don't bother intersecting a segment with itself
            bool isSameSegString = e0 == e1;
            bool isSameSegment = isSameSegString && segIndex0 == segIndex1;
            if (isSameSegment) return;

            /*
             * If enabled, only test end segments (on either segString).
             *
             */
            if (CheckEndSegmentsOnly)
            {
                bool isEndSegPresent = IsEndSegment(e0, segIndex0) || IsEndSegment(e1, segIndex1);
                if (!isEndSegPresent)
                    return;
            }

            var p00 = e0.Coordinates[segIndex0];
            var p01 = e0.Coordinates[segIndex0 + 1];
            var p10 = e1.Coordinates[segIndex1];
            var p11 = e1.Coordinates[segIndex1 + 1];
            bool isEnd00 = segIndex0 == 0;
            bool isEnd01 = segIndex0 + 2 == e0.Count;
            bool isEnd10 = segIndex1 == 0;
            bool isEnd11 = segIndex1 + 2 == e1.Count;
            _li.ComputeIntersection(p00, p01, p10, p11);


            // Check for an intersection in the interior of a segment
            bool isInteriorInt = _li.HasIntersection && _li.IsInteriorIntersection();
            /**
             * Check for an intersection between two vertices which are not both endpoints.
             */
            bool isAdjacentSegment = isSameSegString && Math.Abs(segIndex1 - segIndex0) <= 1;
            bool isInteriorVertexInt = (!isAdjacentSegment) && IsInteriorVertexIntersection(p00, p01, p10, p11,
                                           isEnd00, isEnd01, isEnd10, isEnd11);

            if (isInteriorInt || isInteriorVertexInt)
            {
                // found an intersection!
                _intSegments = new Coordinate[4];
                _intSegments[0] = p00;
                _intSegments[1] = p01;
                _intSegments[2] = p10;
                _intSegments[3] = p11;

                //TODO: record endpoint intersection(s)
                _interiorIntersection = _li.GetIntersection(0);
                if (_keepIntersections) _intersections.Add(_interiorIntersection);
                intersectionCount++;
            }
        }

        /// <summary>
        /// Tests if an intersection occurs between a segmentString interior vertex and another vertex.
        /// Note that intersections between two endpoint vertices are valid noding,
        /// and are not flagged.
        /// </summary>
        /// <param name="p00">A segment vertex</param>
        /// <param name="p01">A segment vertex</param>
        /// <param name="p10">A segment vertex</param>
        /// <param name="p11">A segment vertex</param>
        /// <param name="isEnd00">true if vertex is a segmentString endpoint</param>
        /// <param name="isEnd01">true if vertex is a segmentString endpoint</param>
        /// <param name="isEnd10">true if vertex is a segmentString endpoint</param>
        /// <param name="isEnd11">true if vertex is a segmentString endpoint</param>
        /// <returns><c>true</c> if an intersection is found/</returns>
        private static bool IsInteriorVertexIntersection(
            Coordinate p00, Coordinate p01,
            Coordinate p10, Coordinate p11,
            bool isEnd00, bool isEnd01,
            bool isEnd10, bool isEnd11)
        {
            if (IsInteriorVertexIntersection(p00, p10, isEnd00, isEnd10)) return true;
            if (IsInteriorVertexIntersection(p00, p11, isEnd00, isEnd11)) return true;
            if (IsInteriorVertexIntersection(p01, p10, isEnd01, isEnd10)) return true;
            if (IsInteriorVertexIntersection(p01, p11, isEnd01, isEnd11)) return true;
            return false;
        }

        /// <summary>
        /// Tests if two vertices with at least one in a segmentString interior
        /// are equal.
        /// </summary>
        /// <param name="p0">A segment vertex</param>
        /// <param name="p1">A segment vertex</param>
        /// <param name="isEnd0"><c>true</c> if vertex is a segmentString endpoint</param>
        /// <param name="isEnd1"><c>true</c> if vertex is a segmentString endpoint</param>
        /// <returns><c>true</c> if an intersection is found</returns>
        private static bool IsInteriorVertexIntersection(
            Coordinate p0, Coordinate p1,
            bool isEnd0, bool isEnd1)
        {

            // Intersections between endpoints are valid nodes, so not reported
            if (isEnd0 && isEnd1) return false;

            if (p0.Equals2D(p1))
            {
                return true;
            }
            return false;
        }

        ///<summary>
        /// Tests whether a segment in a <see cref="ISegmentString" /> is an end segment.
        /// (either the first or last).
        ///</summary>
        ///<param name="segStr">a segment string</param>
        ///<param name="index">the index of a segment in the segment string</param>
        ///<returns>true if the segment is an end segment</returns>
        private static bool IsEndSegment(ISegmentString segStr, int index)
        {
            if (index == 0) return true;
            if (index >= segStr.Count - 2) return true;
            return false;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsDone
        {
            get
            {
                if (FindAllIntersections) return false;
                return _interiorIntersection != null;
            }
        }
    }
}
