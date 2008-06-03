using System;
using System.Globalization;
using System.IO;
using System.Xml;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.IO.GML2
{
    /// <summary>
    /// Writes the GML representation of the features of NetTopologySuite model.
    /// Uses GML 2.1.1 <c>Geometry.xsd</c> schema for base for features.
    /// <remarks>
    /// Thanks to <see href="http//www.codeplex.com/Wiki/View.aspx?ProjectName=MsSqlSpatial">rstuven</see> for improvements :)
    /// </remarks>
    /// </summary>
    public class GMLWriter
    {
		private const int InitValue = 150;
		private const int CoordSize = 200;

        /// <summary>
        /// Formatter for double values of coordinates
        /// </summary>
        protected static NumberFormatInfo NumberFormatter
        {
            get { return Global.GetNfi(); }
        }

        /// <summary>
        /// Returns an <c>XmlReader</c> with feature informations.
        /// Use <c>XmlDocument.Load(XmlReader)</c> for obtain a <c>XmlDocument</c> to work.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public XmlReader Write(IGeometry geometry)
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
        public void Write(IGeometry geometry, Stream stream)
        {
            XmlTextWriter writer = new XmlTextWriter(stream, null);
            writer.Namespaces = true;
            writer.WriteStartElement(GMLElements.gmlPrefix, "GML", GMLElements.gmlNS);
            writer.Formatting = Formatting.Indented;
            Write(geometry, writer);
			writer.WriteEndElement();
            writer.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="writer"></param>        
        protected void Write(ICoordinate coordinate, XmlTextWriter writer)
        {
            writer.WriteStartElement(GMLElements.gmlPrefix, "coord", GMLElements.gmlNS);
            writer.WriteElementString(GMLElements.gmlPrefix, "X", GMLElements.gmlNS, coordinate.X.ToString("g", NumberFormatter));
            writer.WriteElementString(GMLElements.gmlPrefix, "Y", GMLElements.gmlNS, coordinate.Y.ToString("g", NumberFormatter));
            writer.WriteEndElement();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="writer"></param>        
        protected void Write(ICoordinate[] coordinates, XmlTextWriter writer)
        {
            foreach (ICoordinate coord in coordinates)
                Write(coord, writer);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="writer"></param>
        protected void Write(IGeometry geometry, XmlTextWriter writer)
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
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="writer"></param>
        protected void Write(IPoint point, XmlTextWriter writer)
        {
            writer.WriteStartElement("Point", GMLElements.gmlNS);
            Write(point.Coordinate, writer);
            writer.WriteEndElement();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineString"></param>
        /// <param name="writer"></param>
        protected void Write(ILineString lineString, XmlTextWriter writer)
        {
            writer.WriteStartElement("LineString", GMLElements.gmlNS);
            Write(lineString.Coordinates, writer);
            writer.WriteEndElement();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="linearRing"></param>
        /// <param name="writer"></param>
        protected void Write(ILinearRing linearRing, XmlTextWriter writer)
        {
            writer.WriteStartElement("LinearRing", GMLElements.gmlNS);
            Write(linearRing.Coordinates, writer);
            writer.WriteEndElement();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="writer"></param>
        protected void Write(IPolygon polygon, XmlTextWriter writer)
        {
            writer.WriteStartElement("Polygon", GMLElements.gmlNS);
            writer.WriteStartElement("outerBoundaryIs", GMLElements.gmlNS);
            Write(polygon.ExteriorRing as ILinearRing, writer);
            writer.WriteEndElement();
            for (int i = 0; i < polygon.NumInteriorRings; i++)
            {
                writer.WriteStartElement("innerBoundaryIs", GMLElements.gmlNS);
                Write(polygon.InteriorRings[i] as ILinearRing, writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiPoint"></param>
        /// <param name="writer"></param>
        protected void Write(IMultiPoint multiPoint, XmlTextWriter writer)
        {
            writer.WriteStartElement("MultiPoint", GMLElements.gmlNS);
            for (int i = 0; i < multiPoint.NumGeometries; i++)
            {
                writer.WriteStartElement("pointMember", GMLElements.gmlNS);
                Write(multiPoint.Geometries[i] as IPoint, writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiLineString"></param>
        /// <param name="writer"></param>
        protected void Write(IMultiLineString multiLineString, XmlTextWriter writer)
        {
            writer.WriteStartElement("MultiLineString", GMLElements.gmlNS);
            for (int i = 0; i < multiLineString.NumGeometries; i++)
            {
                writer.WriteStartElement("lineStringMember", GMLElements.gmlNS);
                Write(multiLineString.Geometries[i] as ILineString, writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiPolygon"></param>
        /// <param name="writer"></param>
        protected void Write(IMultiPolygon multiPolygon, XmlTextWriter writer)
        {
            writer.WriteStartElement("MultiPolygon", GMLElements.gmlNS);
            for (int i = 0; i < multiPolygon.NumGeometries; i++)
            {
                writer.WriteStartElement("polygonMember", GMLElements.gmlNS);
                Write(multiPolygon.Geometries[i] as IPolygon, writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometryCollection"></param>
        /// <param name="writer"></param>
        protected void Write(IGeometryCollection geometryCollection, XmlTextWriter writer)
        {
            writer.WriteStartElement("MultiGeometry", GMLElements.gmlNS);
            for (int i = 0; i < geometryCollection.NumGeometries; i++)
            {
                writer.WriteStartElement("geometryMember", GMLElements.gmlNS);
                Write(geometryCollection.Geometries[i] as IGeometry, writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }


        /// <summary>
        /// Sets corrent length for Byte Stream.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected byte[] GetBytes(IGeometry geometry)
        {
            if (geometry is IPoint)
                return new byte[SetByteStreamLength(geometry as IPoint)];
            else if (geometry is ILineString)
                return new byte[SetByteStreamLength(geometry as ILineString)];
            else if (geometry is IPolygon)
                return new byte[SetByteStreamLength(geometry as IPolygon)];
            else if (geometry is IMultiPoint)
                return new byte[SetByteStreamLength(geometry as IMultiPoint)];
            else if (geometry is IMultiLineString)
                return new byte[SetByteStreamLength(geometry as IMultiLineString)];
            else if (geometry is IMultiPolygon)
                return new byte[SetByteStreamLength(geometry as IMultiPolygon)];
            else if (geometry is IGeometryCollection)
                return new byte[SetByteStreamLength(geometry as IGeometryCollection)];
            else throw new ArgumentException("ShouldNeverReachHere");
        }

        /// <summary>
        /// Sets corrent length for Byte Stream.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(IGeometry geometry)
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
                return SetByteStreamLength(geometry as IGeometryCollection);
            else throw new ArgumentException("ShouldNeverReachHere");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geometryCollection"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(IGeometryCollection geometryCollection)
        {
            int count = InitValue;
            foreach (IGeometry g in geometryCollection.Geometries)
                count += SetByteStreamLength(g);
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiPolygon"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(IMultiPolygon multiPolygon)
        {
            int count = InitValue;
            foreach (IPolygon p in multiPolygon.Geometries)
                count += SetByteStreamLength(p);
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiLineString"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(IMultiLineString multiLineString)
        {
            int count = InitValue;
            foreach (ILineString ls in multiLineString.Geometries)
                count += SetByteStreamLength(ls);
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="multiPoint"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(IMultiPoint multiPoint)
        {
            int count = InitValue;
            foreach (IPoint p in multiPoint.Geometries)
                count += SetByteStreamLength(p);
            return count;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="polygon"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(IPolygon polygon)
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
        protected int SetByteStreamLength(ILineString lineString)
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
        protected int SetByteStreamLength(IPoint point)
        {
            return InitValue + CoordSize;
        }
    }
}
