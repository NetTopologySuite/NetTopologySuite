using System;
using GeoAPI.Geometries;
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
#endif
#endregion

namespace NetTopologySuite.Geometries
{
    /// <summary> 
    /// Indicates an invalid or inconsistent topological situation encountered during processing
    /// </summary>
    public class TopologyException : ApplicationException
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        private static string MsgWithCoord(string msg, ICoordinate pt)
        {
            if (pt != null)
            return msg + " [ " + pt + " ]";
            return msg;
        }

        private ICoordinate pt = null;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        public TopologyException(string msg) : base(msg) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="pt"></param>
        public TopologyException(string msg, ICoordinate pt) 
            : base (MsgWithCoord(msg, pt))
        {            
            this.pt = new Coordinate(pt);
        }

        /// <summary>
        /// 
        /// </summary>
        public ICoordinate Coordinate
        {
            get
            {
                return pt;
            }
        }
    }
}
