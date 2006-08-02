using System;
using System.Data.SqlTypes;
using System.Globalization;
using System.IO;
using System.Text;

using GisSharpBlog.NetTopologySuite.Geometries;

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
        public virtual byte[] Write(Geometry geometry)
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
        public virtual void Write(Geometry geometry, Stream stream)
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
        public virtual void Writer(Geometry geometry, BinaryWriter writer)
        {
            if (geometry is Point)
                Write(geometry as Point, writer);
            else if (geometry is LineString)
                Write(geometry as LineString, writer);
            else if (geometry is Polygon)
                Write(geometry as Polygon, writer);
            else if (geometry is MultiPoint)
                Write(geometry as MultiPoint, writer);
            else if (geometry is MultiLineString)
                Write(geometry as MultiLineString, writer);
            else if (geometry is MultiPolygon)
                Write(geometry as MultiPolygon, writer);
            else if (geometry is GeometryCollection)
                throw new NotSupportedException("GeometryCollection not supported!");
            else throw new ArgumentException("Geometry not recognized: " + geometry.ToString());
        }
    }
}
