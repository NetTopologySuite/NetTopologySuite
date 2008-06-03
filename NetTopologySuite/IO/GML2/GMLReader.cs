using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.IO.GML2
{
    /// <summary>
    /// Reads a GML document and creates a representation of the features based or NetTopologySuite model.
    /// Uses GML 2.1.1 <c>Geometry.xsd</c> schema for base for features.
    /// </summary>
    public class GMLReader
    {
        private IGeometryFactory factory = null;
        
        /// <summary>
        /// <c>Geometry</c> builder.
        /// </summary>
        protected IGeometryFactory Factory
        {
            get { return factory; }
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
            this.factory = factory;
        }

        /// <summary>
        /// Read a GML document and returns relative <c>Geometry</c>.
        /// </summary>
        /// <param name="document"></param>
        /// <returns></returns>
        public IGeometry Read(XmlDocument document)
        {
            XmlReader reader = new XmlTextReader(new StringReader(document.InnerXml));
            return Read(reader);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlText"></param>
        /// <returns></returns>
        public IGeometry Read(string xmlText)
        {
            XmlReader reader = new XmlTextReader(new StringReader(xmlText));
            return Read(reader);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stringReader"></param>
        /// <returns></returns>
        public IGeometry Read(StringReader stringReader)
        {
            XmlReader reader = new XmlTextReader(stringReader);
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
                throw new ApplicationException("Should never reach here!");

            if (IsStartElement(reader, "Point"))
                return ReadPoint(reader);
            else if (IsStartElement(reader, "LineString"))
                return ReadLineString(reader);
            else if (IsStartElement(reader, "Polygon"))
                return ReadPolygon(reader);
            else if (IsStartElement(reader, "MultiPoint"))
                return ReadMultiPoint(reader);
            else if (IsStartElement(reader, "MultiLineString"))
                return ReadMultiLineString(reader);
            else if (IsStartElement(reader, "MultiPolygon"))
                return ReadMultiPolygon(reader);
            else if (IsStartElement(reader, "MultiGeometry"))
                return ReadGeometryCollection(reader);
            else
            {
                // Go away until something readable is found...
                reader.Read();
                return Read(reader);
            }
        }

        /// <summary>
        /// Reads the coordinate.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected ICoordinate ReadCoordinate(XmlReader reader)
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

                    default:
                        break;
                }
            }
            throw new ArgumentException("ShouldNeverReachHere!");            
        }

        /// <summary>
        /// Extract a <see cref="ICoordinate" /> from a x,y string value.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected ICoordinate ReadCoordinates(string value)
        {
            string[] values = value.Split(',');
            double x = XmlConvert.ToDouble(values[0]);
            double y = XmlConvert.ToDouble(values[1]);
            return new Coordinate(x, y);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected IPoint ReadPoint(XmlReader reader)
        {
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (IsStartElement(reader, "coord"))
                            return Factory.CreatePoint(ReadCoordinate(reader));
                        else if (IsStartElement(reader, "coordinates"))
                        {
                            reader.Read(); // Jump to values
                            string[] coords = reader.Value.Split(' ');
                            if (coords.Length != 1)
                                throw new ApplicationException("Should never reach here!");
                            ICoordinate c = ReadCoordinates(coords[0]);
                            Factory.CreatePoint(c);
                        }
                        break;

                    default:
                        break;
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
            List<ICoordinate> coordinates = new List<ICoordinate>();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (IsStartElement(reader, "coord"))
                            coordinates.Add(ReadCoordinate(reader));
                        else if (IsStartElement(reader, "coordinates"))
                        {
                            reader.Read(); // Jump to values
                            string[] coords = reader.Value.Split(' ');                            
                            foreach (string coord in coords)
                            {
                                if (String.IsNullOrEmpty(coord))
                                    continue;
                                ICoordinate c = ReadCoordinates(coord);
                                coordinates.Add(c);
                            }
                            return Factory.CreateLineString(coordinates.ToArray());
                        }
                        break;

                    case XmlNodeType.EndElement:
                        return Factory.CreateLineString(coordinates.ToArray());
                    
                    default:
                        break;
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
            List<ILinearRing> interiors = new List<ILinearRing>();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (IsStartElement(reader, "outerBoundaryIs"))
                            exterior = ReadLinearRing(reader) as LinearRing;
                        else if (IsStartElement(reader, "innerBoundaryIs"))
                            interiors.Add(ReadLinearRing(reader));
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name == GMLElements.gmlPrefix + ":Polygon")
                            return Factory.CreatePolygon(exterior, interiors.ToArray());
                        break;

                    default:
                        break;
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
            List<IPoint> points = new List<IPoint>();
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

                    default:
                        break;
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
            List<ILineString> lines = new List<ILineString>();
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

                    default:
                        break;
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
            List<IPolygon> polygons = new List<IPolygon>();
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

                    default:
                        break;
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
            List<IGeometry> collection = new List<IGeometry>();
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

                    default:
                        break;
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
            return  reader.IsStartElement(name, GMLElements.gmlNS) || 
                    reader.IsStartElement(GMLElements.gmlPrefix + ":" + name);
        }
    }
}
