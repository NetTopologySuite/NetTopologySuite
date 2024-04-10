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

    /// <summary>
    /// Contains additional methods that could not be added to <see cref="ISegmentString"/> directly
    /// (for compatibility reasons).
    /// </summary>
    public interface ISegmentStringEx1 : ISegmentString
    {
        /// <summary>
        /// Gets the segment string <c>Coordinate</c> at a given index
        /// </summary>
        /// <param name="idx">An index</param>
        /// <returns>The <c>Coordinate</c> at the index</returns>
        Coordinate GetCoordinate(int idx);

        /// <summary>
        /// Gets the previous vertex in a ring from a vertex index.
        /// </summary>
        /// <param name="index">The vertex index</param>
        /// <returns>The previous vertex in the ring</returns>
        /// <seealso cref="ISegmentString.IsClosed"/>
        Coordinate PrevInRing(int index);

        /// <summary>
        /// Gets the next vertex in a ring from a vertex index.
        /// </summary>
        /// <param name="index">The vertex index</param>
        /// <returns>The next vertex in the ring</returns>
        /// <seealso cref="ISegmentString.IsClosed"/>
        Coordinate NextInRing(int index);
    }

    /// <summary>
    /// Contains extension methods to bring the <see cref="ISegmentStringEx1"/> functionality to
    /// <see cref="ISegmentString"/> without breaking compatibility.
    /// </summary>
    public static class SegmentStringEx
    {
        /// <inheritdoc cref="ISegmentStringEx1.GetCoordinate"/>
        public static Coordinate GetCoordinate(this ISegmentString self, int idx)
        {
            if (self is ISegmentStringEx1 ex)
            {
                return ex.GetCoordinate(idx);
            }

            return self.Coordinates[idx];
        }

        /// <inheritdoc cref="ISegmentStringEx1.PrevInRing"/>
        public static Coordinate PrevInRing(this ISegmentString self, int index)
        {
            if (self is ISegmentStringEx1 ex)
            {
                return ex.PrevInRing(index);
            }

            int prevIndex = index - 1;
            if (prevIndex < 0)
                prevIndex = self.Count - 2;
            return self.Coordinates[prevIndex];
        }

        /// <inheritdoc cref="ISegmentStringEx1.NextInRing"/>
        public static Coordinate NextInRing(this ISegmentString self, int index)
        {
            if (self is ISegmentStringEx1 ex)
            {
                return ex.NextInRing(index);
            }

            int nextIndex = index + 1;
            if (nextIndex > self.Count - 1)
                nextIndex = 1;
            return self.Coordinates[nextIndex];
        }
    }
}
