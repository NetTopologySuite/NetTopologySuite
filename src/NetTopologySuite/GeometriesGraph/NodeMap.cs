using System.Collections.Generic;
using System.IO;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A map of nodes, indexed by the coordinate of the node.
    /// </summary>
    public class NodeMap
    {
        private readonly SortedDictionary<Coordinate, Node> _nodeMap = new SortedDictionary<Coordinate, Node>();
        private readonly NodeFactory _nodeFact;

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="NodeFactory"/>.
        /// </summary>
        /// <param name="nodeFact">A factory to create <c>Node</c>s</param>
        public NodeMap(NodeFactory nodeFact)
        {
            _nodeFact = nodeFact;
        }

        /// <summary>
        /// This method expects that a node has a coordinate value.
        /// </summary>
        /// <param name="coord">A <c>Coordinate</c></param>
        /// <returns>The <c>Node</c> for the provided <c>Coordinate</c> <paramref name="coord"/></returns>
        public Node AddNode(Coordinate coord)
        {
            if (!_nodeMap.TryGetValue(coord, out var node))
            {
                node = _nodeFact.CreateNode(coord);
                _nodeMap.Add(coord, node);
            }
            return node;
        }

        /// <summary>
        /// Adds a <c>Node</c> to this <c>NodeMap</c>.
        /// If a <c>Node</c> with the same <see cref="Node.Coordinate"/>
        /// is already present in this <c>NodeMap</c>,
        /// their <see cref="GraphComponent.Label"/>s are merged.
        /// </summary>
        /// <param name="n">The <c>Node</c> to add</param>
        /// <returns>Either <paramref name="n"/> or a <c>Node</c> with merged <c>Label</c>s</returns>
        public Node AddNode(Node n)
        {
            if (!_nodeMap.TryGetValue(n.Coordinate, out var node))
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
        /// <param name="e">An <c>EdgeEnd</c></param>
        public void Add(EdgeEnd e)
        {
            var p = e.Coordinate;
            var n = AddNode(p);
            n.Add(e);
        }

        /// <summary>
        /// Searches for a <c>Node</c> at <paramref name="coord"/> position.
        /// </summary>
        /// <param name="coord">A <c>Coordinate</c></param>
        /// <returns>
        /// The node if found; <c>null</c> otherwise.
        /// </returns>
        public Node Find(Coordinate coord)
        {
            if (!_nodeMap.TryGetValue(coord, out var res))
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
