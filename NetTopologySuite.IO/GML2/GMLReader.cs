using System;
using System.Collections;
using System.IO;
using System.Text;
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
        private GeometryFactory factory = null;

        /// <summary>
        /// <c>Geometry</c> builder.
        /// </summary>
        protected GeometryFactory Factory
        {
            get { return factory; }
        }

        /// <summary>
        /// Initialize reader with a standard <c>GeometryFactory</c>. 
        /// </summary>
        public GMLReader() : this(new GeometryFactory()) { }

        /// <summary>
        /// Initialize reader with the given <c>GeometryFactory</c>.
        /// </summary>
        /// <param name="factory"></param>
        public GMLReader(GeometryFactory factory)
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
            XmlTextReader reader = new XmlTextReader(new StringReader(document.InnerXml));
            return Read(reader);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xmlText"></param>
        /// <returns></returns>
        public IGeometry Read(string xmlText)
        {
            XmlTextReader reader = new XmlTextReader(new StringReader(xmlText));
            return Read(reader);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stringReader"></param>
        /// <returns></returns>
        public IGeometry Read(StringReader stringReader)
        {
            XmlTextReader reader = new XmlTextReader(stringReader);
            return Read(reader);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public IGeometry Read(XmlTextReader reader)
        {
            // Read the first two childs...
            reader.Read();
            reader.Read();

            // ... then seek for geometry element
            if (reader.IsStartElement("Point", GMLElements.gmlNS))
                return ReadPoint(reader);
            else if (reader.IsStartElement("LineString", GMLElements.gmlNS))
                return ReadLineString(reader);
            else if (reader.IsStartElement("Polygon", GMLElements.gmlNS))
                return ReadPolygon(reader);
            else if (reader.IsStartElement("MultiPoint", GMLElements.gmlNS))
                return ReadMultiPoint(reader);
            else if (reader.IsStartElement("MultiLineString", GMLElements.gmlNS))
                return ReadMultiLineString(reader);
            else if (reader.IsStartElement("MultiPolygon", GMLElements.gmlNS))
                return ReadMultiPolygon(reader);
            else if (reader.IsStartElement("MultiGeometry", GMLElements.gmlNS))
                return ReadGeometryCollection(reader);
            throw new ArgumentException("ShouldNeverReachHere!");
        }

        /// <summary>
        /// Reads the coordinate.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        protected ICoordinate ReadCoordinate(XmlTextReader reader)
        {
            double x = 0, y = 0;
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.IsStartElement("X", GMLElements.gmlNS))
                        {                            
                            reader.Read();   // Jump to X value
                            x = XmlConvert.ToDouble(reader.Value);                            
                        }
                        else if (reader.IsStartElement("Y", GMLElements.gmlNS))
                        {
                            reader.Read();      // Jump to Y value
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
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected IPoint ReadPoint(XmlTextReader reader)
        {
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.IsStartElement("coord", GMLElements.gmlNS))
                            return Factory.CreatePoint(ReadCoordinate(reader));
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
        protected ILineString ReadLineString(XmlTextReader reader)
        {
            ArrayList coordinates = new ArrayList();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.IsStartElement("coord", GMLElements.gmlNS))
                            coordinates.Add(ReadCoordinate(reader));
                        break;

                    case XmlNodeType.EndElement:
                        return Factory.CreateLineString((ICoordinate[]) coordinates.ToArray(typeof(ICoordinate)));
                    
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
        protected ILinearRing ReadLinearRing(XmlTextReader reader)
        {
            return Factory.CreateLinearRing(ReadLineString(reader).Coordinates);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        protected IPolygon ReadPolygon(XmlTextReader reader)
        {
            ILinearRing exterior = null;
            ArrayList interiors = new ArrayList();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.IsStartElement("outerBoundaryIs", GMLElements.gmlNS))
                            exterior = ReadLinearRing(reader) as LinearRing;
                        else if (reader.IsStartElement("innerBoundaryIs", GMLElements.gmlNS))
                            interiors.Add(ReadLinearRing(reader));
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name == GMLElements.gmlPrefix + ":Polygon")
                            return Factory.CreatePolygon(exterior, (ILinearRing[]) interiors.ToArray(typeof(ILinearRing)));
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
        protected IMultiPoint ReadMultiPoint(XmlTextReader reader)
        {
            ArrayList points = new ArrayList();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.IsStartElement("pointMember", GMLElements.gmlNS))
                            points.Add(ReadPoint(reader));
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name == GMLElements.gmlPrefix + ":MultiPoint")
                            return Factory.CreateMultiPoint((IPoint[]) points.ToArray(typeof(IPoint)));
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
        protected IMultiLineString ReadMultiLineString(XmlTextReader reader)
        {
            ArrayList lines = new ArrayList();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.IsStartElement("lineStringMember", GMLElements.gmlNS))
                            lines.Add(ReadLineString(reader));
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name == GMLElements.gmlPrefix + ":MultiLineString")
                            return Factory.CreateMultiLineString((ILineString[]) lines.ToArray(typeof(ILineString)));
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
        protected IMultiPolygon ReadMultiPolygon(XmlTextReader reader)
        {
            ArrayList polygons = new ArrayList();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.IsStartElement("polygonMember", GMLElements.gmlNS))
                            polygons.Add(ReadPolygon(reader));
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name == GMLElements.gmlPrefix + ":MultiPolygon")
                            return Factory.CreateMultiPolygon((IPolygon[]) polygons.ToArray(typeof(IPolygon)));
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
        protected IGeometryCollection ReadGeometryCollection(XmlTextReader reader)
        {
            ArrayList collection = new ArrayList();
            while (reader.Read())
            {
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (reader.IsStartElement("Point", GMLElements.gmlNS))
                            collection.Add(ReadPoint(reader));
                        else if (reader.IsStartElement("LineString", GMLElements.gmlNS))
                            collection.Add(ReadLineString(reader));
                        else if (reader.IsStartElement("Polygon", GMLElements.gmlNS))
                            collection.Add(ReadPolygon(reader));
                        else if (reader.IsStartElement("MultiPoint", GMLElements.gmlNS))
                            collection.Add(ReadMultiPoint(reader));
                        else if (reader.IsStartElement("MultiLineString", GMLElements.gmlNS))
                            collection.Add(ReadMultiLineString(reader));
                        else if (reader.IsStartElement("MultiPolygon", GMLElements.gmlNS))
                            collection.Add(ReadMultiPolygon(reader));
                        else if (reader.IsStartElement("MultiGeometry", GMLElements.gmlNS))
                            collection.Add(ReadGeometryCollection(reader));
                        break;

                    case XmlNodeType.EndElement:
                        if (reader.Name == GMLElements.gmlPrefix + ":MultiGeometry")
                            return Factory.CreateGeometryCollection((IGeometry[])collection.ToArray(typeof(IGeometry)));
                        break;

                    default:
                        break;
                }
            }
            throw new ArgumentException("ShouldNeverReachHere!");
        }
    }
}
