using System;

namespace NetTopologySuite.Operation.Union
{
    /// <summary>
    /// A data structure that represents a partition of a set
    /// into disjoint subsets, and allows merging subsets.
    /// Set items are represented by integer indices.
    /// Initially each item is in its own subset.
    /// Client code can merge subsets of items as required for the
    /// algorithm being performed (e.g.set partitioning or clustering).
    /// The current partitioning can be computed at any time,
    /// and subset items accessed by their indices.
    /// <para/>
    /// See the Wikipedia article on
    /// <a href='https://en.wikipedia.org/wiki/Disjoint-set_data_structure'>disjoint-set data structures</a>.
    /// </summary>
    /// <author>mdavis</author>
    public class DisjointSets
    {
        private readonly int[] parent;
        private readonly int[] partitionSize;
        private int numSets;
        //private int[] parts;
        //private int[] setItem;
        //private int[] setSize;
        //private int[] setStart;

        /// <summary>
        /// Creates a new structure containing a given number of items
        /// </summary>
        /// <param name="size">The number of items contained in the set</param>
        public DisjointSets(int size)
        {
            parent = ArrayOfIndex(size);
            partitionSize = ArrayOfValue(size, 1);
            numSets = size;
        }

        public bool IsSameSubset(int i, int j)
        {
            return FindRoot(i) == FindRoot(j);
        }

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
            if ((partitionSize[rootj] > partitionSize[rooti])
                || (partitionSize[rooti] == partitionSize[rootj] && rootj <= rooti))
            {
                src = rootj;
                dest = rooti;
            }

            parent[src] = parent[dest];
            partitionSize[dest] += partitionSize[src];
            partitionSize[src] = 0;

            numSets--;
        }

        private int FindRoot(int i)
        {

            // find set root
            int root = i;
            while (parent[root] != root)
            {
                // do path compression by halving
                parent[root] = parent[parent[root]];
                root = parent[root];
            }

            return root;
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

        private static int[] ArrayOfValue(int size, int val)
        {
            int[] arr = new int[size];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = val;
            }

            return arr;
        }

        public Subsets ComputeSubsets()
        {

            //--- sort set items by root and index, 
            int[] item = SortItems();

            //--- compute start and size of each set
            int[] size = new int[numSets];
            int[] start = new int[numSets];
            int currRoot = FindRoot(item[0]);
            start[0] = 0;
            int iSet = 0;
            for (int i = 1; i < item.Length; i++)
            {
                int root = FindRoot(item[i]);
                if (root != currRoot)
                {
                    size[iSet] = i - start[iSet];
                    iSet++;
                    start[iSet] = i;
                    currRoot = root;
                }
            }

            size[numSets - 1] = item.Length - start[numSets - 1];
            return new Subsets(item, size, start);
        }

        private int[] SortItems()
        {
            // can only use comparator on Integer array
            int[] itemsSort = new int[parent.Length];
            for (int i = 0; i < itemsSort.Length; i++)
            {
                itemsSort[i] = i;
            }

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

        /// <summary>
        /// Provides accessors for items in disjoint subsets
        /// </summary>
        public class Subsets
        {
            private readonly int[] _item;
            private readonly int[] _size;
            private readonly int[] _start;

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
            /// <param name="s">The number of the subset</param>
            /// <returns>The size of the subset</returns>
            public int GetSize(int s)
            {
                return _size[s];
            }

            /// <summary>
            /// Gets an item from a subset
            /// </summary>
            /// <param name="s">The subset number</param>
            /// <param name="i">The index of the item in the subset</param>
            /// <returns>The item</returns>
            public int GetItem(int s, int i)
            {
                int index = _start[s] + i;
                return _item[index];
            }
        }
    }
}
