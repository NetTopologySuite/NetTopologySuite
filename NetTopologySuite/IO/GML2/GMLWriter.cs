using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.IO.GML2
{
    /// <summary>
    /// Writes the GML representation of the features of NetTopologySuite model.
    /// Uses GML 2.1.1 <c>Geometry.xsd</c> schema for base for features.
    /// </summary>
    public class GMLWriter
    {
        private const int InitValue = 100;
        private const int CoordSize = 100;   

        /// <summary>
        /// Formatter for double values of coordinates
        /// </summary>
        protected NumberFormatInfo NumberFormatter
        {
            get 
            {
                
                return Global.GetNfi();
            }            
        }

        /// <summary>
        /// Initialize a new <c>GMLWriter</c>.
        /// </summary>
        public GMLWriter() { }

        /// <summary>
        /// Returns an <c>XmlReader</c> with feature informations.
        /// Use <c>XmlDocument.Load(XmlReader)</c> for obtain a <c>XmlDocument</c> to work.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public XmlReader Write(Geometry geometry)
        {
            byte[] data = GetBytes(geometry);
            using (Stream stream = new MemoryStream(data))
                Write(geometry, stream);
            Stream outStream = new MemoryStream(data);
            return new XmlTextReader(outStream);           
        }

        /// <summary>
        /// Writes a GML feature into a generic <c>Stream</c>, such a <c>FileStream</c> or other streams.
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="stream"></param>
        public void Write(Geometry geometry, Stream stream)
        {
            XmlTextWriter writer = new XmlTextWriter(stream, null);
            writer.Formatting = Formatting.Indented;
            Write(geometry, writer);                                  
            writer.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="writer"></param>        
        protected void Write(Coordinate coordinate, XmlTextWriter writer)
        {
            writer.WriteStartElement("coord");
            writer.WriteElementString("X", coordinate.X.ToString("g", NumberFormatter));
            writer.WriteElementString("Y", coordinate.Y.ToString("g", NumberFormatter));
            writer.WriteEndElement();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="writer"></param>        
        protected void Write(Coordinate[] coordinates, XmlTextWriter writer)
        {
            writer.WriteRaw("<coordinates>");
            foreach (Coordinate coordinate in coordinates)
            {
                writer.WriteRaw(coordinate.X.ToString("g", NumberFormatter) + " ");
                writer.WriteRaw(coordinate.Y.ToString("g", NumberFormatter) + " ");
            }
            writer.WriteRaw("</coordinates>");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="writer"></param>
        protected void Write(Geometry geometry, XmlTextWriter writer)
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
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="writer"></param>
        protected void Write(Point point, XmlTextWriter writer)
        {            
            writer.WriteStartElement("Point");
            Write((Coordinate) point.Coordinate, writer);
            writer.WriteEndElement();            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineString"></param>
        /// <param name="writer"></param>
        protected void Write(LineString lineString, XmlTextWriter writer)
        {            
            writer.WriteStartElement("LineString");       
            Write((Coordinate[]) lineString.Coordinates, writer);    
            writer.WriteEndElement();            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="linearRing"></param>
        /// <param name="writer"></param>
        protected void Write(LinearRing linearRing, XmlTextWriter writer)
        {
            writer.WriteStartElement("LinearRing");
            Write((Coordinate[]) linearRing.Coordinates, writer);
            writer.WriteEndElement();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="writer"></param>
        protected void Write(Polygon polygon, XmlTextWriter writer)
        {
            writer.WriteStartElement("Polygon");
            writer.WriteStartElement("outerBoundaryIs");
            Write(polygon.ExteriorRing as LinearRing, writer);
            writer.WriteEndElement();
            for(int i = 0; i < polygon.NumInteriorRings; i++)
            {
                writer.WriteStartElement("innerBoundaryIs");
                Write(polygon.InteriorRings[i] as LinearRing, writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiPoint"></param>
        /// <param name="writer"></param>
        protected void Write(MultiPoint multiPoint, XmlTextWriter writer)
        {
            writer.WriteStartElement("MultiPoint");
            for (int i = 0; i < multiPoint.NumGeometries; i++)
            {
                writer.WriteStartElement("pointMember");
                Write(multiPoint.Geometries[i] as Point, writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiLineString"></param>
        /// <param name="writer"></param>
        protected void Write(MultiLineString multiLineString, XmlTextWriter writer)
        {
            writer.WriteStartElement("MultiLineString");
            for (int i = 0; i < multiLineString.NumGeometries; i++)
            {
                writer.WriteStartElement("lineStringMember");
                Write(multiLineString.Geometries[i] as LineString, writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiPolygon"></param>
        /// <param name="writer"></param>
        protected void Write(MultiPolygon multiPolygon, XmlTextWriter writer)
        {
            writer.WriteStartElement("MultiPolygon");
            for (int i = 0; i < multiPolygon.NumGeometries; i++)
            {
                writer.WriteStartElement("polygonMember");
                Write(multiPolygon.Geometries[i] as Polygon, writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        } 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometryCollection"></param>
        /// <param name="writer"></param>
        protected void Write(GeometryCollection geometryCollection, XmlTextWriter writer)
        {
            writer.WriteStartElement("MultiGeometry");
            for (int i = 0; i < geometryCollection.NumGeometries; i++)
            {
                writer.WriteStartElement("geometryMember");
                Write(geometryCollection.Geometries[i] as Geometry, writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }


        /// <summary>
        /// Sets corrent length for Byte Stream.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected byte[] GetBytes(Geometry geometry)
        {
            if (geometry is Point)
                return new byte[SetByteStreamLength(geometry as Point)];
            else if (geometry is LineString)
                return new byte[SetByteStreamLength(geometry as LineString)];
            else if (geometry is Polygon)
                return new byte[SetByteStreamLength(geometry as Polygon)];
            else if (geometry is MultiPoint)
                return new byte[SetByteStreamLength(geometry as MultiPoint)];
            else if (geometry is MultiLineString)
                return new byte[SetByteStreamLength(geometry as MultiLineString)];
            else if (geometry is MultiPolygon)
                return new byte[SetByteStreamLength(geometry as MultiPolygon)];
            else if (geometry is GeometryCollection)
                return new byte[SetByteStreamLength(geometry as GeometryCollection)];
            else throw new ArgumentException("ShouldNeverReachHere");
        }

        /// <summary>
        /// Sets corrent length for Byte Stream.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(Geometry geometry)
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
                return SetByteStreamLength(geometry as GeometryCollection);
            else throw new ArgumentException("ShouldNeverReachHere");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometryCollection"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(GeometryCollection geometryCollection)
        {
            int count = InitValue;
            foreach (Geometry g in geometryCollection.Geometries)
                count += SetByteStreamLength(g);
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiPolygon"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(MultiPolygon multiPolygon)
        {
            int count = InitValue;
            foreach (Polygon p in multiPolygon.Geometries)
                count += SetByteStreamLength(p);
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiLineString"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(MultiLineString multiLineString)
        {
            int count = InitValue;
            foreach (LineString ls in multiLineString.Geometries)
                count += SetByteStreamLength(ls);
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiPoint"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(MultiPoint multiPoint)
        {
            int count = InitValue;
            foreach (Point p in multiPoint.Geometries)
                count += SetByteStreamLength(p);
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(Polygon polygon)
        {
            int count = InitValue;            
            count += polygon.NumPoints * CoordSize;            
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineString"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(LineString lineString)
        {
            int count = InitValue;            
            count += lineString.NumPoints * CoordSize;
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(Point point)
        {
            return InitValue + CoordSize;    
        }
    }
}
