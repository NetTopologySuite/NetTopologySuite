using NetTopologySuite.Geometries;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// An interface for classes which represent a sequence of contiguous line segments.
    /// SegmentStrings can carry a context object, which is useful
    /// for preserving topological or parentage information.
    /// </summary>
    public interface ISegmentString
    {
        /// <summary>
        /// Gets/Sets the user-defined data for this segment string.
        /// </summary>
        object Context { get; set; }
        /// <summary>
        /// Points that make up ISegmentString
        /// </summary>
        Coordinate[] Coordinates { get; }
        /// <summary>
        /// Size of Coordinate Sequence
        /// </summary>
        int Count { get; }
        /// <summary>
        /// States whether ISegmentString is closed
        /// </summary>
        bool IsClosed { get; }

        LineSegment this[int index] { get; set; }
    }
}