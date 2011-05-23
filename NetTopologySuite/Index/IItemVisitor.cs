namespace GisSharpBlog.NetTopologySuite.Index
{
    /// <summary>
    /// A visitor for items in an index.
    /// </summary>
    public interface IItemVisitor : IItemVisitor<object>
    {
    }

    /// <summary>
    /// A visitor for items in an index.
    /// </summary>
    public interface IItemVisitor<in T>
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        void VisitItem(T item);
    }
}
