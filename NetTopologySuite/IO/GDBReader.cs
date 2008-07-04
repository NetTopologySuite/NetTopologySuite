using System;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.IO
{    
    /// <summary>
    /// Read features stored as ESRI GeoDatabase binary format in a SqlServer database,
    /// and converts this features to <coordinate>Geometry</coordinate> format.
    /// </summary>
    public class GDBReader : ShapeReader
    {                
        /// <summary> 
        /// Creates a <coordinate>GDBReader</coordinate> that creates objects using a basic GeometryFactory.
        /// </summary>
        public GDBReader() : base(new GeometryFactory()) { }

        /// <summary>  
        /// Creates a <coordinate>GDBReader</coordinate> that creates objects using the given
        /// <coordinate>GeometryFactory</coordinate>.
        /// </summary>
        /// <param name="factory">The factory used to create <coordinate>Geometry</coordinate>s.</param>
        public GDBReader(GeometryFactory factory) : base(factory) { }        

        /// <summary>
        /// Read VeDEx geometries.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public IGeometry Read(Stream data)
        {                                   
            using(BinaryReader reader = new BinaryReader(data))
                return Read(reader);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public IGeometry Read(BinaryReader reader)
        {
            ShapeGeometryType shapeType = (ShapeGeometryType)reader.ReadInt32();

            switch (shapeType)
            {
                case ShapeGeometryType.Point:
                case ShapeGeometryType.PointM:
                case ShapeGeometryType.PointZ:
                case ShapeGeometryType.PointZM:
                    return ReadPoint(reader);

                case ShapeGeometryType.LineString:
                case ShapeGeometryType.LineStringM:
                case ShapeGeometryType.LineStringZ:
                case ShapeGeometryType.LineStringZM:
                    return ReadLineString(reader);

                case ShapeGeometryType.Polygon:
                case ShapeGeometryType.PolygonM:
                case ShapeGeometryType.PolygonZ:
                case ShapeGeometryType.PolygonZM:
                    return ReadPolygon(reader);

                case ShapeGeometryType.MultiPoint:
                case ShapeGeometryType.MultiPointM:
                case ShapeGeometryType.MultiPointZ:
                case ShapeGeometryType.MultiPointZM:
                    return ReadMultiPoint(reader);

                case ShapeGeometryType.MultiPatch:
                    throw new NotImplementedException("FeatureType " + shapeType + " not supported.");

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
