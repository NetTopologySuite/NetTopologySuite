using System;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Index.Strtree
{
    /// <summary>
    /// A function method which computes the distance
    /// between two <see cref="IBoundable{T, TItem}"/>s in an <see cref="STRtree{TItem}"/>.
    /// Used for Nearest Neighbour searches.
    /// <para/>
    /// To make a distance function suitable for
    /// querying a single index tree
    /// via <see cref="STRtree{TItem}.NearestNeighbour(IItemDistance{Envelope,TItem})"/>,
    /// the function should have a non-zero <i>reflexive distance</i>.
    /// That is, if the two arguments are the same object,
    /// the distance returned should be non-zero.
    /// If it is required that only pairs of <b>distinct</b> items be returned,
    /// the distance function must be <i>anti-reflexive</i>,
    /// and must return <see cref="double.MaxValue"/> for identical arguments.
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
