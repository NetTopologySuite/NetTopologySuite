using System;
using GeoAPI.Coordinates;
using NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace NetTopologySuite.Noding
{
    ///<summary>
    /// Represents a list of contiguous line segments,
    /// and supports noding the segments.
    /// The line segments are represented by an array of {@link Coordinate}s.
    /// Intended to optimize the noding of contiguous segments by
    /// reducing the number of allocated objects.
    /// SegmentStrings can carry a context object, which is useful
    /// for preserving topological or parentage information.
    /// All noded substrings are initialized with the same context object.
    ///</summary>
    public class BasicSegmentString<TCoordinate> : ISegmentString<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {

        private ICoordinateSequence<TCoordinate> _pts;
        private Object _data;

        ///<summary>
        /// Creates a new segment string from a list of vertices.
        ///</summary>
        ///<param name="pts">the vertices of the segment string</param>
        ///<param name="data">the user-defined data of this segment string (may be null)</param>
        public BasicSegmentString(ICoordinateSequence<TCoordinate> pts, Object data)
        {
            _pts = pts;
            _data = data;
        }

        ///<summary>Gets the user-defined data for this segment string.
        ///</summary>
        public Object Context
        {
            get { return _data; }
            set { _data = value; }
        }

        public ICoordinateSequence<TCoordinate> Coordinates { get { return _pts; } }

        public Boolean IsClosed
        {
            get { return _pts.First.Equals(_pts.Last); }
        }

        public Int32 Count
        {
            get { return _pts.Count; }
        }

        ///<summary>
        /// Gets the octant of the segment starting at vertex <code>index</code>
        ///</summary>
        ///<param name="index">the index of the vertex starting the segment. Must not be the last index in the vertex list</param>
        ///<returns>octant of the segment at the vertex</returns>
        public Octants GetSegmentOctant(int index)
        {
            if (index == _pts.LastIndex) return Octants.Null;
            return Octant.GetOctant(_pts[index], _pts[index + 1]);
        }

        public LineSegment<TCoordinate> this[Int32 index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException("index", index,
                                                          "Parameter must be greater than or equal to 0 and less than TotalItemCount.");
                }

                return new LineSegment<TCoordinate>(_pts[index], _pts[index + 1]);
            }
            set
            {
                throw new NotSupportedException(
                    "Setting line segments in a SegmentString not supported.");
            }
        }
    }
}