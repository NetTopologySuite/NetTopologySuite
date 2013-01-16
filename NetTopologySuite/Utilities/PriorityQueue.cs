#define Original
using System;
using System.Collections.Generic;

namespace NetTopologySuite.Utilities
{
#if Original
    ///<summary>
    /// A priority queue over a set of <see cref="IComparable{T}"/> objects.
    ///</summary>
    /// <typeparam name="T">Objects to add</typeparam>
    /// <author>Martin Davis</author>
    public class PriorityQueue<T>
        where T : IComparable<T>
    {
        private int _size; // Number of elements in queue
        private readonly List<T> _items; // The queue binary heap array

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
        /// <remarks>The smallest item, or <value>default(T)</value> if empty.</remarks>
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
            _items[hole] = tmp;
        }
    }

#elif Alternative
    public class PriorityQueue<T> where T : IComparable<T>
    {
        private List<T> data;

        public PriorityQueue()
        {
            this.data = new List<T>();
        }

        public void Add(T item) { Enqueue(item);}
        public T Poll() { return Dequeue(); }
        public bool IsEmpty() { return data.Count == 0;}
        public void Enqueue(T item)
        {
            data.Add(item);
            int ci = data.Count - 1; // child index; start at end
            while (ci > 0)
            {
                int pi = (ci - 1) / 2; // parent index
                if (data[ci].CompareTo(data[pi]) >= 0) break; // child item is larger than (or equal) parent so we're done
                T tmp = data[ci]; data[ci] = data[pi]; data[pi] = tmp;
                ci = pi;
            }
        }

        public T Dequeue()
        {
            // assumes pq is not empty; up to calling code
            int li = data.Count - 1; // last index (before removal)
            T frontItem = data[0];   // fetch the front
            data[0] = data[li];
            data.RemoveAt(li);

            --li; // last index (after removal)
            int pi = 0; // parent index. start at front of pq
            while (true)
            {
                int ci = pi * 2 + 1; // left child index of parent
                if (ci > li) break;  // no children so done
                int rc = ci + 1;     // right child
                if (rc <= li && data[rc].CompareTo(data[ci]) < 0) // if there is a rc (ci + 1), and it is smaller than left child, use the rc instead
                    ci = rc;
                if (data[pi].CompareTo(data[ci]) <= 0) break; // parent is smaller than (or equal to) smallest child so done
                T tmp = data[pi]; data[pi] = data[ci]; data[ci] = tmp; // swap parent and child
                pi = ci;
            }
            return frontItem;
        }

        public T Peek()
        {
            T frontItem = data[0];
            return frontItem;
        }

        public int Count()
        {
            return data.Count;
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < data.Count; ++i)
                s += data[i].ToString() + " ";
            s += "count = " + data.Count;
            return s;
        }

        public bool IsConsistent()
        {
            // is the heap property true for all data?
            if (data.Count == 0) return true;
            int li = data.Count - 1; // last index
            for (int pi = 0; pi < data.Count; ++pi) // each parent index
            {
                int lci = 2 * pi + 1; // left child index
                int rci = 2 * pi + 2; // right child index

                if (lci <= li && data[pi].CompareTo(data[lci]) > 0) return false; // if lc exists and it's greater than parent then bad.
                if (rci <= li && data[pi].CompareTo(data[rci]) > 0) return false; // check the right child too.
            }
            return true; // passed all checks
        } // IsConsistent
    } // PriorityQueue
#endif
}