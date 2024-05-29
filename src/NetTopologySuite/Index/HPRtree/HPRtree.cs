using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;
using System;
using System.Collections.Generic;
using System.Threading;

namespace NetTopologySuite.Index.HPRtree
{
    /// <summary>
    /// A Hilbert-Packed R-tree.  This is a static R-tree
    /// which is packed by using the Hilbert ordering
    /// of the tree items.
    /// <para/>
    /// The tree is constructed by sorting the items
    /// by the Hilbert code of the midpoint of their envelope.
    /// Then, a set of internal layers is created recursively
    /// as follows:
    /// <list type="bullet">
    /// <item><term>The items/nodes of the previous are partitioned into blocks of size <c>nodeCapacity</c></term></item>
    /// <item><term>For each block a layer node is created with range equal to the envelope of the items/nodess in the block</term></item>
    /// </list>
    /// The internal layers are stored using an array to
    /// store the node bounds.
    /// The link between a node and its children is
    /// stored implicitly in the indexes of the array.
    /// For efficiency, the offsets to the layers
    /// within the node array are pre-computed and stored.
    /// <para/>
    /// NOTE: Based on performance testing,
    /// the HPRtree is somewhat faster than the STRtree.
    /// It should also be more memory-efficent,
    /// due to fewer object allocations.
    /// <para/>
    /// However, it is not clear whether this
    /// will produce a significant improvement
    /// for use in JTS operations.
    /// </summary>
    /// <seealso cref="STRtree{TItem}"/>
    /// <author>Martin Davis</author>
    public class HPRtree<T> : ISpatialIndex<T>
    {
        private const int ENV_SIZE = 4;

        private const int HILBERT_LEVEL = 12;

        private const int DEFAULT_NODE_CAPACITY = 16;

        private List<Item<T>> _itemsToLoad = new List<Item<T>>();

        private readonly int _nodeCapacity;

        private int _numItems;

        private readonly Envelope _totalExtent = new Envelope();

        private int[] _layerStartIndex;

        private double[] _nodeBounds;

        private double[] _itemBounds;

        private T[] _itemValues;

        private volatile bool _isBuilt;

        /// <summary>
        /// Creates a new index with the default node capacity.
        /// </summary>
        public HPRtree() : this(DEFAULT_NODE_CAPACITY)
        {
        }

        /// <summary>
        /// Creates a new index with the given node capacity.
        /// </summary>
        /// <param name="nodeCapacity">The node capacity to use</param>
        public HPRtree(int nodeCapacity)
        {
            _nodeCapacity = nodeCapacity;
        }

        /// <summary>Gets the number of items in the index.</summary>
        /// <returns>The number of items</returns>
        public int Count
        {
            get => _numItems;
        }

        /// <inheritdoc cref="ISpatialIndex{T}.Insert" />
        public void Insert(Envelope itemEnv, T item)
        {
            if (_isBuilt)
            {
                throw new InvalidOperationException("Cannot insert items after tree is built.");
            }

            _numItems++;
            _itemsToLoad.Add(new Item<T>(itemEnv, item));
            _totalExtent.ExpandToInclude(itemEnv);
        }

        /// <inheritdoc cref="ISpatialIndex{T}.Query(Envelope)" />
        public IList<T> Query(Envelope searchEnv)
        {
            Build();

            if (!_totalExtent.Intersects(searchEnv))
                return Array.Empty<T>();

            var visitor = new ArrayListVisitor<T>();
            Query(searchEnv, visitor);
            return visitor.Items;
        }

        /// <inheritdoc cref="ISpatialIndex{T}.Query(Envelope, IItemVisitor{T})" />
        public void Query(Envelope searchEnv, IItemVisitor<T> visitor)
        {
            Build();
            if (!_totalExtent.Intersects(searchEnv))
                return;
            if (_layerStartIndex == null)
            {
                QueryItems(0, searchEnv, visitor);
            }
            else
            {
                QueryTopLayer(searchEnv, visitor);
            }
        }

        private void QueryTopLayer(Envelope searchEnv, IItemVisitor<T> visitor)
        {
            int layerIndex = _layerStartIndex.Length - 2;
            int layerSize = GetLayerSize(layerIndex);
            // query each node in layer
            for (int i = 0; i < layerSize; i += ENV_SIZE)
            {
                QueryNode(layerIndex, i, searchEnv, visitor);
            }
        }

        private void QueryNode(int layerIndex, int nodeOffset, Envelope searchEnv, IItemVisitor<T> visitor)
        {
            int layerStart = _layerStartIndex[layerIndex];
            int nodeIndex = layerStart + nodeOffset;
            if (!Intersects(_nodeBounds, nodeIndex, searchEnv)) return;
            if (layerIndex == 0)
            {
                int childNodesOffset = nodeOffset / ENV_SIZE * _nodeCapacity;
                QueryItems(childNodesOffset, searchEnv, visitor);
            }
            else
            {
                int childNodesOffset = nodeOffset * _nodeCapacity;
                QueryNodeChildren(layerIndex - 1, childNodesOffset, searchEnv, visitor);
            }
        }

