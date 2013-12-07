using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;
using System.Xml.Linq;

#if SILVERLIGHT
using XmlTextWriter = System.Xml.XmlWriter;
using XmlTextReader = System.Xml.XmlReader;
using System.Xml.Linq;
#endif

namespace NetTopologySuite.IO.GML2
{
    /// <summary>
    /// Reads a GML document and creates a representation of the features based on NetTopologySuite model.
    /// Uses GML 2.1.1 <c>Geometry.xsd</c> schema for base for features.
    /// </summary>
    public class GMLReader
    {
        private readonly IGeometryFactory _factory;

        /// <summary>
        /// <c>Geometry</c> builder.
        /// </summary>
        protected IGeometryFactory Factory
        {
            get { return _factory; }
        }

        /// <summary>
        /// Initialize reader with a standard <c>GeometryFactory</c>.
        /// </summary>
        public GMLReader() : this(GeometryFactory.Default) { }

        /// <summary>
        /// Initialize reader with the given <c>GeometryFactory</c>.
        /// </summary>
        /// <param name="factory"></param>
        public GMLReader(IGeometryFactory factory)
        {
            _factory = factory;
        }

#if !SILVERLIGHT && !PCL
        /// <summary>
        /// Read a GML document and returns relative <c>Geometry</c>.
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public IGeometry Read(XDocument document)
        {
            var reader = XmlReader.Create(new StringReader(document.ToString()));
            return Read(reader);
        }
#else
        /// <summary>
        /// Read a GML document and returns relative <c>Geometry</c>.
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public IGeometry Read(XDocument document)
        {
            XmlReader reader = document.CreateReader();
            return Read(reader);
        }
#endif

