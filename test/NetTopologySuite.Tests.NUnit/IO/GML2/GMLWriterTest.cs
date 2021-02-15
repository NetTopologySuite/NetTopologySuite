using NetTopologySuite.IO;
using NetTopologySuite.IO.GML2;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;

namespace NetTopologySuite.Tests.NUnit.IO.GML2
{
    [TestFixture]
    public class GMLWriterTest
    {
        [Test]  
        public void TestGML3MultiPolygon()
        {
            var geometry = new WKTReader().Read("MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)), ((20 35, 10 30, 10 10, 30 5, 45 20, 20 35), (30 20, 20 15, 20 25, 30 20)))");

            var gmlWriter = new GMLWriter(GMLVersion.Three);
            var reader = gmlWriter.Write(geometry);
            var document = XDocument.Load(reader);

            var multiSurfaceElement = document.Element(XName.Get("MultiSurface", "http://www.opengis.net/gml"));
            Assert.IsNotNull(multiSurfaceElement);

            var surfaceMemberElement = multiSurfaceElement.Element(XName.Get("surfaceMember", "http://www.opengis.net/gml"));
            Assert.IsNotNull(surfaceMemberElement);

            var posListElement = surfaceMemberElement.Element(XName.Get("Polygon", "http://www.opengis.net/gml"))
                                                     .Element(XName.Get("exterior", "http://www.opengis.net/gml"))
                                                     .Element(XName.Get("LinearRing", "http://www.opengis.net/gml"))
                                                     .Element(XName.Get("posList", "http://www.opengis.net/gml"));
            Assert.IsNotNull(posListElement);
        }

        [Test]
        public void TestGML3MultiLineString()
        {
            var geometry = new WKTReader().Read("MULTILINESTRING ((10 10, 20 20, 10 40),(40 40, 30 30, 40 20, 30 10))");

            var gmlWriter = new GMLWriter(GMLVersion.Three);
            var reader = gmlWriter.Write(geometry);
            var document = XDocument.Load(reader);

            var multiCurveElement = document.Element(XName.Get("MultiCurve", "http://www.opengis.net/gml"));
            Assert.IsNotNull(multiCurveElement);

            var curveMemberElement = multiCurveElement.Element(XName.Get("curveMember", "http://www.opengis.net/gml"));
            Assert.IsNotNull(curveMemberElement);

            var posListElement = curveMemberElement.Element(XName.Get("LineString", "http://www.opengis.net/gml"))
                                                   .Element(XName.Get("posList", "http://www.opengis.net/gml"));
            Assert.IsNotNull(posListElement);
        }


        [Test]
        public void TestGML3MultiPointWithPos()
        {
            var geometry = new WKTReader().Read("POINT (0 0)");

            var gmlWriter = new GMLWriter(GMLVersion.Three);
            var reader = gmlWriter.Write(geometry);
            var document = XDocument.Load(reader);

            var posElement = document.Element(XName.Get("Point", "http://www.opengis.net/gml")).Element(XName.Get("pos", "http://www.opengis.net/gml"));
            Assert.IsNotNull(posElement);
            Assert.AreEqual(posElement.Value, "0 0");
        }

        [Test]
        public void WriteEmptyMultiPolygon() {

            var geometry = new WKTReader().Read("MULTIPOLYGON EMPTY");

            var gmlWriter = new GMLWriter();
            var reader = gmlWriter.Write(geometry);
            var document = XDocument.Load(reader);

            document.Validate(CreateSchemaSet(), (sender, args) =>
            {
                // Currently also fails because of other issue 469, that is why we filter out the message
                if (args.Message.StartsWith("The element 'MultiPolygon' in namespace 'http://www.opengis.net/gml' has incomplete content"))
                {
                    Assert.Fail(args.Message);
                }
            });

            Assert.IsNotNull(document.Element(XName.Get("MultiPolygon", "http://www.opengis.net/gml")).Element(XName.Get("polygonMember", "http://www.opengis.net/gml")));
        }

        [Test]
        public void WriteEmptyMultiPoint()
        {

            var geometry = new WKTReader().Read("MULTIPOINT EMPTY");

            var gmlWriter = new GMLWriter();
            var reader = gmlWriter.Write(geometry);
            var document = XDocument.Load(reader);

            document.Validate(CreateSchemaSet(), (sender, args) =>
            {
                // Currently also fails because of other issue 469, that is why we filter out the message
                if (args.Message.StartsWith("The element 'MultiPoint' in namespace 'http://www.opengis.net/gml' has incomplete content"))
                {
                    Assert.Fail(args.Message);
                }
            });

            Assert.IsNotNull(document.Element(XName.Get("MultiPoint", "http://www.opengis.net/gml")).Element(XName.Get("pointMember", "http://www.opengis.net/gml")));
        }

        [Test]
        public void WriteEmptyMultiLineString()
        {

            var geometry = new WKTReader().Read("MULTILINESTRING EMPTY");

            var gmlWriter = new GMLWriter();
            var reader = gmlWriter.Write(geometry);
            var document = XDocument.Load(reader);

            document.Validate(CreateSchemaSet(), (sender, args) =>
            {
                // Currently also fails because of other issue 469, that is why we filter out the message
                if (args.Message.StartsWith("The element 'MultiLineString' in namespace 'http://www.opengis.net/gml' has incomplete content"))
                {
                    Assert.Fail(args.Message);
                }
            });

            Assert.IsNotNull(document.Element(XName.Get("MultiLineString", "http://www.opengis.net/gml")).Element(XName.Get("lineStringMember", "http://www.opengis.net/gml")));
        }

        [Test]
        public void WriteEmptyGeometryCollection()
        {

            var geometry = new WKTReader().Read("GEOMETRYCOLLECTION EMPTY");

            var gmlWriter = new GMLWriter();
            var reader = gmlWriter.Write(geometry);
            var document = XDocument.Load(reader);

            document.Validate(CreateSchemaSet(), (sender, args) =>
            {
                // Currently also fails because of other issue 469, that is why we filter out the message
                if (args.Message.StartsWith("The element 'MultiGeometry' in namespace 'http://www.opengis.net/gml' has incomplete content"))
                {
                    Assert.Fail(args.Message);
                }
            });

            Assert.IsNotNull(document.Element(XName.Get("MultiGeometry", "http://www.opengis.net/gml")).Element(XName.Get("geometryMember", "http://www.opengis.net/gml")));
        }

        private XmlSchemaSet CreateSchemaSet()
        {
            var schemaSet = new XmlSchemaSet();
            schemaSet.Add("http://www.opengis.net/gml", "http://schemas.opengis.net/gml/2.1.2/geometry.xsd");
            schemaSet.Add("http://www.w3.org/1999/xlink", "https://www.w3.org/1999/xlink.xsd");
            schemaSet.Add("http://www.w3.org/XML/1998/namespace", "http://www.w3.org/2001/xml.xsd");
            return schemaSet;
        }
    }
}
