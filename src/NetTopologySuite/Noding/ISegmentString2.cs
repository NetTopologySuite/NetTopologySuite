/*
 * JTS has extended this interface in v1.20.
 * The functionality added on the interface is not used so far.
 * We chose to port the functionality using extension methods.
 * If there is need for an implementation accessible via interface,
 * uncomment code in this file and in <c>SegmentStringEx.cs</c>
 */
//using NetTopologySuite.Geometries;

//namespace NetTopologySuite.Noding
//{
//    /// <summary>
//    /// Extension of the <see cref="ISegmentString"/> interface
//    /// </summary>
//    public interface ISegmentString2 : ISegmentString
//    {
//        /// <summary>
//        /// Gets the segment string <c>Coordinate</c> at a given index
//        /// </summary>
//        /// <param name="index">The index</param>
//        /// <returns>The <c>Coordinate</c> at the index</returns>
//        Coordinate GetCoordinate(int index);

//        /// <summary>
//        /// Gets the previous vertex in a ring from a vertex index.
//        /// </summary>
//        /// <param name="index">The vertex index</param>
//        /// <returns>The previous vertex in the ring</returns>
//        /// <seealso cref="ISegmentString.IsClosed"/>
//        Coordinate PrevInRing(int index);

//        /// <summary>
//        /// Gets the next vertex in a ring from a vertex index.
//        /// </summary>
//        /// <param name="index">The vertex index</param>
//        /// <returns>The next vertex in the ring</returns>
//        /// <seealso cref="ISegmentString.IsClosed"/>
//        Coordinate NextInRing(int index);
//    }
//}
