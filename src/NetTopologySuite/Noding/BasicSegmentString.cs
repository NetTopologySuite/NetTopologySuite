using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// Represents a list of contiguous line segments,
    /// and supports noding the segments.
    /// The line segments are represented by an array of <see cref="Coordinate" />s.
    /// Intended to optimize the noding of contiguous segments by
    /// reducing the number of allocated objects.
    /// SegmentStrings can carry a context object, which is useful
    /// for preserving topological or parentage information.
    /// All noded substrings are initialized with the same context object.
    /// </summary>
    public class BasicSegmentString : ISegmentString
    {

        private readonly Coordinate[] _pts;

        /// <summary>
        /// Creates a new segment string from a list of vertices.
        /// </summary>
        /// <param name="pts">the vertices of the segment string</param>
        /// <param name="data">the user-defined data of this segment string (may be null)</param>
        public BasicSegmentString(Coordinate[] pts, object data)
        {
            _pts = pts;
            this.Context = data;
        }

        /// <summary>Gets the user-defined data for this segment string.
        /// </summary>
        public object Context { get; set; }

        public Coordinate[] Coordinates => _pts;

        public bool IsClosed => _pts[0].Equals2D(_pts[_pts.Length - 1]);

        public int Count => _pts.Length;

        /// <summary>
        /// Gets the octant of the segment starting at vertex <c>index</c>
        /// </summary>
        /// <param name="index">the index of the vertex starting the segment. Must not be the last index in the vertex list</param>
        /// <returns>octant of the segment at the vertex</returns>
        public Octants GetSegmentOctant(int index)
        {
            return index == _pts.Length - 1
                ? Octants.Null :
                Octant.GetOctant(_pts[index], _pts[index + 1]);
        }

        public LineSegment this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException("index", index,
                                                          "Parameter must be greater than or equal to 0 and less than TotalItemCount.");
                }

                return new LineSegment(_pts[index], _pts[index + 1]);
            }
            set => throw new NotSupportedException(
                "Setting line segments in a ISegmentString not supported.");
        }

        /// <inheritdoc cref="object.ToString()"/>
        public override string ToString()
        {
            return WKTWriter.ToLineString(new CoordinateArraySequence(_pts));
        }
    }
}
