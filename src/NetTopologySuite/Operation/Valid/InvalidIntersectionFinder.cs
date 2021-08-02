using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;

namespace NetTopologySuite.Operation.Valid
{
    internal class InvalidIntersectionFinder : ISegmentIntersector
    {
        private readonly LineIntersector _li = new RobustLineIntersector();
        private readonly List<Coordinate> _intersectionPts = new List<Coordinate>();
        private bool _hasProperInt;
        private bool _hasIntersection;
        private bool _hasCrossing;
        private bool _hasDoubleTouch;
        private readonly bool _isInvertedRingValid;

        public InvalidIntersectionFinder(bool isInvertedRingValid)
        {
            _isInvertedRingValid = isInvertedRingValid;
        }

        public bool IsDone
        {
            get => _hasIntersection || _hasDoubleTouch;
        }

        public Coordinate IntersectionLocation
        {
            get
            {
                if (_intersectionPts.Count == 0) return null;
                return _intersectionPts[0];
            }
        }

        public bool HasDoubleTouch
        {
            get => _hasDoubleTouch;
        }

        public bool HasIntersection
        {
            get => _intersectionPts.Count > 0;
        }

        public void ProcessIntersections(ISegmentString ss0, int segIndex0, ISegmentString ss1, int segIndex1)
        {
            // don't test a segment with itself
            bool isSameSegString = ss0 == ss1;
            bool isSameSegment = isSameSegString && segIndex0 == segIndex1;
            if (isSameSegment) return;

            _hasIntersection = FindInvalidIntersection(ss0, segIndex0, ss1, segIndex1);

            if (_hasIntersection)
            {
                // found an intersection!
                _intersectionPts.Add(_li.GetIntersection(0));
            }
        }

        private bool FindInvalidIntersection(ISegmentString ss0, int segIndex0,
            ISegmentString ss1, int segIndex1)
        {
            var p00 = ss0.Coordinates[segIndex0];
            var p01 = ss0.Coordinates[segIndex0 + 1];
            var p10 = ss1.Coordinates[segIndex1];
            var p11 = ss1.Coordinates[segIndex1 + 1];

            _li.ComputeIntersection(p00, p01, p10, p11);

            if (!_li.HasIntersection) return false;

            /*
             * Check for an intersection in the interior of both segments.
             */
            _hasProperInt = _li.IsProper;
            if (_hasProperInt)
                return true;

            /*
             * Check for collinear segments (which produces two intersection points).
             * This is invalid - either a zero-width spike or gore,
             * or adjacent rings.
             */
            _hasProperInt = _li.IntersectionNum >= 2;
            if (_hasProperInt) return true;

            /*
             * Now know there is exactly one intersection, 
             * at a vertex of at least one segment.
             */
            var intPt = _li.GetIntersection(0);

            /*
             * If segments are adjacent the intersection must be their common endpoint.
             * (since they are not collinear).
             * This is valid.
             */
            bool isSameSegString = ss0 == ss1;
            bool isAdjacentSegments = isSameSegString && IsAdjacentInRing(ss0, segIndex0, segIndex1);
            // Assert: intersection is an endpoint of both segs
            if (isAdjacentSegments) return false;

            // TODO: allow ring self-intersection - if NOT using OGC semantics

            /*
             * Under OGC semantics, rings cannot self-intersect.
             * So the intersection is invalid.
             */
            if (isSameSegString && !_isInvertedRingValid)
            {
                return true;
            }

            /*
             * Optimization: don't analyze intPts at the endpoint of a segment.
             * This is because they are also start points, so don't need to be
             * evaluated twice.
             * This simplifies following logic, by removing the segment endpoint case.
             */
            if (intPt.Equals2D(p01) || intPt.Equals2D(p11))
                return false;

            /*
             * Check topology of a vertex intersection.
             * The ring(s) must not cross.
             */
            var e00 = p00;
            var e01 = p01;
            if (intPt.Equals2D(p00))
            {
                e00 = PrevCoordinateInRing(ss0, segIndex0);
                e01 = p01;
            }

            var e10 = p10;
            var e11 = p11;
            if (intPt.Equals2D(p10))
            {
                e10 = PrevCoordinateInRing(ss1, segIndex1);
                e11 = p11;
            }

            _hasCrossing = AreaNode.IsCrossing(intPt, e00, e01, e10, e11);
            if (_hasCrossing)
                return true;

            /*
             * If allowing inverted rings, record a self-touch to support later checking
             * that it does not disconnect the interior.
             */
            if (isSameSegString && _isInvertedRingValid)
            {
                AddSelfTouch(ss0, intPt, e00, e01, e10, e11);
            }

            /*
             * If the rings are in the same polygon
             * then record the touch to support connected interior checking.
             * 
             * Also check for an invalid double-touch situation,
             * if the rings are different.
             */
            bool isDoubleTouch = PolygonRing.AddTouch((PolygonRing) ss0.Context, (PolygonRing) ss1.Context, intPt);
            if (isDoubleTouch && !isSameSegString)
            {
                _hasDoubleTouch = true;
                return true;
            }

            return false;
        }

        private void AddSelfTouch(ISegmentString ss, Coordinate intPt, Coordinate e00, Coordinate e01, Coordinate e10,
            Coordinate e11)
        {
            var polyRing = (PolygonRing) ss.Context;
            if (polyRing == null)
            {
                throw new InvalidOperationException(
                    "SegmentString missing PolygonRing data when checking valid self-touches");
            }

            polyRing.AddSelfTouch(intPt, e00, e01, e10, e11);
        }

        /// <summary>
        /// For a segment string for a ring, gets the coordinate
        /// previous to the given index (wrapping if the index is 0)
        /// </summary>
        /// <param name="ringSS">The ring segment string</param>
        /// <param name="segIndex">The segment index</param>
        /// <returns>The coordinate previous to the given segment</returns>
        private static Coordinate PrevCoordinateInRing(ISegmentString ringSS, int segIndex)
        {
            int prevIndex = segIndex - 1;
            if (prevIndex < 0)
            {
                prevIndex = ringSS.Count - 2;
            }

            return ringSS.Coordinates[prevIndex];
        }

        /// <summary>
        /// Tests if two segments in a closed <see cref="ISegmentString"/> are adjacent.
        /// This handles determining adjacency across the start/end of the ring.
        /// </summary>
        /// <param name="ringSS">The segment string</param>
        /// <param name="segIndex0">A segment index</param>
        /// <param name="segIndex1">A segment index</param>
        /// <returns><c>true</c> if the segments are adjacent</returns>
        private static bool IsAdjacentInRing(ISegmentString ringSS, int segIndex0, int segIndex1)
        {
            int delta = Math.Abs(segIndex1 - segIndex0);
            if (delta <= 1) return true;
            /*
             * A string with N vertices has maximum segment index of N-2.
             * If the delta is at least N-2, the segments must be
             * at the start and end of the string and thus adjacent.
             */
            if (delta >= ringSS.Count - 2) return true;
            return false;
        }
    }
}
