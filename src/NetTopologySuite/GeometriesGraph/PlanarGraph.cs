using System.Collections.Generic;
using System.IO;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// The computation of the <c>IntersectionMatrix</c> relies on the use of a structure
    /// called a "topology graph". The topology graph contains nodes and edges
    /// corresponding to the nodes and line segments of a <c>Geometry</c>. Each
    /// node and edge in the graph is labeled with its topological location relative to
    /// the source point.
    /// Note that there is no requirement that points of self-intersection be a vertex.
    /// Thus to obtain a correct topology graph, <c>Geometry</c>s must be
    /// self-noded before constructing their graphs.
    /// Two fundamental operations are supported by topology graphs:
    /// Computing the intersections between all the edges and nodes of a single graph
    /// Computing the intersections between the edges and nodes of two different graphs
    /// </summary>
    public class PlanarGraph
    {
        /// <summary>
        /// For nodes in the Collection, link the DirectedEdges at the node that are in the result.
        /// This allows clients to link only a subset of nodes in the graph, for
        /// efficiency (because they know that only a subset is of interest).
        /// </summary>
        /// <param name="nodes">A collection of <c>Node</c>s</param>
        public static void LinkResultDirectedEdges(IList<Node> nodes)
        {
            foreach (var node in nodes)
                ((DirectedEdgeStar) node.Edges).LinkResultDirectedEdges();
        }

        /// <summary>
        ///
        /// </summary>
        private readonly List<Edge> _edges = new List<Edge>();

        /// <summary>
        ///
        /// </summary>
        private readonly NodeMap _nodes;

        /// <summary>
        ///
        /// </summary>
        protected IList<EdgeEnd> edgeEndList = new List<EdgeEnd>();

        /// <summary>
        /// Creates an instance of this class using the provided <see cref="NodeFactory"/>
        /// </summary>
        /// <param name="nodeFact">A factory to create <c>Node</c>s</param>
        public PlanarGraph(NodeFactory nodeFact)
        {
            _nodes = new NodeMap(nodeFact);
        }

        /// <summary>
        /// Creates an instance of this class using the default <see cref="NodeFactory"/>.
        /// </summary>
        public PlanarGraph()
        {
            _nodes = new NodeMap(new NodeFactory());
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Edge> GetEdgeEnumerator()
        {
            return _edges.GetEnumerator();
        }

        /// <summary>
        ///
        /// </summary>
        public IList<EdgeEnd> EdgeEnds => edgeEndList;

        protected internal IList<Edge> Edges => _edges;

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomIndex"></param>
        /// <param name="coord"></param>
        /// <returns></returns>
        public bool IsBoundaryNode(int geomIndex, Coordinate coord)
        {
            var node = _nodes.Find(coord);
            if (node == null)
                return false;
            var label = node.Label;
            if (label != null && label.GetLocation(geomIndex) == Location.Boundary)
                return true;
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        protected void InsertEdge(Edge e)
        {
            _edges.Add(e);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        public void Add(EdgeEnd e)
        {
            _nodes.Add(e);
            edgeEndList.Add(e);
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Node> GetNodeEnumerator()
        {
            return _nodes.GetEnumerator();
        }

        /// <summary>
        ///
        /// </summary>
        public IList<Node> Nodes => new List<Node>(_nodes.Values);

        protected NodeMap NodeMap => _nodes;

        /// <summary>
        ///
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public Node AddNode(Node node)
        {
            return _nodes.AddNode(node);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="coord"></param>
        /// <returns></returns>
        public Node AddNode(Coordinate coord)
        {
            return _nodes.AddNode(coord);
        }

        /// <summary>
        /// Searches for a <c>Node</c> at <c>Coordinate</c> position <paramref name="coord"/>
        /// </summary>
        /// <param name="coord">A <c>Coordinate</c> position</param>
        /// <returns>
        /// The node if found; <c>null</c> otherwise
        /// </returns>
        public Node Find(Coordinate coord)
        {
            return _nodes.Find(coord);
        }

        /// <summary>
        /// Add a set of edges to the graph.  For each edge two DirectedEdges
        /// will be created.  DirectedEdges are NOT linked by this method.
        /// </summary>
        /// <param name="edgesToAdd">A set of <c>Edge</c>s to add.</param>
        public void AddEdges(IList<Edge> edgesToAdd)
        {
            // create all the nodes for the edges
            foreach (var e in edgesToAdd)
            {
                _edges.Add(e);

                var de1 = new DirectedEdge(e, true);
                var de2 = new DirectedEdge(e, false);
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
            foreach (var node in Nodes)
                ((DirectedEdgeStar) node.Edges).LinkResultDirectedEdges();
        }

        /// <summary>
        /// Link the DirectedEdges at the nodes of the graph.
        /// This allows clients to link only a subset of nodes in the graph, for
        /// efficiency (because they know that only a subset is of interest).
        /// </summary>
        public void LinkAllDirectedEdges()
        {
            foreach (var node in Nodes)
                ((DirectedEdgeStar) node.Edges).LinkAllDirectedEdges();
        }

        /// <summary>
        /// Returns the EdgeEnd which has edge e as its base edge
        /// (MD 18 Feb 2002 - this should return a pair of edges).
        /// </summary>
        /// <param name="e">An <c>Edge</c></param>
        /// <returns> The edge, if found <c>null</c> if the edge was not found.</returns>
        public EdgeEnd FindEdgeEnd(Edge e)
        {
            foreach (var ee in edgeEndList       )
                if (ee.Edge == e) return ee;
            return null;
        }

        /// <summary>
        /// Returns the edge whose first two coordinates are p0 and p1.
        /// </summary>
        /// <param name="p0">The 1st <c>Coordinate</c></param>
        /// <param name="p1">The 2nd <c>Coordinate</c></param>
        /// <returns> The edge, if found <c>null</c> if the edge was not found.</returns>
        public Edge FindEdge(Coordinate p0, Coordinate p1)
        {
            for (int i = 0; i < _edges.Count; i++)
            {
                var e = _edges[i];
                var eCoord = e.Coordinates;
                if (p0.Equals(eCoord[0]) && p1.Equals(eCoord[1]))
                    return e;
            }
            return null;
        }

        /// <summary>
        /// Returns the edge which starts at p0 and whose first segment is
        /// parallel to p1.
        /// </summary>
        /// <param name="p0">Starting <c>Coordinate</c></param>
        /// <param name="p1"><c>Coordinate</c> used to establish direction</param>
        /// <returns>The matching edge, if found <c>null</c> if the edge was not found.</returns>
        public Edge FindEdgeInSameDirection(Coordinate p0, Coordinate p1)
        {
            for (int i = 0; i < _edges.Count; i++)
            {
                var e = _edges[i];
                var eCoord = e.Coordinates;
                if (MatchInSameDirection(p0, p1, eCoord[0], eCoord[1]))
                    return e;
                if (MatchInSameDirection(p0, p1, eCoord[eCoord.Length - 1], eCoord[eCoord.Length - 2]))
                    return e;
            }
            return null;
        }

        /// <summary>
        /// The coordinate pairs match if they define line segments lying in the same direction.
        /// E.g. the segments are parallel and in the same quadrant
        /// (as opposed to parallel and opposite!).
        /// </summary>
        /// <param name="p0"></param>
        /// <param name="p1"></param>
        /// <param name="ep0"></param>
        /// <param name="ep1"></param>
        private static bool MatchInSameDirection(Coordinate p0, Coordinate p1, Coordinate ep0, Coordinate ep1)
        {
            if (! p0.Equals(ep0))
                return false;
            if (Orientation.Index(p0, p1, ep1) == OrientationIndex.Collinear &&
                new Quadrant(p0, p1) == new Quadrant(ep0, ep1) )
                return true;
            return false;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="outstream"></param>
        public void WriteEdges(StreamWriter outstream)
        {
            outstream.WriteLine("Edges:");
            for (int i = 0; i < _edges.Count; i++)
            {
                outstream.WriteLine("edge " + i + ":");
                var e = _edges[i];
                e.Write(outstream);
                e.EdgeIntersectionList.Write(outstream);
            }
        }
    }
}
