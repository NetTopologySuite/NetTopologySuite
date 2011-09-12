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
using NetTopologySuite.GeometriesGraph;

namespace NetTopologySuite.Operation.Overlay
{
    /// <summary>
    /// A ring of edges with the property that no node
    /// has degree greater than 2.  These are the form of rings required
    /// to represent polygons under the OGC SFS spatial data model.
    /// </summary>
    public class MinimalEdgeRing : EdgeRing
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="geometryFactory"></param>
        public MinimalEdgeRing(DirectedEdge start, IGeometryFactory geometryFactory) 
            : base(start, geometryFactory) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="de"></param>
        /// <returns></returns>
        public override DirectedEdge GetNext(DirectedEdge de)
        {
            return de.NextMin;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="de"></param>
        /// <param name="er"></param>
        public override void SetEdgeRing(DirectedEdge de, EdgeRing er)
        {
            de.MinEdgeRing = er;
        }
    }
}
