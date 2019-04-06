namespace GeoAPI.Geometries
{
    /// <summary>
    /// An interface for classes which use the values of the coordinates in a <see cref="IGeometry"/>. 
    /// Coordinate filters can be used to implement centroid and
    /// envelope computation, and many other functions.<para/>
    /// <c>ICoordinateFilter</c> is
    /// an example of the Gang-of-Four Visitor pattern. 
    /// <para/>
    /// <b>Note</b>: it is not recommended to use these filters to mutate the coordinates.
    /// There is no guarantee that the coordinate is the actual object stored in the geometry.
    /// In particular, modified values may not be preserved if the target Geometry uses a non-default <see cref="ICoordinateSequence"/>.
    /// If in-place mutation is required, use <see cref="ICoordinateSequenceFilter"/>.
    /// </summary>
    /// <seealso cref="IGeometry.Apply(ICoordinateFilter)"/>
    /// <seealso cref="ICoordinateSequenceFilter"/>
    public interface ICoordinateFilter
    {
        /// <summary>
	    /// Performs an operation with or on <c>coord</c>.
    	/// </summary>
        /// <param name="coord"><c>Coordinate</c> to which the filter is applied.</param>
    	void Filter(Coordinate coord);
    }

}
