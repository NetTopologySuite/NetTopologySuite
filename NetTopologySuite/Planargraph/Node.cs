using System.Collections;
using GeoAPI.Geometries;
using Iesi_NTS.Collections;

namespace GisSharpBlog.NetTopologySuite.Planargraph
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
        public static IList getEdgesBetween(Node node0, Node node1)
        {
            IList edges0 = DirectedEdge.ToEdges(node0.OutEdges.Edges);
            ISet commonEdges = new HashedSet(edges0);
            IList edges1 = DirectedEdge.ToEdges(node1.OutEdges.Edges);
            commonEdges.RetainAll(edges1);
            return new ArrayList(commonEdges);
        }

        /// <summary>
        /// The location of this Node.
        /// </summary>
        protected ICoordinate pt;

        /// <summary>
        /// The collection of DirectedEdges that leave this Node.
        /// </summary>
        protected DirectedEdgeStar deStar;

        /// <summary>
        /// Constructs a Node with the given location.
        /// </summary>
        /// <param name="pt"></param>
        public Node(ICoordinate pt) : this(pt, new DirectedEdgeStar()) { }

        /// <summary>
        /// Constructs a Node with the given location and collection of outgoing DirectedEdges.
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="deStar"></param>
        public Node(ICoordinate pt, DirectedEdgeStar deStar)
        {
            this.pt = pt;
            this.deStar = deStar;
        }

        /// <summary>
        /// Returns the location of this Node.
        /// </summary>
        public ICoordinate Coordinate
        {
            get
            {
                return pt;
            }
        }

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
        public DirectedEdgeStar OutEdges
        {
            get
            {
                return deStar;
            }
        }

        /// <summary>
        /// Returns the number of edges around this Node.
        /// </summary>
        public int Degree
        {
            get
            {
                return deStar.Degree;
            }
        }

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
        public override bool IsRemoved
        {
            get
            {
                return pt == null;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return "NODE: " + pt.ToString() + ": " + Degree;
        }
    }
}
