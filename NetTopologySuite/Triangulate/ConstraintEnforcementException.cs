using System;
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
using NetTopologySuite.IO;

namespace NetTopologySuite.Triangulate
{
    /// <summary>
    /// Indicates a failure during constraint enforcement.
    /// </summary>
    /// <author>Martin Davis</author>
    /// <version>1.0</version>
    public class ConstraintEnforcementException : ApplicationException
    {

        //private long serialVersionUID = 386496846550080140L;

        private static String MsgWithCoord(String msg, ICoordinate pt) {
            if (pt != null)
                return msg + " [ " + WKTWriter.ToPoint(pt) + " ]";
            return msg;
        }

        private readonly ICoordinate _pt;

        /// <summary>
        /// Creates a new instance with a given message.
        /// </summary>
        /// <param name="msg">a string</param>
        public ConstraintEnforcementException(string msg)
            : base(msg)
        {
        }

        /// <summary>
        /// Creates a new instance with a given message and approximate location.
        /// </summary>
        /// <param name="msg">a string</param>
        /// <param name="pt">the location of the error</param>
        public ConstraintEnforcementException(String msg, ICoordinate pt)
            : base(MsgWithCoord(msg, pt))
        {
            _pt = new Coordinate(pt);
        }

        /// <summary>
        /// Gets the approximate location of this error.
        /// </summary>
        /// <remarks>a location</remarks>
        public ICoordinate Coordinate
        {
            get
            {
                return _pt;
            }
        }
    }
}