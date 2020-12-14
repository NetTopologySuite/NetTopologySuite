using NetTopologySuite.Geometries;

namespace NetTopologySuite.Triangulate
{
    /// <summary>
    /// Models a constraint segment which can be split in two in various ways,
    /// according to certain geometric constraints.
    /// </summary>
    /// <author>Martin Davis</author>
    public class SplitSegment
    {
        /// <summary>
        /// Computes the {@link Coordinate} that lies a given fraction along the line defined by the
        /// reverse of the given segment. A fraction of <c>0.0</c> returns the end point of the
        /// segment; a fraction of <c>1.0</c> returns the start point of the segment.
        /// </summary>
        /// <param name="seg">the LineSegment</param>
        /// <param name="segmentLengthFraction">the fraction of the segment length along the line</param>
        /// <returns>the point at that distance</returns>
        private static Coordinate PointAlongReverse(LineSegment seg, double segmentLengthFraction)
        {
            var coord = new Coordinate();
            coord.X = seg.P1.X - segmentLengthFraction*(seg.P1.X - seg.P0.X);
            coord.Y = seg.P1.Y - segmentLengthFraction*(seg.P1.Y - seg.P0.Y);
            return coord;
        }

        private readonly LineSegment _seg;
        private readonly double _segLen;
        private Coordinate _splitPt;
        private double _minimumLen;

        public SplitSegment(LineSegment seg)
        {
            _seg = seg;
            _segLen = seg.Length;
        }

        public double MinimumLength
        {
            get => _minimumLen;
            set => _minimumLen = value;
        }

        public Coordinate SplitPoint => _splitPt;

        public void SplitAt(double length, Coordinate endPt)
        {
            double actualLen = GetConstrainedLength(length);
            double frac = actualLen/_segLen;
            if (endPt.Equals2D(_seg.P0))
                _splitPt = _seg.PointAlong(frac);
            else
                _splitPt = PointAlongReverse(_seg, frac);
        }

        public void SplitAt(Coordinate pt)
        {
            // check that given pt doesn't violate min length
            double minFrac = _minimumLen/_segLen;
            if (pt.Distance(_seg.P0) < _minimumLen)
            {
                _splitPt = _seg.PointAlong(minFrac);
                return;
            }
            if (pt.Distance(_seg.P1) < _minimumLen)
            {
                _splitPt = PointAlongReverse(_seg, minFrac);
                return;
            }
            // passes minimum distance check - use provided point as split pt
            _splitPt = pt;
        }

        private double GetConstrainedLength(double len)
        {
            if (len < _minimumLen)
                return _minimumLen;
            return len;
        }
    }
}
