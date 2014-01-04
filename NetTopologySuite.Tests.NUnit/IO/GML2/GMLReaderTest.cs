using System;
using System.Xml;
using GeoAPI.Geometries;
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
            string file = String.Format("{0}s", name.ToLowerInvariant().Substring(1));
            string resname = String.Format("NetTopologySuite.Tests.NUnit.TestData.{0}.xml", file);
            string path = EmbeddedResourceManager.SaveEmbeddedResourceToTempFile(resname);
            XmlDocument doc = new XmlDocument();
            doc.Load(path);

            GMLReader gr = new GMLReader();
            IGeometryCollection gc = (IGeometryCollection) gr.Read(doc);
            Assert.IsTrue(gc.NumGeometries == 25);
            for (int i = 0; i < 25; i++)
            {
                IGeometry g = gc.GetGeometryN(i);
                Assert.IsNotNull(g);
                Assert.IsInstanceOf(expectedType, g);
            }
        }
    }
}
