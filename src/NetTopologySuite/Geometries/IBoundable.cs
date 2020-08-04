namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// A spatial object in an AbstractSTRtree.
    /// </summary>
    public interface IBoundable<out T, out TItem>
        where T: IIntersectable<T>, IExpandable<T>
    {
        /// <summary> 
        /// Returns a representation of space that encloses this Boundable, preferably
        /// not much bigger than this Boundable's boundary yet fast to test for intersection
        /// with the bounds of other Boundables. The class of object returned depends
        /// on the subclass of AbstractSTRtree.
        /// </summary>
        /// <returns> 
        /// An Envelope (for STRtrees), an Interval (for SIRtrees), or other object
        /// (for other subclasses of AbstractSTRtree).
        /// </returns>
        T Bounds { get; }

        /// <summary>
        /// Gets the item that is bounded
        /// </summary>
        TItem Item { get; }
    }
}