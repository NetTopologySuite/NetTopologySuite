using NetTopologySuite.Geometries;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// Extension methods to mimic JTS' default methods on SegmentString interface
    /// </summary>
    /// <remarks>
    /// JTS extended its SegmentString interface. The added (default) methods are currently
    /// not used within JTS, so we decided to implement their functionality through
    /// extension methods which can be found here.
    /// <para/>
    /// When the extension of the interface becomes necessary, 
    /// </remarks>
    public static class SegmentStringEx
    {
        /// <summary>
        /// Gets the segment string <c>Coordinate</c> at a given index
        /// </summary>
        /// <param name="self">A segment string forming a ring</param>
        /// <param name="index">The index</param>
        /// <returns>The <c>Coordinate</c> at the index</returns>
        public static Coordinate GetCoordinate(this ISegmentString self, int index)
        {
            /* 
             * NOTE: When ISegmentString2 interface gets to be added and implemented
             *       the following piece of code needs to be uncommented.
             */
            //if (self is ISegmentString2 self2)
            //    return self2.GetCoordinate(index);

            // default implementation
            return self.Coordinates[index];
        }

        /// <summary>
        /// Gets the previous vertex in a ring from a vertex index.
        /// </summary>
        /// <param name="self">A segment string forming a ring</param>
        /// <param name="index">The vertex index</param>
        /// <returns>The previous vertex in the ring</returns>
        /// <seealso cref="ISegmentString.IsClosed"/>
        public static Coordinate PrevInRing(this ISegmentString self, int index)
        {
            /* 
             * NOTE: When ISegmentString2 interface gets to be added and implemented
             *       the following piece of code needs to be uncommented.
             */
            //    if (self is ISegmentString2 self2)
            //        return self2.PrevInRing(index);

            //    return DefaultPrevInRingImpl(self, index);
            //}

            //internal static Coordinate DefaultPrevInRingImpl(ISegmentString self, int index)
            //{
            int prevIndex = index - 1;
            if (prevIndex < 0)
                prevIndex = self.Count - 2;
            return self.Coordinates[prevIndex]; ;
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
            /* 
             * NOTE: When ISegmentString2 interface gets to be added and implemented
             *       the following piece of code needs to be uncommented.
             */
            //    if (self is ISegmentString2 self2)
            //        return self2.NextInRing(index);

            //    return DefaultNextInRingImpl(self, index);
            //}

            //internal static Coordinate DefaultNextInRingImpl(ISegmentString self, int index)
            //{
            int nextIndex = index + 1;
            if (nextIndex > self.Count - 1)
                nextIndex = 1;
            return self.Coordinates[nextIndex]; ;
        }
    }
}
