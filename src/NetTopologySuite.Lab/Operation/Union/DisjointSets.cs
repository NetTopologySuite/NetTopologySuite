using System;

namespace NetTopologySuite.Operation.Union
{
    /// <summary>
    /// A data structure to represent disjoint (partitioned) sets,
    /// and allow merging them. <br/>
    /// See the Wikipedia article on <a href='https://en.wikipedia.org/wiki/Disjoint-set_data_structure'>disjointiset data structures</a>.
    /// </summary>
    /// <author>mdavis</author>
    public class DisjointSets
    {
        private readonly int[] parent;
        private readonly int[] partSize;
        private int numSets;
        private int[] parts;
        private int[] setItem;
        private int[] setSize;
        private int[] setStart;

        public DisjointSets(int size)
        {
            parent = ArrayIndex(size);
            partSize = ArrayValue(size, 1);
            numSets = size;
        }

        public bool InInSameSet(int i, int j)
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
            if ((partSize[rootj] > partSize[rooti])
                || (partSize[rooti] == partSize[rootj] && rootj <= rooti))
            {
                src = rootj;
                dest = rooti;
            }

            parent[src] = parent[dest];
            partSize[dest] += partSize[src];
            partSize[src] = 0;

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

        private int[] ArrayIndex(int size)
        {
            int[] arr = new int[size];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = i;
            }

            return arr;
        }

        private static int[] ArrayValue(int size, int val)
        {
            int[] arr = new int[size];
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = val;
            }

            return arr;
        }

        private void Compute()
        {
            //--- sort set items by root and index, 
            setItem = SortItems();

            //--- compute start and size of each set
            setSize = new int[numSets];
            setStart = new int[numSets];
            int currRoot = FindRoot(setItem[0]);
            setStart[0] = 0;
            int iSet = 0;
            for (int i = 1; i < setItem.Length; i++)
            {
                int root = FindRoot(setItem[i]);
                if (root != currRoot)
                {
                    setSize[iSet] = i - setStart[iSet];
                    iSet++;
                    setStart[iSet] = i;
                    currRoot = root;
                }
            }

            setSize[numSets - 1] = setItem.Length - setStart[numSets - 1];
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

        public int NumSets
        {
            get
            {
                Compute();
                return numSets;
            }
        }

        public int GetSetSize(int s)
        {
            return setSize[s];
        }

        public int GetSetItem(int s, int i)
        {
            int index = setStart[s] + i;
            return setItem[index];
        }
    }
}
