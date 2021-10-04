using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
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
        private readonly GMLVersion _gmlVersion;
        private readonly string _gmlNs;

        /// <summary>
        /// Formatter for double values of coordinates
        /// </summary>
        protected static NumberFormatInfo NumberFormatter => Global.GetNfi();

        /// <summary>
        /// Initializes a new instance of the <see cref="GMLWriter"/> class.
        /// </summary>
        public GMLWriter()
            : this(GMLVersion.Two) // backwards compatibility.
        {
        }

        internal GMLWriter(GMLVersion gmlVersion)
        {
            _gmlVersion = gmlVersion;
            switch (gmlVersion)
            {
                case GMLVersion.Two:
                    _gmlNs = $"{GMLElements.gmlNS}";
                    break;
                case GMLVersion.Three:
                    _gmlNs = $"{GMLElements.gmlNS}/3.2";
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(gmlVersion), gmlVersion, "Only version 2 and 3");
            }
        }

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
            var settings = new XmlWriterSettings
            {
                NamespaceHandling = NamespaceHandling.OmitDuplicates,
                Indent = true,
                OmitXmlDeclaration = true,
            };
            var writer = XmlWriter.Create(stream, settings);

            //writer.WriteStartElement(GMLElements.gmlPrefix, "GML", _gmlNs);

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
            if (_gmlVersion == GMLVersion.Two)
            {
                writer.WriteStartElement(GMLElements.gmlPrefix, "coord", _gmlNs);
                writer.WriteElementString(GMLElements.gmlPrefix, "X", _gmlNs, coordinate.X.ToString("g", NumberFormatter));
                writer.WriteElementString(GMLElements.gmlPrefix, "Y", _gmlNs, coordinate.Y.ToString("g", NumberFormatter));
                writer.WriteEndElement();
            }
            else
            {
                writer.WriteStartElement(GMLElements.gmlPrefix, "pos", _gmlNs);
                writer.WriteValue(string.Format(NumberFormatter, "{0} {1}", coordinate.X, coordinate.Y));
                writer.WriteEndElement();
            }
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
        [Obsolete("Use the overload that accepts a CoordinateSequence instead.")]
        protected void WriteCoordinates(Coordinate[] coordinates, XmlWriter writer)
        {
            WriteCoordinates(new CoordinateArraySequence(coordinates), writer);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="coordinates"></param>
        /// <param name="writer"></param>
        protected void WriteCoordinates(CoordinateSequence coordinates, XmlWriter writer)
        {
            writer.WriteStartElement(GMLElements.gmlPrefix, _gmlVersion == GMLVersion.Two ? "coordinates" : "posList", _gmlNs);
            var sb = new StringBuilder();
            string coordsFormatter = _gmlVersion == GMLVersion.Two ? "{0},{1} " : "{0} {1} ";
            for (int i = 0, cnt = coordinates.Count; i < cnt; i++)
            {
                sb.AppendFormat(NumberFormatter, coordsFormatter, coordinates.GetX(i), coordinates.GetY(i));
            }

            // remove the trailing space.
            if (sb.Length > 0)
            {
                --sb.Length;
            }

            writer.WriteString($"{sb}");
            writer.WriteEndElement();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="geometry"></param>
        /// <param name="writer"></param>
        public void Write(Geometry geometry, XmlWriter writer)
        {
            switch (geometry)
            {
                case Point point:
                    Write(point, writer);
                    break;

                case LineString lineString:
                    Write(lineString, writer);
                    break;

                case Polygon polygon:
                    Write(polygon, writer);
                    break;

                case MultiPoint multiPoint:
                    Write(multiPoint, writer);
                    break;

                case MultiLineString multiLineString:
                    Write(multiLineString, writer);
                    break;

                case MultiPolygon multiPolygon:
                    Write(multiPolygon, writer);
                    break;

                case GeometryCollection collection:
                    Write(collection, writer);
                    break;

                default:
                    throw new ArgumentException("Geometry not recognized: " + geometry);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="point"></param>
        /// <param name="writer"></param>
        protected void Write(Point point, XmlWriter writer)
        {
            writer.WriteStartElement(GMLElements.gmlPrefix, "Point", _gmlNs);
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
            writer.WriteStartElement(GMLElements.gmlPrefix, "LineString", _gmlNs);
            WriteCoordinates(lineString.CoordinateSequence, writer);
            writer.WriteEndElement();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="linearRing"></param>
        /// <param name="writer"></param>
        protected void Write(LinearRing linearRing, XmlWriter writer)
        {
            writer.WriteStartElement(GMLElements.gmlPrefix, "LinearRing", _gmlNs);
            WriteCoordinates(linearRing.CoordinateSequence, writer);
            writer.WriteEndElement();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="polygon"></param>
        /// <param name="writer"></param>
        protected void Write(Polygon polygon, XmlWriter writer)
        {
            writer.WriteStartElement(GMLElements.gmlPrefix, "Polygon", _gmlNs);
            writer.WriteStartElement(_gmlVersion == GMLVersion.Two ? "outerBoundaryIs" : "exterior", _gmlNs);
            Write(polygon.ExteriorRing as LinearRing, writer);
            writer.WriteEndElement();
            for (int i = 0; i < polygon.NumInteriorRings; i++)
            {
                writer.WriteStartElement(_gmlVersion == GMLVersion.Two ? "innerBoundaryIs" : "interior", _gmlNs);
                Write(polygon.GetInteriorRingN(i) as LinearRing, writer);
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
            writer.WriteStartElement(GMLElements.gmlPrefix, "MultiPoint", _gmlNs);
            if (_gmlVersion == GMLVersion.Two)
            {
                // Required in version 2
                writer.WriteAttributeString("srsName", GetEpsgCode(multiPoint.Factory.SRID));
            }
            for (int i = 0; i < multiPoint.NumGeometries; i++)
            {
                writer.WriteStartElement("pointMember", _gmlNs);
                Write(multiPoint.Geometries[i] as Point, writer);
                writer.WriteEndElement();
            }

            if (multiPoint.NumGeometries == 0 && _gmlVersion == GMLVersion.Two)
            {
                writer.WriteStartElement("pointMember", _gmlNs);
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
            writer.WriteStartElement(GMLElements.gmlPrefix, _gmlVersion == GMLVersion.Two ? "MultiLineString" : "MultiCurve", _gmlNs);
            if (_gmlVersion == GMLVersion.Two)
            {
                // Required in version 2
                writer.WriteAttributeString("srsName", GetEpsgCode(multiLineString.Factory.SRID));
            }
            for (int i = 0; i < multiLineString.NumGeometries; i++)
            {
                writer.WriteStartElement(_gmlVersion == GMLVersion.Two ? "lineStringMember" : "curveMember", _gmlNs);
                Write(multiLineString.Geometries[i] as LineString, writer);
                writer.WriteEndElement();
            }

            if (multiLineString.NumGeometries == 0 && _gmlVersion == GMLVersion.Two)
            {
                writer.WriteStartElement("lineStringMember", _gmlNs);
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
            writer.WriteStartElement(GMLElements.gmlPrefix, _gmlVersion == GMLVersion.Two ? "MultiPolygon" : "MultiSurface", _gmlNs);
            if (_gmlVersion == GMLVersion.Two)
            {
                // Required in version 2
                writer.WriteAttributeString("srsName", GetEpsgCode(multiPolygon.Factory.SRID));
            }
            for (int i = 0; i < multiPolygon.NumGeometries; i++)
            {
                writer.WriteStartElement(_gmlVersion == GMLVersion.Two ? "polygonMember" : "surfaceMember", _gmlNs);
                Write(multiPolygon.Geometries[i] as Polygon, writer);
                writer.WriteEndElement();
            }

            if (multiPolygon.NumGeometries == 0 && _gmlVersion == GMLVersion.Two)
            {
                writer.WriteStartElement("polygonMember", _gmlNs);
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
            writer.WriteStartElement(GMLElements.gmlPrefix, "MultiGeometry", _gmlNs);
            if (_gmlVersion == GMLVersion.Two)
            {
                // Required in version 2
                writer.WriteAttributeString("srsName", GetEpsgCode(geometryCollection.Factory.SRID));
            }
            for (int i = 0; i < geometryCollection.NumGeometries; i++)
            {
                writer.WriteStartElement("geometryMember", _gmlNs);
                Write(geometryCollection.Geometries[i], writer);
                writer.WriteEndElement();
            }

            if (geometryCollection.NumGeometries == 0 && _gmlVersion == GMLVersion.Two)
            {
                writer.WriteStartElement("geometryMember", _gmlNs);
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
            switch (geometry)
            {
                case Point point:
                    return SetByteStreamLength(point);

                case LineString lineString:
                    return SetByteStreamLength(lineString);

                case Polygon polygon:
                    return SetByteStreamLength(polygon);

                case MultiPoint multiPoint:
                    return SetByteStreamLength(multiPoint);

                case MultiLineString multiLineString:
                    return SetByteStreamLength(multiLineString);

                case MultiPolygon multiPolygon:
                    return SetByteStreamLength(multiPolygon);

                case GeometryCollection collection:
                    return SetByteStreamLength(collection);

                default:
                    throw new ArgumentException("ShouldNeverReachHere");
            }
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

        /// <summary>
        /// Provides the EPSG code exposing the SRID of the geometry
        /// </summary>
        /// <param name="srid">The SRID of the geometry</param>
        /// <returns></returns>
        protected virtual string GetEpsgCode(int srid) {
            return $"EPSG:{srid}";
        }
    }
}
