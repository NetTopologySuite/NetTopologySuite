using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
#region geoapi vs nts
#if useFullGeoAPI
using ICoordinate = GeoAPI.Geometries.ICoordinate;
using IGeometry = GeoAPI.Geometries.IGeometry;
using IPoint = GeoAPI.Geometries.IPoint;
using ILineString = GeoAPI.Geometries.ILineString;
using ILinearRing = GeoAPI.Geometries.ILinearRing;
using IPolygon = GeoAPI.Geometries.IPolygon;
using IGeometryCollection = GeoAPI.Geometries.IGeometryCollection;
using IMultiPoint = GeoAPI.Geometries.IMultiPoint;
using IMultiLineString = GeoAPI.Geometries.IMultiLineString;
using IMultiPolygon = GeoAPI.Geometries.IMultiPolygon;
#else
using ICoordinate = NetTopologySuite.Geometries.Coordinate;
using IGeometry = NetTopologySuite.Geometries.Geometry;
using IPoint = NetTopologySuite.Geometries.Point;
using ILineString = NetTopologySuite.Geometries.LineString;
using ILinearRing = NetTopologySuite.Geometries.LinearRing;
using IPolygon = NetTopologySuite.Geometries.Polygon;
using IGeometryCollection = NetTopologySuite.Geometries.GeometryCollection;
using IMultiPoint = NetTopologySuite.Geometries.MultiPoint;
using IMultiLineString = NetTopologySuite.Geometries.MultiLineString;
using IMultiPolygon = NetTopologySuite.Geometries.MultiPolygon;
using IGeometryFilter = NetTopologySuite.Geometries.IGeometryFilter;
#endif
#endregion

namespace NetTopologySuite.Operation.Distance
{
    /// <summary>
    /// Extracts a single point
    /// from each connected element in a Geometry
    /// (e.g. a polygon, linestring or point)
    /// and returns them in a list
    /// </summary>
    public class ConnectedElementPointFilter : IGeometryFilter
    {
        /// <summary>
        /// Returns a list containing a Coordinate from each Polygon, LineString, and Point
        /// found inside the specified point. Thus, if the specified point is
        /// not a GeometryCollection, an empty list will be returned.
        /// </summary>
        public static IList<ICoordinate> GetCoordinates(Geometry geom)
        {
            IList<ICoordinate> pts = new List<ICoordinate>();
            geom.Apply(new ConnectedElementPointFilter(pts));
            return pts;
        }

        private readonly IList<ICoordinate> _pts;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pts"></param>
        ConnectedElementPointFilter(IList<ICoordinate> pts)
        {
            _pts = pts;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geom"></param>
        public void Filter(IGeometry geom)
        {
            if (geom is IPoint || geom is ILineString || geom is IPolygon)
                _pts.Add(geom.Coordinate);
        }
    }
}
