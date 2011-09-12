#if !useFullGeoAPI
namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// <c>Geometry</c> classes support the concept of applying a
    /// coordinate filter to every coordinate in the <c>Geometry</c>. A
    /// coordinate filter can either record information about each coordinate or
    /// change the coordinate in some way. Coordinate filters implement the
    /// interface <c>ICoordinateFilter</c>. 
    /// <c>ICoordinateFilter</c> is an example of the Gang-of-Four Visitor pattern. 
    /// Coordinate filters can be
    /// used to implement such things as coordinate transformations, centroid and
    /// envelope computation, and many other functions.
    /// </summary>
    public interface ICoordinateFilter
    {
        /// <summary>
	    /// Performs an operation with or on <c>coord</c>.
    	/// </summary>
        /// <param name="coord"><c>Coordinate</c> to which the filter is applied.</param>
    	void Filter(Coordinate coord);
    }

}
#endif