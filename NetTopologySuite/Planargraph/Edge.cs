using System;

namespace GisSharpBlog.NetTopologySuite.Planargraph
{
    /// <summary>
    /// Represents an undirected edge of a <see cref="PlanarGraph"/>. 
    /// An undirected edge in fact simply acts as a central point of reference 
    /// for two opposite <see cref="DirectedEdge"/>s.
    /// </summary>
    /// <remarks>
    /// Usually a client using a <see cref="PlanarGraph"/> will subclass <see cref="Edge"/>
    /// to add its own application-specific data and methods.
    /// </remarks>
    public class Edge : GraphComponent
    {
        // The two DirectedEdges associated with this Edge.
        private DirectedEdge _directedEdge1;
        private DirectedEdge _directedEdge2;

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
        public Edge(DirectedEdge directedEdge1, DirectedEdge directedEdge2)
        {
            SetDirectedEdges(directedEdge1, directedEdge2);
        }

        /// <summary>
        /// Initializes this Edge's two DirectedEdges, and for each DirectedEdge: sets the
        /// Edge, sets the symmetric DirectedEdge, and adds this Edge to its from-Node.
        /// </summary>
        public void SetDirectedEdges(DirectedEdge directedEdge1, DirectedEdge directedEdge2)
        {
            _directedEdge1 = directedEdge1;
            _directedEdge2 = directedEdge2;

            _directedEdge1.Edge = this;
            _directedEdge2.Edge = this;
            _directedEdge1.Sym = _directedEdge2;
            _directedEdge2.Sym = _directedEdge1;
            _directedEdge1.FromNode.AddOutEdge(_directedEdge1);
            _directedEdge2.FromNode.AddOutEdge(_directedEdge2);
        }

        /// <summary> 
        /// Returns one of the DirectedEdges associated with this Edge.
        /// </summary>
        /// <param name="i">0 or 1.</param>
        public DirectedEdge GetDirectedEdge(Int32 i)
        {
            if (i == 0)
            {
                return _directedEdge1;
            }
            else if (i == 1)
            {
                return _directedEdge2;
            }
            else
            {
                throw new ArgumentOutOfRangeException("i", i, 
                    "Parameter 'i' must be 0 or 1.");
            }
        }

        /// <summary>
        /// Returns the DirectedEdge that starts from the given node, or null if the
        /// node is not one of the two nodes associated with this Edge.
        /// </summary>
        public DirectedEdge GetDirectedEdge(Node fromNode)
        {
            if (_directedEdge1.FromNode == fromNode)
            {
                return _directedEdge1;
            }

            if (_directedEdge2.FromNode == fromNode)
            {
                return _directedEdge2;
            }

            // node not found
            // possibly should throw an exception here?
            return null;
        }

        /// <summary> 
        /// If <c>node</c> is one of the two nodes associated with this Edge,
        /// returns the other node; otherwise returns null.
        /// </summary>
        public Node GetOppositeNode(Node node)
        {
            if (_directedEdge1.FromNode == node)
            {
                return _directedEdge1.ToNode;
            }

            if (_directedEdge2.FromNode == node)
            {
                return _directedEdge2.ToNode;
            }

            // node not found
            // possibly should throw an exception here?
            return null;
        }

        /// <summary>
        /// Removes this edge from its containing graph.
        /// </summary>
        internal void Remove()
        {
            _directedEdge1 = null;
            _directedEdge2 = null;
        }

        /// <summary>
        /// Tests whether this component has been removed from its containing graph.
        /// </summary>
        public override Boolean IsRemoved
        {
            get { return (_directedEdge1 ?? _directedEdge2) == null; }
        }
    }
}