using System;
using GeoAPI.Coordinates;
using NetTopologySuite.Algorithm;
using NPack.Interfaces;

namespace NetTopologySuite.Noding
{
    public class SegmentIntersectionDetector<TCoordinate> : ISegmentIntersector<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly LineIntersector<TCoordinate> _lineIntersector;

        private Boolean _hasIntersection;

        private Boolean _hasNonProperIntersection;
        private Boolean _hasProperIntersection;

        private TCoordinate _intPt;
        private TCoordinate[] _intSegments;

        public SegmentIntersectionDetector(LineIntersector<TCoordinate> lineIntersector)
        {
            _lineIntersector = lineIntersector;
        }

        #region ISegmentIntersector<TCoordinate> Member

        public void ProcessIntersections(ISegmentString<TCoordinate> e0, int segIndex0,
                                         ISegmentString<TCoordinate> e1, int segIndex1)
        {
            // don't bother intersecting a segment with itself
            if (e0 == e1 && segIndex0 == segIndex1) return;

            TCoordinate p00 = e0[segIndex0][0];
            TCoordinate p01 = e0[segIndex0][1];
            TCoordinate p10 = e1[segIndex1][0];
            TCoordinate p11 = e1[segIndex1][1];

            Intersection<TCoordinate> intersection = _lineIntersector.ComputeIntersection(p00, p01, p10, p11);
            //  if (li.hasIntersection() && li.isProper()) Debug.println(li);

            if (intersection.HasIntersection)
            {
                // System.out.println(li);

                // record intersection info
                _hasIntersection = true;

                Boolean isProper = intersection.IsProper;
                if (isProper)
                    _hasProperIntersection = true;
                else
                    _hasNonProperIntersection = true;

                /**
                 * If this is the kind of intersection we are searching for
                 * OR no location has yet been recorded
                 * save the location data
                 */
                Boolean saveLocation = true;
                if (FindProper && !isProper) saveLocation = false;

                Boolean equal = typeof (TCoordinate).IsValueType ? _intPt.Equals(default(TCoordinate)) : _intPt == null;
                if (equal || saveLocation)
                {
                    // record intersection location (approximate)
                    _intPt = intersection.GetIntersectionPoint(0);

                    // record intersecting segments
                    _intSegments = new TCoordinate[4];
                    _intSegments[0] = p00;
                    _intSegments[1] = p01;
                    _intSegments[2] = p10;
                    _intSegments[3] = p11;
                }
            }
        }

        public Boolean IsDone
        {
            get
            {
                /**
                 * If finding all types, we can stop
                 * when both possible types have been found.
                 */
                if (FindAllTypes)
                {
                    return HasProperIntersection && HasNonProperIntersection;
                }

                /**
                 * If searching for a proper intersection, only stop if one is found
                 */
                if (FindProper)
                {
                    return HasProperIntersection;
                }
                return HasIntersection;
            }
        }

        #endregion

        public Boolean FindProper { get; set; }
        public Boolean FindAllTypes { get; set; }

        public Boolean HasIntersection
        {
            get { return _hasIntersection; }
        }

        public Boolean HasProperIntersection
        {
            get { return _hasProperIntersection; }
        }

        public Boolean HasNonProperIntersection
        {
            get { return _hasNonProperIntersection; }
        }
    }
}