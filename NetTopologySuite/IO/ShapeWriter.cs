using System;
using System.IO;
using GeoAPI.Geometries;

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
        public void Write(ICoordinate coordinate, BinaryWriter writer)
        {
            writer.Write((double) coordinate.X);
            writer.Write((double) coordinate.Y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="writer"></param>
        public void Write(IPoint point, BinaryWriter writer)
        {
            writer.Write((int) ShapeGeometryType.Point);
            writer.Write((double) point.X);
            writer.Write((double) point.Y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineString"></param>
        /// <param name="writer"></param>
        public void Write(ILineString lineString, BinaryWriter writer)
        {
            writer.Write((int) ShapeGeometryType.LineString);

            // Write BoundingBox            
            WriteBoundingBox(lineString, writer);

            // Write NumParts and NumPoints
            writer.Write((int) 1);
            writer.Write((int) lineString.NumPoints);

            // Write IndexParts
            writer.Write((int) 0);

            // Write Coordinates
            for (int i = 0; i < lineString.NumPoints; i++)
                Write(lineString.Coordinates[i], writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="writer"></param>
        public void Write(IPolygon polygon, BinaryWriter writer)
        {
            writer.Write((int) ShapeGeometryType.Polygon);

            // Write BoundingBox            
            WriteBoundingBox(polygon, writer);

            // Write NumParts and NumPoints            
            writer.Write((int) (polygon.NumInteriorRings + 1));
            writer.Write((int)  polygon.NumPoints);

            // Write IndexParts
            int count = 0;
            writer.Write((int) count);
            if (polygon.NumInteriorRings != 0)
            {
                // Write external shell index
                count += polygon.ExteriorRing.NumPoints;
                writer.Write((int) count);
                for (int i = 1; i < polygon.NumInteriorRings; i++)
                {
                    // Write internal holes index
                    count += polygon.GetInteriorRingN(i - 1).NumPoints;
                    writer.Write((int) count);
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
        public void Write(IMultiPoint multiPoint, BinaryWriter writer)
        {
            writer.Write((int) ShapeGeometryType.MultiPoint);

            // Write BoundingBox            
            WriteBoundingBox(multiPoint, writer);

            // Write NumPoints            
            writer.Write((int) multiPoint.NumPoints);

            // Write Coordinates
            for (int i = 0; i < multiPoint.NumPoints; i++)
                Write(multiPoint.Coordinates[i], writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiLineString"></param>
        /// <param name="writer"></param>
        public void Write(IMultiLineString multiLineString, BinaryWriter writer)
        {
            writer.Write((int) ShapeGeometryType.LineString);

            // Write BoundingBox            
            WriteBoundingBox(multiLineString, writer);

            // Write NumParts and NumPoints
            writer.Write((int) multiLineString.NumGeometries);
            writer.Write((int) multiLineString.NumPoints);

            // Write IndexParts
            int count = 0;
            writer.Write((int) count);

            // Write linestrings index                                
            for (int i = 0; i < multiLineString.NumGeometries; i++)
            {
                // Write internal holes index
                count += multiLineString.GetGeometryN(i).NumPoints;
                if (count == multiLineString.NumPoints)
                    break;
                writer.Write((int) count);
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
        public void Write(IMultiPolygon multiPolygon, BinaryWriter writer)
        {
            writer.Write((int) ShapeGeometryType.Polygon);

            // Write BoundingBox            
            WriteBoundingBox(multiPolygon, writer);

            // Write NumParts and NumPoints
            int numParts = multiPolygon.NumGeometries;              // Exterior rings count
            for (int i = 0; i < multiPolygon.NumGeometries; i++)    // Adding interior rings count            
                numParts += ((IPolygon) multiPolygon.GetGeometryN(i)).NumInteriorRings;

            writer.Write((int) numParts);
            writer.Write((int) multiPolygon.NumPoints);

            // Write IndexParts
            int count = 0;
            writer.Write((int) count);

            for (int i = 0; i < multiPolygon.NumGeometries; i++)
            {
                IPolygon polygon = (IPolygon) multiPolygon.GetGeometryN(i);
                ILineString shell = polygon.ExteriorRing;
                count += shell.NumPoints;
                if (count == multiPolygon.NumPoints)
                    break;
                writer.Write((int) count);
                for (int j = 0; j < polygon.NumInteriorRings; j++)
                {
                    ILineString hole = (ILineString) polygon.GetInteriorRingN(j);
                    count += hole.NumPoints;
                    if (count == multiPolygon.NumPoints)
                        break;
                    writer.Write((int) count);
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
        public void WriteBoundingBox(IGeometry geometry, BinaryWriter writer)
        {
            IEnvelope boundingBox = geometry.EnvelopeInternal;
            writer.Write((double) boundingBox.MinX);
            writer.Write((double) boundingBox.MinY);
            writer.Write((double) boundingBox.MaxX);
            writer.Write((double) boundingBox.MaxY);
        }

        /// <summary>
        /// Sets correct length for Byte Stream.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public byte[] GetBytes(IGeometry geometry)
        {
            return new byte[GetBytesLength(geometry)];            
        }

        /// <summary>
        /// Return correct length for Byte Stream.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public int GetBytesLength(IGeometry geometry)
        {
            if (geometry is IPoint)
                return SetByteStreamLength(geometry as IPoint);
            else if (geometry is ILineString)
                return SetByteStreamLength(geometry as ILineString);
            else if (geometry is IPolygon)
                return SetByteStreamLength(geometry as IPolygon);
            else if (geometry is IMultiPoint)
                return SetByteStreamLength(geometry as IMultiPoint);
            else if (geometry is IMultiLineString)
                return SetByteStreamLength(geometry as IMultiLineString);
            else if (geometry is IMultiPolygon)
                return SetByteStreamLength(geometry as IMultiPolygon);
            else if (geometry is IGeometryCollection)
                throw new NotSupportedException("GeometryCollection not supported!");
            else throw new ArgumentException("ShouldNeverReachHere!");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiPolygon"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(IMultiPolygon multiPolygon)
        {            
            int numParts = multiPolygon.NumGeometries;               // Exterior rings count            
            foreach (IPolygon polygon in multiPolygon.Geometries)    // Adding interior rings count            
                numParts += polygon.NumInteriorRings;
            int numPoints = multiPolygon.NumPoints;
            return CalculateLength(numParts, numPoints);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiLineString"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(IMultiLineString multiLineString)
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
        protected int SetByteStreamLength(IMultiPoint multiPoint)
        {            
            int numPoints = multiPoint.NumPoints;
            return CalculateLength(numPoints);
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(IPolygon polygon)
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
        protected int SetByteStreamLength(ILineString lineString)
        {
            int numPoints = lineString.NumPoints;
            return CalculateLength(1, numPoints);   // ASSERT: IndexParts.Length == 1;            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(IPoint point)
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="numPoints"></param>
        /// <returns></returns>
        private static int CalculateLength(int numPoints)
        {
            int count = InitCount;
            count += 4;                         // NumPoints
            count += 8 * 2 * numPoints;
            return count;
        }
    }
}
