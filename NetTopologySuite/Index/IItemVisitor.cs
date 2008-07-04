namespace GisSharpBlog.NetTopologySuite.Index
{
    /// <summary>
    /// A visitor for items in an index.
    /// </summary>
    public interface IItemVisitor
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        void VisitItem(object item);
    }
}
