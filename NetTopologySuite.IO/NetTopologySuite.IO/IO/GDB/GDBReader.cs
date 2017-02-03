using System;
using System.IO;
using GeoAPI.Geometries;

namespace NetTopologySuite.IO
{    
    /// <summary>
    /// Read features stored as ESRI GeoDatabase binary format in a SqlServer database,
    /// and converts these features to <see cref="IGeometry"/> format.
    /// </summary>
    public class GDBReader : ShapeReader //, IBinaryGeometryReader
    {                
        /// <summary> 
        /// Creates a <coordinate>GDBReader</coordinate> that creates objects using a basic GeometryFactory.
        /// </summary>
        public GDBReader() : base(GeoAPI.GeometryServiceProvider.Instance.CreateGeometryFactory()) { }

        /// <summary>  
        /// Creates a <coordinate>GDBReader</coordinate> that creates objects using the given
        /// <coordinate>GeometryFactory</coordinate>.
        /// </summary>
        /// <param name="factory">The factory used to create <coordinate>Geometry</coordinate>s.</param>
        public GDBReader(IGeometryFactory factory) : base(factory) { }        

        /// <summary>
        /// Read VeDEx geometries.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IGeometry Read(Stream data)
        {                                   
            using(var reader = new BinaryReader(data))
                return Read(reader);
        }

        private static Ordinates GetOrdinatesFromShapeGeometryType(ShapeGeometryType sgt)
        {
            switch (sgt)
            {
                case ShapeGeometryType.Point:
                case ShapeGeometryType.MultiPoint:
                case ShapeGeometryType.LineString:
                case ShapeGeometryType.Polygon:
                    return Ordinates.XY;

                case ShapeGeometryType.PointZ:
                case ShapeGeometryType.MultiPointZ:
                case ShapeGeometryType.LineStringZ:
                case ShapeGeometryType.PolygonZ:
                    return Ordinates.XYZ;

                case ShapeGeometryType.PointM:
                case ShapeGeometryType.MultiPointM:
                case ShapeGeometryType.LineStringM:
                case ShapeGeometryType.PolygonM:
                    return Ordinates.XYM;

                case ShapeGeometryType.PointZM:
                case ShapeGeometryType.MultiPointZM:
                case ShapeGeometryType.LineStringZM:
                case ShapeGeometryType.PolygonZM:
                    return Ordinates.XYZM;

                case ShapeGeometryType.MultiPatch:
                    throw new NotSupportedException("FeatureType " + sgt + " not supported.");

                default:
                    throw new ArgumentOutOfRangeException("FeatureType " + sgt + " not recognized by the system");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public IGeometry Read(BinaryReader reader)
        {
            var shapeType = (ShapeGeometryType)reader.ReadInt32();
            var ordinates = GetOrdinatesFromShapeGeometryType(shapeType);

            switch (shapeType)
            {
                case ShapeGeometryType.Point:
                case ShapeGeometryType.PointM:
                case ShapeGeometryType.PointZ:
                case ShapeGeometryType.PointZM:
                    return ReadPoint(reader, ordinates);

                case ShapeGeometryType.LineString:
                case ShapeGeometryType.LineStringM:
                case ShapeGeometryType.LineStringZ:
                case ShapeGeometryType.LineStringZM:
                    return ReadLineString(reader, ordinates);

                case ShapeGeometryType.Polygon:
                case ShapeGeometryType.PolygonM:
                case ShapeGeometryType.PolygonZ:
                case ShapeGeometryType.PolygonZM:
                    return ReadPolygon(reader, ordinates);

                case ShapeGeometryType.MultiPoint:
                case ShapeGeometryType.MultiPointM:
                case ShapeGeometryType.MultiPointZ:
                case ShapeGeometryType.MultiPointZM:
                    return ReadMultiPoint(reader, ordinates);

                case ShapeGeometryType.MultiPatch:
                    throw new NotSupportedException("FeatureType " + shapeType + " not supported.");

                default:
                    throw new ArgumentOutOfRangeException("FeatureType " + shapeType + " not recognized by the system");
            }
        }

        /// <summary>
        /// Read VeDEx geometries.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IGeometry Read(byte[] data)
        {
            using(Stream stream = new MemoryStream(data))
                return Read(stream);            
        }
    }
}
