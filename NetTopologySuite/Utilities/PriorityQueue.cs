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
        where T: IComparable<T>
    {
        private int _size; // Number of elements in queue
        private readonly IList<T> _items; // The queue binary heap array

        ///<summary>
        /// Creates a new empty priority queue
        ///</summary>
        public PriorityQueue()
        {
            _size = 0;
            _items = new List<T>();
            // create space for sentinel
            _items.Add(default(T));
        }

        ///<summary>Insert into the priority queue. Duplicates are allowed.
        ///</summary>
        /// <param name="x">The item to insert.</param>
        public void Add(T x)
        {
            // increase the size of the items heap to create a hole for the new item
            _items.Add(default(T));

            // Insert item at end of heap and then re-establish ordering
            _size += 1;
            int hole = _size;
            // set the item as a sentinel at the base of the heap
            _items[0] = x;

            // move the item up from the hole position to its correct place
            for (; x.CompareTo(_items[hole / 2]) < 0; hole /= 2)
            {
                _items[hole] = _items[hole / 2];
            }
            // insert the new item in the correct place
            _items[hole] = x;
        }

        /**
         * Establish heap from an arbitrary arrangement of items. 
         */
        /*
         private void buildHeap( ) {
         for( int i = currentSize / 2; i > 0; i-- )
         reorder( i );
         }
         */

        ///<summary>
        /// Test if the priority queue is logically empty.
        ///</summary>
        /// <returns><c>true</c> if empty, <c>false</c> otherwise.</returns>
        public bool IsEmpty()
        {
            return _size == 0;
        }

        ///<summary>
        /// Returns size.
        ///</summary>
        public int Size
        {
            get { return _size; }
        }

        ///<summary>
        /// Make the priority queue logically empty.
        ///</summary>
        public void Clear()
        {
            _size = 0;
            _items.Clear();
        }

        ///<summary>
        /// Remove the smallest item from the priority queue.
        ///</summary>
        /// <remarks>The smallest item, <see cref="default(T)"/> if empty.</remarks>
        public T Poll()
        {
            if (IsEmpty())
                return default(T);
            T minItem = _items[1];
            _items[1] = _items[_size];
            _size -= 1;
            Reorder(1);

            return minItem;
        }

        ///<summary>
        /// Private method to percolate down in the heap.
        ///</summary>
        /// <param name="hole">The index at which the percolate begins.</param>
        private void Reorder(int hole)
        {
            int child;
            T tmp = _items[hole];

            for (; hole * 2 <= _size; hole = child)
            {
                child = hole * 2;
                if (child != _size
                    && (_items[child + 1]).CompareTo(_items[child]) < 0)
                    child++;
                if ((_items[child]).CompareTo(tmp) < 0)
                    _items[hole] = _items[child];
                else
                    break;
            }
            _items[hole]=tmp;
        }
    }
}