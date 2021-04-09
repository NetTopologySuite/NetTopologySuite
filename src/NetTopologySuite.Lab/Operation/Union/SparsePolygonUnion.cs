using System;
using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Prepared;
using NetTopologySuite.Geometries.Utilities;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Strtree;

namespace NetTopologySuite.Operation.Union
{
    /**
     * Unions a sparse set of polygonal geometries.
     * Sparse means that if the geometries are partioned
     * into a set of intersecting clusters, the number of clusters
     * is a significant fraction of the total number of geometries.
     * The algorithm used provides performance and memory advantages
     * over the {@link CascadedPolygonUnion} algorithm.
     * It also has the advantage that it does not alter input geometries
     * which do not intersect any other input geometry.
     * <p>
     * Non-sparse sets will work, but may be slower than using cascaded union.
     * 
     * @author mdavis
     *
     */
    public class SparsePolygonUnion
    {
        public static Geometry Union(ICollection<Geometry> geoms)
        {
            var op = new SparsePolygonUnion(geoms);
            return op.Union();
        }

        public static Geometry Union(Geometry geoms)
        {
            var polys = PolygonExtracter.GetPolygons(geoms);
            var op = new SparsePolygonUnion(polys);
            return op.Union();
        }

        private readonly ICollection<Geometry> _inputPolys;
        private STRtree<PolygonNode> _index;
        private int _count;
        private readonly List<PolygonNode> _nodes = new List<PolygonNode>();
        //private GeometryFactory _geomFactory;

        public SparsePolygonUnion(ICollection<Geometry> polys)
        {
            this._inputPolys = polys;
            // guard against null input
            if (_inputPolys == null)
                _inputPolys = new List<Geometry>();
        }

        public Geometry Union()
        {
            if (_inputPolys.Count == 0)
                return null;

            LoadIndex(/*inputPolys*/);

            //--- cluster the geometries
            foreach (var queryNode in _nodes)
                _index.Query(queryNode.Envelope, new PolygonNodeVisitor(queryNode));

            //--- compute union of each cluster
            var clusterGeom = new List<Geometry>();
            foreach (var node in _nodes) {
                var geom = node.Union();
                if (geom == null) continue;
                clusterGeom.Add(geom);
            }

            var geomFactory = _inputPolys.First().Factory;
            return geomFactory.BuildGeometry(clusterGeom);
        }

        private void LoadIndex(/*IEnumerable<Geometry> inputPolys*/)
        {
            _index = new STRtree<PolygonNode>();
            foreach (var geom in _inputPolys)
            {
                Add(geom);
            }
        }

        private void Add(Geometry poly)
        {
            var node = new PolygonNode(_count++, poly);
            _nodes.Add(node);
            _index.Insert(poly.EnvelopeInternal, node);
        }

        private class PolygonNode
        {
            private readonly int _id;
            private bool _isFree = true;
            private readonly Geometry _poly;
            private PolygonNode _root;
            private List<PolygonNode> _nodes;

            public PolygonNode(int id, Geometry poly)
            {
                _id = id;
                _poly = poly;
            }

            public int Id
            {
                get => _id;
            }

            public Geometry Polygon => _poly;

            public Envelope Envelope => _poly.EnvelopeInternal;

            //public bool Intersects(PolygonNode node)
            //{
            //    // this would benefit from having a short-circuiting intersects 
            //    var pg = PreparedGeometryFactory.Prepare(_poly);
            //    return pg.Intersects(node._poly);
            //    //return poly.intersects(node.poly);
            //}

            public bool IsInSameCluster(PolygonNode node)
            {
                if (_isFree || node._isFree) return false;
                return _root == node._root;
            }

            public void Merge(PolygonNode node)
            {
                if (this == node)
                    throw new ArgumentException("Can't merge node with itself");

                if (Id < node.Id)
                {
                    Add(node);
                }
                else
                {
                    node.Add(this);
                }
            }

            private void InitCluster()
            {
                _isFree = false;
                _root = this;
                _nodes = new List<PolygonNode>();
                _nodes.Add(this);
            }

            private void Add(PolygonNode node)
            {
                if (_isFree) InitCluster();

                if (node._isFree)
                {
                    node._isFree = false;
                    node._root = _root;
                    _root._nodes.Add(node);
                }
                else
                {
                    _root.MergeRoot(node.Root);
                }
            }

            /**
     * Add the other root's nodes to this root's list.
     * Set the other nodes to have this as root.
     * Free the other root's node list.
     * 
     * @param root the other root node
     */
            private void MergeRoot(PolygonNode root)
            {
                if (_nodes == root._nodes)
                    throw new InvalidOperationException("Attempt to merge same cluster");

                foreach (var node in root._nodes)
                {
                    _nodes.Add(node);
                    node._root = this;
                }

                root._nodes = null;
            }

            private PolygonNode Root
            {
                get
                {
                    if (_isFree)
                        throw new InvalidOperationException("free node has no root");
                    if (_root != null)
                        return _root;
                    return this;
                }
            }

            public Geometry Union()
            {
                // free polys are returned unchanged
                if (_isFree) return _poly;
                // only root nodes can compute a union
                if (_root != this) return null;
                return CascadedPolygonUnion.Union(ToPolygons(_nodes));
            }

            private static List<Geometry> ToPolygons(ICollection<PolygonNode> nodes)
            {
                var polys = new List<Geometry>(nodes.Count);
                foreach (var node in nodes)
                    polys.Add(node._poly);
                return polys;
            }

        }

        private class PolygonNodeVisitor : IItemVisitor<PolygonNode>
        {
            private readonly PolygonNode _queryNode;
            private readonly IPreparedGeometry _prep;
            public PolygonNodeVisitor(PolygonNode queryNode)
            {
                _queryNode = queryNode;
                _prep = PreparedGeometryFactory.Prepare(queryNode.Polygon);
            }

            public void VisitItem(PolygonNode node)
            {
                if (node == _queryNode) return;
                // avoid duplicate intersections
                if (node.Id > _queryNode.Id) return;
                if (_queryNode.IsInSameCluster(node)) return;
                if (!_prep.Intersects(node.Polygon)) return;
                _queryNode.Merge(node);
            }
        }

    }
}
