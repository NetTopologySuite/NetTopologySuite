using NetTopologySuite.Geometries;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Strtree;

namespace NetTopologySuite.Operation.Union
{
    public class SpatialPartition
    {
        public interface IRelation
        {
            bool IsEquivalent(int i1, int i2);
        }

        private STRtree<int> _index;
        private readonly Geometry[] _geoms;
        private DisjointSets _dset;

        public SpatialPartition(Geometry[] geoms, IRelation rel)
        {
            _geoms = geoms;
            Build(rel);
        }

        private void Build(IRelation rel)
        {
            LoadIndex(/*geoms*/);

            _dset = new DisjointSets(_geoms.Length);
            //--- cluster the geometries
            for (int i = 0; i < _geoms.Length; i++)
            {

                int queryIndex = i;
                var queryGeom = _geoms[i];
                _index.Query(queryGeom.EnvelopeInternal, new SpatialPartitionVisitor(rel, _dset, queryIndex));
            }
        }

        private void LoadIndex(/*Geometry[] geoms*/)
        {
            _index = new STRtree<int>();
            for (int i = 0; i < _geoms.Length; i++)
                _index.Insert(_geoms[i].EnvelopeInternal, i);
        }

        public int NumSets => _dset.NumSets;
        
        public int GetSetSize(int s)
        {
            return _dset.GetSetSize(s);
        }
        public int GetSetItem(int s, int i)
        {
            return _dset.GetSetItem(s, i);
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
                if (itemIndex == _queryIndex) return;
                // avoid duplicate intersections
                if (itemIndex < _queryIndex) return;
                if (_dset.InInSameSet(_queryIndex, itemIndex)) return;

                if (_rel.IsEquivalent(_queryIndex, itemIndex))
                {
                    // geometries are in same partition
                    _dset.Merge(_queryIndex, itemIndex);
                }
            }
        }
    }

}
