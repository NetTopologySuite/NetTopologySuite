using System;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;

namespace NetTopologySuite.Operation.Valid
{
    /// <summary>Finds and analyzes intersections in and between polygons,
    /// to determine if they are valid.
    /// <para/>
    /// The <see cref="ISegmentString"/>s which are analyzed can have <see cref="PolygonRing"/>s
    /// attached. If so they will be updated with intersection information
    /// to support further validity analysis which must be done after
    /// basic intersection validity has been confirmed.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class PolygonIntersectionAnalyzer : ISegmentIntersector
    {
        //private const int NoInvalidIntersection = -1;
        private readonly bool _isInvertedRingValid;

        private readonly LineIntersector _li = new RobustLineIntersector();
        private TopologyValidationErrors _invalidCode = TopologyValidationErrors.NoInvalidIntersection;
        private Coordinate _invalidLocation;

        private bool _hasDoubleTouch;
        private Coordinate _doubleTouchLocation;

        /// <summary>
        /// Creates a new finder, allowing for the mode where inverted rings are valid.
        /// </summary>
        /// <param name="isInvertedRingValid"><c>true</c> if inverted rings are valid.</param>
        public PolygonIntersectionAnalyzer(bool isInvertedRingValid)
        {
            _isInvertedRingValid = isInvertedRingValid;
        }

        public bool IsDone => IsInvalid || _hasDoubleTouch;

        public bool IsInvalid => _invalidCode >= 0;

        public TopologyValidationErrors InvalidCode => _invalidCode;

        public Coordinate InvalidLocation => _invalidLocation;

        public bool HasDoubleTouch => _hasDoubleTouch;

        public Coordinate DoubleTouchLocation => _doubleTouchLocation;

        public void ProcessIntersections(ISegmentString ss0, int segIndex0, ISegmentString ss1, int segIndex1)
        {
            // don't test a segment with itself
            bool isSameSegString = ss0 == ss1;
            bool isSameSegment = isSameSegString && segIndex0 == segIndex1;
            if (isSameSegment) return;

            var code = FindInvalidIntersection(ss0, segIndex0, ss1, segIndex1);

            /*
             * Ensure that invalidCode is only set once, 
             * since the short-circuiting in {@link SegmentIntersector} is not guaranteed
             * to happen immediately.
             */
            if (code != TopologyValidationErrors.NoInvalidIntersection)
            {
                _invalidCode = code;
                _invalidLocation = _li.GetIntersection(0);
            }
        }

        private TopologyValidationErrors FindInvalidIntersection(ISegmentString ss0, int segIndex0,
            ISegmentString ss1, int segIndex1)
        {
            var coordinates = ss0.Coordinates;
            var p00 = coordinates[segIndex0];
            var p01 = coordinates[segIndex0 + 1];
            coordinates = ss1.Coordinates;
            var p10 = coordinates[segIndex1];
            var p11 = coordinates[segIndex1 + 1];

            _li.ComputeIntersection(p00, p01, p10, p11);

            if (!_li.HasIntersection)
                return TopologyValidationErrors.NoInvalidIntersection;

            bool isSameSegString = ss0 == ss1;

            /*
             * Check for an intersection in the interior of both segments.
             * Collinear intersections by definition contain an interior intersection.
             */
            if (_li.IsProper || _li.IntersectionNum >= 2)
                return TopologyValidationErrors.SelfIntersection;

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
            bool isAdjacentSegments = isSameSegString && IsAdjacentInRing(ss0, segIndex0, segIndex1);
            // Assert: intersection is an endpoint of both segs
            if (isAdjacentSegments) return TopologyValidationErrors.NoInvalidIntersection;

            /*
             * Under OGC semantics, rings cannot self-intersect.
             * So the intersection is invalid.
             *
             * The return of 'RingSelfIntersection' is to match the previous IsValid semantics.
             */
            if (isSameSegString && !_isInvertedRingValid)
            {
                return TopologyValidationErrors.RingSelfIntersection;
            }

            /*
             * Optimization: don't analyze intPts at the endpoint of a segment.
             * This is because they are also start points, so don't need to be
             * evaluated twice.
             * This simplifies following logic, by removing the segment endpoint case.
             */
            if (intPt.Equals2D(p01) || intPt.Equals2D(p11))
                return TopologyValidationErrors.NoInvalidIntersection;

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

            bool hasCrossing = PolygonNodeTopology.IsCrossing(intPt, e00, e01, e10, e11);
            if (hasCrossing)
            {
                return TopologyValidationErrors.SelfIntersection;
            }

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
            bool isDoubleTouch = AddDoubleTouch(ss0, ss1, intPt);
            if (isDoubleTouch && !isSameSegString)
            {
                _hasDoubleTouch = true;
                _doubleTouchLocation = intPt;
                // TODO: for poly-hole or hole-hole touch, check if it has bad topology.  If so return invalid code
            }

            return TopologyValidationErrors.NoInvalidIntersection;
        }

        private bool AddDoubleTouch(ISegmentString ss0, ISegmentString ss1, Coordinate intPt)
        {
            return PolygonRing.AddTouch((PolygonRing)ss0.Context, (PolygonRing)ss1.Context, intPt);
        }

        private void AddSelfTouch(ISegmentString ss, Coordinate intPt, Coordinate e00, Coordinate e01, Coordinate e10,
            Coordinate e11)
        {
            var polyRing = (PolygonRing) ss.Context;
            if (polyRing == null)
            {
                throw new InvalidOperationException(
                    "SegmentString missing PolygonRing data when checking self-touches");
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
