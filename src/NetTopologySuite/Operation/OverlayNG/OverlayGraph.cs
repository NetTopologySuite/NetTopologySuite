using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NetTopologySuite.EdgeGraph;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.OverlayNg
{
    /// <summary>
    /// A planar graph of {@link OverlayEdge}s, representing
    /// the topology resulting from an overlay operation.
    /// Each source <see cref="Edge"/> is represented
    /// by two OverlayEdges, with opposite orientation.
    /// A single <see cref="OverlayLabel"/> is created for each symmetric pair of OverlayEdges.
    /// </summary>
    /// <author>Martin Davis</author>
    internal class OverlayGraph
    {

        private readonly List<OverlayEdge> _edges = new List<OverlayEdge>();
        private readonly IDictionary<Coordinate, OverlayEdge> _nodeMap = new Dictionary<Coordinate, OverlayEdge>();

        /// <summary>
        /// Creates a new graph for a set of noded, labelled <see cref="Edge"/>s.
        /// </summary>
        /// <param name="edges">The edges on which to build the graph</param>
        public OverlayGraph(ICollection<Edge> edges)
        {
            Build(edges);
        }

        /// <summary>
        /// Gets the set of edges in this graph.
        /// Only one of each symmetric pair of OverlayEdges is included. 
        /// The opposing edge can be found by using <see cref="HalfEdge.Sym"/>.
        /// </summary>
        /// <returns>The collection of representative edges in this graph</returns>
        public IList<OverlayEdge> Edges
        {
            get => _edges;
        }

        /// <summary>
        /// Gets the collection of edges representing the nodes in this graph.
        /// For each star of edges originating at a node
        /// a single representative edge is included.<br/>
        /// The other edges around the node can be found by following the next and prev links.
        /// </summary>
        /// <returns>The collection of representative node edges</returns>
        public ICollection<OverlayEdge> NodeEdges
        {
            get => _nodeMap.Values;
        }

        /// <summary>
        /// Gets an edge originating at the given node point.
        /// </summary>
        /// <param name="nodePt">The node coordinate to query</param>
        /// <returns>An edge originating at the point, or <c>null</c> if none exists</returns>
        public OverlayEdge GetNodeEdge(Coordinate nodePt)
        {
            _nodeMap.TryGetValue(nodePt, out var result);
            return result;
        }

        /**
         * 
         * 
         * @return 
         */
        /// <summary>
        /// Gets the representative edges marked as being in the result area.
        /// </summary>
        /// <returns>The result area edges</returns>
        public IReadOnlyCollection<OverlayEdge> GetResultAreaEdges()
        {
            var resultEdges = new List<OverlayEdge>();
            foreach (var edge in _edges)
            {
                if (edge.IsInResultArea)
                {
                    resultEdges.Add(edge);
                }
            }
            return resultEdges;
        }

        private void Build(IEnumerable<Edge> edges)
        {
            var tmp = new List<Edge>(edges);
            var sort = tmp.ToArray();
            foreach (var e in sort)
            {
                AddEdge(e);
            }
        }

        /// <summary>
        /// Adds an edge between the coordinates orig and dest
        /// to this graph.<br/>
        /// Only valid edges can be added (in particular, zero-length segments cannot be added)
        /// </summary>
        /// <param name="edge">The edge to add.</param>
        ///// <seealso cref="IsValidEdge(Coordinate, Coordinate)"/>
        private OverlayEdge AddEdge(Edge edge)
        {
            //if (! isValidEdge(orig, dest)) return null;
            var e = CreateEdges(edge.Coordinates, edge.CreateLabel());
            //Debug.println("added edge: " + e);
            Insert(e);
            Insert((OverlayEdge)e.Sym);
            return e;
        }

        private static OverlayEdge CreateEdges(Coordinate[] pts, OverlayLabel lbl)
        {
            var e0 = OverlayEdge.CreateEdge(pts, lbl, true);
            var e1 = OverlayEdge.CreateEdge(pts, lbl, false);
            e0.Link(e1);
            return e0;
        }

        /// <summary>
        /// Tests if the given coordinates form a valid edge (with non-zero length).
        /// </summary>
        /// <param name="orig">The start coordinate</param>
        /// <param name="dest">The end coordinate</param>
        /// <returns><c>true</c> if the edge formed is valid</returns>
        private static bool IsValidEdge(Coordinate orig, Coordinate dest)
        {
            int cmp = dest.CompareTo(orig);
            return cmp != 0;
        }

        private void Insert(OverlayEdge e)
        {
            _edges.Add(e);
            if (_nodeMap.TryGetValue(e.Orig, out var nodeEdge))
                nodeEdge.Insert(e);
            else
            {
                // add edge origin to node map
                // (sym is also added in separate call)
                _nodeMap.Add(e.Orig, e);
            }
        }

    }

}
