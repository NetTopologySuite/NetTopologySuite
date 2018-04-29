using System;
using GeoAPI.Geometries;

namespace NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// A function method which computes the distance
    /// between two <see cref="IBoundable{T, TItem}"/>s in an <see cref="STRtree{TItem}"/>.
    /// Used for Nearest Neighbour searches.
    /// </summary>
    /// <author>Martin Davis</author>
    public interface IItemDistance<T, TItem> where T : IIntersectable<T>, IExpandable<T>
    {
        /// <summary>
        /// Computes the distance between two items.
        /// </summary>
        /// <param name="item1">The first item.</param>
        /// <param name="item2">The second item.</param>
        /// <exception cref="ArgumentException">If the metric is not applicable to the arguments</exception>
        /// <returns>The distance between <paramref name="item1"/> and <paramref name="item2"/>.</returns>
        double Distance(IBoundable<T, TItem> item1, IBoundable<T, TItem> item2);

    }
}