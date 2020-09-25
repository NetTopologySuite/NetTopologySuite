using System.Collections.Generic;
using NetTopologySuite.EdgeGraph;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>
    /// A planar graph of edges, representing
    /// the topology resulting from an overlay operation.
    /// Each source edge is represented
    /// by a pair of <see cref="OverlayEdge"/>s,
    /// with opposite(symmetric) orientation.
    /// The pair of OverlayEdges share the edge coordinates
    /// and a single <see cref="OverlayLabel"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    internal sealed class OverlayGraph
    {
        private readonly Dictionary<Coordinate, OverlayEdge> _nodeMap = new Dictionary<Coordinate, OverlayEdge>();

        /// <summary>
        /// Gets the set of edges in this graph.
        /// Only one of each symmetric pair of OverlayEdges is included. 
        /// The opposing edge can be found by using <see cref="HalfEdge.Sym"/>.
        /// </summary>
        /// <returns>The collection of representative edges in this graph</returns>
        public List<OverlayEdge> Edges { get; } = new List<OverlayEdge>();

        /// <summary>
        /// Gets the collection of edges representing the nodes in this graph.
        /// For each star of edges originating at a node
        /// a single representative edge is included.<br/>
        /// The other edges around the node can be found by following the next and prev links.
        /// </summary>
        /// <returns>The collection of representative node edges</returns>
        public IReadOnlyCollection<OverlayEdge> NodeEdges
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

        /// <summary>
        /// Gets the representative edges marked as being in the result area.
        /// </summary>
        /// <returns>The result area edges</returns>
        public IReadOnlyCollection<OverlayEdge> GetResultAreaEdges()
        {
            var resultEdges = new List<OverlayEdge>();
            foreach (var edge in Edges)
            {
                if (edge.IsInResultArea)
                {
                    resultEdges.Add(edge);
                }
            }
            return resultEdges;
        }

        /// <summary>
        /// Adds an edge between the coordinates orig and dest
        /// to this graph.<br/>
        /// Only valid edges can be added (in particular, zero-length segments cannot be added)
        /// </summary>
        /// <param name="pts">The edge to add.</param>
        /// <param name="label">The edge topology information</param>
        /// <returns>The created graph edge with same orientation as the linework</returns>
        public OverlayEdge AddEdge(Coordinate[] pts, OverlayLabel label)
        {
            //if (! isValidEdge(orig, dest)) return null;
            var e = OverlayEdge.CreateEdgePair(pts, label);
            //Debug.println("added edge: " + e);
            Insert(e);
            Insert(e.SymOE);
            return e;
        }

        /// <summary>
        /// Inserts a single half-edge into the graph.
        /// The sym edge must also be inserted.
        /// </summary>
        /// <param name="e">The half-edge to insert</param>
        private void Insert(OverlayEdge e)
        {
            Edges.Add(e);
            /*
             * If the edge origin node is already in the graph, 
             * insert the edge into the star of edges around the node.
             * Otherwise, add a new node for the origin.
             */
            if (_nodeMap.TryGetValue(e.Orig, out var nodeEdge))
                nodeEdge.Insert(e);
            else
            {
                _nodeMap.Add(e.Orig, e);
            }
        }

    }

}
