using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Planargraph
{
    /// <summary>
    /// Represents a directed graph which is embeddable in a planar surface.
    /// This class and the other classes in this package serve as a framework for
    /// building planar graphs for specific algorithms. This class must be
    /// subclassed to expose appropriate methods to construct the graph. This allows
    /// controlling the types of graph components ({DirectedEdge}s,
    /// <c>Edge</c>s and <c>Node</c>s) which can be added to the graph. An
    /// application which uses the graph framework will almost always provide
    /// subclasses for one or more graph components, which hold application-specific
    /// data and graph algorithms.
    /// </summary>
    public abstract class PlanarGraph
    {
        /// <summary>
        ///
        /// </summary>
        private IList<Edge> _edges = new List<Edge>();

        /// <summary>
        ///
        /// </summary>
        protected IList<DirectedEdge> dirEdges = new List<DirectedEdge>();

        /// <summary>
        ///
        /// </summary>
        protected NodeMap nodeMap = new NodeMap();

        /// <summary>
        /// Returns the <see cref="Node"/> at the given <paramref name="pt">location</paramref>, or <c>null</c> if no <see cref="Node"/> was there.
        /// </summary>
        /// <param name="pt">The location</param>
        /// <returns>The node found<br/>
        /// or <c>null</c> if this graph contains no node at the location
        /// </returns>
        public Node FindNode(Coordinate pt)
        {
            return nodeMap.Find(pt);
        }

        /// <summary>
        /// Adds a node to the map, replacing any that is already at that location.
        /// Only subclasses can add Nodes, to ensure Nodes are of the right type.
        /// </summary>
        /// <param name="node"></param>
        /// <returns>The added node.</returns>
        protected void Add(Node node)
        {
            nodeMap.Add(node);
        }

        /// <summary>
        /// Adds the Edge and its DirectedEdges with this PlanarGraph.
        /// Assumes that the Edge has already been created with its associated DirectEdges.
        /// Only subclasses can add Edges, to ensure the edges added are of the right class.
        /// </summary>
        /// <param name="edge"></param>
        protected void Add(Edge edge)
        {
            _edges.Add(edge);
            Add(edge.GetDirEdge(0));
            Add(edge.GetDirEdge(1));
        }

        /// <summary>
        /// Adds the Edge to this PlanarGraph; only subclasses can add DirectedEdges,
        /// to ensure the edges added are of the right class.
        /// </summary>
        /// <param name="dirEdge"></param>
        protected void Add(DirectedEdge dirEdge)
        {
            dirEdges.Add(dirEdge);
        }

        /// <summary>
        /// Returns an IEnumerator over the Nodes in this PlanarGraph.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Node> GetNodeEnumerator()
        {
            return nodeMap.GetEnumerator();
        }

        /// <summary>
        /// Returns the Nodes in this PlanarGraph.
        /// </summary>
        public ICollection<Node> Nodes => nodeMap.Values;

        /// <summary>
        /// Returns an Iterator over the DirectedEdges in this PlanarGraph, in the order in which they
        /// were added.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<DirectedEdge> GetDirEdgeEnumerator()
        {
            return dirEdges.GetEnumerator();
        }

        /// <summary>
        /// Returns an Iterator over the Edges in this PlanarGraph, in the order in which they
        /// were added.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Edge> GetEdgeEnumerator()
        {
            return _edges.GetEnumerator();
        }

        /// <summary>
        /// Returns the Edges that have been added to this PlanarGraph.
        /// </summary>
        public IList<Edge> Edges
        {
            get => _edges;
            protected set => _edges = value;
        }

        /// <summary>
        /// Removes an Edge and its associated DirectedEdges from their from-Nodes and
        /// from this PlanarGraph. Note: This method does not remove the Nodes associated
        /// with the Edge, even if the removal of the Edge reduces the degree of a
        /// Node to zero.
        /// </summary>
        /// <param name="edge"></param>
        public void Remove(Edge edge)
        {
            Remove(edge.GetDirEdge(0));
            Remove(edge.GetDirEdge(1));
            _edges.Remove(edge);
            edge.Remove();
        }

        /// <summary>
        /// Removes a <see cref="DirectedEdge"/> from its from-<see cref="Node"/> and from this PlanarGraph.
        /// </summary>
        /// <remarks>
        /// This method does not remove the <see cref="Node"/>s associated with the DirectedEdge,
        /// even if the removal of the DirectedEdge reduces the degree of a Node to zero.
        /// </remarks>
        /// <param name="de"></param>
        public void Remove(DirectedEdge de)
        {
            var sym = de.Sym;
            if (sym != null)
                sym.Sym = null;
            de.FromNode.Remove(de);
            de.Remove();
            dirEdges.Remove(de);
        }

        /// <summary>
        /// Removes a node from the graph, along with any associated DirectedEdges and
        /// Edges.
        /// </summary>
        /// <param name="node"></param>
        public void Remove(Node node)
        {
            // unhook all directed edges
            var outEdges = node.OutEdges.Edges;
            foreach (var de in outEdges)
            {
                var sym = de.Sym;
                // remove the diredge that points to this node
                if (sym != null)
                    Remove(sym);
                // remove this diredge from the graph collection
                dirEdges.Remove(de);

                var edge = de.Edge;
                if (edge != null)
                    _edges.Remove(edge);
            }
            // remove the node from the graph
            nodeMap.Remove(node.Coordinate);
            node.Remove();
        }

        /// <summary>
        /// Returns all Nodes with the given number of Edges around it.
        /// </summary>
        /// <param name="degree"></param>
        /// <returns></returns>
        public IList<Node> FindNodesOfDegree(int degree)
        {
            var nodesFound = new List<Node>();
            foreach (var node in nodeMap.Values )
            {
                if (node.Degree == degree)
                    nodesFound.Add(node);
            }
            return nodesFound;
        }
    }
}