        /// <summary>
        ///
        /// </summary>
        /// <param name="xmlText"></param>
        /// <returns></returns>
        public IGeometry Read(string xmlText)
        {
            var reader = XmlReader.Create(new StringReader(xmlText));
            return Read(reader);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="stringReader"></param>
        /// <returns></returns>
        public IGeometry Read(StringReader stringReader)
        {
            var reader = XmlReader.Create(stringReader);
            return Read(reader);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public IGeometry Read(XmlReader reader)
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
            if (IsStartElement(reader, "MultiPolygon"))
                return ReadMultiPolygon(reader);
            if (IsStartElement(reader, "MultiGeometry"))
                return ReadGeometryCollection(reader);

            // Go away until something readable is found...
            reader.Read();
            return Read(reader);
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

                    /*
                    default:
                        break;
                     */
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
            string[] values = value.Split(',');
            double x = XmlConvert.ToDouble(values[0]);
            double y = XmlConvert.ToDouble(values[1]);
            return new Coordinate(x, y);
        }

        /// <summary>
        /// Extract a <see cref="Coordinate" /> from a pos entity string value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected Coordinate ReadPosAsCoordinate(string[] value)
        {
            var ordinates = new double[Math.Min(3, value.Length)];
            for (var i = 0; i < ordinates.Length; i++)
            {
                ordinates[i] = XmlConvert.ToDouble(value[i]);
            }
            return ordinates.Length == 2
                ? new Coordinate(ordinates[0], ordinates[1])
                : new Coordinate(ordinates[0], ordinates[1], ordinates[2]);
        }

        /// <summary>
        /// Extract a <see cref="Coordinate" /> from a x,y string value.
        /// </summary>
        /// <param name="numOrdinates"> </param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected Coordinate[] ReadPosListAsCoordinates(int numOrdinates, string[] value)
        {
            Assert.IsTrue(value.Length % numOrdinates == 0);
            var coordinates = new Coordinate[value.Length / numOrdinates];
            var offset = 0;
            for (var i = 0; i < coordinates.Length; i++)
            {
                var ords = new string[numOrdinates];
                Array.Copy(value, offset, ords, 0, numOrdinates);
                offset += numOrdinates;
                coordinates[i] = ReadPosAsCoordinate(ords);
            }

            return coordinates;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected IPoint ReadPoint(XmlReader reader)
        {
            var numOrdinatesText = reader.GetAttribute("srsDimension");

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
                            string[] coords = reader.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (!string.IsNullOrEmpty(numOrdinatesText))
                            {
                                var numOrdinates = XmlConvert.ToInt32(numOrdinatesText);
                                Assert.IsTrue(coords.Length == numOrdinates, "srsDimension doen't match number of provided ordinates");
                            }
                            return Factory.CreatePoint(ReadPosAsCoordinate(coords));
                        }
                        if (IsStartElement(reader, "coordinates"))
                        {
                            reader.Read(); // Jump to values
                            string[] coords = reader.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            if (coords.Length != 1)
                                throw new ApplicationException("Should never reach here!");
                            var c = ReadCoordinates(coords[0]);
                            return Factory.CreatePoint(c);
                        }
                        break;

                    //default:
                    //    break;
                }
            }
            throw new ArgumentException("ShouldNeverReachHere!");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected ILineString ReadLineString(XmlReader reader)
        {
            var coordinates = new List<Coordinate>();
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
                            var ordinates = reader.ReadContentAsString();
                            coordinates.Add(ReadPosAsCoordinate(ordinates.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)));
                            //reader.ReadEndElement();
                            //return Factory.CreateLineString(coordinates.ToArray());
                        }
                        else if (IsStartElement(reader, "coordinates"))
                        {
                            reader.Read(); // Jump to values
                            string[] coords = reader.Value.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string coord in coords)
                            {
                                if (String.IsNullOrEmpty(coord))
                                    continue;
                                var c = ReadCoordinates(coord);
                                coordinates.Add(c);
                            }
                            //reader.ReadEndElement();
                            return Factory.CreateLineString(coordinates.ToArray());
                        }
                        else if (IsStartElement(reader, "posList"))
                        {
                            var tmp = reader.GetAttribute("srsDimension");
                            if (string.IsNullOrEmpty(tmp)) tmp = "2";
                            reader.Read();
                            coordinates.AddRange(ReadPosListAsCoordinates(XmlConvert.ToInt32(tmp), reader.ReadContentAsString().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)));
                            reader.ReadEndElement();
                            return Factory.CreateLineString(coordinates.ToArray());
                        }
                        break;

                    case XmlNodeType.EndElement:
                        return Factory.CreateLineString(coordinates.ToArray());

                    //default:
                    //    break;
                }
            }
            throw new ArgumentException("ShouldNeverReachHere!");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected ILinearRing ReadLinearRing(XmlReader reader)
        {
            return Factory.CreateLinearRing(ReadLineString(reader).Coordinates);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected IPolygon ReadPolygon(XmlReader reader)
        {
            ILinearRing exterior = null;
            var interiors = new List<ILinearRing>();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (IsStartElement(reader, "outerBoundaryIs") ||
                            IsStartElement(reader, "exterior"))
                            exterior = ReadLinearRing(reader);// as LinearRing;
                        else if (IsStartElement(reader, "innerBoundaryIs") ||
                            IsStartElement(reader, "interior"))
                            interiors.Add(ReadLinearRing(reader));
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name == GMLElements.gmlPrefix + ":Polygon")
                            return Factory.CreatePolygon(exterior, interiors.ToArray());
                        break;

                    //default:
                    //    break;
                }
            }
            throw new ArgumentException("ShouldNeverReachHere!");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected IMultiPoint ReadMultiPoint(XmlReader reader)
        {
            var points = new List<IPoint>();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (IsStartElement(reader, "pointMember"))
                            points.Add(ReadPoint(reader));
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name == GMLElements.gmlPrefix + ":MultiPoint")
                            return Factory.CreateMultiPoint(points.ToArray());
                        break;

                    //default:
                    //    break;
                }
            }
            throw new ArgumentException("ShouldNeverReachHere!");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected IMultiLineString ReadMultiLineString(XmlReader reader)
        {
            var lines = new List<ILineString>();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (IsStartElement(reader, "lineStringMember"))
                            lines.Add(ReadLineString(reader));
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name == GMLElements.gmlPrefix + ":MultiLineString")
                            return Factory.CreateMultiLineString(lines.ToArray());
                        break;

                    //default:
                    //    break;
                }
            }
            throw new ArgumentException("ShouldNeverReachHere!");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected IMultiPolygon ReadMultiPolygon(XmlReader reader)
        {
            var polygons = new List<IPolygon>();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (IsStartElement(reader, "polygonMember"))
                            polygons.Add(ReadPolygon(reader));
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name == GMLElements.gmlPrefix + ":MultiPolygon")
                            return Factory.CreateMultiPolygon(polygons.ToArray());
                        break;

                    //default:
                    //    break;
                }
            }
            throw new ArgumentException("ShouldNeverReachHere!");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected IGeometryCollection ReadGeometryCollection(XmlReader reader)
        {
            var collection = new List<IGeometry>();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (IsStartElement(reader, "Point"))
                            collection.Add(ReadPoint(reader));
                        else if (IsStartElement(reader, "LineString"))
                            collection.Add(ReadLineString(reader));
                        else if (IsStartElement(reader, "Polygon"))
                            collection.Add(ReadPolygon(reader));
                        else if (IsStartElement(reader, "MultiPoint"))
                            collection.Add(ReadMultiPoint(reader));
                        else if (IsStartElement(reader, "MultiLineString"))
                            collection.Add(ReadMultiLineString(reader));
                        else if (IsStartElement(reader, "MultiPolygon"))
                            collection.Add(ReadMultiPolygon(reader));
                        else if (IsStartElement(reader, "MultiGeometry"))
                            collection.Add(ReadGeometryCollection(reader));
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name == GMLElements.gmlPrefix + ":MultiGeometry")
                            return Factory.CreateGeometryCollection(collection.ToArray());
                        break;

                    //default:
                    //    break;
                }
            }
            throw new ArgumentException("ShouldNeverReachHere!");
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool IsStartElement(XmlReader reader, string name)
        {
            return reader.IsStartElement(name, GMLElements.gmlNS) ||
                    reader.IsStartElement(GMLElements.gmlPrefix + ":" + name);
        }
    }
}