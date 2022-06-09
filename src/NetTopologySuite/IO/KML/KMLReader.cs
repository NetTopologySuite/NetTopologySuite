using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.IO.KML
{
    /// <summary>
    /// Constructs <see cref="Geometry"/> objects from the OGC KML representation.
    /// Works only with KML geometry elements and may also parse attributes within these elements
    /// </summary>
    public class KMLReader
    {
        //private final XMLInputFactory inputFactory = XMLInputFactory.newInstance();
        private readonly GeometryFactory geometryFactory;
        private readonly ISet<string> attributeNames;
        private readonly Regex whitespaceRegex = new Regex(@"\s+");

        private const string POINT = "Point";
        private const string LINESTRING = "LineString";
        private const string POLYGON = "Polygon";
        private const string MULTIGEOMETRY = "MultiGeometry";

        private const string COORDINATES = "coordinates";
        private const string OUTER_BOUNDARY_IS = "outerBoundaryIs";
        private const string INNER_BOUNDARY_IS = "innerBoundaryIs";

        private const string NO_ELEMENT_ERROR = "No element {0} found in {1}";

        /// <summary>
        /// Creates a reader that creates objects using the default <see cref="GeometryFactory"/>.
        /// </summary>
        public KMLReader()
            : this(GeometryFactory.Default, Array.Empty<string>())
        {
        }

        /// <summary>
        /// Creates a reader that creates objects using the given <see cref="GeometryFactory"/>.
        /// </summary>
        /// <param name="geometryFactory">The factory used to create <c>Geometry</c>s.</param>
        public KMLReader(GeometryFactory geometryFactory)
            : this(geometryFactory, Array.Empty<string>())
        {
        }

        /// <summary>
        /// Creates a reader that creates objects using the default <see cref="GeometryFactory"/>.
        /// </summary>
        /// <param name="attributeNames">Names of attributes that should be parsed (i.e. extrude, altitudeMode, tesselate, etc).</param>
        public KMLReader(params string[] attributeNames)
            : this(new GeometryFactory(), attributeNames)
        {

        }

        /// <summary>
        /// Creates a reader that creates objects using the given <see cref="GeometryFactory"/>.
        /// </summary>
        /// <param name="geometryFactory">The factory used to create <c>Geometry</c>s.</param>
        /// <param name="attributeNames">Names of attributes that should be parsed (i.e. extrude, altitudeMode, tesselate, etc).</param>
        public KMLReader(GeometryFactory geometryFactory, params string[] attributeNames)
        {
            this.geometryFactory = geometryFactory;
            this.attributeNames = new HashSet<string>(attributeNames);
        }

        /// <summary>
        /// Reads a KML representation of a <see cref="Geometry"/> from a <see cref="string"/>.
        /// <para/>
        /// If any attribute names were specified during {@link KMLReader} construction,
        /// they will be stored as <see cref="IDictionary{TKey,TValue}"/> in <see cref="Geometry.UserData"/>.
        /// </summary>
        /// <param name="kmlGeometrystring">The string that specifies kml representation of geometry.</param>
        /// <returns>A <c>Geometry</c></returns>
        /// <exception cref="ParseException">Thrown if a parsing problem occurs.</exception>
        public Geometry Read(string kmlGeometrystring)
        {
            var sr = new StringReader(kmlGeometrystring);
            return Read(sr);
        }

        /// <summary>
        /// Reads a KML representation of a <see cref="Geometry"/> from a <see cref="TextReader"/>.
        /// <para/>
        /// If any attribute names were specified during {@link KMLReader} construction,
        /// they will be stored as <see cref="IDictionary{TKey,TValue}"/> in <see cref="Geometry.UserData"/>.
        /// </summary>
        /// <param name="kmlStreamReader">The text stream reader.</param>
        /// <returns>A <c>Geometry</c></returns>
        /// <exception cref="ParseException">Thrown if a parsing problem occurs.</exception>
        public Geometry Read(TextReader kmlStreamReader)
        {
            try
            {
                using (var xmlSr = XmlReader.Create(kmlStreamReader, new XmlReaderSettings {IgnoreComments = true, IgnoreWhitespace = true}))
                    return ParseKML(xmlSr);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private Coordinate[] ParseKMLCoordinates(XmlReader xmlStreamReader)
        {
            if (xmlStreamReader.IsEmptyElement)
                RaiseParseError("Empty coordinates");


            string coordinates = xmlStreamReader.ReadElementString();
            if (string.IsNullOrWhiteSpace(coordinates))
                RaiseParseError("Empty coordinates");
            
            coordinates = whitespaceRegex.Replace(coordinates.Trim(), " ");

            double[] parsedOrdinates = {double.NaN, double.NaN, double.NaN};
            var coordinateList = new List<Coordinate>();

            int spaceIdx = coordinates.IndexOf(' ');
            int currentIdx = 0;

            while (currentIdx < coordinates.Length)
            {
                if (spaceIdx == -1)
                {
                    spaceIdx = coordinates.Length;
                }

                string coordinate = coordinates.Substring(currentIdx, spaceIdx - currentIdx);

                int yOrdinateComma = coordinate.IndexOf(',');
                if (yOrdinateComma == -1 || yOrdinateComma == coordinate.Length - 1 || yOrdinateComma == 0)
                {
                    RaiseParseError("Invalid coordinate format");
                }

                parsedOrdinates[0] =
                    double.Parse(coordinate.Substring(0, yOrdinateComma), NumberFormatInfo.InvariantInfo);

                int zOrdinateComma = coordinate.IndexOf(',', yOrdinateComma + 1);
                Coordinate crd;
                if (zOrdinateComma == -1)
                {
                    parsedOrdinates[1] = double.Parse(coordinate.Substring(yOrdinateComma + 1),
                        NumberFormatInfo.InvariantInfo);
                    crd = new Coordinate(parsedOrdinates[0], parsedOrdinates[1]);
                }
                else
                {
                    yOrdinateComma += 1;
                    parsedOrdinates[1] = double.Parse(coordinate.Substring(yOrdinateComma, zOrdinateComma-yOrdinateComma),
                        NumberFormatInfo.InvariantInfo);
                    parsedOrdinates[2] = double.Parse(coordinate.Substring(zOrdinateComma + 1),
                        NumberFormatInfo.InvariantInfo);
                    crd = new CoordinateZ(parsedOrdinates[0], parsedOrdinates[1], parsedOrdinates[2]);
                }

                geometryFactory.PrecisionModel.MakePrecise(crd);

                coordinateList.Add(crd);
                currentIdx = spaceIdx + 1;
                if (currentIdx >= coordinates.Length) break;

                spaceIdx = coordinates.IndexOf(' ', currentIdx);
                parsedOrdinates[0] = parsedOrdinates[1] = parsedOrdinates[2] = double.NaN;
            }

            while (xmlStreamReader.NodeType == XmlNodeType.Whitespace)
                xmlStreamReader.Read();


            return coordinateList.ToArray();
        }

        private KMLCoordinatesAndAttributes ParseKMLCoordinatesAndAttributes(XmlReader xmlStreamReader,
            string objectNodeName)
        {
            Coordinate[] coordinates = null;
            IDictionary<string, string> attributes = null;

            Assert.IsTrue(xmlStreamReader.LocalName.Equals(objectNodeName));
            xmlStreamReader.ReadStartElement();

            while (!(xmlStreamReader.NodeType == XmlNodeType.EndElement && xmlStreamReader.LocalName.Equals(objectNodeName)))
            {
                string elementName = xmlStreamReader.LocalName;
                if (xmlStreamReader.IsStartElement())
                {
                    if (elementName.Equals(COORDINATES))
                    {
                        coordinates = ParseKMLCoordinates(xmlStreamReader);
                    }
                    else if (attributeNames.Contains(elementName))
                    {
                        if (attributes == null)
                        {
                            attributes = new Dictionary<string, string>();
                        }

                        attributes.Add(elementName, xmlStreamReader.ReadElementContentAsString());
                    }
                    else
                    {
                        xmlStreamReader.Skip();
                    }
                }
                else
                {
                    xmlStreamReader.Read();
                }
            }
            xmlStreamReader.ReadEndElement();

            if (coordinates == null)
            {
                RaiseParseError(NO_ELEMENT_ERROR, COORDINATES, objectNodeName);
            }

            return new KMLCoordinatesAndAttributes(coordinates, attributes);
        }

        private Geometry ParseKMLPoint(XmlReader xmlStreamReader)
        {
            var kmlCoordinatesAndAttributes =
                ParseKMLCoordinatesAndAttributes(xmlStreamReader, POINT);

            var point = geometryFactory.CreatePoint(kmlCoordinatesAndAttributes.coordinates[0]);
            point.UserData = kmlCoordinatesAndAttributes.attributes;

            return point;
        }

        private Geometry ParseKMLLineString(XmlReader xmlStreamReader)
        {
            var kmlCoordinatesAndAttributes =
                ParseKMLCoordinatesAndAttributes(xmlStreamReader, LINESTRING);

            var linestring = geometryFactory.CreateLineString(kmlCoordinatesAndAttributes.coordinates);
            linestring.UserData = kmlCoordinatesAndAttributes.attributes;

            return linestring;
        }

        private Geometry ParseKMLPolygon(XmlReader xmlStreamReader)
        {
            LinearRing shell = null;
            List<LinearRing> holes = null;
            IDictionary<string, string> attributes = null;

            Assert.IsTrue(xmlStreamReader.LocalName.Equals(POLYGON));
            xmlStreamReader.ReadStartElement();

            while (!(xmlStreamReader.NodeType == XmlNodeType.EndElement && xmlStreamReader.LocalName.Equals(POLYGON)))
            {
                if (xmlStreamReader.NodeType == XmlNodeType.Element)
                {
                    string elementName = xmlStreamReader.LocalName;

                    if (elementName.Equals(OUTER_BOUNDARY_IS))
                    {
                        MoveToElement(xmlStreamReader, COORDINATES, OUTER_BOUNDARY_IS);
                        shell = geometryFactory.CreateLinearRing(ParseKMLCoordinates(xmlStreamReader));
                        // LinearRing
                        xmlStreamReader.ReadEndElement();
                    }
                    else if (elementName.Equals(INNER_BOUNDARY_IS))
                    {
                        MoveToElement(xmlStreamReader, COORDINATES, INNER_BOUNDARY_IS);

                        if (holes == null)
                        {
                            holes = new List<LinearRing>();
                        }

                        holes.Add(geometryFactory.CreateLinearRing(ParseKMLCoordinates(xmlStreamReader)));
                        // LinearRing
                        xmlStreamReader.ReadEndElement();
                    }
                    else if (attributeNames.Contains(elementName))
                    {
                        // Create dictionary to keep attributes
                        if (attributes == null)
                        {
                            attributes = new Dictionary<string, string>();
                        }
                        // Add attribute
                        attributes.Add(elementName, xmlStreamReader.ReadElementContentAsString());
                    }
                    else
                    {
                        // Drop attribute element
                        xmlStreamReader.Skip();
                    }
                }
                else
                {
                    xmlStreamReader.Read();
                }

                while (xmlStreamReader.NodeType == XmlNodeType.Whitespace)
                    xmlStreamReader.Read();

            }
            xmlStreamReader.ReadEndElement();

            if (shell == null)
            {
                RaiseParseError("No outer boundary for Polygon");
            }

            var polygon =
                geometryFactory.CreatePolygon(shell, holes == null ? null : holes.ToArray());
            polygon.UserData = attributes;

            return polygon;
        }

        private Geometry ParseKMLMultiGeometry(XmlReader xmlStreamReader)
        {
            var geometries = new List<Geometry>();
            string firstParsedType = null;
            bool allTypesAreSame = true;

            Assert.IsTrue(xmlStreamReader.LocalName.Equals(MULTIGEOMETRY));
            xmlStreamReader.ReadStartElement();

            while (!(xmlStreamReader.NodeType == XmlNodeType.EndElement && xmlStreamReader.LocalName.Equals(MULTIGEOMETRY)))
            {
                if (xmlStreamReader.NodeType == XmlNodeType.Element)
                {
                    string elementName = xmlStreamReader.LocalName;
                    switch (elementName)
                    {
                        case POINT:
                        case LINESTRING:
                        case POLYGON:
                        case MULTIGEOMETRY:
                            var geometry = ParseKML(xmlStreamReader);

                            if (firstParsedType == null)
                            {
                                firstParsedType = geometry.GeometryType;
                            }
                            else if (!firstParsedType.Equals(geometry.GeometryType))
                            {
                                allTypesAreSame = false;
                            }

                            geometries.Add(geometry);
                            break;
                    }
                }
                else
                {
                    xmlStreamReader.Read();
                }

                if (xmlStreamReader.NodeType == XmlNodeType.Whitespace)
                    xmlStreamReader.Read();
            }
            xmlStreamReader.ReadEndElement();

            if (geometries.Count == 0)
            {
                return null;
            }

            if (geometries.Count == 1)
            {
                return geometries[0];
            }

            if (allTypesAreSame)
            {
                switch (firstParsedType)
                {
                    case POINT:
                        return geometryFactory.CreateMultiPoint(PrepareTypedArray<Point>(geometries, typeof(Point)));
                    case LINESTRING:
                        return geometryFactory.CreateMultiLineString(PrepareTypedArray<LineString>(geometries, typeof(LineString)));
                    case POLYGON:
                        return geometryFactory.CreateMultiPolygon(PrepareTypedArray<Polygon>(geometries, typeof(Polygon)));
                    default:
                        return geometryFactory.CreateGeometryCollection(geometries.ToArray());
                }
            }
            else
            {
                return geometryFactory.CreateGeometryCollection(geometries.ToArray());
            }
        }

        private Geometry ParseKML(XmlReader xmlStreamReader)
        {
            bool hasElement = false;

            // Seek for a start element
            while (!xmlStreamReader.EOF)
            {
                if (xmlStreamReader.IsStartElement())
                {
                    hasElement = true;
                    break;
                }
            }

            if (!hasElement)
            {
                RaiseParseError("Invalid KML format");
            }

            string elementName = xmlStreamReader.LocalName;
            switch (elementName)
            {
                case POINT:
                    return ParseKMLPoint(xmlStreamReader);
                case LINESTRING:
                    return ParseKMLLineString(xmlStreamReader);
                case POLYGON:
                    return ParseKMLPolygon(xmlStreamReader);
                case MULTIGEOMETRY:
                    return ParseKMLMultiGeometry(xmlStreamReader);
            }

            RaiseParseError("Unknown KML geometry type {0}", elementName);
            return null;
        }

        private void MoveToElement(XmlReader xmlStreamReader, string elementName, string endElementName)
        {
            bool elementFound = false;

            while (xmlStreamReader.Read())
            {
                if (xmlStreamReader.IsStartElement() && xmlStreamReader.LocalName.Equals(elementName))
                {
                    elementFound = true;
                    break;
                }
            }

            if (!elementFound)
            {
                RaiseParseError(NO_ELEMENT_ERROR, elementName, endElementName);
            }
        }

        private void RaiseParseError(string template, params object[] parameters)
        {
            throw new ParseException(string.Format(template, parameters));
        }

        private static T[] PrepareTypedArray<T>(List<Geometry> geometryList, Type geomClass) where T : Geometry
        {

            var res = (T[]) Array.CreateInstance(geomClass, geometryList.Count);
            for (int i = 0; i < geometryList.Count; i++)
                res[i] = (T) geometryList[i];
            return res;
        }

        private class KMLCoordinatesAndAttributes
        {
            public readonly Coordinate[] coordinates;
            public readonly IDictionary<string, string> attributes;

            public KMLCoordinatesAndAttributes(Coordinate[] coordinates, IDictionary<string, string> attributes)
            {

                this.coordinates = coordinates;
                this.attributes = attributes;
            }
        }
    }

}
