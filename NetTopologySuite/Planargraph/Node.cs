using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures.Collections.Generic;
using GisSharpBlog.NetTopologySuite.Geometries.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Planargraph
{
    /// <summary>
    /// A node in a <see cref="PlanarGraph{TCoordinate}"/> is a location 
    /// where 0 or more <see cref="Edge{TCoordinate}"/>s meet. 
    /// </summary>
    /// <remarks>
    /// A node is connected to each of its incident <see cref="Edge{TCoordinate}"/>s 
    /// via an outgoing <see cref="DirectedEdge{TCoordinate}"/>. 
    /// Some clients using a <see cref="PlanarGraph{TCoordinate}"/> may want to
    /// subclass <see cref="Node{TCoordinate}"/> to add their own application-specific
    /// data and methods.
    /// </remarks>
    public class Node<TCoordinate> : GraphComponent<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<TCoordinate>, IConvertible
    {
        /// <summary>
        /// Returns all Edges that connect the two nodes (which are assumed to be different).
        /// </summary>
        public static IEnumerable<Edge<TCoordinate>> GetEdgesBetween(Node<TCoordinate> node0, Node<TCoordinate> node1)
        {
            IEnumerable<Edge<TCoordinate>> edges0 = DirectedEdge<TCoordinate>.ToEdges(node0.OutEdges.Edges);
            ISet<Edge<TCoordinate>> commonEdges = new HashedSet<Edge<TCoordinate>>(edges0);
            IEnumerable<Edge<TCoordinate>> edges1 = DirectedEdge<TCoordinate>.ToEdges(node1.OutEdges.Edges);

            commonEdges.RetainAll(edges1);
            return commonEdges;
        }

        /// <summary>
        /// The location of this Node.
        /// </summary>
        private TCoordinate _coordinate;

        /// <summary>
        /// The collection of DirectedEdges that leave this Node.
        /// </summary>
        private readonly DirectedEdgeStar<TCoordinate> _directedEdgeStar;

        /// <summary>
        /// Constructs a Node with the given location.
        /// </summary>
        public Node(TCoordinate coordinate) 
            : this(coordinate, new DirectedEdgeStar<TCoordinate>()) {}

        /// <summary>
        /// Constructs a Node with the given location and collection of outgoing DirectedEdges.
        /// </summary>
        public Node(TCoordinate coordinate, DirectedEdgeStar<TCoordinate> deStar)
        {
            _coordinate = coordinate;
            _directedEdgeStar = deStar;
        }

        /// <summary>
        /// Returns the location of this Node.
        /// </summary>
        public TCoordinate Coordinate
        {
            get { return _coordinate; }
        }

        /// <summary>
        /// Adds an outgoing DirectedEdge to this Node.
        /// </summary>
        public void AddOutEdge(DirectedEdge<TCoordinate> de)
        {
            _directedEdgeStar.Add(de);
        }

        /// <summary>
        /// Returns the collection of DirectedEdges that leave this Node.
        /// </summary>
        public DirectedEdgeStar<TCoordinate> OutEdges
        {
            get { return _directedEdgeStar; }
        }

        /// <summary>
        /// Returns the number of edges around this Node.
        /// </summary>
        public Int32 Degree
        {
            get { return _directedEdgeStar.Degree; }
        }

        /// <summary>
        /// Returns the zero-based index of the given Edge, after sorting in ascending order
        /// by angle with the positive x-axis.
        /// </summary>
        public Int32 GetIndex(Edge<TCoordinate> edge)
        {
            return _directedEdgeStar.GetIndex(edge);
        }

        /// <summary>
        /// Removes this node from its containing graph.
        /// </summary>
        internal void Remove()
        {
            _coordinate = default(TCoordinate);
        }

        /// <summary>
        /// Tests whether this component has been removed from its containing graph.
        /// </summary>
        public override Boolean IsRemoved
        {
            get { return CoordinateHelper.IsEmpty(_coordinate); }
        }

        public override string ToString()
        {
            return "NODE: " + _coordinate + ": " + Degree;
        }
    }
}