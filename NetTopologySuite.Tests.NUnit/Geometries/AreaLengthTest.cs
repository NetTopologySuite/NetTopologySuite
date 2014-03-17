using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixtureAttribute]
    public class AreaLengthTest
    {
        private IPrecisionModel precisionModel;
        private IGeometryFactory geometryFactory;
        WKTReader reader;

        public AreaLengthTest()
        {
            precisionModel = new PrecisionModel(1);
            geometryFactory = new GeometryFactory(precisionModel, 0);
            reader = new WKTReader(geometryFactory);
        }

        private static double TOLERANCE = 1E-5;

        [TestAttribute]
        public void TestLength()
        {
            checkLength("MULTIPOINT (220 140, 180 280)", 0.0);
            checkLength("LINESTRING (220 140, 180 280)", 145.6021977);
            checkLength("LINESTRING (0 0, 100 100)", 141.4213562373095);
            checkLength("POLYGON ((20 20, 40 20, 40 40, 20 40, 20 20))", 80.0);
            checkLength("POLYGON ((20 20, 40 20, 40 40, 20 40, 20 20), (25 35, 35 35, 35 25, 25 25, 25 35))", 120.0);
        }

        [TestAttribute]
        public void TestArea()
        {
            checkArea("MULTIPOINT (220 140, 180 280)", 0.0);
            checkArea("LINESTRING (220 140, 180 280)", 0.0);
            checkArea("POLYGON ((20 20, 40 20, 40 40, 20 40, 20 20))", 400.0);
            checkArea("POLYGON ((20 20, 40 20, 40 40, 20 40, 20 20), (25 35, 35 35, 35 25, 25 25, 25 35))", 300.0);
        }

        public void checkLength(String wkt, double expectedValue)
        {
            IGeometry g = reader.Read(wkt);
            double len = g.Length;
            //System.out.println(len);
            Assert.AreEqual(expectedValue, len, TOLERANCE);
        }

        public void checkArea(String wkt, double expectedValue)
        {
            IGeometry g = reader.Read(wkt);
            Assert.AreEqual(expectedValue, g.Area, TOLERANCE);
        }
    }
}
