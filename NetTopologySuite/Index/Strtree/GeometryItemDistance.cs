using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// An <see cref="IItemDistance{Envelope, IGeometry}"/> function for
    /// items which are <see cref="IGeometry"/> using the <see cref="IGeometry.Distance(IGeometry)"/> method.
    /// </summary>
    /// <author>Martin Davis</author>
    public class GeometryItemDistance : IItemDistance<Envelope, IGeometry>
    {
        /// <summary>
        /// Computes the distance between two <see cref="IGeometry"/> items,
        /// using the <see cref="IGeometry.Distance(IGeometry)"/> method.
        /// </summary>
        /// <param name="item1">An item which is a geometry.</param>
        /// <param name="item2">An item which is a geometry.</param>
        /// <exception cref="InvalidCastException">if either item is not a Geometry</exception>
        /// <returns>The distance between the two items.</returns>
        public double Distance(IBoundable<Envelope, IGeometry> item1, IBoundable<Envelope, IGeometry> item2)
        {
            var g1 = item1.Item;
            var g2 = item2.Item;
            return g1.Distance(g2);
        }
    }
}