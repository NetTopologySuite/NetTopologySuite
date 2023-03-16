using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace NetTopologySuite.Utilities
{
    /// <summary>
    /// A priority queue over a set of <see cref="IComparable{T}"/> objects.
    /// </summary>
    /// <typeparam name="T">Objects to add</typeparam>
    /// <author>Martin Davis</author>
    public class PriorityQueue<T> : IEnumerable<T>
        where T : IComparable<T>
    {
        private readonly AlternativePriorityQueue<T, T> _queue;

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        public PriorityQueue()
        {
            _queue = new AlternativePriorityQueue<T, T>();
        }

        /// <summary>
        /// Creates an instance of this class
        /// </summary>
        /// <param name="capacity">The capacity of the queue</param>
        /// <param name="comparer">The comparer to use for computing priority values</param>
        public PriorityQueue(int capacity, IComparer<T> comparer)
        {
            _queue = new AlternativePriorityQueue<T, T>(capacity, comparer);
        }

        /// <summary>Insert into the priority queue. Duplicates are allowed.
        /// </summary>
        /// <param name="x">The item to insert.</param>
        public void Add(T x)
        {
            var node = new PriorityQueueNode<T, T>(x);
            this._queue.Enqueue(node, x);
        }

        /// <summary>
        /// Test if the priority queue is logically empty.
        /// </summary>
        /// <returns><c>true</c> if empty, <c>false</c> otherwise.</returns>
        public bool IsEmpty()
        {
            return this._queue.Count == 0;
        }

        /// <summary>
        /// Returns size.
        /// </summary>
        public int Size => this._queue.Count;

        /// <summary>
        /// Make the priority queue logically empty.
        /// </summary>
        public void Clear()
        {
            this._queue.Clear();
        }

        /// <summary>
        /// Remove the smallest item from the priority queue.
        /// </summary>
        /// <remarks>The smallest item, or <c>default(T)</c> if empty.</remarks>
        public T Poll()
        {
            var node = _queue.Dequeue();
            return node == null
                ? default
                : node.Data;
        }

        /// <summary>
        /// Gets the smallest item without removing it from the queue
        /// </summary>
        /// <returns></returns>
        public T Peek()
        {
            var node = _queue.Head;
            return node == null
                ? default
                : node.Data;
        }

        /// <inheritdoc cref="IEnumerable{T}.GetEnumerator()"/>>
        public IEnumerator<T> GetEnumerator()
        {
            return new DataEnumerator(_queue.GetEnumerator());
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class DataEnumerator : IEnumerator<T>
        {
            private readonly IEnumerator<PriorityQueueNode<T, T>> _pqnEnumerator;
            public DataEnumerator(IEnumerator<PriorityQueueNode<T, T>> pqnEnumerator)
            {
                _pqnEnumerator = pqnEnumerator;
            }

            public void Dispose()
            {
                _pqnEnumerator.Dispose();
            }

            public bool MoveNext()
            {
                return _pqnEnumerator.MoveNext();
            }

            public void Reset()
            {
                _pqnEnumerator.Reset();
            }

            public T Current
            {
                get
                {
                    var n = _pqnEnumerator.Current;
                    return n != null ? n.Data : default(T);
                }
            }

            object IEnumerator.Current => Current;
        }
    }
}
