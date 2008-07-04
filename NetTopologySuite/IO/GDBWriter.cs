using System;
using System.IO;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.IO
{
    /// <summary>
    /// Writes features as ESRI GeoDatabase binary format in a SqlServer database,
    /// and converts this features to <coordinate>Geometry</coordinate> format.
    /// </summary>
    public class GDBWriter : ShapeWriter
    {
        /// <summary> 
        /// Creates a <coordinate>GDBWriter</coordinate> that creates objects using a basic GeometryFactory.
        /// </summary>
        public GDBWriter() : base() { }

        /// <summary>
        /// Returns a byte array containing binary data for the given <c>Geometry</c>.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns>Byte[] data</returns>
        public byte[] Write(IGeometry geometry)
        {
            byte[] bytes = GetBytes(geometry);
            Write(geometry, new MemoryStream(bytes));
            return bytes;
        }

        /// <summary>
        /// Writes a <c>Geometry</c> into a given <c>Stream</c>.
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="stream"></param>
        public void Write(IGeometry geometry, Stream stream)
        {           
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                Writer(geometry, writer);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="writer"></param>
        public void Writer(IGeometry geometry, BinaryWriter writer)
        {
            if (geometry is IPoint)
                Write(geometry as IPoint, writer);
            else if (geometry is ILineString)
                Write(geometry as ILineString, writer);
            else if (geometry is IPolygon)
                Write(geometry as IPolygon, writer);
            else if (geometry is IMultiPoint)
                Write(geometry as IMultiPoint, writer);
            else if (geometry is IMultiLineString)
                Write(geometry as IMultiLineString, writer);
            else if (geometry is IMultiPolygon)
                Write(geometry as IMultiPolygon, writer);
            else if (geometry is IGeometryCollection)
                throw new NotSupportedException("GeometryCollection not supported!");
            else throw new ArgumentException("Geometry not recognized: " + geometry.ToString());
        }
    }
}
