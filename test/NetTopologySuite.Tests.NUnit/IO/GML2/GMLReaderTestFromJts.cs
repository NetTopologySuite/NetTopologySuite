using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.GML2;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO.GML2
{
    public class GMLReaderTestFromJts : GeometryTestCase
    {
        private const int DefaultSrid = 9876;

        private static readonly GeometryFactory
            geometryFactory = new GeometryFactory(new PrecisionModel(), DefaultSrid);

        [Test]
        public void TestPoint()
        {
            CheckRead("<gml:Point>"
                      + "    <gml:coordinates>45.67,88.56</gml:coordinates>"
                      + " </gml:Point>",
                "POINT (45.67 88.56)");
        }

        [Test]
        public void TestPointNoNamespace()
        {
            CheckRead("<Point>"
                      + "    <coordinates>45.67,88.56</coordinates>"
                      + " </Point>",
                "POINT (45.67 88.56)");
        }

        [Test]
        public void TestPointWithCoordSepSpace()
        {
            CheckRead("<gml:Point>"
                      + "    <gml:coordinates>45.67, 88.56</gml:coordinates>"
                      + " </gml:Point>",
                "POINT (45.67 88.56)");
        }

        [Test]
        public void TestPointWithCoordSepMultiSpaceAfter()
        {
            CheckRead("<gml:Point>"
                      + "    <gml:coordinates>45.67,     88.56</gml:coordinates>"
                      + " </gml:Point>",
                "POINT (45.67 88.56)");
        }

        [Test]
        public void TestPointWithCoordSepMultiSpaceBefore()
        {
            CheckRead("<gml:Point>"
                      + "    <gml:coordinates>45.67   ,88.56</gml:coordinates>"
                      + " </gml:Point>",
                "POINT (45.67 88.56)");
        }

        [Test]
        public void TestPointWithCoordSepMultiSpaceBoth()
        {
            CheckRead("<gml:Point>"
                      + "    <gml:coordinates>45.67   ,   88.56</gml:coordinates>"
                      + " </gml:Point>",
                "POINT (45.67 88.56)");
        }

        [Test]
        public void TestPointSRIDInt()
        {
            CheckRead("<gml:Point srsName='1234'>"
                      + "    <gml:coordinates>45.67,     88.56</gml:coordinates>"
                      + " </gml:Point>",
                "POINT (45.67 88.56)", 1234);
        }

        [Test]
        public void TestPointSRIDHash()
        {
            CheckRead("<gml:Point srsName='some.prefix#4326'>"
                      + "    <gml:coordinates>45.67,     88.56</gml:coordinates>"
                      + " </gml:Point>",
                "POINT (45.67 88.56)",
                4326);
        }

        [Test]
        public void TestPointSRIDSlash()
        {
            CheckRead("<gml:Point srsName='http://www.opengis.net/def/crs/EPSG/0/4326'>"
                      + "    <gml:coordinates>45.67,     88.56</gml:coordinates>"
                      + " </gml:Point>",
                "POINT (45.67 88.56)",
                4326);
        }

        [Test]
        public void TestPointSRIDColon()
        {
            CheckRead("<gml:Point srsName='urn:ogc:def:crs:EPSG::4326'>"
                      + "    <gml:coordinates>45.67,     88.56</gml:coordinates>"
                      + " </gml:Point>",
                "POINT (45.67 88.56)",
                4326);
        }

        [Test]
        public void TestLineStringWithCoordSepSpace()
        {
            CheckRead("<gml:LineString>"
                      + "    <gml:coordinates>45.67, 88.56 55.56,89.44</gml:coordinates>"
                      + " </gml:LineString >",
                "LINESTRING (45.67 88.56, 55.56 89.44)");
        }

        [Test]
        public void TestLineStringWithManySpaces()
        {
            CheckRead("<gml:LineString>"
                      + "    <gml:coordinates>45.67,   88.56    55.56,89.44</gml:coordinates>"
                      + " </gml:LineString >",
                "LINESTRING (45.67 88.56, 55.56 89.44)");
        }

        private void CheckRead(string gml, string wktExpected)
        {
            CheckRead(gml, wktExpected, DefaultSrid);
        }

        private void CheckRead(string gml, string wktExpected, int srid)
        {
            gml = gml.Replace("gml:", "");
            var gr = new GMLReader(geometryFactory);
            Geometry g = null;
            try
            {
                g = gr.Read(gml);
            }
            catch (Exception e)
            {
                //e.printStackTrace();
                Assert.Fail(e.Message);
            }

            var expected = Read(wktExpected);
            CheckEqual(expected, g);
            Assert.AreEqual(srid, g.SRID, "SRID incorrect - ");
        }
    }

}
