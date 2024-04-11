using NetTopologySuite.Geometries;

namespace NetTopologySuite.Noding
{
    /*
     * JTS has extended this interface in v1.20.
     * The functionality added on the interface is not used so far.
     * We chose to port the functionality using extension methods.
     * If there is need for an implementation accessible via interface,
     * uncomment code in <c>ISegmentString2.cs</c> and <c>SegmentStringEx.cs</c>
     */

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
        /// Gets the number of <c>Coordinate</c>s in this segment string.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Points that make up <see cref="ISegmentString"/>
        /// </summary>
        Coordinate[] Coordinates { get; }

        /// <summary>
        /// States whether ISegmentString is closed
        /// </summary>
        bool IsClosed { get; }

        LineSegment this[int index] { get; set; }

    }
}
