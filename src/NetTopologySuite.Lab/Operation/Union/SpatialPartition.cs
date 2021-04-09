using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Strtree;

namespace NetTopologySuite.Operation.Union
{
    /// <summary>
    /// Computes a partition of a set of geometries into disjoint subsets,
    /// based on a provided equivalence relation.
    /// Uses a spatial index for efficient processing.
    /// </summary>
    /// <author>mdavis</author>
    public class SpatialPartition
    {
        public interface IRelation
        {
            bool IsEquivalent(int i, int j);
        }

        private readonly Geometry[] _geoms;
        private DisjointSets.Subsets _sets;

        public SpatialPartition(Geometry[] geoms, IRelation rel)
        {
            _geoms = geoms;
            _sets = Build(rel);
        }

        public int Count => _sets.Count;
        
        public int GetSize(int s)
        {
            return _sets.GetSize(s);
        }

        public int GetItem(int s, int i)
        {
            return _sets.GetItem(s, i);
        }

        public Geometry GetGeometry(int s, int i)
        {
            return _geoms[GetItem(s, i)];
        }

        private DisjointSets.Subsets Build(IRelation rel)
        {
            var index = CreateIndex(/*geoms*/);

            var dset = new DisjointSets(_geoms.Length);
            //--- cluster the geometries
            for (int i = 0; i < _geoms.Length; i++)
            {

                int queryIndex = i;
                var queryGeom = _geoms[i];
                index.Query(queryGeom.EnvelopeInternal, new SpatialPartitionVisitor(rel, dset, queryIndex));
            }

            return dset.ComputeSubsets();
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
            private readonly IRelation _rel;
            private readonly DisjointSets _dset;
            private readonly int _queryIndex;

            public SpatialPartitionVisitor(IRelation rel, DisjointSets dset, int queryIndex)
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
                if (_dset.IsSameSubset(_queryIndex, itemIndex)) return;

                if (_rel.IsEquivalent(_queryIndex, itemIndex))
                {
                    // geometries are in same partition
                    _dset.Merge(_queryIndex, itemIndex);
                }
            }
        }
    }

}
