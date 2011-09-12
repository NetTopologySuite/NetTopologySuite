#if useFullGeoAPI
using GeoAPI.Geometries;
#else
using ICoordinate = NetTopologySuite.Geometries.Coordinate;
using IEnvelope = NetTopologySuite.Geometries.Envelope;
using IGeometry = NetTopologySuite.Geometries.Geometry;
using IPoint = NetTopologySuite.Geometries.Point;
using ILineString = NetTopologySuite.Geometries.LineString;
using ILinearRing = NetTopologySuite.Geometries.LinearRing;
using IPolygon = NetTopologySuite.Geometries.Polygon;
using IGeometryCollection = NetTopologySuite.Geometries.GeometryCollection;
using IMultiPoint = NetTopologySuite.Geometries.MultiPoint;
using IMultiLineString = NetTopologySuite.Geometries.MultiLineString;
using IMultiPolygon = NetTopologySuite.Geometries.MultiPolygon;
using IGeometryFactory = NetTopologySuite.Geometries.GeometryFactory;
using IPrecisionModel = NetTopologySuite.Geometries.PrecisionModel;
#endif
using NetTopologySuite.Geometries;
using NetTopologySuite.Planargraph;

namespace NetTopologySuite.Operation.Linemerge
{
    /// <summary>
    /// A planar graph of edges that is analyzed to sew the edges together. The 
    /// <c>marked</c> flag on <c>com.vividsolutions.planargraph.Edge</c>s 
    /// and <c>com.vividsolutions.planargraph.Node</c>s indicates whether they have been
    /// logically deleted from the graph.
    /// </summary>
    public class LineMergeGraph : PlanarGraph 
    {
        /// <summary>
        /// Adds an Edge, DirectedEdges, and Nodes for the given LineString representation
        /// of an edge. 
        /// </summary>
        public void AddEdge(ILineString lineString)
        {
            if (lineString.IsEmpty)
                return;
            ICoordinate[] coordinates = CoordinateArrays.RemoveRepeatedPoints(lineString.Coordinates);
            ICoordinate startCoordinate = coordinates[0];
            ICoordinate endCoordinate = coordinates[coordinates.Length - 1];
            Node startNode = GetNode(startCoordinate);
            Node endNode = GetNode(endCoordinate);
            DirectedEdge directedEdge0 = new LineMergeDirectedEdge(startNode, endNode, 
                                                coordinates[1], true);
            DirectedEdge directedEdge1 = new LineMergeDirectedEdge(endNode, startNode, 
                                                coordinates[coordinates.Length - 2], false);
            Edge edge = new LineMergeEdge(lineString);
            edge.SetDirectedEdges(directedEdge0, directedEdge1);
            Add(edge);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        private Node GetNode(ICoordinate coordinate) 
        {
            Node node = FindNode(coordinate);
            if (node == null) 
            {
                node = new Node(coordinate);
                Add(node);
            }
            return node;
        }
    }
}
