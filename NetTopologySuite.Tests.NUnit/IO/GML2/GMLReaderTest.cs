using System;
using System.Collections.Generic;
using System.IO;
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
            DoTest(typeof(IPoint));
        }

        [Test]
        public void TestLineStringRead()
        {
            DoTest(typeof(ILineString));
        }

        [Test]
        public void TestPolygonRead()
        {
            DoTest(typeof(IPolygon));
        }

        [Test]
        public void TestMultiPointRead()
        {
            DoTest(typeof(IMultiPoint));
        }

        [Test]
        public void TestMultiLineStringRead()
        {
            DoTest(typeof(IMultiLineString));
        }

        [Test]
        public void TestMultiPolygonRead()
        {
            DoTest(typeof(IMultiPolygon));
        }

        private static void DoTest(Type expectedType)
        {
            string name = expectedType.Name;
            string file = string.Format("{0}s", name.ToLowerInvariant().Substring(1));
            string resname = string.Format("NetTopologySuite.Tests.NUnit.TestData.{0}.xml", file);
            string xml = new StreamReader(EmbeddedResourceManager.GetResourceStream(resname)).ReadToEnd();

            var gr = new GMLReader();

            // different target frameworks have different overload sets...
            foreach (var readMethod in GetReadMethods())
            {
                var gc = (IGeometryCollection)readMethod(gr, xml);
                Assert.IsTrue(gc.NumGeometries == 25);
                for (int i = 0; i < 25; i++)
                {
                    var g = gc.GetGeometryN(i);
                    Assert.IsNotNull(g);
                    Assert.IsInstanceOf(expectedType, g);
                }
            }
        }

        private static List<Func<GMLReader, string, IGeometry>> GetReadMethods()
        {
            var result = new List<Func<GMLReader, string, IGeometry>>(5)
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
                    return (IGeometry)xmlDocMethod.Invoke(reader, new object[] { doc });
                });
            }

            var xDocMethod = typeof(GMLReader).GetMethod("Read", new Type[] { typeof(XDocument) });
            if (xDocMethod != null)
            {
                result.Add((reader, xml) => (IGeometry)xDocMethod.Invoke(reader, new object[] { XDocument.Parse(xml) }));
            }

            return result;
        }
    }
}
