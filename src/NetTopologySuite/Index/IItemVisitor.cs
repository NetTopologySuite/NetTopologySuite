using System;

namespace NetTopologySuite.Index
{
    /// <summary>
    /// A visitor for items in a <see cref="ISpatialIndex{T}"/>.
    /// </summary>
    public interface IItemVisitor<in T>
    {
        /// <summary>
        /// Visits an item in the index.
        /// </summary>
        /// <param name="item">The index item to be visited.</param>
        void VisitItem(T item);
    }

    /// <summary>
    /// A visitor for items in a <see cref="ISpatialIndex{T}"/>
    /// <para><b>Not used, commited by accident!</b></para>
    /// </summary>
    /// <typeparam name="T">The type of the items in the index</typeparam>
    /// [Obsolete]
    public interface ILimitingItemVisitor<in T> : IItemVisitor<T>
    {
        /// <summary>
        /// Gets a value indicating if no more items need to be visited
        /// </summary>
        bool IsDone { get; }
    }
}
