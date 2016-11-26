using System;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    ///     A priority queue over a set of <see cref="IComparable{T}" /> objects.
    /// </summary>
    /// <typeparam name="T">Objects to add</typeparam>
    /// <author>Martin Davis</author>
    public class PriorityQueue<T>
        where T : IComparable<T>
    {
        private readonly AlternativePriorityQueue<T, T> queue = new AlternativePriorityQueue<T, T>();

        /// <summary>
        ///     Returns size.
        /// </summary>
        public int Size => queue.Count;

        /// <summary>
        ///     Insert into the priority queue. Duplicates are allowed.
        /// </summary>
        /// <param name="x">The item to insert.</param>
        public void Add(T x)
        {
            var node = new PriorityQueueNode<T, T>(x);
            queue.Enqueue(node, x);
        }

        /// <summary>
        ///     Test if the priority queue is logically empty.
        /// </summary>
        /// <returns><c>true</c> if empty, <c>false</c> otherwise.</returns>
        public bool IsEmpty()
        {
            return queue.Count == 0;
        }

        /// <summary>
        ///     Make the priority queue logically empty.
        /// </summary>
        public void Clear()
        {
            queue.Clear();
        }

        /// <summary>
        ///     Remove the smallest item from the priority queue.
        /// </summary>
        /// <remarks>The smallest item, or
        ///     <value>default(T)</value>
        ///     if empty.
        /// </remarks>
        public T Poll()
        {
            var node = queue.Dequeue();
            return node == null
                ? default(T)
                : node.Data;
        }
    }
}