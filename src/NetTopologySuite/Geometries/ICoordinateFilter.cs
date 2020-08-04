namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// An interface for classes which use the values of the coordinates in a <see cref="Geometry"/>. 
    /// Coordinate filters can be used to implement centroid and
    /// envelope computation, and many other functions.<para/>
    /// <para/>
    /// <c>ICoordinateFilter</c> is
    /// an example of the Gang-of-Four Visitor pattern. 
    /// <para/>
    /// <b>Note</b>: it is not recommended to use these filters to mutate the coordinates.
    /// There is no guarantee that the coordinate is the actual object stored in the source geometry.
    /// In particular, modified values may not be preserved if the source Geometry uses a non-default <see cref="CoordinateSequence"/>.
    /// If in-place mutation is required, use <see cref="ICoordinateSequenceFilter"/>.
    /// </summary>
    /// <seealso cref="Geometry.Apply(ICoordinateFilter)"/>
    /// <seealso cref="ICoordinateSequenceFilter"/>
    public interface ICoordinateFilter
    {
        /// <summary>
        /// Performs an operation with the provided <c>coord</c>.
        /// Note that there is no guarantee that the input coordinate 
        /// is the actual object stored in the source geometry,
        /// so changes to the coordinate object may not be persistent.
    	/// </summary>
        /// <param name="coord">A <c>Coordinate</c> to which the filter is applied.</param>
    	void Filter(Coordinate coord);
    }

}
