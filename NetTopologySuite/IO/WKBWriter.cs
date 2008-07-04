using System;
using System.IO;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.IO
{
    /// <summary>
    /// Writes a Well-Known Binary byte data representation of a <c>Geometry</c>.
    /// </summary>
    /// <remarks>
    /// WKBWriter stores <see cref="ICoordinate" /> X,Y,Z values if <see cref="ICoordinate.Z" /> is not <see cref="double.NaN"/>, 
    /// otherwise <see cref="ICoordinate.Z" /> value is discarded and only X,Y are stored.
    /// </remarks>
    // Thanks to Roberto Acioli for ICoordinate.Z patch
    public class WKBWriter
    {
        protected ByteOrder encodingType;

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
        public WKBWriter() : 
            this(ByteOrder.LittleEndian) { }

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
        public virtual byte[] Write(IGeometry geometry)
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
        public virtual void Write(IGeometry geometry, Stream stream)
        {
            BinaryWriter writer = null;
            try
            {
                if (encodingType == ByteOrder.LittleEndian)
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
        protected void Write(IGeometry geometry, BinaryWriter writer)
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
                Write(geometry as IGeometryCollection, writer);
            else throw new ArgumentException("Geometry not recognized: " + geometry.ToString());
        }

        /// <summary>
        /// Writes LittleIndian ByteOrder.
        /// </summary>
        /// <param name="writer"></param>
        protected void WriteByteOrder(BinaryWriter writer)
        {
            writer.Write((byte) ByteOrder.LittleEndian);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="writer"></param>
        protected void Write(ICoordinate coordinate, BinaryWriter writer)
        {
            writer.Write((double) coordinate.X);
            writer.Write((double) coordinate.Y);
            if (!Double.IsNaN(coordinate.Z))
                writer.Write((double)coordinate.Z);            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="writer"></param>
        protected void Write(IPoint point, BinaryWriter writer)
        {
            WriteByteOrder(writer);     // LittleIndian
            if (Double.IsNaN(point.Coordinate.Z))
                 writer.Write((int)WKBGeometryTypes.WKBPoint);
            else writer.Write((int)WKBGeometryTypes.WKBPointZ);
            Write(point.Coordinate, writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineString"></param>
        /// <param name="writer"></param>
        protected void Write(ILineString lineString, BinaryWriter writer)
        {
            WriteByteOrder(writer);     // LittleIndian
            if (Double.IsNaN(lineString.Coordinate.Z))
                 writer.Write((int)WKBGeometryTypes.WKBLineString);
            else writer.Write((int)WKBGeometryTypes.WKBLineStringZ);
            writer.Write((int) lineString.NumPoints);
            for (int i = 0; i < lineString.Coordinates.Length; i++)
                Write(lineString.Coordinates[i], writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ring"></param>
        /// <param name="writer"></param>
        protected void Write(ILinearRing ring, BinaryWriter writer)
        {
            writer.Write((int) ring.NumPoints);
            for (int i = 0; i < ring.Coordinates.Length; i++)
                Write(ring.Coordinates[i], writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="writer"></param>
        protected void Write(IPolygon polygon, BinaryWriter writer)
        {
            WriteByteOrder(writer);     // LittleIndian
            if (Double.IsNaN(polygon.Coordinate.Z))
                 writer.Write((int)WKBGeometryTypes.WKBPolygon);
            else writer.Write((int)WKBGeometryTypes.WKBPolygonZ);            
            writer.Write((int) polygon.NumInteriorRings + 1);
            Write(polygon.ExteriorRing as ILinearRing, writer);
            for (int i = 0; i < polygon.NumInteriorRings; i++)
                Write(polygon.InteriorRings[i] as ILinearRing, writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiPoint"></param>
        /// <param name="writer"></param>
        protected void Write(IMultiPoint multiPoint, BinaryWriter writer)
        {
            WriteByteOrder(writer);     // LittleIndian
            if (Double.IsNaN(multiPoint.Coordinate.Z))
                 writer.Write((int)WKBGeometryTypes.WKBMultiPoint);
            else writer.Write((int)WKBGeometryTypes.WKBMultiPointZ);
            writer.Write((int) multiPoint.NumGeometries);
            for (int i = 0; i < multiPoint.NumGeometries; i++)
                Write(multiPoint.Geometries[i] as IPoint, writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiLineString"></param>
        /// <param name="writer"></param>
        protected void Write(IMultiLineString multiLineString, BinaryWriter writer)
        {
            WriteByteOrder(writer);     // LittleIndian
            if (Double.IsNaN(multiLineString.Coordinate.Z))
                 writer.Write((int)WKBGeometryTypes.WKBMultiLineString);
            else writer.Write((int)WKBGeometryTypes.WKBMultiLineStringZ);
            writer.Write((int) multiLineString.NumGeometries);
            for (int i = 0; i < multiLineString.NumGeometries; i++)
                Write(multiLineString.Geometries[i] as ILineString, writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiPolygon"></param>
        /// <param name="writer"></param>
        protected void Write(IMultiPolygon multiPolygon, BinaryWriter writer)
        {
            WriteByteOrder(writer);     // LittleIndian
            if (Double.IsNaN(multiPolygon.Coordinate.Z))
                 writer.Write((int)WKBGeometryTypes.WKBMultiPolygon);
            else writer.Write((int)WKBGeometryTypes.WKBMultiPolygonZ);
            writer.Write((int) multiPolygon.NumGeometries);
            for (int i = 0; i < multiPolygon.NumGeometries; i++)
                Write(multiPolygon.Geometries[i] as IPolygon, writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geomCollection"></param>
        /// <param name="writer"></param>
        protected void Write(IGeometryCollection geomCollection, BinaryWriter writer)
        {
            WriteByteOrder(writer);     // LittleIndian
            if (Double.IsNaN(geomCollection.Coordinate.Z))
                 writer.Write((int)WKBGeometryTypes.WKBGeometryCollection);
            else writer.Write((int)WKBGeometryTypes.WKBGeometryCollectionZ);
            writer.Write((int)geomCollection.NumGeometries);
            for (int i = 0; i < geomCollection.NumGeometries; i++)
                Write(geomCollection.Geometries[i], writer); ;
        }

        /// <summary>
        /// Sets corrent length for Byte Stream.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected byte[] GetBytes(IGeometry geometry)
        {
            if (geometry is IPoint)
                return new byte[SetByteStream(geometry as IPoint)];
            else if (geometry is ILineString)
                return new byte[SetByteStream(geometry as ILineString)];
            else if (geometry is IPolygon)
                return new byte[SetByteStream(geometry as IPolygon)];
            else if (geometry is IMultiPoint)
                return new byte[SetByteStream(geometry as IMultiPoint)];
            else if (geometry is IMultiLineString)
                return new byte[SetByteStream(geometry as IMultiLineString)];
            else if (geometry is IMultiPolygon)
                return new byte[SetByteStream(geometry as IMultiPolygon)];
            else if (geometry is IGeometryCollection)
                return new byte[SetByteStream(geometry as IGeometryCollection)];
            else throw new ArgumentException("ShouldNeverReachHere");
        }

        /// <summary>
        /// Sets corrent length for Byte Stream.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected virtual int SetByteStream(IGeometry geometry)
        {
            if (geometry is IPoint)
                return SetByteStream(geometry as IPoint);
            else if (geometry is ILineString)
                return SetByteStream(geometry as ILineString);
            else if (geometry is IPolygon)
                return SetByteStream(geometry as IPolygon);
            else if (geometry is IMultiPoint)
                return SetByteStream(geometry as IMultiPoint);
            else if (geometry is IMultiLineString)
                return SetByteStream(geometry as IMultiLineString);
            else if (geometry is IMultiPolygon)
                return SetByteStream(geometry as IMultiPolygon);
            else if (geometry is IGeometryCollection)
                return SetByteStream(geometry as IGeometryCollection);
            else throw new ArgumentException("ShouldNeverReachHere");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(IGeometryCollection geometry)
        {            
            int count = InitCount;
            count += 4;
            foreach (IGeometry geom in geometry.Geometries)
                count += SetByteStream(geom);
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(IMultiPolygon geometry)
        {            
            int count = InitCount;
            count += 4;
            foreach (IPolygon geom in geometry.Geometries)
                count += SetByteStream(geom);
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(IMultiLineString geometry)
        {                       
            int count = InitCount;
            count += 4;
            foreach (ILineString geom in geometry.Geometries)
                count += SetByteStream(geom);
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(IMultiPoint geometry)
        {                        
            int count = InitCount;
            count += 4;     // NumPoints
            foreach (IPoint geom in geometry.Geometries)
                count += SetByteStream(geom);            
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(IPolygon geometry)
        {
            int pointSize = Double.IsNaN(geometry.Coordinate.Z) ? 16 : 24;
            int count = InitCount;
            count += 4 + 4;                                 // NumRings + NumPoints
            count += 4 * (geometry.NumInteriorRings + 1);   // Index parts
            count += geometry.NumPoints * pointSize;        // Points in exterior and interior rings
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(ILineString geometry)
        {
            int pointSize = Double.IsNaN(geometry.Coordinate.Z) ? 16 : 24;
            int numPoints = geometry.NumPoints;
            int count = InitCount;
            count += 4;                             // NumPoints
            count += pointSize * numPoints;            
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStream(IPoint geometry)
        {
            return Double.IsNaN(geometry.Coordinate.Z) ? 21 : 29;
        }
    }
}
