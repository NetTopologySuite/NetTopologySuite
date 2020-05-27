using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.OverlayNg
{
    /// <summary>
    /// Extracts Point resultants from an overlay graph
    /// created by an Intersection operation
    /// between non-Point inputs.
    /// Points may be created during intersection
    /// if lines or areas touch one another at single points.
    /// Intersection is the only overlay operation which can
    /// result in Points from non-Point inputs.
    /// <para/>
    /// Overlay operations where one or more inputs
    /// are Points are handled via a different code path.
    /// </summary>
    /// <author>Martin Davis</author>
    /// <seealso cref="OverlayPoints"/>
    class IntersectionPointBuilder
    {

        private readonly GeometryFactory _geometryFactory;
        private readonly OverlayGraph _graph;
        private List<Point> _points = new List<Point>();

        public IntersectionPointBuilder(OverlayGraph graph,
            GeometryFactory geomFact)
        {
            _graph = graph;
            _geometryFactory = geomFact;
        }

        public List<Point> Points
        {
            get
            {
                if (_points == null)
                {
                    _points = new List<Point>();
                    AddResultPoints();
                }

                return _points;
            }
        }

        private void AddResultPoints()
        {
            foreach (var nodeEdge in _graph.NodeEdges)
            {
                if (IsResultPoint(nodeEdge))
                {
                    var pt = _geometryFactory.CreatePoint(nodeEdge.Coordinate.Copy());
                    _points.Add(pt);
                }
            }
        }

        /// <summary>
        /// Tests if a node is a result point.
        /// This is the case if the node is incident on edges from both
        /// inputs, and none of the edges are themselves in the result.
        /// </summary>
        /// <param name="nodeEdge">An edge originating at the node</param>
        /// <returns><c>true</c> if the node is a result point.</returns>
        private bool IsResultPoint(OverlayEdge nodeEdge)
        {
            bool isEdgeOfA = false;
            bool isEdgeOfB = false;

            var edge = nodeEdge;
            do
            {
                if (edge.IsInResult) return false;
                var label = edge.Label;
                isEdgeOfA |= IsEdgeOf(label, 0);
                isEdgeOfB |= IsEdgeOf(label, 1);
                edge = edge.ONextOE;
            } while (edge != nodeEdge);
            bool isNodeInBoth = isEdgeOfA && isEdgeOfB;
            return isNodeInBoth;
        }

        private static bool IsEdgeOf(OverlayLabel label, int i)
        {
            return label.isBoundary(i) || label.isLine(i);
        }

    }
}
