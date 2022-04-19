using NetTopologySuite.Geometries;
using NetTopologySuite.Mathematics;
using System.Collections.Generic;

namespace NetTopologySuite.Index
{
    /// <summary>
    /// A semi-static spatial index for points which occur
    /// in a spatially-coherent sequence.
    /// In particular, this is suitable for indexing the vertices
    /// of a {@link LineString} or {@link Polygon} ring.
    /// <para/>
    /// The index is constructed in a batch fashion on a given sequence of coordinates.
    /// Coordinates can be removed via the {@link #remove(int)} method.
    /// <para/>
    /// Note that this index queries only the individual points
    /// of the input coordinate sequence,
    /// <b>not</b> any line segments which might be lie between them.
    /// <para/>
    /// The input coordinate array is read-only,
    /// and is not changed when vertices are removed.
    /// </summary>
    /// <author>Martin Davis</author>
    public class VertexSequencePackedRtree
    {
        /// <summary>
        /// Number of items/nodes in a parent node.
        /// Determined empirically.  Performance is not too sensitive to this.
        /// </summary>
        private const int NodeCapacity = 16;

        private readonly Coordinate[] _items;
        private int[] _levelOffset;
        private readonly int _nodeCapacity = NodeCapacity;
        private Envelope[] _bounds;
        private bool[] _isRemoved;

        /// <summary>
        /// Creates a new tree over the given sequence of coordinates.
        /// The sequence should be spatially coherent to provide query performance.
        /// </summary>
        /// <param name="pts">A sequence of points</param>
        public VertexSequencePackedRtree(Coordinate[] pts)
        {
            _items = pts;
            _isRemoved = new bool[pts.Length];
            Build();
        }

        public Envelope[] GetBounds()
        {
            return (Envelope[])_bounds.Clone();
        }

        private void Build()
        {
            _levelOffset = ComputeLevelOffsets();
            _bounds = CreateBounds();
        }

        /// <summary>
        /// Computes the level offsets.
        /// This is the position in the <tt>bounds</tt> array of each level.
        /// The levelOffsets array includes a sentinel value of <c>offset[0] = 0</c>.
        /// The top level is always of size 1,
        /// and so also indicates the total number of bounds.
        /// </summary>
        /// <returns>The level offsets</returns>
        private int[] ComputeLevelOffsets()
        {
            var offsets = new List<int>();
            offsets.Add(0);
            int levelSize = _items.Length;
            int currOffset = 0;
            do
            {
                levelSize = LevelNodeCount(levelSize);
                currOffset += levelSize;
                offsets.Add(currOffset);
            } while (levelSize > 1);
            return offsets.ToArray();
        }

        private int LevelNodeCount(int numNodes)
        {
            return MathUtil.Ceiling(numNodes, _nodeCapacity);
        }

        private Envelope[] CreateBounds()
        {
            int boundsSize = _levelOffset[_levelOffset.Length - 1] + 1;
            var bounds = new Envelope[boundsSize];
            FillItemBounds(bounds);

            for (int lvl = 1; lvl < _levelOffset.Length; lvl++)
            {
                FillLevelBounds(lvl, bounds);
            }
            return bounds;
        }

        private void FillLevelBounds(int lvl, Envelope[] bounds)
        {
            int levelStart = _levelOffset[lvl - 1];
            int levelEnd = _levelOffset[lvl];
            int nodeStart = levelStart;
            int levelBoundIndex = _levelOffset[lvl];
            do
            {
                int nodeEnd = MathUtil.ClampMax(nodeStart + _nodeCapacity, levelEnd);
                bounds[levelBoundIndex++] = ComputeNodeEnvelope(bounds, nodeStart, nodeEnd);
                nodeStart = nodeEnd;
            }
            while (nodeStart < levelEnd);
        }

        private void FillItemBounds(Envelope[] bounds)
        {
            int nodeStart = 0;
            int boundIndex = 0;
            do
            {
                int nodeEnd = MathUtil.ClampMax(nodeStart + _nodeCapacity, _items.Length);
                bounds[boundIndex++] = ComputeItemEnvelope(_items, nodeStart, nodeEnd);
                nodeStart = nodeEnd;
            }
            while (nodeStart < _items.Length);
        }

