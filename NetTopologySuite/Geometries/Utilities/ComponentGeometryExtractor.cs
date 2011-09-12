using System.Collections.Generic;
#if useFullGeoAPI
using GeoAPI.Geometries;
#else
using ICoordinate = NetTopologySuite.Geometries.Coordinate;
using IEnvelope = NetTopologySuite.Geometries.Envelope;
using IPrecisionModel = NetTopologySuite.Geometries.PrecisionModel;
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
#endif

namespace NetTopologySuite.Geometries.Utilities
{
    ///<summary>
    /// Extracts a single representative <see cref="ICoordinate"/> from each connected component of a <see cref="IGeometry"/>.
    ///</summary>
    /// <version>1.9</version>
    public class ComponentCoordinateExtracter : IGeometryComponentFilter
    {

        ///<summary>
        /// Extracts a single representative <see cref="ICoordinate"/> from each connected component of a <see cref="IGeometry"/>.
        ///</summary>
        /// <param name="geom">The Geometry from which to extract</param>
        /// <returns>A list of Coordinates</returns>
        public static IList<ICoordinate> GetCoordinates(IGeometry geom)
        {
            IList<ICoordinate> coords = new List<ICoordinate>();
            geom.Apply(new ComponentCoordinateExtracter(coords));
            return coords;
        }

        private readonly IList<ICoordinate> _coords;

        ///<summary>
        /// Constructs a LineExtracterFilter with a list in which to store LineStrings found.
        ///</summary>
        public ComponentCoordinateExtracter(IList<ICoordinate> coords)
        {
            _coords = coords;
        }

        public void Filter(IGeometry geom)
        {
            // add coordinates from connected components
            if (geom is ILineString
                || geom is IPoint)
                _coords.Add(geom.Coordinate);
        }

    }
}