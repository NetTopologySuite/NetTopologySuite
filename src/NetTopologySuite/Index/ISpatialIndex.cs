using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Index
{
    /// <summary>
    /// The basic insertion and query operations supported by classes
    /// implementing spatial index algorithms.
    /// A spatial index typically provides a primary filter for range rectangle queries. A
    /// secondary filter is required to test for exact intersection. Of course, this
    /// secondary filter may consist of other tests besides intersection, such as
    /// testing other kinds of spatial relationships.
    /// </summary>
    public interface ISpatialIndex<T>
    {
        /// <summary>
        /// Adds a spatial item with an extent specified by the given <c>Envelope</c> to the index.
        /// </summary>
        void Insert(Envelope itemEnv, T item);

        /// <summary>
        /// Queries the index for all items whose extents intersect the given search <c>Envelope</c>
        /// Note that some kinds of indexes may also return objects which do not in fact
        /// intersect the query envelope.
        /// </summary>
        /// <param name="searchEnv">The envelope to query for.</param>
        /// <returns>A list of the items found by the query.</returns>
        IList<T> Query(Envelope searchEnv);

        /// <summary>
        /// Queries the index for all items whose extents intersect the given search <see cref="Envelope" />,
        /// and applies an <see cref="IItemVisitor{T}" /> to them.
        /// Note that some kinds of indexes may also return objects which do not in fact
        /// intersect the query envelope.
        /// </summary>
        /// <param name="searchEnv">The envelope to query for.</param>
        /// <param name="visitor">A visitor object to apply to the items found.</param>
        void Query(Envelope searchEnv, IItemVisitor<T> visitor);

        /// <summary>
        /// Removes a single item from the tree.
        /// </summary>
        /// <param name="itemEnv">The Envelope of the item to remove.</param>
        /// <param name="item">The item to remove.</param>
        /// <returns> <c>true</c> if the item was found.</returns>
        bool Remove(Envelope itemEnv, T item);
    }

    public interface ISpatialIndexEx<T> : ISpatialIndex<T>
    {
        IEnumerable<T> Query(Envelope extent);
        IEnumerable<T> Query(Envelope extent, Func<T, bool> predicate);
    }
}
