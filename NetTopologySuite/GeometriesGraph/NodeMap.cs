using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;

namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A map of nodes, indexed by the coordinate of the node.
    /// </summary>
    public class NodeMap
    {
        private readonly IDictionary<Coordinate, Node > _nodeMap = new SortedDictionary<Coordinate, Node>();
        private readonly NodeFactory _nodeFact;

        /// <summary>
        ///
        /// </summary>
        /// <param name="nodeFact"></param>
        public NodeMap(NodeFactory nodeFact)
        {
            _nodeFact = nodeFact;
        }

        /// <summary>
        /// This method expects that a node has a coordinate value.
        /// </summary>
        /// <param name="coord"></param>
        public Node AddNode(Coordinate coord)
        {
            Node node;
            if (!_nodeMap.TryGetValue(coord, out node))
            {
                node = _nodeFact.CreateNode(coord);
                _nodeMap.Add(coord, node);
            }
            return node;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public Node AddNode(Node n)
        {
            var node = _nodeMap[n.Coordinate];
            if (node == null)
            {
                _nodeMap.Add(n.Coordinate, n);
                return n;
            }
            node.MergeLabel(n);
            return node;
        }

        /// <summary>
        /// Adds a node for the start point of this EdgeEnd
        /// (if one does not already exist in this map).
        /// Adds the EdgeEnd to the (possibly new) node.
        /// </summary>
        /// <param name="e"></param>
        public void Add(EdgeEnd e)
        {
            var p = e.Coordinate;
            var n = AddNode(p);
            n.Add(e);
        }

        /// <returns>
        /// The node if found; null otherwise.
        /// </returns>
        /// <param name="coord"></param>
        public Node Find(Coordinate coord)
        {
            Node res;
            if (!_nodeMap.TryGetValue(coord, out res))
                return null;
            return res;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Node> GetEnumerator()
        {
            return _nodeMap.Values.GetEnumerator();
        }

        /// <summary>
        ///
        /// </summary>
        public IList<Node> Values => new List<Node>(_nodeMap.Values);

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <returns></returns>
        public IList<Node> GetBoundaryNodes(int geomIndex)
        {
            var bdyNodes = new List<Node>();
            foreach (var node in _nodeMap.Values)
            {
                if (node.Label.GetLocation(geomIndex) == Location.Boundary)
                    bdyNodes.Add(node);
            }
            return bdyNodes;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="outstream"></param>
        public void Write(StreamWriter outstream)
        {
            foreach (var node in _nodeMap.Values)
            {
                node.Write(outstream);
            }
        }
    }
}
