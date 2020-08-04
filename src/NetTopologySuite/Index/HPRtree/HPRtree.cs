using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.Strtree;

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

        private static int DEFAULT_NODE_CAPACITY = 16;

        private readonly List<Item<T>> _items = new List<Item<T>>();

        private readonly int _nodeCapacity;

        private readonly Envelope _totalExtent = new Envelope();

        private int[] _layerStartIndex;

        private double[] _nodeBounds;

        private bool _isBuilt;

        //public int nodeIntersectsCount;

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
            get => _items.Count;
        }

        /// <inheritdoc cref="ISpatialIndex{T}.Insert" />
        public void Insert(Envelope itemEnv, T item)
        {
            if (_isBuilt)
            {
                throw new InvalidOperationException("Cannot insert items after tree is built.");
            }

            _items.Add(new Item<T>(itemEnv, item));
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
            if (!Intersects(nodeIndex, searchEnv)) return;
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

        private bool Intersects(int nodeIndex, Envelope env)
        {
            //nodeIntersectsCount++;
            bool isBeyond = (env.MaxX < _nodeBounds[nodeIndex])
                            || (env.MaxY < _nodeBounds[nodeIndex + 1])
                            || (env.MinX > _nodeBounds[nodeIndex + 2])
                            || (env.MinY > _nodeBounds[nodeIndex + 3]);
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
                if (itemIndex >= _items.Count) break;

                // visit the item if its envelope intersects search env
                var item = _items[itemIndex];
                //nodeIntersectsCount++;
                if (Intersects(item.Envelope, searchEnv))
                {
                    //if (item.getEnvelope().intersects(searchEnv)) {
                    visitor.VisitItem(item.Value);
                }
            }
        }

        /// <summary>
        /// Tests whether two envelopes intersect.<para/>
        /// Avoids the <c>null</c> check in <see cref="Envelope.Intersects(Envelope)"/>.</summary>
        /// <param name="env1">An envelope</param>
        /// <param name="env2">An envelope</param>
        /// <returns><c>true</c> if the envelopes intersect</returns>
        private static bool Intersects(Envelope env1, Envelope env2)
        {
            return !(env2.MinX > env1.MaxX ||
                     env2.MaxX < env1.MinX ||
                     env2.MinY > env1.MaxY ||
                     env2.MaxY < env1.MinY);
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

            Monitor.Enter(_items);
            _isBuilt = true;
            // don't need to build an empty or very small tree
            if (_items.Count <= _nodeCapacity) return;

            SortItems();
            //dumpItems(items);

            _layerStartIndex = ComputeLayerIndices(_items.Count, _nodeCapacity);
            // allocate storage
            int nodeCount = _layerStartIndex[_layerStartIndex.Length - 1] / 4;
            _nodeBounds = CreateBoundsArray(nodeCount);

            // compute tree nodes
            ComputeLeafNodes(_layerStartIndex[1]);
            for (int i = 1; i < _layerStartIndex.Length - 1; i++)
            {
                ComputeLayerNodes(i);
            }

            //dumpNodes();
        }

        /*
        private void dumpNodes() {
          GeometryFactory fact = new GeometryFactory();
          for (int i = 0; i < nodeMinX.length; i++) {
            Envelope env = new Envelope(nodeMinX[i], nodeMaxX[i], nodeMinY[i], nodeMaxY[i]);;
            System.out.println(fact.toGeometry(env));
          }
        }

        private static void dumpItems(IList<Item<T>> items)
        {
            var fact = GeometryFactory.Default;
            foreach (var item in items)
            {
                var env = item.Envelope;
                Console.WriteLine(fact.ToGeometry(env));
            }
        }
      */

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
                //System.out.println("Layer: " + layerIndex + " node: " + i + " - " + getNodeEnvelope(layerStart + i));
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
                if (itemIndex >= _items.Count) break;
                var env = _items[itemIndex].Envelope;
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

        private Envelope GetNodeEnvelope(int i)
        {
            //return new Envelope(nodeBounds[i], nodeBounds[i + 1], nodeBounds[i + 2], nodeBounds[i + 3]);
            return new Envelope(_nodeBounds[i], _nodeBounds[i + 2], _nodeBounds[i + 1], _nodeBounds[i + 3]);
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
                bounds[i] = GetNodeEnvelope(boundIndex);
                //          new Envelope(nodeBounds[boundIndex], nodeBounds[boundIndex + 2],
                //                       nodeBounds[boundIndex + 1], nodeBounds[boundIndex + 3]);
            }

            return bounds;
        }

        private void SortItems()
        {
            var comp = new ItemComparator(new HilbertEncoder(HILBERT_LEVEL, _totalExtent));
            _items.Sort(comp);
        }

        private class ItemComparator : IComparer<Item<T>>
        {
            private readonly HilbertEncoder _encoder;

            public ItemComparator(HilbertEncoder encoder)
            {
                _encoder = encoder;
            }

            public int Compare(Item<T> item1, Item<T> item2)
            {
                if (item1 == null)
                    throw new ArgumentNullException(nameof(item1));
                if (item2 == null)
                    throw new ArgumentNullException(nameof(item1));

                int hcode1 = _encoder.Encode(item1.Envelope);
                int hcode2 = _encoder.Encode(item2.Envelope );
                return hcode1.CompareTo(hcode2);
            }
        }

    }
}
