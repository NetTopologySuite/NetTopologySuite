namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// <c>GeometryCollection</c> classes support the concept of
    /// applying a <c>IGeometryFilter</c> to the <c>Geometry</c>.
    /// The filter is applied to every element <c>Geometry</c>.
    /// A <c>IGeometryFilter</c> can either record information about the <c>Geometry</c>
    /// or change the <c>Geometry</c> in some way.
    /// <c>IGeometryFilter</c> is an example of the Gang-of-Four Visitor pattern.
    /// </summary>
    public interface IGeometryFilter
    {
        /// <summary>
        /// Performs an operation with or on <c>geom</c>.
        /// </summary>
        /// <param name="geom">A <c>Geometry</c> to which the filter is applied.</param>
        void Filter(Geometry geom);
    }
}
