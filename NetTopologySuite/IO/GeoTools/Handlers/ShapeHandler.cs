using System.Collections.Generic;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.IO.Handlers
{
    /// <summary>
    /// Abstract class that defines the interfaces that other 'Shape' handlers must implement.
    /// </summary>
    public abstract class ShapeHandler 
    {
        protected int bbindex = 0;
        protected double[] bbox;
        protected ShapeGeometryType type;
        protected IGeometry geom;

        /// <summary>
        /// Returns the ShapeType the handler handles.
        /// </summary>
        public abstract ShapeGeometryType ShapeType { get; }

        /// <summary>
        /// Reads a stream and converts the shapefile record to an equilivent geometry object.
        /// </summary>
        /// <param name="file">The stream to read.</param>
        /// <param name="geometryFactory">The geometry factory to use when making the object.</param>
        /// <returns>The Geometry object that represents the shape file record.</returns>
        public abstract IGeometry Read(BigEndianBinaryReader file, IGeometryFactory geometryFactory);

        /// <summary>
        /// Writes to the given stream the equilivent shape file record given a Geometry object.
        /// </summary>
        /// <param name="geometry">The geometry object to write.</param>
        /// <param name="file">The stream to write to.</param>
        /// <param name="geometryFactory">The geometry factory to use.</param>
        public abstract void Write(IGeometry geometry, BinaryWriter file,  IGeometryFactory geometryFactory);

        /// <summary>
        /// Gets the length in bytes the Geometry will need when written as a shape file record.
        /// </summary>
        /// <param name="geometry">The Geometry object to use.</param>
        /// <returns>The length in 16bit words the Geometry will use when represented as a shape file record.</returns>
        public abstract int GetLength(IGeometry geometry);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="envelope"></param>
        /// <returns></returns>
        public static IEnvelope GetEnvelopeExternal(IEnvelope envelope)
        {
            // Get envelope in external coordinates
            ICoordinate min = new Coordinate(envelope.MinX, envelope.MinY);
            ICoordinate max = new Coordinate(envelope.MaxX, envelope.MaxY);
            IEnvelope bounds = new Envelope(min.X, max.X, min.Y, max.Y);
            return bounds;
        }

        /// <summary>
        /// Get Envelope in external coordinates.
        /// </summary>
        /// <param name="precisionModel"></param>
        /// <param name="envelope"></param>
        /// <returns></returns>
        public static IEnvelope GetEnvelopeExternal(IPrecisionModel precisionModel, IEnvelope envelope)
        {
            return GetEnvelopeExternal(envelope);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected bool HasZValue()
        {
            return HasZValue(type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shapeType"></param>
        /// <returns></returns>
        public static bool HasZValue(ShapeGeometryType shapeType)
        {
            return  shapeType == ShapeGeometryType.PointZ ||
                    shapeType == ShapeGeometryType.PointZM ||
                    shapeType == ShapeGeometryType.LineStringZ ||
                    shapeType == ShapeGeometryType.LineStringZM ||
                    shapeType == ShapeGeometryType.PolygonZ ||
                    shapeType == ShapeGeometryType.PolygonZM;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected bool HasMValue()
        {
            return HasMValue(type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shapeType"></param>
        /// <returns></returns>
        public static bool HasMValue(ShapeGeometryType shapeType)
        {
            return  shapeType == ShapeGeometryType.PointM ||
                    shapeType == ShapeGeometryType.PointZM ||
                    shapeType == ShapeGeometryType.LineStringM ||
                    shapeType == ShapeGeometryType.LineStringZM ||
                    shapeType == ShapeGeometryType.PolygonM ||
                    shapeType == ShapeGeometryType.PolygonZM;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected bool IsPoint()
        {
            return IsPoint(type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shapeType"></param>
        /// <returns></returns>
        public static bool IsPoint(ShapeGeometryType shapeType)
        {
            return shapeType == ShapeGeometryType.Point ||
                   shapeType == ShapeGeometryType.PointZ ||
                   shapeType == ShapeGeometryType.PointM ||
                   shapeType == ShapeGeometryType.PointZM;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected bool IsMultiPoint()
        {
            return IsMultiPoint(type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shapeType"></param>
        /// <returns></returns>
        public static bool IsMultiPoint(ShapeGeometryType shapeType)
        {
            return shapeType == ShapeGeometryType.MultiPoint ||
                   shapeType == ShapeGeometryType.MultiPointZ ||
                   shapeType == ShapeGeometryType.MultiPointM ||
                   shapeType == ShapeGeometryType.MultiPointZM;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        protected bool IsLineString()
        {
            return IsLineString(type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shapeType"></param>
        /// <returns></returns>
        public static bool IsLineString(ShapeGeometryType shapeType)
        {
            return shapeType == ShapeGeometryType.LineString ||
                   shapeType == ShapeGeometryType.LineStringZ ||
                   shapeType == ShapeGeometryType.LineStringM ||
                   shapeType == ShapeGeometryType.LineStringZM;
        }

        /// <summary>
        /// 
        /// </summary>        
        /// <returns></returns>
        protected bool IsPolygon()
        {
            return IsPolygon(type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="shapeType"></param>
        /// <returns></returns>
        public static bool IsPolygon(ShapeGeometryType shapeType)
        {
            return shapeType == ShapeGeometryType.Polygon ||
                   shapeType == ShapeGeometryType.PolygonZ ||
                   shapeType == ShapeGeometryType.PolygonM ||
                   shapeType == ShapeGeometryType.PolygonZM;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="data"></param>
        protected void GetZValue(BigEndianBinaryReader file, IDictionary<ShapeGeometryType, double> data)
        {
            double z = file.ReadDouble();
            // data.Add(ShapeGeometryType.PointZ, z);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="data"></param>
        protected void GetMValue(BigEndianBinaryReader file, IDictionary<ShapeGeometryType, double> data)
        {            
            double m = file.ReadDouble();
            // data.Add(ShapeGeometryType.PointM, m);
        }

        protected void GrabZMValue(BigEndianBinaryReader file)
        {
            if (HasZValue() || HasMValue())
            {
                IDictionary<ShapeGeometryType, double> data = new Dictionary<ShapeGeometryType, double>(2);
                if (HasZValue())
                    GetZValue(file, data);
                if (HasMValue())
                    GetMValue(file, data);
                // geom.UserData = data;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        protected void GrabZMValues(BigEndianBinaryReader file)
        {
            if (HasZValue() || HasMValue())
            {
                IDictionary<ShapeGeometryType, double>[] datas =
                    new Dictionary<ShapeGeometryType, double>[geom.NumPoints];
                if (HasZValue())
                {
                    bbox[bbindex++] = file.ReadDouble();
                    bbox[bbindex++] = file.ReadDouble();
                    for (int i = 0; i < geom.NumPoints; i++)
                    {
                        if (datas[i] == null)
                            datas[i] = new Dictionary<ShapeGeometryType, double>(2);
                        GetZValue(file, datas[i]);
                    }
                }

                if (HasMValue())
                {
                    bbox[bbindex++] = file.ReadDouble();
                    bbox[bbindex++] = file.ReadDouble();
                    for (int i = 0; i < geom.NumPoints; i++)
                    {
                        if (datas[i] == null)
                            datas[i] = new Dictionary<ShapeGeometryType, double>(2);
                        GetMValue(file, datas[i]);
                    }
                }
                // geom.UserData = datas;
            }
        }

        /// <summary>
        /// 
        /// </summary>        
        /// <returns></returns>
        protected int GetBoundingBoxLength()
        {
            bbindex = 0;
            int bblength = 4;
            if (HasZValue())
                bblength += 2;
            if (HasMValue())
                bblength += 2;
            return bblength;
        }
    }
}