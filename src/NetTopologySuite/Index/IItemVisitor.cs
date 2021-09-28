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

    public interface ILimitingItemVisitor<in T> : IItemVisitor<T>
    {
        bool IsDone { get; }
    }
}
