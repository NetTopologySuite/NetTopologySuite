using System;
using System.Collections;
using System.IO;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;

namespace GisSharpBlog.NetTopologySuite.IO
{
    /// <summary>
    /// Contains methods for writing a single <c>Geometry</c> in binary ESRI shapefile format.
    /// </summary>
    public class ShapeWriter
    {
        /// <summary>
        /// Standard byte size for each complex point.
        /// Each complex point (LineString, Polygon, ...) contains
        ///     4 bytes for ShapeTypes and
        ///     32 bytes for Boundingbox.      
        /// </summary>
        protected const int InitCount = 36;

        /// <summary> 
        /// Creates a <coordinate>ShapeWriter</coordinate> that creates objects using a basic GeometryFactory.
        /// </summary>
        public ShapeWriter() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="writer"></param>
        public void Write(Coordinate coordinate, BinaryWriter writer)
        {
            writer.Write((double)coordinate.X);
            writer.Write((double)coordinate.Y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="writer"></param>
        public void Write(Point point, BinaryWriter writer)
        {
            writer.Write((int)ShapeGeometryTypes.Point);
            writer.Write((double)point.X);
            writer.Write((double)point.Y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineString"></param>
        /// <param name="writer"></param>
        public void Write(LineString lineString, BinaryWriter writer)
        {
            writer.Write((int)ShapeGeometryTypes.LineString);

            // Write BoundingBox            
            WriteBoundingBox(lineString, writer);

            // Write NumParts and NumPoints
            writer.Write((int)1);
            writer.Write((int)lineString.NumPoints);

            // Write IndexParts
            writer.Write((int)0);

            // Write Coordinates
            for (int i = 0; i < lineString.NumPoints; i++)
                Write(lineString.Coordinates[i], writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="writer"></param>
        public void Write(Polygon polygon, BinaryWriter writer)
        {
            writer.Write((int)ShapeGeometryTypes.Polygon);

            // Write BoundingBox            
            WriteBoundingBox(polygon, writer);

            // Write NumParts and NumPoints            
            writer.Write((int)(polygon.NumInteriorRings + 1));
            writer.Write((int)polygon.NumPoints);

            // Write IndexParts
            int count = 0;
            writer.Write((int)count);
            if (polygon.NumInteriorRings != 0)
            {
                // Write external shell index
                count += polygon.ExteriorRing.NumPoints;
                writer.Write((int)count);
                for (int i = 1; i < polygon.NumInteriorRings; i++)
                {
                    // Write internal holes index
                    count += polygon.GetInteriorRingN(i - 1).NumPoints;
                    writer.Write((int)count);
                }
            }

            // Write Coordinates
            for (int i = 0; i < polygon.NumPoints; i++)
                Write(polygon.Coordinates[i], writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiPoint"></param>
        /// <param name="writer"></param>
        public void Write(MultiPoint multiPoint, BinaryWriter writer)
        {
            writer.Write((int)ShapeGeometryTypes.MultiPoint);

            // Write BoundingBox            
            WriteBoundingBox(multiPoint, writer);

            // Write NumPoints            
            writer.Write((int)multiPoint.NumPoints);

            // Write Coordinates
            for (int i = 0; i < multiPoint.NumPoints; i++)
                Write(multiPoint.Coordinates[i], writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiLineString"></param>
        /// <param name="writer"></param>
        public void Write(MultiLineString multiLineString, BinaryWriter writer)
        {
            writer.Write((int)ShapeGeometryTypes.LineString);

            // Write BoundingBox            
            WriteBoundingBox(multiLineString, writer);

            // Write NumParts and NumPoints
            writer.Write((int)multiLineString.NumGeometries);
            writer.Write((int)multiLineString.NumPoints);

            // Write IndexParts
            int count = 0;
            writer.Write((int)count);

            // Write linestrings index                                
            for (int i = 0; i < multiLineString.NumGeometries; i++)
            {
                // Write internal holes index
                count += multiLineString.GetGeometryN(i).NumPoints;
                if (count == multiLineString.NumPoints)
                    break;
                writer.Write((int)count);
            }

            // Write Coordinates
            for (int i = 0; i < multiLineString.NumPoints; i++)
                Write(multiLineString.Coordinates[i], writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiPolygon"></param>
        /// <param name="writer"></param>
        public void Write(MultiPolygon multiPolygon, BinaryWriter writer)
        {
            writer.Write((int)ShapeGeometryTypes.Polygon);

            // Write BoundingBox            
            WriteBoundingBox(multiPolygon, writer);

            // Write NumParts and NumPoints
            int numParts = multiPolygon.NumGeometries;              // Exterior rings count
            for (int i = 0; i < multiPolygon.NumGeometries; i++)    // Adding interior rings count            
                numParts += (multiPolygon.GetGeometryN(i) as Polygon).NumInteriorRings;

            writer.Write((int)numParts);
            writer.Write((int)multiPolygon.NumPoints);

            // Write IndexParts
            int count = 0;
            writer.Write((int)count);

            for (int i = 0; i < multiPolygon.NumGeometries; i++)
            {
                Polygon polygon = multiPolygon.GetGeometryN(i) as Polygon;
                LineString shell = polygon.ExteriorRing;
                count += shell.NumPoints;
                if (count == multiPolygon.NumPoints)
                    break;
                writer.Write((int)count);
                for (int j = 0; j < polygon.NumInteriorRings; j++)
                {
                    LineString hole = polygon.GetInteriorRingN(j);
                    count += hole.NumPoints;
                    if (count == multiPolygon.NumPoints)
                        break;
                    writer.Write((int)count);
                }
            }

            // Write Coordinates
            for (int i = 0; i < multiPolygon.NumPoints; i++)
                Write(multiPolygon.Coordinates[i], writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="writer"></param>
        public void WriteBoundingBox(Geometry geometry, BinaryWriter writer)
        {
            Envelope boundingBox = geometry.EnvelopeInternal;
            writer.Write((double)boundingBox.MinX);
            writer.Write((double)boundingBox.MinY);
            writer.Write((double)boundingBox.MaxX);
            writer.Write((double)boundingBox.MaxY);
        }

        /// <summary>
        /// Sets correct length for Byte Stream.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public byte[] GetBytes(Geometry geometry)
        {
            return new byte[GetBytesLength(geometry)];            
        }

        /// <summary>
        /// Return correct length for Byte Stream.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public int GetBytesLength(Geometry geometry)
        {
            if (geometry is Point)
                return SetByteStreamLength(geometry as Point);
            else if (geometry is LineString)
                return SetByteStreamLength(geometry as LineString);
            else if (geometry is Polygon)
                return SetByteStreamLength(geometry as Polygon);
            else if (geometry is MultiPoint)
                return SetByteStreamLength(geometry as MultiPoint);
            else if (geometry is MultiLineString)
                return SetByteStreamLength(geometry as MultiLineString);
            else if (geometry is MultiPolygon)
                return SetByteStreamLength(geometry as MultiPolygon);
            else if (geometry is GeometryCollection)
                throw new NotSupportedException("GeometryCollection not supported!");
            else throw new ArgumentException("ShouldNeverReachHere!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiPolygon"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(MultiPolygon multiPolygon)
        {            
            int numParts = multiPolygon.NumGeometries;              // Exterior rings count            
            foreach (Polygon polygon in multiPolygon.Geometries)    // Adding interior rings count            
                numParts += polygon.NumInteriorRings;
            int numPoints = multiPolygon.NumPoints;
            return CalculateLength(numParts, numPoints);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiLineString"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(MultiLineString multiLineString)
        {            
            int numParts = multiLineString.NumGeometries;
            int numPoints = multiLineString.NumPoints;
            return CalculateLength(numParts, numPoints);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiPoint"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(MultiPoint multiPoint)
        {            
            int numPoints = multiPoint.NumPoints;
            return CalculateLength(numPoints);
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(Polygon polygon)
        {
            int numParts = polygon.InteriorRings.Length + 1;
            int numPoints = polygon.NumPoints;
            return CalculateLength(numParts, numPoints);
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineString"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(LineString lineString)
        {
            int numPoints = lineString.NumPoints;
            return CalculateLength(1, numPoints);   // ASSERT: IndexParts.Length == 1;            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(Point point)
        {
            return 20;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numParts"></param>
        /// <param name="numPoints"></param>
        /// <returns></returns>
        private static int CalculateLength(int numParts, int numPoints)
        {
            int count = InitCount;
            count += 8;                         // NumParts and NumPoints
            count += 4 * numParts;
            count += 8 * 2 * numPoints;
            return count;
        }

        private static int CalculateLength(int numPoints)
        {
            int count = InitCount;
            count += 4;                         // NumPoints
            count += 8 * 2 * numPoints;
            return count;
        }
    }
}
