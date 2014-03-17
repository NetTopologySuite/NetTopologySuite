using System;
using System.Xml;
#if PCL
using System.Xml.Linq;
#endif
using GeoAPI.Geometries;
using NetTopologySuite.IO.GML2;
using NetTopologySuite.Tests.NUnit.TestData;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO.GML2
{
    [TestFixtureAttribute]
    public class GMLReaderTest
    {
        [TestAttribute]
        public void TestPointRead()
        {
            DoTest(typeof(IPoint));
        }

        [TestAttribute]
        public void TestLineStringRead()
        {
            DoTest(typeof(ILineString));
        }

        [TestAttribute]
        public void TestPolygonRead()
        {
            DoTest(typeof(IPolygon));
        }

        [TestAttribute]
        public void TestMultiPointRead()
        {
            DoTest(typeof(IMultiPoint));
        }

        [TestAttribute]
        public void TestMultiLineStringRead()
        {
            DoTest(typeof(IMultiLineString));
        }

        [TestAttribute]
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

            var doc = 
#if !PCL
                new XmlDocument();
            doc.Load(path);
#else
            XDocument.Load(path);
#endif

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
