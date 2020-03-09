using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// An <see cref="IItemDistance{Envelope, Geometry}"/> function for
    /// items which are <see cref="Geometry"/> using the <see cref="Geometry.Distance(Geometry)"/> method.
    /// <para/>
    /// To make this distance function suitable for
    /// using to query a single index tree,
    /// the distance metric is <i>anti-reflexive</i>.
    /// That is, if the two arguments are the same Geometry object,
    /// the distance returned is <see cref="double.MaxValue"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    public class GeometryItemDistance : IItemDistance<Envelope, Geometry>
    {
        /// <summary>
        /// Computes the distance between two <see cref="Geometry"/> items,
        /// using the <see cref="Geometry.Distance(Geometry)"/> method.
        /// </summary>
        /// <param name="item1">An item which is a geometry.</param>
        /// <param name="item2">An item which is a geometry.</param>
        /// <exception cref="InvalidCastException">if either item is not a Geometry</exception>
        /// <returns>The distance between the two items.</returns>
        public double Distance(IBoundable<Envelope, Geometry> item1, IBoundable<Envelope, Geometry> item2)
        {
            if (item1 == item2) return double.MaxValue;

            var g1 = item1.Item;
            var g2 = item2.Item;
            return g1.Distance(g2);
        }
    }
}
