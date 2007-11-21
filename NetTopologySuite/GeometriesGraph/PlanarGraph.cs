using System;
using System.Diagnostics;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;
using System.Collections.Generic;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary> 
    /// The computation of the <see cref="IntersectionMatrix"/> relies on the use of a structure
    /// called a "topology graph". The topology graph contains nodes and edges
    /// corresponding to the nodes and line segments of a <see cref="Geometry{TCoordinate}"/>. Each
    /// node and edge in the graph is labeled with its topological location relative to
    /// the source point.
    /// </summary>
    /// <remarks>
    /// Note that there is no requirement that points of self-intersection be a vertex.
    /// Thus to obtain a correct topology graph, <see cref="Geometry{TCoordinate}"/>s must be
    /// self-noded before constructing their graphs.
    /// Two fundamental operations are supported by topology graphs:
    /// Computing the intersections between all the edges and nodes of a single graph
    /// Computing the intersections between the edges and nodes of two different graphs
    /// </remarks>
    public class PlanarGraph<TCoordinate>
         where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                             IComputable<TCoordinate>, IConvertible
    {
        /// <summary> 
        /// For nodes in the Collection, link the DirectedEdges at the node that are in the result.
        /// This allows clients to link only a subset of nodes in the graph, for
        /// efficiency (because they know that only a subset is of interest).
        /// </summary>
        public static void LinkResultDirectedEdges(IEnumerable<Node<TCoordinate>> nodes)
        {
            foreach (Node<TCoordinate> node in nodes)
            {
                ((DirectedEdgeStar<TCoordinate>)node.Edges).LinkResultDirectedEdges();
            }
        }

        private readonly List<Edge<TCoordinate>> _edgeList = new List<Edge<TCoordinate>>();
        private readonly NodeMap<TCoordinate> _nodes = null;
        private readonly List<EdgeEnd<TCoordinate>> _edgeEndList = new List<EdgeEnd<TCoordinate>>();

        public PlanarGraph(NodeFactory<TCoordinate> nodeFactory)
        {
            _nodes = new NodeMap<TCoordinate>(nodeFactory);
        }

        public PlanarGraph()
        {
            _nodes = new NodeMap<TCoordinate>(new NodeFactory<TCoordinate>());
        }

        public IEnumerable<Edge<TCoordinate>> Edges
        {
            get
            {
                foreach (Edge<TCoordinate> edge in _edgeList)
                {
                    yield return edge;
                }
            }
        }

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

        public Boolean IsBoundaryNode(Int32 geomIndex, TCoordinate coord)
        {
            Node<TCoordinate> node = _nodes.Find(coord);

            if (node == null)
            {
                return false;
            }

            Label label = node.Label;

            if (label != null && label.GetLocation(geomIndex) == Locations.Boundary)
            {
                return true;
            }

            return false;
        }

        protected void InsertEdge(Edge<TCoordinate> e)
        {
            _edgeList.Add(e);
        }

        public void Add(EdgeEnd<TCoordinate> e)
        {
            _nodes.Add(e);
            _edgeEndList.Add(e);
        }

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

        public Node<TCoordinate> AddNode(Node<TCoordinate> node)
        {
            return _nodes.AddNode(node);
        }

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
        /// Link the DirectedEdges at the nodes of the graph.
        /// This allows clients to link only a subset of nodes in the graph, for
        /// efficiency (because they know that only a subset is of interest).
        /// </summary>
        public void LinkResultDirectedEdges()
        {
            foreach (Node<TCoordinate> node in _nodes)
            {
                if (node.Edges is DirectedEdgeStar<TCoordinate>)
                {
                    DirectedEdgeStar<TCoordinate> directedEdgeStar 
                        = node.Edges as DirectedEdgeStar<TCoordinate>;
                    Debug.Assert(directedEdgeStar != null);
                    directedEdgeStar.LinkResultDirectedEdges();
                }
            }
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
        /// Returns the EdgeEnd which has edge e as its base edge.
        /// </summary>
        /// <returns> The edge, if found; <see langword="null" /> if the edge was not found.</returns>
        // TODO: MD 18 Feb 2002 - this should return a pair of edges.
        public EdgeEnd<TCoordinate> FindEdgeEnd(Edge<TCoordinate> e)
        {
            foreach (EdgeEnd<TCoordinate> edgeEnd in _edgeEndList)
            {
                if (edgeEnd.Edge == e)
                {
                    return edgeEnd;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the edge whose first two coordinates are p0 and p1.
        /// </summary>
        /// <returns> The edge, if found <see langword="null" /> if the edge was not found.</returns>
        public Edge<TCoordinate> FindEdge(TCoordinate p0, TCoordinate p1)
        {
            foreach (Edge<TCoordinate> edge in _edgeList)
            {
                Pair<TCoordinate> coordinates = Slice.GetPair(edge.Coordinates);

                if (p0.Equals(coordinates.First) && p1.Equals(coordinates.Second))
                {
                    return edge;
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the edge which starts at p0 and whose first segment is
        /// parallel to p1.
        /// </summary>
        /// <returns> The edge, if found <see langword="null" /> if the edge was not found.</returns>
        public Edge<TCoordinate> FindEdgeInSameDirection(TCoordinate p0, TCoordinate p1)
        {
            foreach (Edge<TCoordinate> edge in _edgeList)
            {
                Pair<TCoordinate> firstPair = Slice.GetPair(edge.Coordinates);

                if (matchInSameDirection(p0, p1, firstPair.First, firstPair.Second))
                {
                    return edge;
                }

                Pair<TCoordinate> secondPair = Slice.GetPair(edge.CoordinatesReversed);

                if (matchInSameDirection(p0, p1, secondPair.First, secondPair.Second))
                {
                    return edge;
                }
            }

            return null;
        }

        public void WriteEdges(StreamWriter outstream)
        {
            outstream.WriteLine("Edges:");

            Int32 edgeCount = 0;

            foreach (Edge<TCoordinate> edge in _edgeList)
            {
                outstream.WriteLine("edge " + edgeCount + ":");
                edge.Write(outstream);
                edge.EdgeIntersectionList.Write(outstream);
                edgeCount++;
            }
        }

        protected NodeMap<TCoordinate> NodeMap
        {
            get { return _nodes; }
        }

        /// <summary>
        /// The coordinate pairs match if they define line segments lying in the same direction.
        /// E.g. the segments are parallel and in the same quadrant
        /// (as opposed to parallel and opposite!).
        /// </summary>
        private static Boolean matchInSameDirection(TCoordinate p0, TCoordinate p1, TCoordinate ep0, TCoordinate ep1)
        {
            if (!p0.Equals(ep0))
            {
                return false;
            }

            if (CGAlgorithms<TCoordinate>.ComputeOrientation(p0, p1, ep1) == Orientation.Collinear &&
                QuadrantOp<TCoordinate>.Quadrant(p0, p1) == QuadrantOp<TCoordinate>.Quadrant(ep0, ep1))
            {
                return true;
            }

            return false;
        }
    }
}