using System;

namespace GeoAPI.Geometries
{
    /// <summary>
    /// <c>Geometry</c> classes support the concept of applying
    /// an <c>IGeometryComponentFilter</c> filter to the <c>Geometry</c>.
    /// The filter is applied to every component of the <c>Geometry</c>
    /// which is itself a <c>Geometry</c>.
    /// (For instance, all the LinearRings in Polygons are visited.)
    /// An <c>IGeometryComponentFilter</c> filter can either
    /// record information about the <c>Geometry</c>
    /// or change the <c>Geometry</c> in some way.
    /// <c>IGeometryComponentFilter</c> is an example of the Gang-of-Four Visitor pattern.
    /// </summary>    
    public interface IGeometryComponentFilter
    {
        /// <summary>
        /// Performs an operation with or on <c>geom</c>.
        /// </summary>
        /// <param name="geom">A <c>Geometry</c> to which the filter is applied.</param>
        void Filter(IGeometry geom);
    }
}
