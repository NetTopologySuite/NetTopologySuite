using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Planargraph
{
    /// <summary>
    /// A map of <see cref="Node">nodes</see>, indexed by the coordinate of the node.
    /// </summary>
    public class NodeMap
    {
        private readonly IDictionary<Coordinate, Node> _nodeMap = new SortedDictionary<Coordinate, Node>();

        /*
        /// <summary>
        /// Constructs a NodeMap without any Nodes.
        /// </summary>
        public NodeMap() { }
        */
        /// <summary>
        /// Adds a node to the map, replacing any that is already at that location.
        /// </summary>
        /// <param name="n"></param>
        /// <returns>The added node.</returns>
        public Node Add(Node n)
        {
            _nodeMap[n.Coordinate] = n;
            return n;
        }

        /// <summary>
        /// Removes the Node at the given location, and returns it (or null if no Node was there).
        /// </summary>
        /// <param name="pt"></param>
        /// <returns></returns>
        public Node Remove(Coordinate pt)
        {
            if (!_nodeMap.ContainsKey(pt))
                return null;
            var node = _nodeMap[pt];
            _nodeMap.Remove(pt);
            return node;
        }

        /// <summary>
        /// Returns the Node at the given location, or null if no Node was there.
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public Node Find(Coordinate coord)
        {
            Node res;
            if (_nodeMap.TryGetValue(coord, out res))
                return res;
            return null;
        }

        /// <summary>
        /// Returns an Iterator over the Nodes in this NodeMap, sorted in ascending order
        /// by angle with the positive x-axis.
        /// </summary>
        public IEnumerator<Node> GetEnumerator()
        {
            return _nodeMap.Values.GetEnumerator();
        }

        /// <summary>
        /// Returns the Nodes in this NodeMap, sorted in ascending order
        /// by angle with the positive x-axis.
        /// </summary>
        public ICollection<Node> Values => _nodeMap.Values;

        /// <summary>
        /// Returns the number of Nodes in this NodeMap.
        /// </summary>
        public int Count => _nodeMap.Count;
    }
}
