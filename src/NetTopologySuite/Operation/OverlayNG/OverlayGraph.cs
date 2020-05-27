using System.Collections.Generic;
using System.Collections.ObjectModel;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.OverlayNg
{
    /**
     * A planar graph of {@link OverlayEdge}s, representing
     * the topology resulting from an overlay operation.
     * Each source {@link Edge} is represented
     * by two OverlayEdges, with opposite orientation.
     * A single {@link OverlayLabel} is created for each symmetric pair of OverlayEdges.
     * 
     * @author mdavis
     *
     */
    class OverlayGraph
    {

        private readonly List<OverlayEdge> _edges = new List<OverlayEdge>();
        private readonly IDictionary<Coordinate, OverlayEdge> _nodeMap = new Dictionary<Coordinate, OverlayEdge>();

        /**
         * Creates a new graph for a set of noded, labelled {@link Edge}s.
         * 
         * @param edges the edges on which to build the graph
         */
        public OverlayGraph(ICollection<Edge> edges)
        {
            Build(edges);
        }

        /**
         * Gets the set of edges in this graph.
         * Only one of each symmetric pair of OverlayEdges is included. 
         * The opposing edge can be found by using {@link OverlayEdge#sym()}.
         * 
         * @return the collection of representative edges in this graph
         */
        public IList<OverlayEdge> Edges
        {
            get => _edges;
        }

        /**
         * Gets the collection of edges representing the nodes in this graph.
         * For each star of edges originating at a node
         * a single representative edge is included.
         * The other edges around the node can be found by following the next and prev links.
         * 
         * @return the collection of representative node edges
         */
        public ICollection<OverlayEdge> NodeEdges
        {
            get => _nodeMap.Values;
        }

        /**
         * Gets an edge originating at the given node point.
         * 
         * @param nodePt the node coordinate to query
         * @return an edge originating at the point, or null if none exists
         */
        public OverlayEdge GetNodeEdge(Coordinate nodePt)
        {
            _nodeMap.TryGetValue(nodePt, out var result);
            return result;
        }

        /**
         * Gets the representative edges marked as being in the result area.
         * 
         * @return the result area edges
         */
        public IReadOnlyCollection<OverlayEdge> getResultAreaEdges()
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
            foreach (var e in edges)
            {
                AddEdge(e);
            }
        }

        /**
         * Adds an edge between the coordinates orig and dest
         * to this graph.
         * Only valid edges can be added (in particular, zero-length segments cannot be added)
         * 
         * @param orig the edge origin location
         * @param dest the edge destination location.
         * @return the created edge
         * @return null if the edge was invalid and not added
         * 
         * @see #isValidEdge(Coordinate, Coordinate)
         */
        private OverlayEdge AddEdge(Edge edge)
        {
            //if (! isValidEdge(orig, dest)) return null;
            var e = createEdges(edge.getCoordinates(), edge.createLabel());
            //Debug.println("added edge: " + e);
            Insert(e);
            Insert((OverlayEdge)e.Sym);
            return e;
        }

        private OverlayEdge createEdges(Coordinate[] pts, OverlayLabel lbl)
        {
            var e0 = OverlayEdge.createEdge(pts, lbl, true);
            var e1 = OverlayEdge.createEdge(pts, lbl, false);
            e0.Link(e1);
            return e0;
        }

        /**
         * Tests if the given coordinates form a valid edge (with non-zero length).
         * 
         * @param orig the start coordinate
         * @param dest the end coordinate
         * @return true if the edge formed is valid
         */
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