        private static Envelope ComputeNodeEnvelope(Envelope[] bounds, int start, int end)
        {
            var env = new Envelope();
            for (int i = start; i < end; i++)
            {
                env.ExpandToInclude(bounds[i]);
            }
            return env;
        }

        private static Envelope ComputeItemEnvelope(Coordinate[] items, int start, int end)
        {
            var env = new Envelope();
            for (int i = start; i < end; i++)
            {
                env.ExpandToInclude(items[i]);
            }
            return env;
        }

        //------------------------

        /// <summary>
        /// Queries the index to find all items which intersect an extent.
        /// The query result is a list of the indices of input coordinates
        /// which intersect the extent.
        /// </summary>
        /// <param name="queryEnv">The query extent</param>
        /// <returns>An array of the indices of the input coordinates</returns>
        public int[] Query(Envelope queryEnv)
        {
            var resultList = new List<int>();
            int level = _levelOffset.Length - 1;
            QueryNode(queryEnv, level, 0, resultList);
            int[] result = resultList.ToArray();
            return result;
        }

        private void QueryNode(Envelope queryEnv, int level, int nodeIndex, List<int> resultList)
        {
            int boundsIndex = _levelOffset[level] + nodeIndex;
            var nodeEnv = _bounds[boundsIndex];
            //--- node is empty
            if (nodeEnv == null)
                return;
            if (!queryEnv.Intersects(nodeEnv))
                return;

            int childNodeIndex = nodeIndex * _nodeCapacity;
            if (level == 0)
            {
                QueryItemRange(queryEnv, childNodeIndex, resultList);
            }
            else
            {
                QueryNodeRange(queryEnv, level - 1, childNodeIndex, resultList);
            }
        }

        private void QueryNodeRange(Envelope queryEnv, int level, int nodeStartIndex, List<int> resultList)
        {
            int levelMax = LevelSize(level);
            for (int i = 0; i < _nodeCapacity; i++)
            {
                int index = nodeStartIndex + i;
                if (index >= levelMax)
                    return;
                QueryNode(queryEnv, level, index, resultList);
            }
        }

        private int LevelSize(int level)
        {
            return _levelOffset[level + 1] - _levelOffset[level];
        }

        private void QueryItemRange(Envelope queryEnv, int itemIndex, List<int> resultList)
        {
            for (int i = 0; i < _nodeCapacity; i++)
            {
                int index = itemIndex + i;
                if (index >= _items.Length)
                    return;
                var p = _items[index];
                if (!_isRemoved[index]
                    && queryEnv.Contains(p))
                    resultList.Add(index);
            }
        }

        //------------------------

        /// <summary>
        /// Removes the input item at the given index from the spatial index.
        /// </summary>
        /// <remarks>
        /// This does not change the underlying coordinate array.
        /// </remarks>
        /// <param name="index">Index the index of the item in the input</param>
        public void RemoveAt(int index)
        {
            _isRemoved[index] = true;

            //--- prune the item parent node if all its items are removed
            int nodeIndex = index / _nodeCapacity;
            if (!IsItemsNodeEmpty(nodeIndex))
                return;

            _bounds[nodeIndex] = null;

            if (_levelOffset.Length <= 2)
                return;

            //-- prune the node parent if all children removed
            int nodeLevelIndex = nodeIndex / _nodeCapacity;
            if (!IsNodeEmpty(1, nodeLevelIndex))
                return;
            int nodeIndex1 = _levelOffset[1] + nodeLevelIndex;
            _bounds[nodeIndex1] = null;

            //TODO: propagate removal up the tree nodes?
        }

        private bool IsNodeEmpty(int level, int index)
        {
            int start = index * _nodeCapacity;
            int end = MathUtil.ClampMax(start + _nodeCapacity, _levelOffset[level]);
            for (int i = start; i < end; i++)
            {
                if (_bounds[i] != null) return false;
            }
            return true;
        }

        private bool IsItemsNodeEmpty(int nodeIndex)
        {
            int start = nodeIndex * _nodeCapacity;
            int end = MathUtil.ClampMax(start + _nodeCapacity, _items.Length);
            for (int i = start; i < end; i++)
            {
                if (!_isRemoved[i]) return false;
            }
            return true;
        }

    }

}
