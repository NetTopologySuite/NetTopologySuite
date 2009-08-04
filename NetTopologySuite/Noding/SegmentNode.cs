using System;
using System.IO;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Represents an intersection point between two 
    /// <see cref="NodedSegmentString{TCoordinate}" />s.
    /// </summary>
    public struct SegmentNode<TCoordinate> : IEquatable<SegmentNode<TCoordinate>>, IComparable<SegmentNode<TCoordinate>>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly TCoordinate _coordinate; // the point of intersection
        private readonly Boolean _isInterior;

        // the index of the containing line segment in the parent edge
        private readonly Int32 _segmentIndex;

        //private readonly SegmentString<TCoordinate> _segString;
        private readonly Octants _segmentOctant;

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="SegmentNode{TCoordinate}"/> class.
        /// </summary>
        public SegmentNode(NodedSegmentString<TCoordinate> segString, TCoordinate coord, Int32 segmentIndex,
                           Octants segmentOctant)
        {
            //_segString = segString;
            _coordinate = coord;
            _segmentIndex = segmentIndex;
            _segmentOctant = segmentOctant;
            _isInterior = !coord.Equals(segString.Coordinates[segmentIndex]);
            //jd: was !coord.Equals(segString[segmentIndex])
        }

        public Int32 SegmentIndex
        {
            get { return _segmentIndex; }
        }

        public TCoordinate Coordinate
        {
            get { return _coordinate; }
        }

        public Boolean IsInterior
        {
            get { return _isInterior; }
        }

        #region IComparable<SegmentNode<TCoordinate>> Members

        /// <returns>
        /// -1 this SegmentNode is located before the argument location, or
        ///  0 this SegmentNode is at the argument location, or
        ///  1 this SegmentNode is located after the argument location.   
        /// </returns>
        public Int32 CompareTo(SegmentNode<TCoordinate> other)
        {
            if (SegmentIndex < other.SegmentIndex)
            {
                return -1;
            }

            if (SegmentIndex > other.SegmentIndex)
            {
                return 1;
            }

            if (_coordinate.Equals(other._coordinate))
            {
                return 0;
            }

            return SegmentPointComparator.Compare(_segmentOctant, _coordinate, other._coordinate);
        }

        #endregion

        #region IEquatable<SegmentNode<TCoordinate>> Members

        public Boolean Equals(SegmentNode<TCoordinate> other)
        {
            return other._coordinate.Equals(_coordinate) &&
                   other._isInterior == _isInterior &&
                   other._segmentIndex == _segmentIndex &&
                   other._segmentOctant == _segmentOctant;
        }

        #endregion

        public override string ToString()
        {
            return String.Format("{0} seg # = {1}", _coordinate, SegmentIndex);
        }

        public override Boolean Equals(Object obj)
        {
            if (ReferenceEquals(obj, null) || !(obj is SegmentNode<TCoordinate>))
            {
                return false;
            }

            return Equals((SegmentNode<TCoordinate>) obj);
        }

        public override int GetHashCode()
        {
            return _coordinate.GetHashCode() ^
                   _isInterior.GetHashCode() ^
                   _segmentIndex.GetHashCode() ^
                   _segmentOctant.GetHashCode();
        }

        public Boolean IsEndPoint(Int32 maxSegmentIndex)
        {
            if (SegmentIndex == 0 && ! _isInterior)
            {
                return true;
            }

            if (SegmentIndex == maxSegmentIndex)
            {
                return true;
            }

            return false;
        }

        public void Write(StreamWriter outstream)
        {
            outstream.Write(_coordinate);
            outstream.Write(" seg # = " + SegmentIndex);
        }
    }
}