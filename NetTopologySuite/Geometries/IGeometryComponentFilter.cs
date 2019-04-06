namespace GeoAPI.Geometries
{
    /// <summary>
    /// <c>Geometry</c> classes support the concept of applying
    /// an <c>IGeometryComponentFilter</c> filter to the <c>Geometry</c>.
    /// </summary>
    /// <remarks>
    /// The filter is applied to every component of the <c>Geometry</c>
    /// which is itself a <c>Geometry</c>
    /// and which does not itself contain any components.
    /// (For instance, all the LinearRings in Polygons are visited,
    /// but in a MultiPolygon the Polygons themselves are not visited.)
    /// Thus the only classes of Geometry which must be 
    /// handled as arguments to <see cref="Filter"/>
    /// are <see cref="ILineString"/>s, <see cref="ILinearRing"/>s and <see cref="IPoint"/>s.
    /// An <c>IGeometryComponentFilter</c> filter can either
    /// record information about the <c>Geometry</c>
    /// or change the <c>Geometry</c> in some way.
    /// <c>IGeometryComponentFilter</c> is an example of the Gang-of-Four Visitor pattern.
    /// </remarks>>    
    public interface IGeometryComponentFilter
    {
        /// <summary>
        /// Performs an operation with or on <c>geom</c>.
        /// </summary>
        /// <param name="geom">A <c>Geometry</c> to which the filter is applied.</param>
        void Filter(IGeometry geom);
    }
}
