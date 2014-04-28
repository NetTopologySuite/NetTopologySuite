using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Handlers;

namespace NetTopologySuite.IO
{
    /// <summary>
    /// This class is used to read and write ESRI Shapefiles.
    /// </summary>
    public partial class Shapefile
    {
        internal const int ShapefileId = 9994;
        internal const int Version = 1000;

        /// <summary>
        /// Given a geomtery object, returns the equivalent shape file type.
        /// </summary>
        /// <param name="geom">A Geometry object.</param>
        /// <returns>The equivalent for the geometry object.</returns>
        public static ShapeGeometryType GetShapeType(IGeometry geom)
        {
            geom = GetNonEmptyGeometry(geom);

            if (geom == null)
                return ShapeGeometryType.NullShape;

            switch (geom.OgcGeometryType)
            {
                case OgcGeometryType.Point:
                    switch (((IPoint)geom).CoordinateSequence.Ordinates)
                    {
                        case Ordinates.XYM:
                            return ShapeGeometryType.PointM;
                        case Ordinates.XYZ:
                        case Ordinates.XYZM:
                            return ShapeGeometryType.PointZM;
                        default:
                            return ShapeGeometryType.Point;
                    }
                case OgcGeometryType.MultiPoint:
                    switch (((IPoint)geom.GetGeometryN(0)).CoordinateSequence.Ordinates)
                    {
                        case Ordinates.XYM:
                            return ShapeGeometryType.MultiPointM;
                        case Ordinates.XYZ:
                        case Ordinates.XYZM:
                            return ShapeGeometryType.MultiPointZM;
                        default:
                            return ShapeGeometryType.MultiPoint;
                    }
                case OgcGeometryType.LineString:
                case OgcGeometryType.MultiLineString:
                    switch (((ILineString)geom.GetGeometryN(0)).CoordinateSequence.Ordinates)
                    {
                        case Ordinates.XYM:
                            return ShapeGeometryType.LineStringM;
                        case Ordinates.XYZ:
                        case Ordinates.XYZM:
                            return ShapeGeometryType.LineStringZM;
                        default:
                            return ShapeGeometryType.LineString;
                    }
                case OgcGeometryType.Polygon:
                case OgcGeometryType.MultiPolygon:
                    switch (((IPolygon)geom.GetGeometryN(0)).Shell.CoordinateSequence.Ordinates)
                    {
                        case Ordinates.XYM:
                            return ShapeGeometryType.PolygonM;
                        case Ordinates.XYZ:
                        case Ordinates.XYZM:
                            return ShapeGeometryType.PolygonZM;
                        default:
                            return ShapeGeometryType.Polygon;
                    }
                /*
                case OgcGeometryType.GeometryCollection:
                    if (geom.NumGeometries > 1)
                    {
                        for (var i = 0; i < geom.NumGeometries; i++)
                        {
                            var sgt = GetShapeType(geom.GetGeometryN(i));
                            if (sgt != ShapeGeometryType.NullShape)
                                return sgt;
                        }
                        return ShapeGeometryType.NullShape;
                    }
                    throw new NotSupportedException();
                 */
                default:
                    throw new NotSupportedException();
            }
            /*
            var pt = geom as IPoint;
            if (pt != null)
            {
                switch (pt.CoordinateSequence.Ordinates)
                {
                    case Ordinates.XYZ:
                        return ShapeGeometryType.PointZ;
                    case Ordinates.XYM:
                        return ShapeGeometryType.PointM;
                    case Ordinates.XYZM:
                        return ShapeGeometryType.PointZM;
                    default:
                        return ShapeGeometryType.Point;
                }
            }
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
             */
        }

        private static IGeometry GetNonEmptyGeometry(IGeometry geom)
        {
            if (geom == null || geom.IsEmpty)
                return null;

            for (var i = 0; i < geom.NumGeometries; i++)
            {
                var testGeom = geom.GetGeometryN(i);
                if (testGeom != null && !testGeom.IsEmpty)
                    return testGeom;
            }
            return null;
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
                case ShapeGeometryType.PointZM:
                    return new PointHandler(type);

                case ShapeGeometryType.Polygon:
                case ShapeGeometryType.PolygonM:
                case ShapeGeometryType.PolygonZM:
                    return new PolygonHandler(type);

                case ShapeGeometryType.LineString:
                case ShapeGeometryType.LineStringM:
                case ShapeGeometryType.LineStringZM:
                    return new MultiLineHandler(type);

                case ShapeGeometryType.MultiPoint:
                case ShapeGeometryType.MultiPointM:
                case ShapeGeometryType.MultiPointZM:
                    return new MultiPointHandler(type);

                case ShapeGeometryType.NullShape:
                    return new NullShapeHandler();

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