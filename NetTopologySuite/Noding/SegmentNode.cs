using System;
using System.IO;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Represents an intersection point between two 
    /// <see cref="SegmentString{TCoordinate}" />s.
    /// </summary>
    public class SegmentNode<TCoordinate> : IComparable<SegmentNode<TCoordinate>>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private readonly TCoordinate _coordinate; // the point of intersection
        
        // the index of the containing line segment in the parent edge
        public readonly Int32 SegmentIndex = 0; 

        private readonly SegmentString<TCoordinate> _segString = null;
        private readonly Octants _segmentOctant = Octants.Null;
        private readonly Boolean _isInterior = false;

        /// <summary>
        /// Initializes a new instance of the 
        /// <see cref="SegmentNode{TCoordinate}"/> class.
        /// </summary>
        public SegmentNode(SegmentString<TCoordinate> segString, TCoordinate coord, Int32 segmentIndex, Octants segmentOctant)
        {
            _segString = segString;
            _coordinate = new TCoordinate(coord);
            SegmentIndex = segmentIndex;
            _segmentOctant = segmentOctant;
            _isInterior = !coord.Equals(segString.GetCoordinate(segmentIndex));
        }

        public TCoordinate Coordinate
        {
            get { return _coordinate; }
        }

        public Boolean IsInterior
        {
            get { return _isInterior; }
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

        /// <returns>
        /// -1 this SegmentNode is located before the argument location, or
        ///  0 this SegmentNode is at the argument location, or
        ///  1 this SegmentNode is located after the argument location.   
        /// </returns>
        public Int32 CompareTo(SegmentNode<TCoordinate> other)
        {
            if (other == null)
            {
                throw new ArgumentNullException("other");
            }

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

        public void Write(StreamWriter outstream)
        {
            outstream.Write(_coordinate);
            outstream.Write(" seg # = " + SegmentIndex);
        }
    }
}