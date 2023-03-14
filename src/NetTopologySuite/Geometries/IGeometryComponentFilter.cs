namespace NetTopologySuite.Geometries
{
    /// <summary>
    /// <c>Geometry</c> classes support the concept of applying
    /// an <c>IGeometryComponentFilter</c> filter to a geometry.
    /// </summary>
    /// <remarks>
    /// The filter is applied to every component of a geometry
    /// as well as to the geometry itself.
    /// For instance, in a <see cref="Polygon"/>,
    /// all the <see cref="LinearRing"/>
    /// components for the shell and holes are visited,
    /// as well as the polygon itself.
    /// In order to process only atomic components,
    /// the <see cref="Filter"/> method code must
    /// explicitly handle only <see cref="LineString"/>s, <see cref="LinearRing"/>s and <see cref="Point"/>s.
    /// <para/>
    /// An <c>IGeometryComponentFilter</c> filter can either
    /// record information about the <c>Geometry</c>
    /// or change the <c>Geometry</c> in some way.
    /// <para/>
    /// <c>IGeometryComponentFilter</c> is an example of the Gang-of-Four Visitor pattern.
    /// </remarks>
    public interface IGeometryComponentFilter
    {
        /// <summary>
        /// Performs an operation with or on a geometry component.
        /// </summary>
        /// <param name="geom">A component of the geometry to which the filter is applied.</param>
        void Filter(Geometry geom);
    }
}
