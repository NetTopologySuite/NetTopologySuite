using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO.GML2;
using NetTopologySuite.Tests.NUnit.TestData;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO.GML2
{
    [TestFixture]
    public class GMLReaderTest
    {
        [Test]
        public void TestPointRead()
        {
            DoTest(typeof(Point));
        }

        [Test]
        public void TestLineStringRead()
        {
            DoTest(typeof(LineString));
        }

        [Test]
        public void TestPolygonRead()
        {
            DoTest(typeof(Polygon));
        }

        [Test]
        [Category("GitHub Issue")]
        [Category("Issue331")]
        public void TestMultiPointRead()
        {
            var gc = DoTest(typeof(MultiPoint));

            // the last MultiPoint has z values in its <coordinates>
            var lastCoords = gc.Geometries.Last().Coordinates;
            Assert.That(lastCoords, Is.All.Property(nameof(Coordinate.Z)).Not.NaN);
        }

        [Test]
        public void TestMultiLineStringRead()
        {
            DoTest(typeof(MultiLineString));
        }

        [Test]
        public void TestMultiPolygonRead()
        {
            DoTest(typeof(MultiPolygon));
        }

        [Test]
        public void TestMultiSurfaceRead() {
            string xml = @"<gml:MultiSurface xmlns:gml=""http://www.opengis.net/gml\"">
                             <gml:surfaceMember>
                               <gml:Polygon>
                                 <gml:exterior>
                                   <gml:LinearRing>
                                     <gml:posList>40 40 20 45 45 30 40 40</gml:posList>
                                   </gml:LinearRing>
                                 </gml:exterior>
                               </gml:Polygon>
                             </gml:surfaceMember>
                             <gml:surfaceMember>
                               <gml:Polygon>
                                 <gml:exterior>
                                   <gml:LinearRing>
                                     <gml:posList>20 35 10 30 10 10 30 5 45 20 20 35</gml:posList>
                                   </gml:LinearRing>
                                 </gml:exterior>
                                 <gml:interior>
                                   <gml:LinearRing>
                                     <gml:posList>30 20 20 15 20 25 30 20</gml:posList>
                                   </gml:LinearRing>
                                 </gml:interior>
                               </gml:Polygon>
                             </gml:surfaceMember>
                           </gml:MultiSurface>";

            var gr = new GMLReader();

            foreach (var readMethod in GetReadMethods())
            {
                var gc = (MultiPolygon)readMethod(gr, xml);
                Assert.IsTrue(gc.NumGeometries == 2);
                for (int i = 0; i < 1; i++)
                {
                    var g = gc.GetGeometryN(i);
                    Assert.IsNotNull(g);
                    Assert.IsInstanceOf(typeof(Polygon), g);
                }
            }
        }

        [Test]
        public void TestMultiCurveRead()
        {
            string xml = @"<gml:MultiCurve xmlns:gml=""http://www.opengis.net/gml"">
                             <gml:curveMember>
                               <gml:LineString>
                                 <gml:posList>10 10 20 20 10 40</gml:posList>
                               </gml:LineString>
                             </gml:curveMember>
                             <gml:curveMember>
                               <gml:LineString>
                                 <gml:posList>40 40 30 30 40 20 30 10</gml:posList>
                               </gml:LineString>
                             </gml:curveMember>
                           </gml:MultiCurve>";

            var gr = new GMLReader();

            foreach (var readMethod in GetReadMethods())
            {
                var gc = (MultiLineString)readMethod(gr, xml);
                Assert.IsTrue(gc.NumGeometries == 2);
                for (int i = 0; i < 1; i++)
                {
                    var g = gc.GetGeometryN(i);
                    Assert.IsNotNull(g);
                    Assert.IsInstanceOf(typeof(LineString), g);
                }
            }
        }

        [Test]
        [Category("GitHub Issue")]
        [Category("Issue437")]
        public void CustomGeometryFactoryShouldBeAllowedWithSRID()
        {
            var pm = new PrecisionModel(10);
            const int initialSRID = 0;
            var csf = PackedCoordinateSequenceFactory.FloatFactory;
            const LinearRingOrientation orientation = LinearRingOrientation.Clockwise;

            var gf = new GeometryFactoryEx(pm, initialSRID, csf)
            {
                OrientationOfExteriorRing = orientation,
            };

            const int expectedSRID = 4326;
            string xml = $@"
<gml:Point srsName='urn:ogc:def:crs:EPSG::{expectedSRID}' xmlns:gml='http://www.opengis.net/gml'>
  <gml:coordinates>45.67, 65.43</gml:coordinates>
</gml:Point>";

            var gr = new GMLReader(gf);
            foreach (var readMethod in GetReadMethods())
            {
                var pt = (Point)readMethod(gr, xml);
                Assert.That(pt.Factory, Is.InstanceOf<GeometryFactoryEx>()
                                          .With.Property(nameof(GeometryFactoryEx.SRID)).EqualTo(expectedSRID)
                                          .With.Property(nameof(GeometryFactoryEx.OrientationOfExteriorRing)).EqualTo(orientation)
                                          .With.Property(nameof(GeometryFactoryEx.PrecisionModel)).EqualTo(pm)
                                          .With.Property(nameof(GeometryFactoryEx.CoordinateSequenceFactory)).EqualTo(csf));
            }
        }

        [TestCase("<gml:Polygon xmlns:gml=\"http://www.opengis.net/gml\" srsDimension=\"3\" srsName=\"http://www.opengis.net/gml/srs/epsg.xml#4326\" ><gml:exterior><gml:LinearRing><gml:posList>244420.388 488213.902 0.0 244407.058 488204.758 0.0 244413.487 488195.387 0.0 244418.813 488199.041 0.0 244415.419 488203.989 0.0 244423.422 488209.478 0.0 244420.388 488213.902 0.0</gml:posList></gml:LinearRing></gml:exterior></gml:Polygon>")]
        [TestCase("<gml:MultiPoint xmlns:gml=\"http://www.opengis.net/gml\" srsDimension=\"3\" srsName=\"http://www.opengis.net/gml/srs/epsg.xml#4326\" ><gml:pointMember><gml:Point><gml:pos>1.0 2.0 3.0</gml:pos></gml:Point></gml:pointMember></gml:MultiPoint>")]
        [TestCase("<gml:MultiLineString xmlns:gml=\"http://www.opengis.net/gml\" srsDimension=\"3\" srsName=\"http://www.opengis.net/gml/srs/epsg.xml#4326\" ><gml:lineStringMember><gml:LineString><gml:posList>1.0 2.0 3.0 2.0 2.0 3.0</gml:posList></gml:LineString></gml:lineStringMember></gml:MultiLineString>")]
        [Category("GitHub Issue")]
        [Category("Issue 685")]
        public void TestSrsDimension3OnTopLevelGeometry(string gmlData)
        {

            var reader = new GMLReader();
            Geometry geom = null;
            Assert.That( () => geom = reader.Read(gmlData), Throws.Nothing);
            //Assert.That(geom, Is.InstanceOf<Polygon>());
            //Assert.That(((Polygon)geom).ExteriorRing.CoordinateSequence.Dimension, Is.EqualTo(3));
            Assert.That(geom.Coordinate, Is.InstanceOf<CoordinateZ>());
        }

        private static GeometryCollection DoTest(Type expectedType)
        {
            string name = expectedType.Name;
            string file = string.Format("{0}s", name.ToLowerInvariant());
            string resname = string.Format("NetTopologySuite.Tests.NUnit.TestData.{0}.xml", file);
            string xml = new StreamReader(EmbeddedResourceManager.GetResourceStream(resname)).ReadToEnd();

            var gr = new GMLReader();

            GeometryCollection gc = null;

            // different target frameworks have different overload sets...
            foreach (var readMethod in GetReadMethods())
            {
                gc = (GeometryCollection)readMethod(gr, xml);
                Assert.IsTrue(gc.NumGeometries == 25);
                for (int i = 0; i < 25; i++)
                {
                    var g = gc.GetGeometryN(i);
                    Assert.IsNotNull(g);
                    Assert.IsInstanceOf(expectedType, g);
                }
            }

            return gc;
        }

        private static List<Func<GMLReader, string, Geometry>> GetReadMethods()
        {
            var result = new List<Func<GMLReader, string, Geometry>>(5)
            {
                (reader, xml) => reader.Read(xml),
                (reader, xml) => reader.Read(new StringReader(xml)),
                (reader, xml) => reader.Read(XmlReader.Create(new StringReader(xml)))
            };

            var xmlDocMethod = typeof(GMLReader).GetMethod("Read", new Type[] { typeof(XmlDocument) });
            if (xmlDocMethod != null)
            {
                result.Add((reader, xml) =>
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(xml);
                    return (Geometry)xmlDocMethod.Invoke(reader, new object[] { doc });
                });
            }

            var xDocMethod = typeof(GMLReader).GetMethod("Read", new Type[] { typeof(XDocument) });
            if (xDocMethod != null)
            {
                result.Add((reader, xml) => (Geometry)xDocMethod.Invoke(reader, new object[] { XDocument.Parse(xml) }));
            }

            return result;
        }
    }
}
