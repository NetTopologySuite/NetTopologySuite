using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Strtree;

namespace NetTopologySuite.Operation.Union
{
    /// <summary>
    /// Computes a partition of a set of geometries into disjoint subsets,
    /// based on a provided equivalence <see cref="IEquivalenceRelation"/>.<br/>
    /// Uses a spatial index for efficient processing.
    /// </summary>
    /// <author>mdavis</author>
    public class SpatialPartition
    {
        /// <summary>
        /// An interface for a function to compute an equivalence relation.
        /// An equivalence relation must be symmetric, reflexive and transitive.
        /// Examples are <c>intersects</c> or <c>withinDistance</c>.
        /// </summary>
        public interface IEquivalenceRelation
        {
            /// <summary>
            /// Tests whether two geometry items are equivalent to each other under the relation.
            /// </summary>
            /// <param name="i">The index of a geometry</param>
            /// <param name="j">The index of another geometry</param>
            /// <returns><c>true</c> if the geometry items are equivalent</returns>
            bool IsEquivalent(int i, int j);

        }

        private readonly Geometry[] _geoms;
        private DisjointSets.Subsets _sets;

        public SpatialPartition(Geometry[] geoms, IEquivalenceRelation rel)
        {
            _geoms = geoms;
            _sets = Build(rel);
        }

        /// <summary>
        /// Gets the number of partitions
        /// </summary>
        /// <returns>The number of partitions</returns>
        public int Count => _sets.Count;

        /// <summary>
        /// Gets the number of geometries in a given partition
        /// </summary>
        /// <param name="s">The partition index</param>
        /// <returns>The size of the partition</returns>
        public int GetSize(int s)
        {
            return _sets.GetSize(s);
        }

        /// <summary>
        /// Gets the index of a geometry in a partition
        /// </summary>
        /// <param name="s">The partition index</param>
        /// <param name="i">The item index</param>
        /// <returns>The index of an item in a partition</returns>
        public int GetItem(int s, int i)
        {
            return _sets.GetItem(s, i);
        }

        /// <summary>
        /// Gets a geometry in a given partition
        /// </summary>
        /// <param name="s">The partition index</param>
        /// <param name="i">The item index</param>
        /// <returns>The geometry for the given partition and item index</returns>
        public Geometry GetGeometry(int s, int i)
        {
            return _geoms[GetItem(s, i)];
        }

        private DisjointSets.Subsets Build(IEquivalenceRelation rel)
        {
            var index = CreateIndex(/*geoms*/);

            var dset = new DisjointSets(_geoms.Length);
            //--- partition the geometries
            for (int i = 0; i < _geoms.Length; i++)
            {

                int queryIndex = i;
                var queryGeom = _geoms[i];
                index.Query(queryGeom.EnvelopeInternal, new SpatialPartitionVisitor(rel, dset, queryIndex));
            }

            return dset.GetSubsets();
        }

        private STRtree<int> CreateIndex(/*Geometry[] geoms*/)
        {
            var index = new STRtree<int>();
            for (int i = 0; i < _geoms.Length; i++)
                index.Insert(_geoms[i].EnvelopeInternal, i);
            return index;
        }

        private class SpatialPartitionVisitor : IItemVisitor<int>
        {
            private readonly IEquivalenceRelation _rel;
            private readonly DisjointSets _dset;
            private readonly int _queryIndex;

            public SpatialPartitionVisitor(IEquivalenceRelation rel, DisjointSets dset, int queryIndex)
            {
                _rel = rel;
                _dset = dset;
                _queryIndex = queryIndex;
            }

            public void VisitItem(int itemIndex)
            {
                // avoid reflexive and symmetric comparisons by comparing only lower to higher
                if (itemIndex <= _queryIndex) return;
                // already in same partition
                if (_dset.IsInSameSubset(_queryIndex, itemIndex)) return;

                if (_rel.IsEquivalent(_queryIndex, itemIndex))
                {
                    // geometries are in same partition
                    _dset.Merge(_queryIndex, itemIndex);
                }
            }
        }
    }

}
