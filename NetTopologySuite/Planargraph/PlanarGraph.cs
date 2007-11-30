using System;
using System.Collections.Generic;
using NPack.Interfaces;
using GeoAPI.Coordinates;

namespace GisSharpBlog.NetTopologySuite.Planargraph
{
    /// <summary>
    /// Represents a directed graph which is embeddable in a planar surface.
    /// </summary>
    /// <remarks>
    /// This class and the other classes in this package serve as a framework for
    /// building planar graphs for specific algorithms. This class must be
    /// subclassed to expose appropriate methods to construct the graph. This allows
    /// controlling the types of graph components (<see cref="DirectedEdge{TCoordinate}"/>s,
    /// <see cref="Edge{TCoordinate}"/>s and <see cref="Node{TCoordinate}"/>s) which can be added to the graph. An
    /// application which uses the graph framework will almost always provide
    /// subclasses for one or more graph components, which hold application-specific
    /// data and graph algorithms.
    /// </remarks>
    public abstract class PlanarGraph<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private readonly List<Edge<TCoordinate>> _edges = new List<Edge<TCoordinate>>();
        private readonly List<DirectedEdge<TCoordinate>> _dirEdges = new List<DirectedEdge<TCoordinate>>();
        private readonly NodeMap<TCoordinate> _nodeMap = new NodeMap<TCoordinate>();

        /// <summary>
        /// Returns the Node at the given location, or null if no Node is there.
        /// </summary>
        public Node<TCoordinate> FindNode(TCoordinate pt)
        {
            return _nodeMap.Find(pt);
        }

        /// <summary>
        /// Tests whether an <see cref="Edge{TCoordinate}" /> 
        /// is contained in this graph.
        /// </summary>
        /// <param name="e">The <see cref="Edge{TCoordinate}" /> to test.</param>
        /// <returns>
        /// <see langword="true"/> if the <see cref="Edge{TCoordinate}" /> 
        /// is contained in this graph.
        /// </returns>
        public Boolean Contains(Edge<TCoordinate> e)
        {
            return _edges.Contains(e);
        }

        /// <summary>
        /// Returns the Nodes in this PlanarGraph.
        /// </summary>
        public IEnumerable<Node<TCoordinate>> Nodes
        {
            get { return _nodeMap.Values; }
        }

        /// <summary> 
        /// Returns a set of the <see cref="DirectedEdge{TCoordinate}"/>s 
        /// in this <see cref="PlanarGraph{TCoordinate}"/>, in the order in which they
        /// were added.
        /// </summary>
        public IList<DirectedEdge<TCoordinate>> DirectedEdges
        {
            get { return _dirEdges; }
        }

        /// <summary>
        /// Returns the Edges that have been added to this PlanarGraph.
        /// </summary>
        public IList<Edge<TCoordinate>> Edges
        {
            get { return _edges; }
        }

        /// <summary>
        /// Removes an Edge and its associated DirectedEdges from their from-Nodes and
        /// from this PlanarGraph. Note: This method does not remove the Nodes associated
        /// with the Edge, even if the removal of the Edge reduces the degree of a
        /// Node to zero.
        /// </summary>
        public void Remove(Edge<TCoordinate> edge)
        {
            Remove(edge.GetDirectedEdge(0));
            Remove(edge.GetDirectedEdge(1));
            _edges.Remove(edge);
            edge.Remove();
        }

        /// <summary> 
        /// Removes DirectedEdge from its from-Node and from this PlanarGraph. Note:
        /// This method does not remove the Nodes associated with the DirectedEdge,
        /// even if the removal of the DirectedEdge reduces the degree of a Node to
        /// zero.
        /// </summary>
        public void Remove(DirectedEdge<TCoordinate> de)
        {
            DirectedEdge<TCoordinate> sym = de.Sym;

            if (sym != null)
            {
                sym.Sym = null;
            }

            de.FromNode.OutEdges.Remove(de);
            de.Remove();
            _dirEdges.Remove(de);
        }

        /// <summary>
        /// Removes a node from the graph, along with any associated DirectedEdges and
        /// Edges.
        /// </summary>
        public void Remove(Node<TCoordinate> node)
        {
            // unhook all directed edges
            IList<DirectedEdge<TCoordinate>> outEdges = node.OutEdges.Edges;

            foreach (DirectedEdge<TCoordinate> directedEdge in outEdges)
            {
                DirectedEdge<TCoordinate> sym = directedEdge.Sym;

                // remove the diredge that points to this node
                if (sym != null)
                {
                    Remove(sym);
                }

                // remove this diredge from the graph collection
                _dirEdges.Remove(directedEdge);

                Edge<TCoordinate> edge = directedEdge.Edge;

                if (edge != null)
                {
                    _edges.Remove(edge);
                }   
            }

            // remove the node from the graph
            _nodeMap.Remove(node.Coordinate);
            node.Remove();
        }

        /// <summary>
        /// Returns all <see cref="Node{TCoordinate}"/>s with the 
        /// given number of <see cref="Edge{TCoordinate}"/>s around it.
        /// </summary>
        public IEnumerable<Node<TCoordinate>> FindNodesOfDegree(Int32 degree)
        {
            foreach (Node<TCoordinate> node in Nodes)
            {
                if (node.Degree == degree)
                {
                    yield return node;
                }
            }
        }

        /// <summary>
        /// Adds a node to the map, replacing any that is already at that location.
        /// Only subclasses can add Nodes, to ensure Nodes are of the right type.
        /// </summary>
        /// <returns>The added node.</returns>
        protected void Add(Node<TCoordinate> node)
        {
            _nodeMap.Add(node);
        }

        /// <summary>
        /// Adds the Edge and its DirectedEdges with this PlanarGraph.
        /// Assumes that the Edge has already been created with its associated DirectEdges.
        /// Only subclasses can add Edges, to ensure the edges added are of the right class.
        /// </summary>
        protected void AddInternal(Edge<TCoordinate> edge)
        {
            if (_edges.Contains(edge))
            {
                return;
            }

            _edges.Add(edge);
            Add(edge.GetDirectedEdge(0));
            Add(edge.GetDirectedEdge(1));
        }

        /// <summary>
        /// Adds the Edge to this PlanarGraph; only subclasses can add DirectedEdges,
        /// to ensure the edges added are of the right class.
        /// </summary>
        protected void Add(DirectedEdge<TCoordinate> dirEdge)
        {
            _dirEdges.Add(dirEdge);
        }

        protected NodeMap<TCoordinate> NodeMap
        {
            get { return _nodeMap; }
        }
    }
}