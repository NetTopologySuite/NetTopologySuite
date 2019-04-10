using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.IO.GML2
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
        protected static NumberFormatInfo NumberFormatter => Global.GetNfi();

        /// <summary>
        /// Returns an <c>XmlReader</c> with feature informations.
        /// Use <c>XmlDocument.Load(XmlReader)</c> for obtain a <c>XmlDocument</c> to work.
        /// </summary>
        /// <param name="geometry"></param>
        /// <returns></returns>
        public XmlReader Write(Geometry geometry)
        {
            byte[] data;
            using (var stream = new MemoryStream(SetByteStreamLength(geometry)))
            {
                Write(geometry, stream);
                data = stream.ToArray();
            }

            var outStream = new MemoryStream(data);
            return XmlReader.Create(outStream);
        }

        /// <summary>
        /// Writes a GML feature into a generic <c>Stream</c>, such a <c>FileStream</c> or other streams.
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="stream"></param>
        public void Write(Geometry geometry, Stream stream)
        {
            var settings = new XmlWriterSettings()
            {
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
                Indent = true,
                OmitXmlDeclaration = true,
            };
            var writer = XmlWriter.Create(stream, settings);

            //writer.WriteStartElement(GMLElements.gmlPrefix, "GML", GMLElements.gmlNS);

            Write(geometry, writer);
            //writer.WriteEndElement();
            ((IDisposable)writer).Dispose();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="coordinate"></param>
        /// <param name="writer"></param>
        protected void Write(Coordinate coordinate, XmlWriter writer)
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
        protected void Write(Coordinate[] coordinates, XmlWriter writer)
        {
            foreach (var coord in coordinates)
                Write(coord, writer);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="writer"></param>
        protected void WriteCoordinates(Coordinate[] coordinates, XmlWriter writer)
        {
            writer.WriteStartElement(GMLElements.gmlPrefix, "coordinates", GMLElements.gmlNS);
            var elements = new List<string>(coordinates.Length);
            foreach (var coordinate in coordinates)
                elements.Add(string.Format(NumberFormatter, "{0},{1}", coordinate.X, coordinate.Y));

            writer.WriteString(string.Join(" ", elements.ToArray()));
            writer.WriteEndElement();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="writer"></param>
        public void Write(Geometry geometry, XmlWriter writer)
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
            else throw new ArgumentException("Geometry not recognized: " + geometry);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="point"></param>
        /// <param name="writer"></param>
        protected void Write(Point point, XmlWriter writer)
        {
            writer.WriteStartElement(GMLElements.gmlPrefix, "Point", GMLElements.gmlNS);
            Write(point.Coordinate, writer);
            writer.WriteEndElement();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="lineString"></param>
        /// <param name="writer"></param>
        protected void Write(LineString lineString, XmlWriter writer)
        {
            writer.WriteStartElement(GMLElements.gmlPrefix, "LineString", GMLElements.gmlNS);
            WriteCoordinates(lineString.Coordinates, writer);
            writer.WriteEndElement();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="linearRing"></param>
        /// <param name="writer"></param>
        protected void Write(LinearRing linearRing, XmlWriter writer)
        {
            writer.WriteStartElement(GMLElements.gmlPrefix, "LinearRing", GMLElements.gmlNS);
            WriteCoordinates(linearRing.Coordinates, writer);
            writer.WriteEndElement();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="writer"></param>
        protected void Write(Polygon polygon, XmlWriter writer)
        {
            writer.WriteStartElement(GMLElements.gmlPrefix, "Polygon", GMLElements.gmlNS);
            writer.WriteStartElement("outerBoundaryIs", GMLElements.gmlNS);
            Write(polygon.ExteriorRing as LinearRing, writer);
            writer.WriteEndElement();
            for (int i = 0; i < polygon.NumInteriorRings; i++)
            {
                writer.WriteStartElement("innerBoundaryIs", GMLElements.gmlNS);
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
        protected void Write(MultiPoint multiPoint, XmlWriter writer)
        {
            writer.WriteStartElement(GMLElements.gmlPrefix, "MultiPoint", GMLElements.gmlNS);
            for (int i = 0; i < multiPoint.NumGeometries; i++)
            {
                writer.WriteStartElement("pointMember", GMLElements.gmlNS);
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
        protected void Write(MultiLineString multiLineString, XmlWriter writer)
        {
            writer.WriteStartElement(GMLElements.gmlPrefix, "MultiLineString", GMLElements.gmlNS);
            for (int i = 0; i < multiLineString.NumGeometries; i++)
            {
                writer.WriteStartElement("lineStringMember", GMLElements.gmlNS);
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
        protected void Write(MultiPolygon multiPolygon, XmlWriter writer)
        {
            writer.WriteStartElement(GMLElements.gmlPrefix, "MultiPolygon", GMLElements.gmlNS);
            for (int i = 0; i < multiPolygon.NumGeometries; i++)
            {
                writer.WriteStartElement("polygonMember", GMLElements.gmlNS);
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
        protected void Write(GeometryCollection geometryCollection, XmlWriter writer)
        {
            writer.WriteStartElement(GMLElements.gmlPrefix, "MultiGeometry", GMLElements.gmlNS);
            for (int i = 0; i < geometryCollection.NumGeometries; i++)
            {
                writer.WriteStartElement("geometryMember", GMLElements.gmlNS);
                Write(geometryCollection.Geometries[i], writer);
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
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
            if (geometry is LineString)
                return SetByteStreamLength(geometry as LineString);
            if (geometry is Polygon)
                return SetByteStreamLength(geometry as Polygon);
            if (geometry is MultiPoint)
                return SetByteStreamLength(geometry as MultiPoint);
            if (geometry is MultiLineString)
                return SetByteStreamLength(geometry as MultiLineString);
            if (geometry is MultiPolygon)
                return SetByteStreamLength(geometry as MultiPolygon);
            if (geometry is GeometryCollection)
                return SetByteStreamLength(geometry as GeometryCollection);
            throw new ArgumentException("ShouldNeverReachHere");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geometryCollection"></param>
        /// <returns></returns>
        protected int SetByteStreamLength(GeometryCollection geometryCollection)
        {
            int count = InitValue;
            foreach (var g in geometryCollection.Geometries)
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
