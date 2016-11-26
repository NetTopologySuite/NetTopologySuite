using System;
using System.Collections.Generic;

namespace NetTopologySuite.Utilities
{
    ///<summary>
    /// A priority queue over a set of <see cref="IComparable{T}"/> objects.
    ///</summary>
    /// <typeparam name="T">Objects to add</typeparam>
    /// <author>Martin Davis</author>
    public class PriorityQueue<T>
        where T : IComparable<T>
    {
        private readonly AlternativePriorityQueue<T, T> queue = new AlternativePriorityQueue<T, T>();

        ///<summary>Insert into the priority queue. Duplicates are allowed.
        ///</summary>
        /// <param name="x">The item to insert.</param>
        public void Add(T x)
        {
            var node = new PriorityQueueNode<T, T>(x);
            this.queue.Enqueue(node, x);
        }

        ///<summary>
        /// Test if the priority queue is logically empty.
        ///</summary>
        /// <returns><c>true</c> if empty, <c>false</c> otherwise.</returns>
        public bool IsEmpty()
        {
            return this.queue.Count == 0;
        }

        ///<summary>
        /// Returns size.
        ///</summary>
        public int Size
        {
            get { return this.queue.Count; }
        }

        ///<summary>
        /// Make the priority queue logically empty.
        ///</summary>
        public void Clear()
        {
            this.queue.Clear();
        }

        ///<summary>
        /// Remove the smallest item from the priority queue.
        ///</summary>
        /// <remarks>The smallest item, or <value>default(T)</value> if empty.</remarks>
        public T Poll()
        {
            var node = this.queue.Dequeue();
            return node == null
                ? default(T)
                : node.Data;
        }
    }
}