        private static bool Intersects(double[] bounds, int nodeIndex, Envelope env)
        {
            //nodeIntersectsCount++;
            bool isBeyond = (env.MaxX < bounds[nodeIndex])
                || (env.MaxY < bounds[nodeIndex + 1])
                || (env.MinX > bounds[nodeIndex + 2])
                || (env.MinY > bounds[nodeIndex + 3]);

            return !isBeyond;
        }

        private void QueryNodeChildren(int layerIndex, int blockOffset, Envelope searchEnv, IItemVisitor<T> visitor)
        {
            int layerStart = _layerStartIndex[layerIndex];
            int layerEnd = _layerStartIndex[layerIndex + 1];
            for (int i = 0; i < _nodeCapacity; i++)
            {
                int nodeOffset = blockOffset + ENV_SIZE * i;
                // don't query past layer end
                if (layerStart + nodeOffset >= layerEnd) break;

                QueryNode(layerIndex, nodeOffset, searchEnv, visitor);
            }
        }

        private void QueryItems(int blockStart, Envelope searchEnv, IItemVisitor<T> visitor)
        {
            for (int i = 0; i < _nodeCapacity; i++)
            {
                int itemIndex = blockStart + i;
                // don't query past end of items
                if (itemIndex >= _numItems) break;

                if (Intersects(_itemBounds, itemIndex * ENV_SIZE, searchEnv)) {
                    visitor.VisitItem(_itemValues[itemIndex]);
                }
            }
        }

        private int GetLayerSize(int layerIndex)
        {
            int layerStart = _layerStartIndex[layerIndex];
            int layerEnd = _layerStartIndex[layerIndex + 1];
            return layerEnd - layerStart;
        }

        /// <inheritdoc cref="ISpatialIndex{T}.Remove" />
        /// <remarks>Not supported, will always return <c>false</c></remarks>
        public bool Remove(Envelope itemEnv, T item)
        {
            // TODO Auto-generated method stub
            return false;
        }

        /// <summary>
        /// Builds the index, if not already built.
        /// </summary>
        public void Build()
        {
            // skip if already built
            if (_isBuilt) return;

            Monitor.Enter(this);
            if (!_isBuilt)
            {
                PrepareIndex();
                prepareItems();
                _isBuilt = true;
            }
            Monitor.Exit(this);
        }

        private void PrepareIndex()
        { 
            // don't need to build an empty or very small tree
            if (_numItems <= _nodeCapacity) return;

            SortItems();

            _layerStartIndex = ComputeLayerIndices(_numItems, _nodeCapacity);
            // allocate storage
            int nodeCount = _layerStartIndex[_layerStartIndex.Length - 1] / 4;
            _nodeBounds = CreateBoundsArray(nodeCount);

            // compute tree nodes
            ComputeLeafNodes(_layerStartIndex[1]);
            for (int i = 1; i < _layerStartIndex.Length - 1; i++)
            {
                ComputeLayerNodes(i);
            }
        }

        private void prepareItems()
        {
            // copy item contents out to arrays for querying
            int boundsIndex = 0;
            int valueIndex = 0;
            _itemBounds = new double[_numItems * 4];
            _itemValues = new T[_numItems];
            foreach (var item in _itemsToLoad)
            {
                var envelope = item.Envelope;
                _itemBounds[boundsIndex++] = envelope.MinX;
                _itemBounds[boundsIndex++] = envelope.MinY;
                _itemBounds[boundsIndex++] = envelope.MaxX;
                _itemBounds[boundsIndex++] = envelope.MaxY;
                _itemValues[valueIndex++] = item.Value;
            }
            // and let GC free the original list
            _itemsToLoad = null;
        }

        private static double[] CreateBoundsArray(int size)
        {
            double[] a = new double[4 * size];
            for (int i = 0; i < size; i++)
            {
                int index = 4 * i;
                a[index] = double.MaxValue;
                a[index + 1] = double.MaxValue;
                a[index + 2] = double.MinValue;
                a[index + 3] = double.MinValue;
            }

            return a;
        }

        private void ComputeLayerNodes(int layerIndex)
        {
            int layerStart = _layerStartIndex[layerIndex];
            int childLayerStart = _layerStartIndex[layerIndex - 1];
            int layerSize = GetLayerSize(layerIndex);
            int childLayerEnd = layerStart;
            for (int i = 0; i < layerSize; i += ENV_SIZE)
            {
                int childStart = childLayerStart + _nodeCapacity * i;
                ComputeNodeBounds(layerStart + i, childStart, childLayerEnd);
            }
        }

