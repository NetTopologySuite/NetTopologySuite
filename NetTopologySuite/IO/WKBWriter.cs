using System;
using System.Collections;
using System.IO;

using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.IO
{
    /// <summary>
    /// Writes a Well-Known Binary byte data representation of a <c>Geometry</c>.
    /// </summary>
    public class WKBWriter
    {
        private ByteOrder encodingType;

        /// <summary>
        /// Standard byte size for each complex point.
        /// Each complex point (LineString, Polygon, ...) contains:
        ///     1 byte for ByteOrder and
        ///     4 bytes for WKBType.      
        /// </summary>
        protected const int InitCount = 5;        

        /// <summary>
        /// Initializes writer with LittleIndian byte order.
        /// </summary>
        public WKBWriter() : this(ByteOrder.LittleIndian) { }

        /// <summary>
        /// Initializes writer with the specified byte order.
        /// </summary>
        /// <param name="encodingType">Encoding type</param>
        public WKBWriter(ByteOrder encodingType)
        {
            this.encodingType = encodingType;
        }

        /// <summary>
        /// Writes a WKB representation of a given point.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public byte[] Write(Geometry geometry)
        {
            byte[] bytes = GetBytes(geometry);
            Write(geometry, new MemoryStream(bytes));
            return bytes;
        }

        /// <summary>
        /// Writes a WKB representation of a given point.
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public void Write(Geometry geometry, Stream stream)
        {
            BinaryWriter writer = null;
            try
            {
                if (encodingType == ByteOrder.LittleIndian)
                     writer = new BinaryWriter(stream);
                else writer = new BEBinaryWriter(stream);
                Write(geometry, writer);
            }
            finally
            {
                if (writer != null)
                    writer.Close();
            }

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="writer"></param>
        protected void Write(Geometry geometry, BinaryWriter writer)
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
                Write(geometry as GeometryCollection, writer);
            else throw new ArgumentException("Geometry not recognized: " + geometry.ToString());
        }

        /// <summary>
        /// Writes LittleIndian ByteOrder.
        /// </summary>
        /// <param name="writer"></param>
        protected void WriteByteOrder(BinaryWriter writer)
        {
            writer.Write((byte)ByteOrder.LittleIndian);
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="writer"></param>
        protected void Write(Point point, BinaryWriter writer)
        {
            WriteByteOrder(writer);     // LittleIndian
            writer.Write((int) WKBGeometryTypes.WKBPoint);
            Write((Coordinate) point.Coordinate, writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineString"></param>
        /// <param name="writer"></param>
        protected void Write(LineString lineString, BinaryWriter writer)
        {
            WriteByteOrder(writer);     // LittleIndian
            writer.Write((int) WKBGeometryTypes.WKBLineString);
            writer.Write((int) lineString.NumPoints);
            for (int i = 0; i < lineString.Coordinates.Length; i++)
                Write((Coordinate) lineString.Coordinates[i], writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="writer"></param>
        protected void Write(Polygon polygon, BinaryWriter writer)
        {
            WriteByteOrder(writer);     // LittleIndian
            writer.Write((int) WKBGeometryTypes.WKBPolygon);
            writer.Write((int) polygon.NumInteriorRings + 1);
            Write(polygon.ExteriorRing as LinearRing, writer);
            for (int i = 0; i < polygon.NumInteriorRings; i++)
                Write(polygon.InteriorRings[i] as LinearRing, writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiPoint"></param>
        /// <param name="writer"></param>
        protected void Write(MultiPoint multiPoint, BinaryWriter writer)
        {
            WriteByteOrder(writer);     // LittleIndian
            writer.Write((int) WKBGeometryTypes.WKBMultiPoint);
            writer.Write((int) multiPoint.NumGeometries);
            for (int i = 0; i < multiPoint.NumGeometries; i++)
                Write(multiPoint.Geometries[i] as Point, writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiLineString"></param>
        /// <param name="writer"></param>
        protected void Write(MultiLineString multiLineString, BinaryWriter writer)
        {
            WriteByteOrder(writer);     // LittleIndian
            writer.Write((int) WKBGeometryTypes.WKBMultiLineString);
            writer.Write((int) multiLineString.NumGeometries);
            for (int i = 0; i < multiLineString.NumGeometries; i++)
                Write(multiLineString.Geometries[i] as LineString, writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiPolygon"></param>
        /// <param name="writer"></param>
        protected void Write(MultiPolygon multiPolygon, BinaryWriter writer)
        {
            WriteByteOrder(writer);     // LittleIndian
            writer.Write((int) WKBGeometryTypes.WKBMultiPolygon);
            writer.Write((int) multiPolygon.NumGeometries);
            for (int i = 0; i < multiPolygon.NumGeometries; i++)
                Write(multiPolygon.Geometries[i] as Polygon, writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geomCollection"></param>
        /// <param name="writer"></param>
        protected void Write(GeometryCollection geomCollection, BinaryWriter writer)
        {
            WriteByteOrder(writer);     // LittleIndian
            writer.Write((int) WKBGeometryTypes.WKBGeometryCollection);
            writer.Write((int) geomCollection.NumGeometries);
            for (int i = 0; i < geomCollection.NumGeometries; i++)
                Write((Geometry) geomCollection.Geometries[i], writer); ;                
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="writer"></param>
        protected void Write(Coordinate coordinate, BinaryWriter writer)
        {
            writer.Write((double) coordinate.X);
            writer.Write((double) coordinate.Y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ring"></param>
        /// <param name="writer"></param>
        protected void Write(LinearRing ring, BinaryWriter writer)
        {
            writer.Write((int) ring.NumPoints);
            for (int i = 0; i < ring.Coordinates.Length; i++)
                Write((Coordinate) ring.Coordinates[i], writer);
        }

        /// <summary>
        /// Sets corrent length for Byte Stream.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected byte[] GetBytes(Geometry geometry)
        {
            if (geometry is Point)
                return new byte[SetByteStream(geometry as Point)];
            else if (geometry is LineString)
                return new byte[SetByteStream(geometry as LineString)];
            else if (geometry is Polygon)
                return new byte[SetByteStream(geometry as Polygon)];
            else if (geometry is MultiPoint)
                return new byte[SetByteStream(geometry as MultiPoint)];
            else if (geometry is MultiLineString)
                return new byte[SetByteStream(geometry as MultiLineString)];
            else if (geometry is MultiPolygon)
                return new byte[SetByteStream(geometry as MultiPolygon)];
            else if (geometry is GeometryCollection)
                return new byte[SetByteStream(geometry as GeometryCollection)];
            else throw new ArgumentException("ShouldNeverReachHere");
        }

        /// <summary>
        /// Sets corrent length for Byte Stream.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(Geometry geometry)
        {
            if (geometry is Point)
                return SetByteStream(geometry as Point);
            else if (geometry is LineString)
                return SetByteStream(geometry as LineString);
            else if (geometry is Polygon)
                return SetByteStream(geometry as Polygon);
            else if (geometry is MultiPoint)
                return SetByteStream(geometry as MultiPoint);
            else if (geometry is MultiLineString)
                return SetByteStream(geometry as MultiLineString);
            else if (geometry is MultiPolygon)
                return SetByteStream(geometry as MultiPolygon);
            else if (geometry is GeometryCollection)
                return SetByteStream(geometry as GeometryCollection);
            else throw new ArgumentException("ShouldNeverReachHere");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(GeometryCollection geometry)
        {            
            int count = InitCount;
            count += 4;
            foreach (Geometry geom in geometry.Geometries)
                count += SetByteStream(geom);
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(MultiPolygon geometry)
        {            
            int count = InitCount;
            count += 4;
            foreach (Polygon geom in geometry.Geometries)
                count += SetByteStream(geom);
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(MultiLineString geometry)
        {                       
            int count = InitCount;
            count += 4;
            foreach (LineString geom in geometry.Geometries)
                count += SetByteStream(geom);
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(MultiPoint geometry)
        {                        
            int count = InitCount;
            count += 4;     // NumPoints
            foreach (Point geom in geometry.Geometries)
                count += SetByteStream(geom);            
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(Polygon geometry)
        {                
            int count = InitCount;
            count += 4 + 4;                                 // NumRings + NumPoints
            count += 4 * (geometry.NumInteriorRings + 1);   // Index parts
            count += geometry.NumPoints * 16;               // Points in exterior and interior rings
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(LineString geometry)
        {
            int numPoints = geometry.NumPoints;
            int count = InitCount;
            count += 4;                             // NumPoints
            count += 16 * numPoints;            
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(Point geometry)
        {
            return 21;
        }
    }
}
