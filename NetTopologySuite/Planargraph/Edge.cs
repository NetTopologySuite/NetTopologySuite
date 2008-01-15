using System;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Planargraph
{
    /// <summary>
    /// Represents an undirected edge of a <see cref="PlanarGraph{TCoordinate}"/>. 
    /// An undirected edge in fact simply acts as a central point of reference 
    /// for two opposite <see cref="DirectedEdge{TCoordinate}"/>s.
    /// </summary>
    /// <remarks>
    /// Usually a client using a <see cref="PlanarGraph{TCoordinate}"/> will subclass 
    /// <see cref="Edge{TCoordinate}"/> to add its own application-specific 
    /// data and methods.
    /// </remarks>
    public class Edge<TCoordinate> : GraphComponent<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        // The two DirectedEdges associated with this Edge.
        private DirectedEdge<TCoordinate> _directedEdge1;
        private DirectedEdge<TCoordinate> _directedEdge2;

        /// <summary>
        /// Constructs an Edge initialized with the given DirectedEdges, and for each
        /// DirectedEdge: sets the Edge, sets the symmetric DirectedEdge, and adds
        /// this Edge to its from-Node.
        /// </summary>
        public Edge(DirectedEdge<TCoordinate> directedEdge1, DirectedEdge<TCoordinate> directedEdge2)
        {
            SetDirectedEdges(directedEdge1, directedEdge2);
        }

        /// <summary>
        /// Initializes this Edge's two DirectedEdges, and for each DirectedEdge: sets the
        /// Edge, sets the symmetric DirectedEdge, and adds this Edge to its from-Node.
        /// </summary>
        public void SetDirectedEdges(DirectedEdge<TCoordinate> directedEdge1, DirectedEdge<TCoordinate> directedEdge2)
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
        public DirectedEdge<TCoordinate> GetDirectedEdge(Int32 i)
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
        public DirectedEdge<TCoordinate> GetDirectedEdge(Node<TCoordinate> fromNode)
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
        /// If <paramref name="node"/> is one of the two nodes associated 
        /// with this <see cref="Edge{TCoordinate}"/>,
        /// returns the other node; otherwise returns null.
        /// </summary>
        public Node<TCoordinate> GetOppositeNode(Node<TCoordinate> node)
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
        /// Tests whether this component has been removed from its containing graph.
        /// </summary>
        public override Boolean IsRemoved
        {
            get { return (_directedEdge1 ?? _directedEdge2) == null; }
        }

        /// <summary>
        /// Removes this edge from its containing graph.
        /// </summary>
        internal void Remove()
        {
            _directedEdge1 = null;
            _directedEdge2 = null;
        }
    }
}