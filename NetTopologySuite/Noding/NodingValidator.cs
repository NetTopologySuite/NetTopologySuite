using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Validates that a collection of <see cref="SegmentString{TCoordinate}" />s is correctly noded.
    /// Throws an appropriate exception if an noding error is found.
    /// </summary>
    public class NodingValidator<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private readonly LineIntersector<TCoordinate> _li = new RobustLineIntersector<TCoordinate>();
        private readonly List<SegmentString<TCoordinate>> _segStrings = new List<SegmentString<TCoordinate>>();

        /// <summary>
        /// Initializes a new instance of the <see cref="NodingValidator{TCoordinate}"/> 
        /// class.
        /// </summary>
        /// <param name="segStrings">The seg strings.</param>
        public NodingValidator(IEnumerable<SegmentString<TCoordinate>> segStrings)
        {
            _segStrings.AddRange(segStrings);
        }

        public void CheckValid()
        {
            checkEndPtVertexIntersections();
            checkInteriorIntersections();
            checkCollapses();
        }

        /// <summary>
        /// Checks if a segment string contains a segment pattern a-b-a (which implies a self-intersection).
        /// </summary>   
        private void checkCollapses()
        {
            foreach (SegmentString<TCoordinate> ss in _segStrings)
            {
                checkCollapses(ss);
            }
        }

        private static void checkCollapses(SegmentString<TCoordinate> ss)
        {
            IEnumerable<TCoordinate> pts = ss.Coordinates;

            foreach (Triple<TCoordinate> triple in Slice.GetOverlappingTriples(pts))
            {
                checkCollapse(triple.First, triple.Second, triple.Third);
            }
        }

        private static void checkCollapse(TCoordinate p0, TCoordinate p1, TCoordinate p2)
        {
            if (p0.Equals(p2))
            {
                throw new Exception("found non-noded collapse at: " + p0 + ", " + p1 + " " + p2);
            }
        }

        /// <summary>
        /// Checks all pairs of segments for intersections at an interior point of a segment.
        /// </summary>
        private void checkInteriorIntersections()
        {
            foreach (SegmentString<TCoordinate> ss0 in _segStrings)
            {
                foreach (SegmentString<TCoordinate> ss1 in _segStrings)
                {
                    checkInteriorIntersections(ss0, ss1);
                }
            }
        }

        private void checkInteriorIntersections(SegmentString<TCoordinate> ss0, SegmentString<TCoordinate> ss1)
        {
            IEnumerable<TCoordinate> pts0 = ss0.Coordinates;
            IEnumerable<TCoordinate> pts1 = ss1.Coordinates;

            foreach (Pair<TCoordinate> p0 in Slice.GetOverlappingPairs(pts0))
            {
                foreach (Pair<TCoordinate> p1 in Slice.GetOverlappingPairs(pts1))
                {
                    checkInteriorIntersections(ss0, p0.First, p0.Second, ss1, p1.First, p1.Second);
                }
            }
        }

        private void checkInteriorIntersections(SegmentString<TCoordinate> e0, TCoordinate p00, TCoordinate p01, SegmentString<TCoordinate> e1, TCoordinate p10, TCoordinate p11)
        {
            if (e0 == e1 && p00.Equals(p10) && p01.Equals(p11))
            {
                return;
            }

            _li.ComputeIntersection(p00, p01, p10, p11);

            if (_li.HasIntersection)
            {
                if (_li.IsProper || hasInteriorIntersection(_li, p00, p01) || hasInteriorIntersection(_li, p10, p11))
                {
                    throw new TopologyException("Found non-noded intersection at " 
                        + p00 + "-" + p01 + " and " + p10 + "-" + p11);
                }
            }
        }

        /// <returns><see langword="true"/> if there is an intersection point which is not an endpoint of the segment p0-p1.</returns>
        private static Boolean hasInteriorIntersection(LineIntersector<TCoordinate> li, TCoordinate p0, TCoordinate p1)
        {
            for (Int32 i = 0; i < (Int32)li.IntersectionType; i++)
            {
                TCoordinate intPt = li.GetIntersection(i);

                if (!(intPt.Equals(p0) || intPt.Equals(p1)))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Checks for intersections between an endpoint of a segment string
        /// and an interior vertex of another segment string
        /// </summary>
        private void checkEndPtVertexIntersections()
        {
            foreach (SegmentString<TCoordinate> ss in _segStrings)
            {
                IEnumerable<TCoordinate> pts = ss.Coordinates;
                checkEndPtVertexIntersections(Slice.GetFirst(pts), _segStrings);
                checkEndPtVertexIntersections(Slice.GetLast(pts), _segStrings);
            }
        }

        private static void checkEndPtVertexIntersections(ICoordinate testPt, IEnumerable<SegmentString<TCoordinate>> segStrings)
        {
            foreach (SegmentString<TCoordinate> ss in segStrings)
            {
                IEnumerable<TCoordinate> pts = ss.Coordinates;

                Int32 pointIndex = 0;

                foreach (TCoordinate coordinate in Slice.StartAt(1, pts))
                {
                    if (coordinate.Equals(testPt))
                    {
                        throw new TopologyException(
                            "Found end point / interior point intersection at index " 
                            + pointIndex + " :pt " + testPt);
                    }

                    pointIndex += 1;
                }
            }
        }
    }
}