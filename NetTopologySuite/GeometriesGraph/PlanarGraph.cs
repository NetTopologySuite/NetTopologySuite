#define C5
using System;
using System.Collections.Generic;
using System.Diagnostics;
#if C5
using C5;
#endif
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    using sgc = System.Collections.Generic;
    /// <summary> 
    /// The computation of the <see cref="IntersectionMatrix"/> relies on the use of a structure
    /// called a "topology graph". The topology graph contains nodes and edges
    /// corresponding to the vertexes and line segments of a <see cref="Geometry{TCoordinate}"/>. 
    /// Each node and edge in the graph is labeled with its topological location relative to
    /// the source point.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Note that there is no requirement that points of self-intersection be a vertex.
    /// Thus to obtain a correct topology graph, <see cref="Geometry{TCoordinate}"/> instances 
    /// must be self-noded before constructing their graphs.
    /// </para>
    /// <para>
    /// Two fundamental operations are supported by topology graphs:
    /// <list type="bullet">
    /// <item>
    /// <description>
    /// Computing the intersections between all the edges and nodes of a single graph.
    /// </description>
    /// </item>
    /// <item>
    /// <description>
    /// Computing the intersections between the edges and nodes of two different graphs.
    /// </description>
    /// </item>
    /// </list>
    /// </para>
    /// </remarks>
    public class PlanarGraph<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
            IComparable<TCoordinate>, IConvertible,
            IComputable<Double, TCoordinate>
    {
        private readonly sgc.IList<EdgeEnd<TCoordinate>> _edgeEndList;
        private readonly sgc.IList<Edge<TCoordinate>> _edgeList;
        private readonly NodeMap<TCoordinate> _nodes;

        public PlanarGraph(NodeFactory<TCoordinate> nodeFactory)
        {
            _nodes = new NodeMap<TCoordinate>(nodeFactory);
            _edgeList = CreateEdgeList();
            _edgeEndList = CreateEdgeEndList();
        }

        public PlanarGraph()
            : this(new NodeFactory<TCoordinate>())
        {
        }

        /// <summary>
        /// Gets the internal <see cref="GeometriesGraph.NodeMap{TCoordinate}"/>.
        /// </summary>
        protected NodeMap<TCoordinate> NodeMap
        {
            get { return _nodes; }
        }

        /// <summary> 
        /// For nodes in the enumeration, link the <see cref="DirectedEdge{TCoordinate}"/>s 
        /// at the node that are in the result.
        /// This allows clients to link only a subset of nodes in the graph, for
        /// efficiency (because they know that only a subset is of interest).
        /// </summary>
        public static void LinkResultDirectedEdges(IEnumerable<Node<TCoordinate>> nodes)
        {
            foreach (Node<TCoordinate> node in nodes)
            {
                ((DirectedEdgeStar<TCoordinate>) node.Edges).LinkResultDirectedEdges();
            }
        }

        protected virtual sgc.IList<Edge<TCoordinate>> CreateEdgeList()
        {
#if C5
            return new C5.ArrayList<Edge<TCoordinate>>();
#else
            return new List<EdgeEnd<TCoordinate>>();
#endif
        }

        protected virtual sgc.IList<EdgeEnd<TCoordinate>> CreateEdgeEndList()
        {
#if C5
            return new C5.ArrayList<EdgeEnd<TCoordinate>>();
#else
            return new List<EdgeEnd<TCoordinate>>();
#endif
            }

        public override String ToString()
        {
            return String.Format("{0}; Edges: {1}; Edge Ends {2};", NodeMap,
                                 _edgeList.Count,
                                 _edgeEndList.Count);
        }

        public void Add(EdgeEnd<TCoordinate> e)
        {
            _nodes.Add(e);
            _edgeEndList.Add(e);
        }

        /// <summary> 
        /// Add a set of edges to the graph.  For each edge two DirectedEdges
        /// will be created.  DirectedEdges are NOT linked by this method.
        /// </summary>
        public void AddEdges(IEnumerable<Edge<TCoordinate>> edgesToAdd)
        {
            // create all the nodes for the edges
            foreach (Edge<TCoordinate> edge in edgesToAdd)
            {
                _edgeList.Add(edge);

                DirectedEdge<TCoordinate> de1 = new DirectedEdge<TCoordinate>(edge, true);
                DirectedEdge<TCoordinate> de2 = new DirectedEdge<TCoordinate>(edge, false);
                de1.Sym = de2;
                de2.Sym = de1;

                Add(de1);
                Add(de2);
            }
        }

        /// <summary>
        /// Adds the given <see cref="Node{TCoordinate}"/> to the node map.
        /// </summary>
        /// <param name="node">The <see cref="Node{TCoordinate}"/> to add.</param>
        /// <returns>The added node.</returns>
        public Node<TCoordinate> AddNode(Node<TCoordinate> node)
        {
            return _nodes.AddNode(node);
        }

        /// <summary>
        /// Adds the given <typeparamref name="TCoordinate"/> as a <see cref="Node{TCoordinate}"/> 
        /// to the node map.
        /// </summary>
        /// <param name="coord">The <typeparamref name="TCoordinate"/> to add.</param>
        /// <returns>The added node.</returns>
        public Node<TCoordinate> AddNode(TCoordinate coord)
        {
            return _nodes.AddNode(coord);
        }

        /// <returns> 
        /// The node if found; null otherwise
        /// </returns>
        public Node<TCoordinate> Find(TCoordinate coord)
        {
            return _nodes.Find(coord);
        }

        /// <summary>
        /// Returns the edge whose first two coordinates are p0 and p1.
        /// </summary>
        /// <returns> The edge, if found <see langword="null" /> if the edge was not found.</returns>
        public Edge<TCoordinate> FindEdge(TCoordinate p0, TCoordinate p1)
        {
            foreach (Edge<TCoordinate> edge in _edgeList)
            {
                Pair<TCoordinate> coordinates = Slice.GetPair(edge.Coordinates).Value;

                if (p0.Equals(coordinates.First) && p1.Equals(coordinates.Second))
                {
                    return edge;
                }
            }

            return null;
        }

        /// <summary> 
        /// Returns the <see cref="EdgeEnd{TCoordinate}"/> which has edge e as its base edge.
        /// </summary>
        /// <param name="e">
        /// The <see cref="Edge{TCoordinate}"/> to lookup the <see cref="EdgeEnd{TCoordinate}"/>s
        /// for.
        /// </param>
        /// <returns>
        /// The <see cref="Pair{TItem}"/> of <see cref="EdgeEnd{TCoordinate}"/>, 
        /// if found; <see langword="null" /> if the edge was not found.
        /// </returns>
        // [2008-05-05 codekaizen] Changed return type to Pair to represent both edge ends connected
        //                         by the given edge 'e'.
        public Pair<EdgeEnd<TCoordinate>>? FindEdgeEnd(Edge<TCoordinate> e)
        {
            EdgeEnd<TCoordinate> first = null;

            foreach (EdgeEnd<TCoordinate> edgeEnd in _edgeEndList)
            {
                if (edgeEnd.Edge == e)
                {
                    if (first == null)
                    {
                        first = edgeEnd;
                    }
                    else
                    {
                        return new Pair<EdgeEnd<TCoordinate>>(first, edgeEnd);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the edge which starts at <paramref name="p0"/> and whose first segment is
        /// parallel to <paramref name="p1"/>.
        /// </summary>
        /// <param name="p0">The coordinate where the sought edge starts.</param>
        /// <param name="p1">The direction of the edge to find.</param>
        /// <returns>The edge, if found; <see langword="null" /> if the edge was not found.</returns>
        public Edge<TCoordinate> FindEdgeInSameDirection(TCoordinate p0, TCoordinate p1)
        {
            foreach (Edge<TCoordinate> edge in _edgeList)
            {
                Pair<TCoordinate> firstPair = Slice.GetPair(edge.Coordinates).Value;

                if (matchInSameDirection(p0, p1, firstPair.First, firstPair.Second))
                {
                    return edge;
                }

                Pair<TCoordinate> secondPair = Slice.GetPair(edge.CoordinatesReversed).Value;

                if (matchInSameDirection(p0, p1, secondPair.First, secondPair.Second))
                {
                    return edge;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns true if the node at <paramref name="coord"/> is on the boundary
        /// of the geometry.
        /// </summary>
        /// <param name="geomIndex">The index of the geometry to check.</param>
        /// <param name="coord">The coordinate of the node.</param>
        /// <returns><see langword="true"/> if the node at <paramref name="coord"/> is on the boundary,
        /// <see langword="false"/> otherwise.</returns>
        public Boolean IsBoundaryNode(Int32 geomIndex, TCoordinate coord)
        {
            Node<TCoordinate> node = _nodes.Find(coord);

            if (node == null)
            {
                return false;
            }

            Label? label = node.Label;

            return label != null && label.Value[geomIndex, Positions.On] == Locations.Boundary;
        }

        /// <summary> 
        /// Link the DirectedEdges at the nodes of the graph.
        /// This allows clients to link only a subset of nodes in the graph, for
        /// efficiency (because they know that only a subset is of interest).
        /// </summary>
        public void LinkAllDirectedEdges()
        {
            foreach (Node<TCoordinate> node in _nodes)
            {
                DirectedEdgeStar<TCoordinate> directedEdgeStar
                    = node.Edges as DirectedEdgeStar<TCoordinate>;
                Debug.Assert(directedEdgeStar != null);
                directedEdgeStar.LinkAllDirectedEdges();
            }
        }

        /// <summary> 
        /// Link the DirectedEdges at the nodes of the graph.
        /// This allows clients to link only a subset of nodes in the graph, for
        /// efficiency (because they know that only a subset is of interest).
        /// </summary>
        public void LinkResultDirectedEdges()
        {
            foreach (Node<TCoordinate> node in _nodes)
            {
                DirectedEdgeStar<TCoordinate> directedEdgeStar
                    = node.Edges as DirectedEdgeStar<TCoordinate>;
                Debug.Assert(directedEdgeStar != null);
                directedEdgeStar.LinkResultDirectedEdges();
            }
        }

        /// <summary>
        /// Adds an <see cref="Edge{TCoordinate}"/> to the graph.
        /// </summary>
        /// <param name="e">The <see cref="Edge{TCoordinate}"/> to add.</param>
        protected void InsertEdge(Edge<TCoordinate> e)
        {
            _edgeList.Add(e);
        }

        /// <summary>
        /// The coordinate pairs match if they define line segments lying in the same direction.
        /// E.g. the segments are parallel and in the same quadrant
        /// (as opposed to parallel and opposite!).
        /// </summary>
        private static Boolean matchInSameDirection(TCoordinate p0, TCoordinate p1,
                                                    TCoordinate ep0, TCoordinate ep1)
        {
            if (!p0.Equals(ep0))
            {
                return false;
            }

            return CGAlgorithms<TCoordinate>.ComputeOrientation(p0, p1, ep1) == Orientation.Collinear &&
                   QuadrantOp<TCoordinate>.Quadrant(p0, p1) == QuadrantOp<TCoordinate>.Quadrant(ep0, ep1);
        }

        #region Public Properties

        /// <summary>
        /// Gets the set of <see cref="Edge{TCoordinate}"/>s in this graph.
        /// </summary>
        public sgc.IList<Edge<TCoordinate>> Edges
        {
            get
            {
                return _edgeList;
                //foreach (Edge<TCoordinate> edge in _edgeList)
                //{
                //    yield return edge;
                //}
            }
        }

        /// <summary>
        /// Gets the set of <see cref="EdgeEnd{TCoordinate}"/>s in this graph.
        /// </summary>
        public IEnumerable<EdgeEnd<TCoordinate>> EdgeEnds
        {
            get
            {
                foreach (EdgeEnd<TCoordinate> edgeEnd in _edgeEndList)
                {
                    yield return edgeEnd;
                }
            }
        }

        /// <summary>
        /// Gets the set of <see cref="Node{TCoordinate}"/>s in this graph.
        /// </summary>
        public IEnumerable<Node<TCoordinate>> Nodes
        {
            get
            {
                foreach (Node<TCoordinate> node in _nodes)
                {
                    yield return node;
                }
            }
        }

        #endregion
    }
}