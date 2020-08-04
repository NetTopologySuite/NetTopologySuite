using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using NetTopologySuite.Geometries;
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
