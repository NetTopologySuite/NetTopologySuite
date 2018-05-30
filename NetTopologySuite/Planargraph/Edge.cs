namespace NetTopologySuite.Planargraph
{
    /// <summary>
    /// Represents an undirected edge of a {PlanarGraph}. An undirected edge
    /// in fact simply acts as a central point of reference for two opposite
    /// <c>DirectedEdge</c>s.
    /// Usually a client using a <c>PlanarGraph</c> will subclass <c>Edge</c>
    /// to add its own application-specific data and methods.
    /// </summary>
    public class Edge : GraphComponent
    {
        /// <summary>
        /// The two DirectedEdges associated with this Edge.
        /// </summary>
        protected DirectedEdge[] dirEdge;

        /// <summary>
        /// Constructs an Edge whose DirectedEdges are not yet set. Be sure to call
        /// <c>SetDirectedEdges(DirectedEdge, DirectedEdge)</c>.
        /// </summary>
        public Edge() { }

        /// <summary>
        /// Constructs an Edge initialized with the given DirectedEdges, and for each
        /// DirectedEdge: sets the Edge, sets the symmetric DirectedEdge, and adds
        /// this Edge to its from-Node.
        /// </summary>
        /// <param name="de0"></param>
        /// <param name="de1"></param>
        public Edge(DirectedEdge de0, DirectedEdge de1)
        {
            SetDirectedEdges(de0, de1);
        }

        /// <summary>
        /// Initializes this Edge's two DirectedEdges, and for each DirectedEdge: sets the
        /// Edge, sets the symmetric DirectedEdge, and adds this Edge to its from-Node.
        /// </summary>
        /// <param name="de0"></param>
        /// <param name="de1"></param>
        public void SetDirectedEdges(DirectedEdge de0, DirectedEdge de1)
        {
            dirEdge = new[] { de0, de1, };
            de0.Edge = this;
            de1.Edge = this;
            de0.Sym = de1;
            de1.Sym = de0;
            de0.FromNode.AddOutEdge(de0);
            de1.FromNode.AddOutEdge(de1);
        }

        /// <summary>
        /// Returns one of the DirectedEdges associated with this Edge.
        /// </summary>
        /// <param name="i">0 or 1.</param>
        /// <returns></returns>
        public DirectedEdge GetDirEdge(int i)
        {
            return dirEdge[i];
        }

        /// <summary>
        /// Returns the DirectedEdge that starts from the given node, or null if the
        /// node is not one of the two nodes associated with this Edge.
        /// </summary>
        /// <param name="fromNode"></param>
        /// <returns></returns>
        public DirectedEdge GetDirEdge(Node fromNode)
        {
            if (dirEdge[0].FromNode == fromNode)
                return dirEdge[0];
            if (dirEdge[1].FromNode == fromNode)
                return dirEdge[1];
            // node not found
            // possibly should throw an exception here?
            return null;
        }

        /// <summary>
        /// If <c>node</c> is one of the two nodes associated with this Edge,
        /// returns the other node; otherwise returns null.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Node GetOppositeNode(Node node)
        {
            if (dirEdge[0].FromNode == node)
                return dirEdge[0].ToNode;
            if (dirEdge[1].FromNode == node)
                return dirEdge[1].ToNode;
            // node not found
            // possibly should throw an exception here?
            return null;
        }

        /// <summary>
        /// Removes this edge from its containing graph.
        /// </summary>
        internal void Remove()
        {
            dirEdge = null;
        }

        /// <summary>
        /// Tests whether this component has been removed from its containing graph.
        /// </summary>
        /// <value></value>
        public override bool IsRemoved => dirEdge == null;
    }
}
