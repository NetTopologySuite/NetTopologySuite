using System;

namespace NetTopologySuite.Operation.Union
{
    /// <summary>
    /// A data structure that represents a partition of a set
    /// into disjoint subsets, and allows merging subsets.
    /// Set items are represented by integer indices
    /// (which will typically be an index into an array
    /// of the objects actually being partitioned).
    /// Initially each item is in its own subset.
    /// Client code can merge subsets of items as required for the
    /// algorithm being performed (e.g.set partitioning or clustering).
    /// The current partitioning can be computed at any time
    /// and subset items accessed
    /// using the <see cref="Subsets"/> accessor.
    /// <para/>
    /// See the Wikipedia article on
    /// <a href='https://en.wikipedia.org/wiki/Disjoint-set_data_structure'>disjoint-set data structures</a>.
    /// </summary>
    /// <author>Martin Davis</author>
    public class DisjointSets
    {
        private readonly int[] _parent;
        private readonly int[] _setSize;
        private int _numSets;
        //private int[] parts;
        //private int[] setItem;
        //private int[] setSize;
        //private int[] setStart;

        /// <summary>
        /// Creates a new set containing a given number of items
        /// </summary>
        /// <param name="size">The number of items contained in the set</param>
        public DisjointSets(int size)
        {
            _parent = ArrayOfIndex(size);
            _setSize = ArrayOfValue(size, 1);
            _numSets = size;
        }

        /// <summary>
        /// Tests if two items are in the same subset.
        /// </summary>
        /// <param name="i">An item index</param>
        /// <param name="j">Another item index</param>
        /// <returns><c>true</c> if items are in the same subset</returns>
        public bool IsInSameSubset(int i, int j)
        {
            return FindRoot(i) == FindRoot(j);
        }

        /// <summary>
        /// Merges two subsets containing the given items.
        /// Note that the items do not have to be the roots of
        /// their respective subsets.
        /// If the items are in the same subset
        /// the partitioning does not change.
        /// </summary>
        /// <param name="i">An item index</param>
        /// <param name="j">Another item index</param>
        public void Merge(int i, int j)
        {
            int rooti = FindRoot(i);
            int rootj = FindRoot(j);

            // already in same cluster
            if (rooti == rootj)
            {
                return;
            }

            // merge smaller cluster into larger
            int src = rooti;
            int dest = rootj;
            if ((_setSize[rootj] > _setSize[rooti])
                || (_setSize[rooti] == _setSize[rootj] && rootj <= rooti))
            {
                src = rootj;
                dest = rooti;
            }

            _parent[src] = _parent[dest];
            _setSize[dest] += _setSize[src];
            _setSize[src] = 0;

            _numSets--;
        }

        private int FindRoot(int i)
        {
            // find set root
            int root = i;
            while (_parent[root] != root)
            {
                // do path compression by halving
                _parent[root] = _parent[_parent[root]];
                root = _parent[root];
            }

            return root;
        }

        /// <summary>
        /// Gets a representation of the current partitioning.
        /// This creates a snapshot of the partitioning;
        /// the set can be merged further after this call.
        /// </summary>
        /// <returns>A representation of the current subset partitioning.</returns>
        public Subsets GetSubsets()
        {
            if (_numSets == 0)
                return new Subsets();

            //--- sort set items by root and index, 
            int[] items = ItemsSortedBySubset();

            //--- compute start and size of each set
            int[] size = new int[_numSets];
            int[] start = new int[_numSets];
            int currRoot = FindRoot(items[0]);
            start[0] = 0;
            int iSet = 0;
            for (int i = 1; i < items.Length; i++)
            {
                int root = FindRoot(items[i]);
                if (root != currRoot)
                {
                    size[iSet] = i - start[iSet];
                    iSet++;
                    start[iSet] = i;
                    currRoot = root;
                }
            }

            size[_numSets - 1] = items.Length - start[_numSets - 1];
            return new Subsets(items, size, start);
        }

        private int[] ItemsSortedBySubset()
        {
            // can only use comparator on Integer array
            int[] itemsSort = ArrayOfIntegerIndex(_parent.Length);

            // sort items by their subset root
            Array.Sort(itemsSort, (i1, i2) =>
            {
                int root1 = FindRoot(i1);
                int root2 = FindRoot(i2);
                if (root1 < root2) return -1;
                if (root1 > root2) return 1;
                // in same set - sort by value
                return i1.CompareTo(i2);
            });

            return itemsSort;
        }

        private static int[] ArrayOfIndex(int size)
        {
            int[] arr = new int[size];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = i;
            }

            return arr;
        }

        private static int[] ArrayOfIntegerIndex(int size)
        {
            int[] arr = new int[size];
            for (int i = 0; i < arr.Length; i++)
                arr[i] = i;

            return arr;
        }

        private static int[] ArrayOfValue(int size, int val)
        {
            int[] arr = new int[size];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = val;
            }

            return arr;
        }

        /// <summary>
        /// A representation of a partition of a set of items into disjoint subsets.
        /// It provides accessors for the number of subsets,
        /// the size of each subset, and the items of each subset.
        /// <para/>
        /// The item indices in each subset are sorted.
        /// This means that the item ordering is stable; that is,
        /// the items have the same order they did in the original set.
        /// </summary>
        public class Subsets
        {
            private readonly int[] _item;
            private readonly int[] _size;
            private readonly int[] _start;

            public Subsets()
            {
                _item = null;
                _size = Array.Empty<int>();
                _start = null;
            }

            public Subsets(int[] item, int[] size, int[] start)
            {
                _item = item;
                _size = size;
                _start = start;
            }

            /// <summary>
            /// Gets the number of disjoint subsets.
            /// </summary>
            /// <returns>The number of subsets</returns>
            public int Count => _size.Length;

            /// <summary>
            /// Gets the number of items in a given subset.
            /// </summary>
            /// <param name="s">The subset index</param>
            /// <returns>The size of the subset</returns>
            public int GetSize(int s)
            {
                if (s < 0 || s >= _size.Length)
                    throw new ArgumentOutOfRangeException(nameof(s), $"Subset index out of range: {s}");

                return _size[s];
            }

            /// <summary>
            /// Gets an item from a subset
            /// </summary>
            /// <param name="s">The subset index</param>
            /// <param name="i">The index of the item in the subset</param>
            /// <returns>The item</returns>
            public int GetItem(int s, int i)
            {
                if (s < 0 || s >= _size.Length)
                    throw new ArgumentOutOfRangeException(nameof(s), $"Subset index out of range: {s}");
                int index = _start[s] + i;
                if (index < 0 || index >= _item.Length)
                    throw new ArgumentOutOfRangeException(nameof(i), $"Item index out of range: {i}");
                return _item[index];
            }
        }
    }
}
