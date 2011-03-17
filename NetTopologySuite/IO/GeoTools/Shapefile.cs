using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO.Handlers;

namespace GisSharpBlog.NetTopologySuite.IO
{
    /// <summary>
    /// This class is used to read and write ESRI Shapefiles.
    /// </summary>
    public partial class Shapefile
    {
        internal const int ShapefileId = 9994;
        internal const int Version = 1000;

        /// <summary>
        /// Given a geomtery object, returns the equilivent shape file type.
        /// </summary>
        /// <param name="geom">A Geometry object.</param>
        /// <returns>The equilivent for the geometry object.</returns>
        public static ShapeGeometryType GetShapeType(IGeometry geom)
        {
            if (geom is IPoint)
                return ShapeGeometryType.Point;
            if (geom is IPolygon)
                return ShapeGeometryType.Polygon;
            if (geom is IMultiPolygon)
                return ShapeGeometryType.Polygon;
            if (geom is ILineString)
                return ShapeGeometryType.LineString;
            if (geom is IMultiLineString)
                return ShapeGeometryType.LineString;
            if (geom is IMultiPoint)
                return ShapeGeometryType.MultiPoint;
            return ShapeGeometryType.NullShape;
        }

        /// <summary>
        /// Returns the appropriate class to convert a shaperecord to an OGIS geometry given the type of shape.
        /// </summary>
        /// <param name="type">The shapefile type.</param>
        /// <returns>An instance of the appropriate handler to convert the shape record to a Geometry object.</returns>
        public static ShapeHandler GetShapeHandler(ShapeGeometryType type)
        {
            switch (type)
            {
                case ShapeGeometryType.Point:
                case ShapeGeometryType.PointM:
                case ShapeGeometryType.PointZ:
                case ShapeGeometryType.PointZM:
                    return new PointHandler();

                case ShapeGeometryType.Polygon:
                case ShapeGeometryType.PolygonM:
                case ShapeGeometryType.PolygonZ:
                case ShapeGeometryType.PolygonZM:
                    return new PolygonHandler();

                case ShapeGeometryType.LineString:
                case ShapeGeometryType.LineStringM:
                case ShapeGeometryType.LineStringZ:
                case ShapeGeometryType.LineStringZM:
                    return new MultiLineHandler();

                case ShapeGeometryType.MultiPoint:
                case ShapeGeometryType.MultiPointM:
                case ShapeGeometryType.MultiPointZ:
                case ShapeGeometryType.MultiPointZM:
                    return new MultiPointHandler();

                default:
                    return null;
            }
        }

        /// <summary>
        /// Returns an ShapefileDataReader representing the data in a shapefile.
        /// </summary>
        /// <param name="filename">The filename (minus the . and extension) to read.</param>
        /// <param name="geometryFactory">The geometry factory to use when creating the objects.</param>
        /// <returns>An ShapefileDataReader representing the data in the shape file.</returns>
        public static ShapefileDataReader CreateDataReader(string filename, GeometryFactory geometryFactory)
        {
            if (filename == null)
                throw new ArgumentNullException("filename");
            if (geometryFactory == null)
                throw new ArgumentNullException("geometryFactory");
            var shpDataReader = new ShapefileDataReader(filename, geometryFactory);
            return shpDataReader;
        }
    }
}