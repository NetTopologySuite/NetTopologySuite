using System.Collections.Generic;
using System.Linq;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Planargraph
{
    /// <summary>
    /// A node in a <c>PlanarGraph</c> is a location where 0 or more <c>Edge</c>s
    /// meet. A node is connected to each of its incident Edges via an outgoing
    /// DirectedEdge. Some clients using a <c>PlanarGraph</c> may want to
    /// subclass <c>Node</c> to add their own application-specific
    /// data and methods.
    /// </summary>
    public class Node : GraphComponent
    {
        /// <summary>
        /// Returns all Edges that connect the two nodes (which are assumed to be different).
        /// </summary>
        /// <param name="node0"></param>
        /// <param name="node1"></param>
        /// <returns></returns>
        public static IList<DirectedEdge> GetEdgesBetween(Node node0, Node node1)
        {
            var edges0 = DirectedEdge.ToEdges(node0.OutEdges.Edges);
            var commonEdges = new HashSet<DirectedEdge>(edges0.Cast<DirectedEdge>());
            var edges1 = DirectedEdge.ToEdges(node1.OutEdges.Edges);
            commonEdges.ExceptWith(edges1.Cast<DirectedEdge>());
            return new List<DirectedEdge>(commonEdges);
        }

        /// <summary>
        /// The location of this Node.
        /// </summary>
        protected Coordinate pt;

        /// <summary>
        /// The collection of DirectedEdges that leave this Node.
        /// </summary>
        protected DirectedEdgeStar deStar;

        /// <summary>
        /// Constructs a Node with the given location.
        /// </summary>
        /// <param name="pt"></param>
        public Node(Coordinate pt) : this(pt, new DirectedEdgeStar()) { }

        /// <summary>
        /// Constructs a Node with the given location and collection of outgoing DirectedEdges.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="deStar"></param>
        public Node(Coordinate pt, DirectedEdgeStar deStar)
        {
            this.pt = pt;
            this.deStar = deStar;
        }

        /// <summary>
        /// Returns the location of this Node.
        /// </summary>
        public Coordinate Coordinate => pt;

        /// <summary>
        /// Adds an outgoing DirectedEdge to this Node.
        /// </summary>
        /// <param name="de"></param>
        public void AddOutEdge(DirectedEdge de)
        {
            deStar.Add(de);
        }

        /// <summary>
        /// Returns the collection of DirectedEdges that leave this Node.
        /// </summary>
        public DirectedEdgeStar OutEdges => deStar;

        /// <summary>
        /// Returns the number of edges around this Node.
        /// </summary>
        public int Degree => deStar.Degree;

        /// <summary>
        /// Returns the zero-based index of the given Edge, after sorting in ascending order
        /// by angle with the positive x-axis.
        /// </summary>
        /// <param name="edge"></param>
        /// <returns></returns>
        public int GetIndex(Edge edge)
        {
            return deStar.GetIndex(edge);
        }

        /// <summary>
        /// Removes a <see cref="DirectedEdge"/> incident on this node. Does not change the state of the directed edge.
        /// </summary>
        public void Remove(DirectedEdge de)
        {
            deStar.Remove(de);
        }

        /// <summary>
        /// Removes this node from its containing graph.
        /// </summary>
        internal void Remove()
        {
            pt = null;
        }

        /// <summary>
        /// Tests whether this component has been removed from its containing graph.
        /// </summary>
        /// <value></value>
        public override bool IsRemoved => pt == null;

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "NODE: " + pt + ": " + Degree;
        }
    }
}
