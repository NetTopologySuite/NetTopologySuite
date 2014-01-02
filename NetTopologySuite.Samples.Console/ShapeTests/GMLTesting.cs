using System.Diagnostics;
using System.Xml;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.IO.GML2;

namespace NetTopologySuite.Samples.SimpleTests.ShapeTests
{
    public class GMLTesting : BaseSamples
    {
        XmlReader _xmlreader;
        private XmlDocument _document;
        IGeometry _result;

        private readonly GMLWriter _writer;
        private readonly GMLReader _reader;

        private readonly IPoint _point;
        private readonly ILineString _line;
        private readonly IPolygon _polygon;
        private readonly IMultiPoint _multiPoint;

        public GMLTesting()
        {
            _point = Factory.CreatePoint(new Coordinate(100, 100));

            Coordinate[] coordinates =
            {
                new Coordinate(10,10),
                new Coordinate(20,20),
                new Coordinate(20,10)
            };
            _line = Factory.CreateLineString(coordinates);

            coordinates = new[]
            {
                new Coordinate(100,100),
                new Coordinate(200,100),
                new Coordinate(200,200),                
                new Coordinate(100,200),
                new Coordinate(100,100)
            };
            Coordinate[] interior1 =
            { 
                new Coordinate(120,120),
                new Coordinate(180,120),
                new Coordinate(180,180),                
                new Coordinate(120,180),
                new Coordinate(120,120)
            };
            ILinearRing linearRing = Factory.CreateLinearRing(coordinates);
            ILinearRing[] holes = { Factory.CreateLinearRing(interior1)};
            _polygon = Factory.CreatePolygon(linearRing, holes);

            coordinates = new[]
            {
                new Coordinate(100,100),
                new Coordinate(200,200),
                new Coordinate(300,300),                
                new Coordinate(400,400),
                new Coordinate(500,500)
            };
            _multiPoint = Factory.CreateMultiPoint(coordinates);

            _writer = new GMLWriter();
            _reader = new GMLReader();
        }

        public override void Start()
        {
            _xmlreader = _writer.Write(_point);
            _document = new XmlDocument();
            _document.Load(_xmlreader);
            Write(_document.InnerXml);
            _result = _reader.Read(_document);
            Debug.Assert(_point.Equals(_result), "ERROR!");

            //string gml = document.InnerXml;
            //gml = gml.Replace("gml:", "");
            //result = reader.Read(gml);

            _xmlreader = _writer.Write(_line);
            _document = new XmlDocument();
            _document.Load(_xmlreader);
            Write(_document.InnerXml);
            _result = _reader.Read(_document);
            Debug.Assert(_line.Equals(_result), "ERROR!");

            _xmlreader = _writer.Write(_polygon);
            _document = new XmlDocument();
            _document.Load(_xmlreader);
            Write(_document.InnerXml);
            _result = _reader.Read(_document);
            Debug.Assert(_polygon.Equals(_result), "ERROR!");

            _xmlreader = _writer.Write(_multiPoint);
            _document = new XmlDocument();
            _document.Load(_xmlreader);
            Write(_document);
            _result = _reader.Read(_document);
            Debug.Assert(_multiPoint.Equals(_result), "ERROR!");

            MultiLineString multiLineString = new WKTReader().Read("MULTILINESTRING ((10 10, 20 20), (30 30, 40 40, 50 50, 70 80, 990 210), (2000.1 22, 457891.2334 3456.2, 33333 44444))") as MultiLineString;
            _xmlreader = _writer.Write(multiLineString);
            _document = new XmlDocument();
            _document.Load(_xmlreader);
            Write(_document.InnerXml);
            _result = _reader.Read(_document);
            Debug.Assert(multiLineString.Equals(_result), "ERROR!");

            MultiPolygon multiPolygon = new WKTReader().Read("MULTIPOLYGON (((10 10, 10 20, 20 20, 20 10, 10 10), (12 12, 18 12, 18 18, 12 18, 12 12), (14 14, 16 14, 16 16, 14 16, 14 14)), ((30 30, 30 40, 40 40, 40 30, 30 30), (32 32, 38 32, 38 38, 32 38, 32 32)))") as MultiPolygon;
            _xmlreader = _writer.Write(multiPolygon);
            _document = new XmlDocument();
            _document.Load(_xmlreader);
            Write(_document.InnerXml);
            _result = _reader.Read(_document);
            Debug.Assert(multiPolygon.EqualsExact(_result), "ERROR!");

            IGeometry[] geometries = { _point, _line, _polygon, _multiPoint, multiLineString, multiPolygon};
            IGeometryCollection geometryCollection = Factory.CreateGeometryCollection(geometries);
            _xmlreader = _writer.Write(geometryCollection);
            _document = new XmlDocument();
            _document.Load(_xmlreader);
            Write(_document.InnerXml);
            _result = _reader.Read(_document);
            Debug.Assert(geometryCollection.EqualsExact(_result), "ERROR!");
        }
    }
}
