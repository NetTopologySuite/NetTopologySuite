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

//#if NETSTANDARD2_1_OR_GREATER
//        /// <summary>
//        /// Gets the segment string <c>Coordinate</c> at a given index
//        /// </summary>
//        /// <param name="idx">An index</param>
//        /// <returns>The <c>Coordinate</c> at the index</returns>
//        Coordinate GetCoordinate(int idx) => Coordinates[idx];
//#endif
        /// <summary>
        /// Points that make up <see cref="ISegmentString"/>
        /// </summary>
        Coordinate[] Coordinates { get; }

        /// <summary>
        /// States whether ISegmentString is closed
        /// </summary>
        bool IsClosed { get; }

        LineSegment this[int index] { get; set; }

//#if NETSTANDARD2_1_OR_GREATER || false
//        /// <summary>
//        /// Gets the previous vertex in a ring from a vertex index.
//        /// </summary>
//        /// <param name="index">The vertex index</param>
//        /// <returns>The previous vertex in the ring</returns>
//        /// <seealso cref="ISegmentString.IsClosed"/>
//        public Coordinate PrevInRing(int index)
//        {
//            int prevIndex = index - 1;
//            if (prevIndex < 0)
//                prevIndex = Count - 2;
//            return GetCoordinate(prevIndex);
//        }

//        /// <summary>
//        /// Gets the next vertex in a ring from a vertex index.
//        /// </summary>
//        /// <param name="index">The vertex index</param>
//        /// <returns>The next vertex in the ring</returns>
//        /// <seealso cref="ISegmentString.IsClosed"/>
//        public Coordinate NextInRing(int index)
//        {
//            int nextIndex = index + 1;
//            if (nextIndex > Count - 1)
//                nextIndex = 1;
//            return GetCoordinate(nextIndex);
//        }
//#endif
    }

//#if NETSTANDARD2_0
    /// <summary>
    /// Extension methods to mimic JTS' default methods on SegmentString interface
    /// </summary>
    public static class SegmentStringEx
    {
        /// <summary>
        /// Gets the segment string <c>Coordinate</c> at a given index
        /// </summary>
        /// <param name="self">A segment string forming a ring</param>
        /// <param name="idx">An index</param>
        /// <returns>The <c>Coordinate</c> at the index</returns>
        public static Coordinate GetCoordinate(this ISegmentString self, int idx) => self.Coordinates[idx];

        /// <summary>
        /// Gets the previous vertex in a ring from a vertex index.
        /// </summary>
        /// <param name="self">A segment string forming a ring</param>
        /// <param name="index">The vertex index</param>
        /// <returns>The previous vertex in the ring</returns>
        /// <seealso cref="ISegmentString.IsClosed"/>
        public static Coordinate PrevInRing(this ISegmentString self, int index)
        {
            int prevIndex = index - 1;
            if (prevIndex < 0)
                prevIndex = self.Count - 2;
            return self.GetCoordinate(prevIndex);
        }

        /// <summary>
        /// Gets the next vertex in a ring from a vertex index.
        /// </summary>
        /// <param name="self">A segment string forming a ring</param>
        /// <param name="index">The vertex index</param>
        /// <returns>The next vertex in the ring</returns>
        /// <seealso cref="ISegmentString.IsClosed"/>
        public static Coordinate NextInRing(this ISegmentString self, int index)
        {
            int nextIndex = index + 1;
            if (nextIndex > self.Count - 1)
                nextIndex = 1;
            return self.GetCoordinate(nextIndex);
        }
    }
//#endif
}
