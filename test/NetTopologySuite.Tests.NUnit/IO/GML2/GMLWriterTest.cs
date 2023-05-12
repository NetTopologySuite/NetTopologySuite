using NetTopologySuite.IO;
using NetTopologySuite.IO.GML2;
using NUnit.Framework;
using System.Xml.Linq;
using System.Xml.Schema;

namespace NetTopologySuite.Tests.NUnit.IO.GML2
{
    [TestFixture]
    public class GMLWriterTest
    {
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

        private static XmlSchemaSet CreateSchemaSet()
        {
            var schemaSet = new XmlSchemaSet();
            schemaSet.Add("http://www.opengis.net/gml", "http://schemas.opengis.net/gml/2.1.2/geometry.xsd");
            schemaSet.Add("http://www.w3.org/1999/xlink", "https://www.w3.org/1999/xlink.xsd");
            schemaSet.Add("http://www.w3.org/XML/1998/namespace", "http://www.w3.org/2001/xml.xsd");
            return schemaSet;
        }
    }
}
