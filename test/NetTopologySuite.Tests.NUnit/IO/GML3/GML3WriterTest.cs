using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.IO.GML2;
using NetTopologySuite.IO.GML3;

using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO.GML3
{
    [TestFixture]
    public class GML3WriterTest
    {
        private static readonly XmlWriterSettings XmlWriterSettings = new XmlWriterSettings
        {
            CloseOutput = false,
            NamespaceHandling = NamespaceHandling.OmitDuplicates,
            Encoding = Encoding.UTF8,
        };

        private static readonly WKTReader WKTReader = new WKTReader
        {
            IsOldNtsCoordinateSyntaxAllowed = false,
            IsOldNtsMultiPointSyntaxAllowed = false,
        };

        [Test]
        public void TestGML3Point()
        {
            var document = ToGML3(CreatePoint());
            AssertPoint(document);
        }

        [Test]
        public void TestGML3LineString()
        {
            var document = ToGML3(CreateLineString());
            AssertLineString(document);
        }

        [Test]
        public void TestGML3PolygonWithoutHoles()
        {
            var document = ToGML3(CreatePolygonWithoutHoles());
            AssertPolygonWithoutHoles(document);
        }

        [Test]
        public void TestGML3PolygonWithHoles()
        {
            var document = ToGML3(CreatePolygonWithHoles());
            AssertPolygonWithHoles(document);
        }

        [Test]
        public void TestGML3MultiPoint()
        {
            var document = ToGML3(CreateMultiPoint());
            AssertMultiPoint(document);
        }

        [Test]
        public void TestGML3MultiLineString()
        {
            var document = ToGML3(CreateMultiLineString());
            AssertMultiLineString(document);
        }

        [Test]
        public void TestGML3MultiPolygon()
        {
            var document = ToGML3(CreateMultiPolygon());
            AssertMultiPolygon(document);
        }

        [Test]
        public void TestGML3GeometryCollection()
        {
            var document = ToGML3(CreateGeometryCollection());
            AssertGeometryCollection(document);
        }

        private static T NotNull<T>(T val)
        {
            Assert.That(val, Is.Not.Null);
            return val;
        }

        private static XName GML3Name(string localName)
        {
            return XName.Get(localName, "http://www.opengis.net/gml");
        }

        private static XDocument ToGML3(Geometry geom)
        {
            using var ms = new MemoryStream();
            using (var writer = XmlWriter.Create(ms, XmlWriterSettings))
            {
                new GML3Writer().Write(geom, writer);
            }

            ms.Position = 0;
            return XDocument.Load(ms);
        }

        private static Geometry CreatePoint()
        {
            return WKTReader.Read("POINT (0 0)");
        }

        private static void AssertPoint(XContainer container)
        {
            var pointElement = NotNull(container.Element(GML3Name("Point")));
            var posElement = NotNull(pointElement.Element(GML3Name("pos")));
            Assert.That(posElement.Value, Is.EqualTo("0 0"));
            AssertRoundTrip(pointElement, CreatePoint());
        }

        private static Geometry CreateLineString()
        {
            return WKTReader.Read("LINESTRING (1 1, 2 2, 3 3, 4 4, 5 5)");
        }

        private static void AssertLineString(XContainer container)
        {
            var lineStringElement = NotNull(container.Element(GML3Name("LineString")));
            var posListElement = NotNull(lineStringElement.Element(GML3Name("posList")));
            Assert.That(posListElement.Value, Is.EqualTo("1 1 2 2 3 3 4 4 5 5"));
            AssertRoundTrip(lineStringElement, CreateLineString());
        }

        private static Geometry CreatePolygonWithoutHoles()
        {
            return WKTReader.Read("POLYGON ((0 0, 0 1, 1 1, 1 0, 0 0))");
        }

        private static void AssertPolygonWithoutHoles(XContainer container)
        {
            var polygonElement = NotNull(container.Element(GML3Name("Polygon")));
            var exteriorElement = NotNull(polygonElement.Element(GML3Name("exterior")));
            var exteriorLinearRingElement = NotNull(exteriorElement.Element(GML3Name("LinearRing")));
            var exteriorPosListElement = NotNull(exteriorLinearRingElement.Element(GML3Name("posList")));
            Assert.That(exteriorPosListElement.Value, Is.EqualTo("0 0 0 1 1 1 1 0 0 0"));
            Assert.That(polygonElement.Elements(GML3Name("interior")), Is.Empty);
            AssertRoundTrip(polygonElement, CreatePolygonWithoutHoles());
        }

        private static Geometry CreatePolygonWithHoles()
        {
            return WKTReader.Read("POLYGON ((0 0, 0 1, 1 1, 1 0, 0 0), (0.1 0.1, 0.1 0.2, 0.2 0.2, 0.2 0.1, 0.1 0.1), (0.4 0.4, 0.4 0.6, 0.6 0.6, 0.6 0.4, 0.4 0.4))");
        }

        private static void AssertPolygonWithHoles(XContainer container)
        {
            var polygonElement = NotNull(container.Element(GML3Name("Polygon")));
            var exteriorElement = NotNull(polygonElement.Element(GML3Name("exterior")));
            var exteriorLinearRingElement = NotNull(exteriorElement.Element(GML3Name("LinearRing")));
            var exteriorPosListElement = NotNull(exteriorLinearRingElement.Element(GML3Name("posList")));
            Assert.That(exteriorPosListElement.Value, Is.EqualTo("0 0 0 1 1 1 1 0 0 0"));
            var interiorPosListValues =
                from interiorElement in polygonElement.Elements(GML3Name("interior"))
                let interiorLinearRingElement = NotNull(interiorElement.Element(GML3Name("LinearRing")))
                let interiorPosListElement = NotNull(interiorLinearRingElement.Element(GML3Name("posList")))
                select interiorPosListElement.Value;
            Assert.That(interiorPosListValues, Is.EqualTo(new[]
            {
                "0.1 0.1 0.1 0.2 0.2 0.2 0.2 0.1 0.1 0.1",
                "0.4 0.4 0.4 0.6 0.6 0.6 0.6 0.4 0.4 0.4",
            }));
            AssertRoundTrip(polygonElement, CreatePolygonWithHoles());
        }

        private static Geometry CreateMultiPoint()
        {
            return WKTReader.Read("MULTIPOINT ((1 1), (2 2), (3 3))");
        }

        private static void AssertMultiPoint(XContainer container)
        {
            var multiPointElement = NotNull(container.Element(GML3Name("MultiPoint")));
            var posValues =
                from pointMemberElement in multiPointElement.Elements(GML3Name("pointMember"))
                let pointElement = NotNull(pointMemberElement.Element(GML3Name("Point")))
                let posElement = NotNull(pointElement.Element(GML3Name("pos")))
                select posElement.Value;
            Assert.That(posValues, Is.EqualTo(new[]
            {
                "1 1",
                "2 2",
                "3 3",
            }));
            AssertRoundTrip(multiPointElement, CreateMultiPoint());
        }

        private static Geometry CreateMultiLineString()
        {
            return WKTReader.Read("MULTILINESTRING ((10 10, 20 20, 10 40),(40 40, 30 30, 40 20, 30 10))");
        }

        private static void AssertMultiLineString(XContainer container)
        {
            var multiCurveElement = NotNull(container.Element(GML3Name("MultiCurve")));
            var posListValues =
                from curveMemberElement in multiCurveElement.Elements(GML3Name("curveMember"))
                let lineStringElement = NotNull(curveMemberElement.Element(GML3Name("LineString")))
                let posListElement = NotNull(lineStringElement.Element(GML3Name("posList")))
                select posListElement.Value;
            Assert.That(posListValues, Is.EqualTo(new[]
            {
                "10 10 20 20 10 40",
                "40 40 30 30 40 20 30 10",
            }));
            AssertRoundTrip(multiCurveElement, CreateMultiLineString());
        }

        private static Geometry CreateMultiPolygon()
        {
            return WKTReader.Read("MULTIPOLYGON (((40 40, 20 45, 45 30, 40 40)), ((20 35, 10 30, 10 10, 30 5, 45 20, 20 35), (30 20, 20 15, 20 25, 30 20)))");
        }

        private static void AssertMultiPolygon(XContainer container)
        {
            var multiSurfaceElement = NotNull(container.Element(GML3Name("MultiSurface")));
            var posListValues =
                from surfaceMemberElement in multiSurfaceElement.Elements(GML3Name("surfaceMember"))
                let polygonElement = NotNull(surfaceMemberElement.Element(GML3Name("Polygon")))
                let exteriorElement = NotNull(polygonElement.Element(GML3Name("exterior")))
                let exteriorLinearRingElement = NotNull(exteriorElement.Element(GML3Name("LinearRing")))
                let exteriorPosListElement = NotNull(exteriorLinearRingElement.Element(GML3Name("posList")))
                // prepend a dummy so we can handle polygons without holes.  we'll skip it later to compensate.
                from interiorElement in polygonElement.Elements(GML3Name("interior")).Prepend(DummyInteriorElement())
                let interiorLinearRingElement = NotNull(interiorElement.Element(GML3Name("LinearRing")))
                let interiorPosListElement = NotNull(interiorLinearRingElement.Element(GML3Name("posList")))
                group NotNull(interiorPosListElement.Value) by exteriorPosListElement into grp
                select (grp.Key.Value, grp.Skip(1).ToArray());
            Assert.That(posListValues, Is.EqualTo(new[]
            {
                ("40 40 20 45 45 30 40 40", Array.Empty<string>()),
                ("20 35 10 30 10 10 30 5 45 20 20 35", new[] { "30 20 20 15 20 25 30 20" }),
            }));
            AssertRoundTrip(multiSurfaceElement, CreateMultiPolygon());

            static XElement DummyInteriorElement()
            {
                return new XElement(
                    GML3Name("interior"),
                    new XElement(
                        GML3Name("LinearRing"),
                        new XElement(
                            GML3Name("posList"),
                            "it doesn't matter what I put here")));
            }
        }

        private static GeometryCollection CreateGeometryCollection()
        {
            Geometry[] geoms =
            {
                CreatePoint(),
                CreateLineString(),
                CreatePolygonWithoutHoles(),
                CreatePolygonWithHoles(),
                CreateMultiPoint(),
                CreateMultiLineString(),
                CreateMultiPolygon(),
            };

            return geoms[0].Factory.CreateGeometryCollection(geoms);
        }

        private static void AssertGeometryCollection(XContainer container)
        {
            var multiGeometryElement = NotNull(container.Element(GML3Name("MultiGeometry")));
            var elements = new Queue<XElement>(multiGeometryElement.Elements(GML3Name("geometryMember")));
            Assert.That(elements, Has.Count.EqualTo(7));
            Assert.Multiple(
                () =>
                {
                    AssertPoint(elements.Dequeue());
                    AssertLineString(elements.Dequeue());
                    AssertPolygonWithoutHoles(elements.Dequeue());
                    AssertPolygonWithHoles(elements.Dequeue());
                    AssertMultiPoint(elements.Dequeue());
                    AssertMultiLineString(elements.Dequeue());
                    AssertMultiPolygon(elements.Dequeue());
                });
            AssertRoundTrip(multiGeometryElement, CreateGeometryCollection());
        }

        private static void AssertRoundTrip(XElement element, Geometry expected)
        {
            var document = new XDocument(element);
            var actual = new GMLReader().Read(document);
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
