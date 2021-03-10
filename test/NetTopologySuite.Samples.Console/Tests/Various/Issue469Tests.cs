using NetTopologySuite.IO;
using NetTopologySuite.IO.GML2;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using NetTopologySuite.Geometries;
using System.IO;

namespace NetTopologySuite.Samples.Tests.Various
{
    public class Issue469Tests
    {
        [Test, Category("Issue469")]
        public void Issue469MultiPolygon()
        {
            int srid = 31370;
            var geometry = new WKTReader(new NtsGeometryServices(new PrecisionModel(), srid)).Read("MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)), ((20 35, 10 30, 10 10, 30 5, 45 20, 20 35), (30 20, 20 15, 20 25, 30 20)))");

            var gmlWriter = new GMLWriter();
            var reader = gmlWriter.Write(geometry);
            var document = XDocument.Load(reader);

            document.Validate(CreateSchemaSet(), (sender, args) =>
            {
                Assert.Fail(args.Message);
            });

            var element = document.Element(XName.Get($"{geometry.GeometryType}", "http://www.opengis.net/gml"));
            var attribute = element.Attribute(XName.Get("srsName"));
            Assert.IsNotNull(attribute);
            Assert.AreEqual(attribute.Value, $"EPSG:{geometry.Factory.SRID}");
            Assert.AreEqual(attribute.Value, $"EPSG:{srid}");
        }

        [Test, Category("Issue469")]
        public void Issue469MultiLineString()
        {
            int srid = 31370;
            var geometry = new WKTReader(new NtsGeometryServices(new PrecisionModel(), srid)).Read("MULTILINESTRING ((10 10, 20 20, 10 40),(40 40, 30 30, 40 20, 30 10))");

            var gmlWriter = new GMLWriter();
            var reader = gmlWriter.Write(geometry);
            var document = XDocument.Load(reader);

            document.Validate(CreateSchemaSet(), (sender, args) =>
            {
                Assert.Fail(args.Message);
            });

            var element = document.Element(XName.Get($"{geometry.GeometryType}", "http://www.opengis.net/gml"));
            var attribute = element.Attribute(XName.Get("srsName"));
            Assert.IsNotNull(attribute);
            Assert.AreEqual(attribute.Value, $"EPSG:{geometry.Factory.SRID}");
            Assert.AreEqual(attribute.Value, $"EPSG:{srid}");
        }

        [Test, Category("Issue469")]
        public void Issue469MultiPoint()
        {
            int srid = 31370;
            var geometry = new WKTReader(new NtsGeometryServices(new PrecisionModel(), srid)).Read("MULTIPOINT (10 40, 40 30, 20 20, 30 10)");

            var gmlWriter = new GMLWriter();
            var reader = gmlWriter.Write(geometry);
            var document = XDocument.Load(reader);

            document.Validate(CreateSchemaSet(), (sender, args) =>
            {
                Assert.Fail(args.Message);
            });

            var element = document.Element(XName.Get($"{geometry.GeometryType}", "http://www.opengis.net/gml"));
            var attribute = element.Attribute(XName.Get("srsName"));
            Assert.IsNotNull(attribute);
            Assert.AreEqual(attribute.Value, $"EPSG:{geometry.Factory.SRID}");
            Assert.AreEqual(attribute.Value, $"EPSG:{srid}");
        }

        [Test, Category("Issue469")]
        public void Issue469GeometryCollection()
        {
            int srid = 31370;
            var geometry = new WKTReader(new NtsGeometryServices(new PrecisionModel(), srid)).Read("GEOMETRYCOLLECTION (POINT (40 10), LINESTRING(10 10, 20 20, 10 40), POLYGON((40 40, 20 45, 45 30, 40 40)))");

            var gmlWriter = new GMLWriter();
            var reader = gmlWriter.Write(geometry);
            var document = XDocument.Load(reader);

            document.Validate(CreateSchemaSet(), (sender, args) =>
            {
                Assert.Fail(args.Message);
            });

            var element = document.Element(XName.Get("MultiGeometry", "http://www.opengis.net/gml"));
            var attribute = element.Attribute(XName.Get("srsName"));
            Assert.IsNotNull(attribute);
            Assert.AreEqual(attribute.Value, $"EPSG:{geometry.Factory.SRID}");
            Assert.AreEqual(attribute.Value, $"EPSG:{srid}");
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
