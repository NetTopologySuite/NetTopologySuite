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
        private readonly List<T> _items = new List<T>();

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
            _items.Add(item);
        }

        /// <summary>
        ///
        /// </summary>
        public IList<T> Items => _items;
    }
}
