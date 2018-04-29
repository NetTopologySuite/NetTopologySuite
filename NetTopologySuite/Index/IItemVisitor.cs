namespace NetTopologySuite.Index
{
    /// <summary>
    /// A visitor for items in an index.
    /// </summary>
    public interface IItemVisitor< T>
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="item"></param>
        void VisitItem(T item);
    }
}
