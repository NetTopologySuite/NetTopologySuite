using System.Collections.Generic;

namespace NetTopologySuite.Index
{
    /// <summary>
    /// Builds an array of all visited items.
    /// </summary>
    public class ArrayListVisitor : ArrayListVisitor<object>
    {
    }

    /// <summary>
    /// Builds an array of all visited items.
    /// </summary>
    public class ArrayListVisitor<T> : IItemVisitor<T>
    {
        private readonly List<T> _items = new List<T>();

        /// <summary>
        /// Visits an item.
        /// </summary>
        /// <param name="item">The item to visit.</param>
        public void VisitItem(T item)
        {
            _items.Add(item);
        }

        /// <summary>
        /// Gets the array of visited items.
        /// </summary>
        public IList<T> Items => _items;
    }
}
