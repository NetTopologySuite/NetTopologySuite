using System;
using GeoAPI.Coordinates;
using NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Triangulate
{
    ///<summary>
    /// Models a constraint segment which can be split in two in various ways, according to certain geometric constraints.
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public class SplitSegment<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        /**
         * Computes the {@link Coordinate} that lies a given fraction along the line defined by the
         * reverse of the given segment. A fraction of <code>0.0</code> returns the end point of the
         * segment; a fraction of <code>1.0</code> returns the start point of the segment.
         * 
         * @param seg the LineSegment
         * @param segmentLengthFraction the fraction of the segment length along the line
         * @return the point at that distance
         */
        private static TCoordinate PointAlongReverse(ICoordinateFactory<TCoordinate>factory, LineSegment<TCoordinate> seg, Double segmentLengthFraction)
        {
            Double x = seg.P1[Ordinates.X] - segmentLengthFraction * (seg.P1[Ordinates.X] - seg.P0[Ordinates.X]);
            Double y = seg.P1[Ordinates.Y] - segmentLengthFraction * (seg.P1[Ordinates.Y] - seg.P0[Ordinates.Y]);
            return factory.Create(x, y);
        }
        
        private static Boolean Equals2D(TCoordinate one, TCoordinate other)
        {
            return one[0].Equals(other[0]) && one[1].Equals(other[1]);
        }

        private readonly LineSegment<TCoordinate> _seg;
        private readonly double _segLen;
        private TCoordinate _splitPt;
        private double _minimumLen;

        ///<summary>
        /// Creates an instance of this class
        ///</summary>
        ///<param name="seg">a line segment</param>
        public SplitSegment(LineSegment<TCoordinate> seg)
        {
            _seg = seg;
            _segLen = seg.Length;
        }

        public Double MinimumLength
        {
            get { return _minimumLen; }
            set { _minimumLen = value; }
        }

        public TCoordinate SplitPoint
        {
            get {return _splitPt;}
        }

        public void SplitAt(ICoordinateFactory<TCoordinate> factory, Double length, TCoordinate endPt)
        {
            double actualLen = GetConstrainedLength(length);
            double frac = actualLen / _segLen;
            if (Equals2D(endPt, _seg.P0))
                _splitPt = _seg.PointAlong(factory, frac);
            else
                _splitPt = PointAlongReverse(factory, _seg, frac);
        }

        public void SplitAt(ICoordinateFactory<TCoordinate> factory, TCoordinate pt)
        {
            // check that given pt doesn't violate min length
            double minFrac = _minimumLen / _segLen;
            if (pt.Distance(_seg.P0) < _minimumLen)
            {
                _splitPt = _seg.PointAlong(factory, minFrac);
                return;
            }
            if (pt.Distance(_seg.P1) < _minimumLen)
            {
                _splitPt = PointAlongReverse(factory, _seg, minFrac);
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
