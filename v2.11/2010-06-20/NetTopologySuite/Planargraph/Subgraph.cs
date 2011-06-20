using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures.Collections.Generic;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Planargraph
{
    /// <summary>
    /// A subgraph of a <see cref="PlanarGraph{TCoordinate}" />.
    /// A subgraph may contain any subset of <see cref="Edge{TCoordinate}" />s
    /// from the parent graph.
    /// It will also automatically contain all <see cref="DirectedEdge{TCoordinate}" />s
    /// and <see cref="Node{TCoordinate}" />s associated with those edges.
    /// No new objects are created when edges are added -
    /// all associated components must already exist in the parent graph.
    /// </summary>
    public class Subgraph<TCoordinate> : PlanarGraph<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly PlanarGraph<TCoordinate> _parentGraph;
        private readonly IDictionary<Edge<TCoordinate>, Edge<TCoordinate>> _edges = new Dictionary<Edge<TCoordinate>, Edge<TCoordinate>>();
        private readonly IList<DirectedEdge<TCoordinate>> _dirEdges = new List<DirectedEdge<TCoordinate>>();
        private readonly NodeMap<TCoordinate> _nodeMap = new NodeMap<TCoordinate>();

        /// <summary>
        /// Creates a new subgraph of the given <see cref="PlanarGraph{TCoordinate}" />.
        /// </summary>
        public Subgraph(PlanarGraph<TCoordinate> parentGraph)
        {
            _parentGraph = parentGraph;
        }

        /// <summary>
        ///  Gets the <see cref="PlanarGraph{TCoordinate}" /> which this subgraph is part of.
        /// </summary>
        public PlanarGraph<TCoordinate> Parent
        {
            get { return _parentGraph; }
        }

        /// <summary>
        /// Adds an <see cref="Edge{TCoordinate}" /> to the subgraph.
        /// The associated <see cref="DirectedEdge{TCoordinate}" />s and 
        /// <see cref="Node{TCoordinate}" />s are also added.
        /// </summary>
        /// <param name="e">The <see cref="Edge{TCoordinate}" /> to add.</param>
        public void Add(Edge<TCoordinate> e)
        {
            Edge<TCoordinate> tmp;
            if (_edges.TryGetValue( e, out tmp )) return;
            
            _edges.Add(e, null);

            _dirEdges.Add(e.GetDirectedEdge(0));
            _dirEdges.Add(e.GetDirectedEdge(1));

            _nodeMap.Add(e.GetDirectedEdge(0).FromNode);
            _nodeMap.Add(e.GetDirectedEdge(1).FromNode);
        }

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}" /> over the 
        /// <see cref="DirectedEdge{TCoordinate}" />s in this graph,
        /// in the order in which they were added.
        /// </summary>
        public new IEnumerable<DirectedEdge<TCoordinate>> DirectedEdges
        {
            get { return _dirEdges; }
        }

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}" /> over the <see cref="Edge{TCoordinate}" />s in this graph,
        /// in the order in which they were added.
        /// </summary>
        public new IEnumerable<Edge<TCoordinate>> Edges
        {
            get { return _edges.Keys; }
        }

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}" /> over the <see cref="Node{TCoordinate}" />s in this graph.
        /// </summary>
        public new IEnumerable<Node<TCoordinate>> Nodes
        {
            get { return _nodeMap.Values; }
        }
    }
}