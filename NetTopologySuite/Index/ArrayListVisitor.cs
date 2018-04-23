using System.Collections.Generic;

namespace NetTopologySuite.Index
{
    public class ArrayListVisitor : ArrayListVisitor<object>
    {}
    /// <summary>
    /// 
    /// </summary>
    public class ArrayListVisitor<T> : IItemVisitor<T>
    {
        ///// <summary>
        ///// 
        ///// </summary>
        //public ArrayListVisitor() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void VisitItem(T item)
        {
            Items.Add(item);
        }

        /// <summary>
        /// 
        /// </summary>
        public IList<T> Items { get; } = new List<T>();
    }
}