        private void ComputeNodeBounds(int nodeIndex, int blockStart, int nodeMaxIndex)
        {
            for (int i = 0; i <= _nodeCapacity; i++)
            {
                int index = blockStart + 4 * i;
                if (index >= nodeMaxIndex) break;
                UpdateNodeBounds(nodeIndex, _nodeBounds[index], _nodeBounds[index + 1], _nodeBounds[index + 2],
                    _nodeBounds[index + 3]);
            }
        }

        private void ComputeLeafNodes(int layerSize)
        {
            for (int i = 0; i < layerSize; i += ENV_SIZE)
            {
                ComputeLeafNodeBounds(i, _nodeCapacity * i / 4);
            }
        }

        private void ComputeLeafNodeBounds(int nodeIndex, int blockStart)
        {
            for (int i = 0; i <= _nodeCapacity; i++)
            {
                int itemIndex = blockStart + i;
                if (itemIndex >= _numItems) break;
                var env = _itemsToLoad[itemIndex].Envelope;
                UpdateNodeBounds(nodeIndex, env.MinX, env.MinY, env.MaxX, env.MaxY);
            }
        }

        private void UpdateNodeBounds(int nodeIndex, double minX, double minY, double maxX, double maxY)
        {
            if (minX < _nodeBounds[nodeIndex]) _nodeBounds[nodeIndex] = minX;
            if (minY < _nodeBounds[nodeIndex + 1]) _nodeBounds[nodeIndex + 1] = minY;
            if (maxX > _nodeBounds[nodeIndex + 2]) _nodeBounds[nodeIndex + 2] = maxX;
            if (maxY > _nodeBounds[nodeIndex + 3]) _nodeBounds[nodeIndex + 3] = maxY;
        }

        private static int[] ComputeLayerIndices(int itemSize, int nodeCapacity)
        {
            var layerIndexList = new List<int>();
            int layerSize = itemSize;
            int index = 0;
            do
            {
                layerIndexList.Add(index);
                layerSize = NumNodesToCover(layerSize, nodeCapacity);
                index += ENV_SIZE * layerSize;
            } while (layerSize > 1);

            return layerIndexList.ToArray();
        }

         /// <summary>
         /// Computes the number of blocks (nodes) required to
         /// cover a given number of children.
         /// </summary>
         /// <param name="nChild"></param>
         /// <param name="nodeCapacity"></param>
         /// <returns>the number of nodes needed to cover the children</returns>
        private static int NumNodesToCover(int nChild, int nodeCapacity)
        {
            int mult = nChild / nodeCapacity;
            int total = mult * nodeCapacity;
            if (total == nChild) return mult;
            return mult + 1;
        }

        /// <summary>
        /// Gets the extents of the internal index nodes
        /// </summary>
        /// <returns>A list of the internal node extents</returns>
        public Envelope[] GetBounds()
        {
            int numNodes = _nodeBounds.Length / 4;
            var bounds = new Envelope[numNodes];
            // create from largest to smallest
            for (int i = numNodes - 1; i >= 0; i--)
            {
                int boundIndex = 4 * i;
                bounds[i] = new Envelope(_nodeBounds[boundIndex], _nodeBounds[boundIndex + 2],
                                         _nodeBounds[boundIndex + 1], _nodeBounds[boundIndex + 3]);
            }

            return bounds;
        }

        private void SortItems()
        {
            var encoder = new HilbertEncoder(HILBERT_LEVEL, _totalExtent);
            int[] hilbertValues = new int[_numItems];
            int pos = 0;
            foreach (var item in _itemsToLoad)
            {
                hilbertValues[pos++] = encoder.Encode(item.Envelope);
            }
            QuickSortItemsIntoNodes(hilbertValues, 0, _numItems - 1);
        }

        private void QuickSortItemsIntoNodes(int[] values, int lo, int hi)
        {
            // stop sorting when left/right pointers are within the same node
            // because queryItems just searches through them all sequentially
            if (lo / _nodeCapacity < hi / _nodeCapacity)
            {
                int pivot = HoarePartition(values, lo, hi);
                QuickSortItemsIntoNodes(values, lo, pivot);
                QuickSortItemsIntoNodes(values, pivot + 1, hi);
            }
        }

        private int HoarePartition(int[] values, int lo, int hi)
        {
            int pivot = values[(lo + hi) >> 1];
            int i = lo - 1;
            int j = hi + 1;

            while (true)
            {
                do i++; while (values[i] < pivot);
                do j--; while (values[j] > pivot);
                if (i >= j) return j;
                SwapItems(values, i, j);
            }
        }

        private void SwapItems(int[] values, int i, int j)
        {
            var tmpItem = _itemsToLoad[i];
            _itemsToLoad[i] = _itemsToLoad[j];
            _itemsToLoad[j] = tmpItem;

            int tmpValue = values[i];
            values[i] = values[j];
            values[j] = tmpValue;
        }

    }
}
