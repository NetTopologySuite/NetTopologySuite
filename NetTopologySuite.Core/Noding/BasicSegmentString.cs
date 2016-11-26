using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;

namespace NetTopologySuite.Noding
{
    ///<summary>
    /// Represents a list of contiguous line segments,
    /// and supports noding the segments.
    /// The line segments are represented by an array of <see cref="Coordinate" />s.
    /// Intended to optimize the noding of contiguous segments by
    /// reducing the number of allocated objects.
    /// SegmentStrings can carry a context object, which is useful
    /// for preserving topological or parentage information.
    /// All noded substrings are initialized with the same context object.
    ///</summary>
    public class BasicSegmentString : ISegmentString
    {
        ///<summary>
        /// Creates a new segment string from a list of vertices.
        ///</summary>
        ///<param name="pts">the vertices of the segment string</param>
        ///<param name="data">the user-defined data of this segment string (may be null)</param>
        public BasicSegmentString(Coordinate[] pts, Object data)
        {
            Coordinates = pts;
            Context = data;
        }

        ///<summary>Gets the user-defined data for this segment string.
        ///</summary>
        public Object Context { get; set; }

        public Coordinate[] Coordinates { get; }

        public Boolean IsClosed => Coordinates[0].Equals2D(Coordinates[Coordinates.Length]);

        public Int32 Count => Coordinates.Length;

        ///<summary>
        /// Gets the octant of the segment starting at vertex <code>index</code>
        ///</summary>
        ///<param name="index">the index of the vertex starting the segment. Must not be the last index in the vertex list</param>
        ///<returns>octant of the segment at the vertex</returns>
        public Octants GetSegmentOctant(int index)
        {
            return index == Coordinates.Length - 1
                ? Octants.Null :
                Octant.GetOctant(Coordinates[index], Coordinates[index + 1]);
        }

        public LineSegment this[Int32 index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
#if PCL
                    throw new ArgumentOutOfRangeException("index", "Parameter must be greater than or equal to 0 and less than TotalItemCount.");
#else
                    throw new ArgumentOutOfRangeException(nameof(index), index,
                                                          "Parameter must be greater than or equal to 0 and less than TotalItemCount.");
#endif
                }

                return new LineSegment(Coordinates[index], Coordinates[index + 1]);
            }
            set
            {
                throw new NotSupportedException(
                    "Setting line segments in a ISegmentString not supported.");
            }
        }

        public override string ToString()
        {
            return WKTWriter.ToLineString(new CoordinateArraySequence(Coordinates));
        }
    }
}