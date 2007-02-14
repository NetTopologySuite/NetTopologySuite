using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.IO;
using GisSharpBlog.NetTopologySuite.IO.GML2;
using GisSharpBlog.NetTopologySuite.Samples.SimpleTests.Geometries;

namespace GisSharpBlog.NetTopologySuite.Samples.SimpleTests.ShapeTests
{
    /// <summary>
    /// 
    /// </summary>
    public class GMLTesting : BaseSamples
    {
        XmlReader xmlreader = null;
        private XmlDocument document = null;
        Geometry result = null;

        private GMLWriter writer = null;
        private GMLReader reader = null;

        private Point point = null;
        private LineString line = null;
        private Polygon polygon = null;
        private MultiPoint multiPoint = null;

        /// <summary>
        /// 
        /// </summary>
        public GMLTesting() 
        {
            point = Factory.CreatePoint(new Coordinate(100, 100));

            Coordinate[] coordinates = new Coordinate[]
            {
                 new Coordinate(10,10),
                 new Coordinate(20,20),
                 new Coordinate(20,10),                 
            };
            line = Factory.CreateLineString(coordinates);

            coordinates = new Coordinate[]
            {
                new Coordinate(100,100),
                new Coordinate(200,100),
                new Coordinate(200,200),                
                new Coordinate(100,200),
                new Coordinate(100,100),
            };
            Coordinate[] interior1 = new Coordinate[] 
            { 
                new Coordinate(120,120),
                new Coordinate(180,120),
                new Coordinate(180,180),                
                new Coordinate(120,180),
                new Coordinate(120,120),
            };
            LinearRing linearRing = Factory.CreateLinearRing(coordinates);
            LinearRing[] holes = new LinearRing[] { Factory.CreateLinearRing(interior1), };
            polygon = Factory.CreatePolygon(linearRing, holes);

            coordinates = new Coordinate[]
            {
                new Coordinate(100,100),
                new Coordinate(200,200),
                new Coordinate(300,300),                
                new Coordinate(400,400),
                new Coordinate(500,500),
            };
            multiPoint = Factory.CreateMultiPoint(coordinates);

            writer = new GMLWriter();
            reader = new GMLReader();
        }

        public override void Start()
        {                       
            xmlreader = writer.Write(point);
            document = new XmlDocument();
            document.Load(xmlreader);
            Write(document.InnerXml);
            result = reader.Read(document);
            Debug.Assert(point == result, "ERROR!");
            
            xmlreader = writer.Write(line);
            document = new XmlDocument();
            document.Load(xmlreader);
            Write(document.InnerXml);
            result = reader.Read(document);
            Debug.Assert(line == result, "ERROR!");
            
            xmlreader = writer.Write(polygon);
            document = new XmlDocument();
            document.Load(xmlreader);
            Write(document.InnerXml);
            result = reader.Read(document);
            Debug.Assert(polygon == result, "ERROR!");
            
            xmlreader = writer.Write(multiPoint);
            document = new XmlDocument();
            document.Load(xmlreader);
            Write(document);
            result = reader.Read(document);
            Debug.Assert(multiPoint == result, "ERROR!");
            
            MultiLineString multiLineString = new WKTReader().Read("MULTILINESTRING ((10 10, 20 20), (30 30, 40 40, 50 50, 70 80, 990 210), (2000.1 22, 457891.2334 3456.2, 33333 44444))") as MultiLineString;
            xmlreader = writer.Write(multiLineString);            
            document = new XmlDocument();
            document.Load(xmlreader);
            Write(document.InnerXml);
            result = reader.Read(document);
            Debug.Assert(multiLineString == result, "ERROR!");
            
            MultiPolygon multiPolygon = new WKTReader().Read("MULTIPOLYGON (((10 10, 10 20, 20 20, 20 10, 10 10), (12 12, 18 12, 18 18, 12 18, 12 12), (14 14, 16 14, 16 16, 14 16, 14 14)), ((30 30, 30 40, 40 40, 40 30, 30 30), (32 32, 38 32, 38 38, 32 38, 32 32)))") as MultiPolygon;
            xmlreader = writer.Write(multiPolygon);
            document = new XmlDocument();
            document.Load(xmlreader);
            Write(document.InnerXml);
            result = reader.Read(document);
            Debug.Assert(multiPolygon == result, "ERROR!");
            
            Geometry[] geometries = new Geometry[]  { point, line, polygon, multiPoint, multiLineString, multiPolygon, };
            GeometryCollection geometryCollection = Factory.CreateGeometryCollection(geometries);
            xmlreader = writer.Write(geometryCollection);
            document = new XmlDocument();
            document.Load(xmlreader);
            Write(document.InnerXml);
            result = reader.Read(document);
            Debug.Assert(geometryCollection == result, "ERROR!");            
        }
    }
}
