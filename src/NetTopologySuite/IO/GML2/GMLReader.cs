using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.IO.GML2
{
    /// <summary>
    /// Reads a GML document and creates a representation of the features based on NetTopologySuite model.
    /// Uses GML 2.1.1 <c>Geometry.xsd</c> schema for base for features.
    /// </summary>
    public class GMLReader
    {
        private readonly GeometryFactory _factory;

        /// <summary>
        /// <see cref="Geometry"/> builder.
        /// </summary>
        protected GeometryFactory Factory => _factory;

        /// <summary>
        /// Initialize reader with a standard <see cref="GeometryFactory"/>.
        /// </summary>
        public GMLReader() : this(NtsGeometryServices.Instance.CreateGeometryFactory()) { }

        /// <summary>
        /// Initialize reader with the given <see cref="GeometryFactory"/>.
        /// </summary>
        public GMLReader(GeometryFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Read a GML document and returns relative <see cref="Geometry"/>.
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public Geometry Read(XmlDocument document)
        {
            return Read(document.InnerXml);
        }

        /// <summary>
        /// Read a GML document and returns relative <see cref="Geometry"/>.
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public Geometry Read(XDocument document)
        {
            var reader = document.CreateReader();
            return Read(reader);
        }

        public Geometry Read(string xmlText)
        {
            return Read(new StringReader(xmlText));
        }

        public Geometry Read(StringReader stringReader)
        {
            return Read(XmlReader.Create(stringReader));
        }

        public Geometry Read(XmlReader reader)
        {
            if (reader.NodeType == XmlNodeType.EndElement)
                throw new ArgumentException("EndElement node type cannot be read by GMLReader.");

            if (IsStartElement(reader, "Point"))
                return ReadPoint(reader);
            if (IsStartElement(reader, "LineString"))
                return ReadLineString(reader);
            if (IsStartElement(reader, "Polygon"))
                return ReadPolygon(reader);
            if (IsStartElement(reader, "MultiPoint"))
                return ReadMultiPoint(reader);
            if (IsStartElement(reader, "MultiLineString"))
                return ReadMultiLineString(reader);
            if (IsStartElement(reader, "MultiCurve"))
                return ReadMultiCurve(reader);
            if (IsStartElement(reader, "MultiPolygon"))
                return ReadMultiPolygon(reader);
            if (IsStartElement(reader, "MultiSurface"))
                return ReadMultiSurface(reader);
            if (IsStartElement(reader, "MultiGeometry"))
                return ReadGeometryCollection(reader);

            // Go away until something readable is found...
            reader.Read();
            return Read(reader);
        }

        protected static int ReadSrsDimension(XmlReader reader, int srsDimension)
        {
            string tmp = reader.GetAttribute("srsDimension");
            return tmp == null ? srsDimension : XmlConvert.ToInt32(tmp);
        }

        /// <summary>
        /// Reads the coordinate.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected Coordinate ReadCoordinate(XmlReader reader)
        {
            double x = 0, y = 0;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (IsStartElement(reader, "X"))
                        {
                            reader.Read(); // Jump to X value
                            x = XmlConvert.ToDouble(reader.Value);
                        }
                        else if (IsStartElement(reader, "Y"))
                        {
                            reader.Read(); // Jump to Y value
                            y = XmlConvert.ToDouble(reader.Value);
                        }
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name == GMLElements.gmlPrefix + ":coord")
                            return new Coordinate(x, y);
                        break;
                }
            }
            throw new ArgumentException("ShouldNeverReachHere!");
        }

        /// <summary>
        /// Extract a <see cref="Coordinate" /> from a x,y string value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected Coordinate ReadCoordinates(string value)
        {

            string[] values = RemoveUnneccessaryWhitespace(value).Split(',');
            return ReadPosAsCoordinate(values);
        }

        /// <summary>
        /// Extract a <see cref="Coordinate" /> from a pos entity string value.
        /// </summary>
        /// <param name="value">An array of string ordinate values</param>
        /// <returns>A coordinate</returns>
        protected Coordinate ReadPosAsCoordinate(string[] value)
        {
            double[] ordinates = new double[Math.Min(3, value.Length)];
            for (int i = 0; i < ordinates.Length; i++)
            {
                ordinates[i] = XmlConvert.ToDouble(value[i]);
            }
            return ordinates.Length == 2
                ? new Coordinate(ordinates[0], ordinates[1])
                : new CoordinateZ(ordinates[0], ordinates[1], ordinates[2]);
        }

        /// <summary>
        /// Extract an enumeration of <see cref="Coordinate"/>s from a x-,y-[, z-]ordinate string values.
        /// </summary>
        /// <param name="numOrdinates">The number of ordinates</param>
        /// <param name="value">An array of string ordinate values</param>
        /// <returns>An enumeration of coordinates</returns>
        [Obsolete("Use ReadPosList(xmlReader, int, CoordinateSequenceFactory)")]
        protected IEnumerable<Coordinate> ReadPosListAsCoordinates(int numOrdinates, string[] value)
        {
            Assert.IsTrue(value.Length % numOrdinates == 0);
            var coordinates = new Coordinate[value.Length / numOrdinates];
            int offset = 0;
            string[] ords = new string[numOrdinates];
            for (int i = 0; i < coordinates.Length; i++)
            {
                Array.Copy(value, offset, ords, 0, numOrdinates);
                offset += numOrdinates;
                coordinates[i] = ReadPosAsCoordinate(ords);
            }

            return coordinates;
        }

        /// <summary>
        /// Extract a <see cref="CoordinateSequence" /> from a series of x-, y-[, z-]ordinate string values.
        /// </summary>
        /// <param name="reader">A xml-reader</param>
        /// <param name="srsDimension">The number of ordinates to read per coordinate</param>
        /// <param name="csFactory">The factory to use in order to create the result</param>
        /// <returns>A coordinate sequence</returns>
        protected CoordinateSequence ReadPosList(XmlReader reader, int srsDimension, CoordinateSequenceFactory csFactory)
        {
            Assert.IsTrue(IsStartElement(reader, "posList"));

            // Read, update srsDimension
            srsDimension = ReadSrsDimension(reader, srsDimension);
            reader.Read();

            var coordinatesSpan = reader.ReadContentAsString().AsSpan().Trim();
            var coordinates = new List<double>();

            while (!coordinatesSpan.IsEmpty)
            {
                int seperatorIndex = coordinatesSpan.IndexOf(' ');
                if (seperatorIndex == -1)
                    seperatorIndex = coordinatesSpan.Length;

                var ordinateSpan = coordinatesSpan.Slice(0, seperatorIndex);
                if (!ordinateSpan.IsEmpty)
                {
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_1_OR_GREATER
                    if (double.TryParse(ordinateSpan, System.Globalization.NumberStyles.Number,
                        System.Globalization.NumberFormatInfo.InvariantInfo, out double ordinate))
#else
                    if (double.TryParse(ordinateSpan.ToString(), System.Globalization.NumberStyles.Number,
                        System.Globalization.NumberFormatInfo.InvariantInfo, out double ordinate))
#endif
                        coordinates.Add(ordinate);
                }

                if (seperatorIndex < coordinatesSpan.Length - 1) seperatorIndex++;
                coordinatesSpan = coordinatesSpan.Slice(seperatorIndex);
            }

            Assert.IsTrue(coordinates.Count % srsDimension == 0);

            reader.ReadEndElement();

            var res = csFactory.Create(coordinates.Count / srsDimension, srsDimension, 0);
            for (int i = 0, k = 0; i < res.Count; i++) {
                for (int j = 0; j < srsDimension; j++)
                    res.SetOrdinate(i, j, coordinates[k++]);
            }

            return res;
        }

        protected Point ReadPoint(XmlReader reader)
        {
            return ReadPoint(reader, Factory);
        }

        protected Point ReadPoint(XmlReader reader, GeometryFactory gf)
        {
            return ReadPoint(reader, gf, 2);
        }

        protected Point ReadPoint(XmlReader reader, GeometryFactory gf, int srsDimension)
        {
            string numOrdinatesText = reader.GetAttribute("srsDimension");
            if (numOrdinatesText != null) srsDimension = XmlConvert.ToInt32(numOrdinatesText);
            
            gf = GetFactory(reader.GetAttribute("srsName"), gf);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (IsStartElement(reader, "coord"))
                            return Factory.CreatePoint(ReadCoordinate(reader));
                        if (IsStartElement(reader, "pos"))
                        {
                            reader.Read(); // Jump to values
                            string[] coords = RemoveUnneccessaryWhitespace(reader.Value).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            // Test srsDimension
                            Assert.IsTrue(coords.Length == srsDimension, "srsDimension doen't match number of provided ordinates");

                            return gf.CreatePoint(ReadPosAsCoordinate(coords));
                        }
                        if (IsStartElement(reader, "coordinates"))
                        {
                            reader.Read(); // Jump to values
                            string[] coords = RemoveUnneccessaryWhitespace(reader.Value).Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (coords.Length != 1)
                                throw new ApplicationException("Should never reach here!");
                            var c = ReadCoordinates(coords[0]);
                            return gf.CreatePoint(c);
                        }
                        break;
                }
            }
            throw new ArgumentException("ShouldNeverReachHere!");
        }



        protected LineString ReadLineString(XmlReader reader)
        {
            return ReadLineString(reader, Factory);
        }

        protected LineString ReadLineString(XmlReader reader, GeometryFactory gf)
        {
            return ReadLineString(reader, gf, 2);
        }

        protected LineString ReadLineString(XmlReader reader, GeometryFactory gf, int srsDimension)
        {
            var coordinates = new List<Coordinate>();
            gf = GetFactory(reader.GetAttribute("srsName"), gf);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (IsStartElement(reader, "coord"))
                        {
                            coordinates.Add(ReadCoordinate(reader));
                        }
                        else if (IsStartElement(reader, "pos"))
                        {
                            reader.Read();
                            string ordinates = reader.ReadContentAsString();
                            coordinates.Add(ReadPosAsCoordinate(ordinates.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)));
                        }
                        else if (IsStartElement(reader, "coordinates"))
                        {
                            reader.Read(); // Jump to values
                            string value = reader.Value;
                            string cleaned = value.Replace("\n", " ").Replace("\t", " ");
                            while (cleaned.Contains(", "))
                                cleaned = cleaned.Replace(", ", ",");
                            while (cleaned.Contains("  "))
                                cleaned = cleaned.Replace("  ", " ");
                            string[] coords = cleaned.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string coord in coords)
                            {
                                if (string.IsNullOrEmpty(coord))
                                    continue;
                                var c = ReadCoordinates(coord);
                                coordinates.Add(c);
                            }
                            return gf.CreateLineString(coordinates.ToArray());
                        }
                        else if (IsStartElement(reader, "posList"))
                        {
                            var cs = ReadPosList(reader, srsDimension, gf.CoordinateSequenceFactory);
                            return gf.CreateLineString(cs);
                        }
                        break;

                    case XmlNodeType.EndElement:
                        return Factory.CreateLineString(coordinates.ToArray());
                }
            }
            throw new ArgumentException("ShouldNeverReachHere!");
        }

        protected LinearRing ReadLinearRing(XmlReader reader)
        {
            return ReadLinearRing(reader, Factory);
        }

        protected LinearRing ReadLinearRing(XmlReader reader, GeometryFactory gf)
        {
            return ReadLinearRing(reader, gf, 2);
        }

        protected LinearRing ReadLinearRing(XmlReader reader, GeometryFactory gf, int srsDimension)
        {
            var ls = ReadLineString(reader, gf, srsDimension);
            return ls.Factory.CreateLinearRing(ls.CoordinateSequence);
        }

        protected Polygon ReadPolygon(XmlReader reader)
        {
            return ReadPolygon(reader, Factory);
        }

        protected Polygon ReadPolygon(XmlReader reader, GeometryFactory gf)
        {
            return ReadPolygon(reader, gf, 2);
        }

        protected Polygon ReadPolygon(XmlReader reader, GeometryFactory gf, int srsDimension)
        {
            LinearRing exterior = null;
            var interiors = new List<LinearRing>();

            // Read, update srsDimension
            srsDimension = ReadSrsDimension(reader, srsDimension);

            gf = GetFactory(reader.GetAttribute("srsName"), gf);
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (IsStartElement(reader, "outerBoundaryIs") ||
                            IsStartElement(reader, "exterior"))
                            exterior = ReadLinearRing(reader, gf, srsDimension);// as LinearRing;
                        else if (IsStartElement(reader, "innerBoundaryIs") ||
                            IsStartElement(reader, "interior"))
                            interiors.Add(ReadLinearRing(reader, gf, srsDimension));
                        break;

                    case XmlNodeType.EndElement:
                        string name = reader.Name;
                        if (name == "Polygon" ||
                            name == GMLElements.gmlPrefix + ":Polygon")
                            return gf.CreatePolygon(exterior, interiors.ToArray());
                        break;
                }
            }
            throw new ArgumentException("ShouldNeverReachHere!");
        }

        protected MultiPoint ReadMultiPoint(XmlReader reader)
        {
            return ReadMultiPoint(reader, Factory, 2);
        }

        protected MultiPoint ReadMultiPoint(XmlReader reader, GeometryFactory gf, int srsDimension)
        {
            var points = new List<Point>();

            // Read, update factory
            gf = GetFactory(reader.GetAttribute("srsName"), Factory);

            // Read, update srsDimension
            srsDimension = ReadSrsDimension(reader, srsDimension);

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (IsStartElement(reader, "pointMember"))
                            points.Add(ReadPoint(reader, gf, srsDimension));
                        break;

                    case XmlNodeType.EndElement:
                        string name = reader.Name;
                        if (name == "MultiPoint" ||
                            name == GMLElements.gmlPrefix + ":MultiPoint")
                            return gf.CreateMultiPoint(points.ToArray());
                        break;
                }
            }
            throw new ArgumentException("ShouldNeverReachHere!");
        }

        protected MultiLineString ReadMultiLineString(XmlReader reader)
        {
            return ReadMultiLineString(reader, Factory, 2);
        }

        protected MultiLineString ReadMultiLineString(XmlReader reader, GeometryFactory gf, int srsDimension)
        {
            var lines = new List<LineString>();

            // Read, update factory
            gf = GetFactory(reader.GetAttribute("srsName"), Factory);

            // Read, update srsDimension
            srsDimension = ReadSrsDimension(reader, srsDimension);

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (IsStartElement(reader, "lineStringMember"))
                            lines.Add(ReadLineString(reader, gf, srsDimension));
                        break;

                    case XmlNodeType.EndElement:
                        string name = reader.Name;
                        if (name == "MultiLineString" ||
                            name == GMLElements.gmlPrefix + ":MultiLineString")
                            return gf.CreateMultiLineString(lines.ToArray());
                        break;
                }
            }
            throw new ArgumentException("ShouldNeverReachHere!");
        }

        protected MultiLineString ReadMultiCurve(XmlReader reader)
        {
            return ReadMultiCurve(reader, Factory, 2);
        }

        protected MultiLineString ReadMultiCurve(XmlReader reader, GeometryFactory gf, int srsDimension)
        {
            var lines = new List<LineString>();

            // Read, update factory
            gf = GetFactory(reader.GetAttribute("srsName"), Factory);

            // Read, update srsDimension
            srsDimension = ReadSrsDimension(reader, srsDimension);

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (IsStartElement(reader, "curveMember"))
                            lines.Add(ReadLineString(reader, gf, srsDimension));
                        break;

                    case XmlNodeType.EndElement:
                        string name = reader.Name;
                        if (name == "MultiCurve" ||
                            name == GMLElements.gmlPrefix + ":MultiCurve")
                            return gf.CreateMultiLineString(lines.ToArray());
                        break;
                }
            }
            throw new ArgumentException("ShouldNeverReachHere!");
        }

        protected MultiPolygon ReadMultiPolygon(XmlReader reader)
        {
            return ReadMultiPolygon(reader, Factory, 2);
        }

        protected MultiPolygon ReadMultiPolygon(XmlReader reader, GeometryFactory gf, int srsDimension)
        {
            var polygons = new List<Polygon>();

            // Read, update factory
            gf = GetFactory(reader.GetAttribute("srsName"), gf);

            // Read, update srsDimension
            srsDimension = ReadSrsDimension(reader, srsDimension);

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (IsStartElement(reader, "polygonMember"))
                            polygons.Add(ReadPolygon(reader, gf, srsDimension));
                        break;

                    case XmlNodeType.EndElement:
                        string name = reader.Name;
                        if (name == "MultiPolygon" ||
                            name == GMLElements.gmlPrefix + ":MultiPolygon")
                            return gf.CreateMultiPolygon(polygons.ToArray());
                        break;
                }
            }
            throw new ArgumentException("ShouldNeverReachHere!");
        }

        protected MultiPolygon ReadMultiSurface(XmlReader reader)
        {
            return ReadMultiSurface(reader, Factory, 2);
        }

        protected MultiPolygon ReadMultiSurface(XmlReader reader, GeometryFactory gf, int srsDimension)
        {
            var polygons = new List<Polygon>();

            // Read, update factory
            gf = GetFactory(reader.GetAttribute("srsName"), gf);

            // Read, update srsDimension
            srsDimension = ReadSrsDimension(reader, srsDimension);

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (IsStartElement(reader, "surfaceMember"))
                            polygons.Add(ReadPolygon(reader, gf, srsDimension));
                        break;

                    case XmlNodeType.EndElement:
                        string name = reader.Name;
                        if (name == "MultiSurface" ||
                            name == GMLElements.gmlPrefix + ":MultiSurface")
                            return gf.CreateMultiPolygon(polygons.ToArray());
                        break;
                }
            }
            throw new ArgumentException("ShouldNeverReachHere!");
        }

        protected GeometryCollection ReadGeometryCollection(XmlReader reader)
        {
            var gf = GetFactory(reader.GetAttribute("srsName"), Factory);
            return ReadGeometryCollection(reader, gf, 2);
        }

        protected GeometryCollection ReadGeometryCollection(XmlReader reader, GeometryFactory gf, int srsDimension)
        {
            var collection = new List<Geometry>();

            // Read, update srsDimension
            srsDimension = ReadSrsDimension(reader, srsDimension);

            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (IsStartElement(reader, "Point"))
                            collection.Add(ReadPoint(reader, gf, srsDimension));
                        else if (IsStartElement(reader, "LineString"))
                            collection.Add(ReadLineString(reader, gf, srsDimension));
                        else if (IsStartElement(reader, "Polygon"))
                            collection.Add(ReadPolygon(reader, gf, srsDimension));
                        else if (IsStartElement(reader, "MultiPoint"))
                            collection.Add(ReadMultiPoint(reader));
                        else if (IsStartElement(reader, "MultiLineString"))
                            collection.Add(ReadMultiLineString(reader));
                        if (IsStartElement(reader, "MultiCurve"))
                            collection.Add(ReadMultiCurve(reader));
                        else if (IsStartElement(reader, "MultiPolygon"))
                            collection.Add(ReadMultiPolygon(reader));
                        if (IsStartElement(reader, "MultiSurface"))
                            collection.Add(ReadMultiSurface(reader));
                        else if (IsStartElement(reader, "MultiGeometry"))
                            collection.Add(ReadGeometryCollection(reader));
                        break;

                    case XmlNodeType.EndElement:
                        string name = reader.Name;
                        if (name == "MultiGeometry" ||
                            name == GMLElements.gmlPrefix + ":MultiGeometry")
                            return Factory.CreateGeometryCollection(collection.ToArray());
                        break;
                }
            }
            throw new ArgumentException("ShouldNeverReachHere!");
        }

        private static bool IsStartElement(XmlReader reader, string name)
        {
            return reader.IsStartElement(name) ||
                   reader.IsStartElement(name, GMLElements.gmlNS) ||
                   reader.IsStartElement(GMLElements.gmlPrefix + ":" + name);
        }

        private static readonly Regex IntSuffixPattern = new Regex("(\\d+)$");

        protected virtual GeometryFactory GetFactory(string srsName, GeometryFactory gfDefault)
        {
            var factory = gfDefault ?? Factory;
            if (string.IsNullOrWhiteSpace(srsName))
                return factory;

            if (!int.TryParse(srsName, out int srid))
            {
                var match = IntSuffixPattern.Match(srsName);
                if (match.Success)
                    srid = int.Parse(match.Groups[1].Value);
                else
                    return factory;
            }

            return factory.WithSRID(srid);
        }

        protected static string RemoveUnneccessaryWhitespace(string text)
        {
            text = Regex.Replace(text, "\\s", " ");
            text = Regex.Replace(text, "\\s*, \\s*", ",");
            text = Regex.Replace(text, "\\s* ,\\s*", ",");
            return text;
        }
    }
}
