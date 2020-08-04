using NetTopologySuite.Geometries;
using NetTopologySuite.Planargraph;

namespace NetTopologySuite.Operation.Linemerge
{
    /// <summary>
    /// A planar graph of edges that is analyzed to sew the edges together. The
    /// <c>marked</c> flag on <see cref="T:NetTopologySuite.Planargraph.Edge"/>s
    /// and <see cref="T:NetTopologySuite.Planargraph.Node"/>s indicates whether they have been
    /// logically deleted from the graph.
    /// </summary>
    public class LineMergeGraph : PlanarGraph
    {
        /// <summary>
        /// Adds an Edge, DirectedEdges, and Nodes for the given LineString representation
        /// of an edge.
        /// </summary>
        public void AddEdge(LineString lineString)
        {
            if (lineString.IsEmpty)
                return;

            var coordinates = CoordinateArrays.RemoveRepeatedPoints(lineString.Coordinates);
            if (coordinates.Length < 2)
                return; // same check already added in PolygonizeGraph (see #87 and #146)

            var startCoordinate = coordinates[0];
            var endCoordinate = coordinates[coordinates.Length - 1];
            var startNode = GetNode(startCoordinate);
            var endNode = GetNode(endCoordinate);
            var directedEdge0 = new LineMergeDirectedEdge(startNode, endNode,
                                                          coordinates[1], true);
            var directedEdge1 = new LineMergeDirectedEdge(endNode, startNode,
                                                          coordinates[coordinates.Length - 2], false);
            var edge = new LineMergeEdge(lineString);
            edge.SetDirectedEdges(directedEdge0, directedEdge1);
            Add(edge);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        private Node GetNode(Coordinate coordinate)
        {
            var node = FindNode(coordinate);
            if (node == null)
            {
                node = new Node(coordinate);
                Add(node);
            }
            return node;
        }
    }
}
