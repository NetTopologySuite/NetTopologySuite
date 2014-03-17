#if !PCL
using System;
using GeoAPI.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.IO.KML;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO.KML
{
    [TestFixtureAttribute]
    public class KMLWriterTest
    {
        [TestAttribute]
        public void group_separator_should_never_be_used()
        {
            CheckEqual("POINT (100000 100000)",
                "<Point><coordinates>100000,100000</coordinates></Point>");
        }

        [TestAttribute]
        public void TestPoint()
        {
            CheckEqual("POINT (1 1)",
                "<Point><coordinates>1,1</coordinates></Point>");
        }

        [TestAttribute]
        public void TestLine()
        {
            CheckEqual("LINESTRING (1 1, 2 2)",
                "<LineString><coordinates>1,1 2,2</coordinates></LineString>");
        }

        [TestAttribute]
        public void TestPolygon()
        {
            CheckEqual("POLYGON ((1 1, 2 1, 2 2, 1 2, 1 1))",
                "<Polygon><outerBoundaryIs><LinearRing><coordinates>1,1 2,1 2,2 1,2 1,1</coordinates></LinearRing></outerBoundaryIs></Polygon>");
        }

        [TestAttribute]
        public void TestPolygonWithHole()
        {
            CheckEqual("POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9), (2 8, 8 8, 8 2, 2 2, 2 8))",
                "<Polygon><outerBoundaryIs><LinearRing><coordinates>1,9 9,9 9,1 1,1 1,9</coordinates></LinearRing></outerBoundaryIs><innerBoundaryIs><LinearRing><coordinates>2,8 8,8 8,2 2,2 2,8</coordinates></LinearRing></innerBoundaryIs></Polygon>");
        }

        [TestAttribute]
        public void TestMultiPoint()
        {
            CheckEqual("MULTIPOINT ((1 1), (2 2))",
                "<MultiGeometry><Point><coordinates>1,1</coordinates></Point><Point><coordinates>2,2</coordinates></Point></MultiGeometry>");
        }

        [TestAttribute]
        public void TestMultiLineString()
        {
            CheckEqual("MULTILINESTRING ((2 9, 2 2), (5 5, 8 5))",
                "<MultiGeometry><LineString><coordinates>2,9 2,2</coordinates></LineString><LineString><coordinates>5,5 8,5</coordinates></LineString></MultiGeometry>");
        }

        [TestAttribute]
        public void TestMultiPolygon()
        {
            CheckEqual("MULTIPOLYGON (((2 9, 5 9, 5 5, 2 5, 2 9)), ((6 4, 8 4, 8 2, 6 2, 6 4)))",
                "<MultiGeometry><Polygon><outerBoundaryIs><LinearRing><coordinates>2,9 5,9 5,5 2,5 2,9</coordinates></LinearRing></outerBoundaryIs></Polygon><Polygon><outerBoundaryIs><LinearRing><coordinates>6,4 8,4 8,2 6,2 6,4</coordinates></LinearRing></outerBoundaryIs></Polygon></MultiGeometry>");
        }

        [TestAttribute]
        public void TestGeometryCollection()
        {
            CheckEqual("GEOMETRYCOLLECTION (LINESTRING (1 9, 1 2, 3 2), POLYGON ((3 9, 5 9, 5 7, 3 7, 3 9)), POINT (5 5))",
                "<MultiGeometry><LineString><coordinates>1,9 1,2 3,2</coordinates></LineString><Polygon><outerBoundaryIs><LinearRing><coordinates>3,9 5,9 5,7 3,7 3,9</coordinates></LinearRing></outerBoundaryIs></Polygon><Point><coordinates>5,5</coordinates></Point></MultiGeometry>");
        }

        [TestAttribute]
        public void TestExtrudeAltitudeLineString()
        {
            KMLWriter writer = new KMLWriter { Extrude = true, AltitudeMode = KMLWriter.AltitudeModeAbsolute };
            CheckEqual(writer, "LINESTRING (1 1, 2 2)",
                "<LineString><extrude>1</extrude><altitudeMode>absolute</altitudeMode><coordinates>1,1 2,2</coordinates></LineString>");
        }

        [TestAttribute]
        public void TestExtrudeTesselateLineString()
        {
            KMLWriter writer = new KMLWriter { Extrude = true, Tesselate = true };
            CheckEqual(writer, "LINESTRING (1 1, 2 2)",
                "<LineString><extrude>1</extrude><tesselate>1</tesselate><coordinates>1,1 2,2</coordinates></LineString>");
        }

        [TestAttribute]
        public void TestExtrudeAltitudePolygon()
        {
            KMLWriter writer = new KMLWriter { Extrude = true, AltitudeMode = KMLWriter.AltitudeModeAbsolute };
            CheckEqual(writer, "POLYGON ((1 1, 2 1, 2 2, 1 2, 1 1))",
                "<Polygon><extrude>1</extrude><altitudeMode>absolute</altitudeMode><outerBoundaryIs><LinearRing><coordinates>1,1 2,1 2,2 1,2 1,1</coordinates></LinearRing></outerBoundaryIs></Polygon>");
        }

        [TestAttribute]
        public void TestExtrudeGeometryCollection()
        {
            KMLWriter writer = new KMLWriter { Extrude = true };
            CheckEqual(writer, "GEOMETRYCOLLECTION (LINESTRING (1 9, 1 2, 3 2), POLYGON ((3 9, 5 9, 5 7, 3 7, 3 9)), POINT (5 5))",
                "<MultiGeometry><LineString><extrude>1</extrude><coordinates>1,9 1,2 3,2</coordinates></LineString><Polygon><extrude>1</extrude><outerBoundaryIs><LinearRing><coordinates>3,9 5,9 5,7 3,7 3,9</coordinates></LinearRing></outerBoundaryIs></Polygon><Point><extrude>1</extrude><coordinates>5,5</coordinates></Point></MultiGeometry>");
        }

        [TestAttribute]
        public void TestPrecision()
        {
            KMLWriter writer = new KMLWriter { Precision = 1 };
            CheckEqual(writer, "LINESTRING (1.0001 1.1234, 2.5555 2.99999)",
                " <LineString><coordinates>1,1.1 2.6,3</coordinates></LineString>");
        }

        [TestAttribute]
        public void precision_zero_means_only_integers_allowed()
        {
            KMLWriter writer = new KMLWriter { Precision = 0 };
            CheckEqual(writer, "LINESTRING (1.0001 1.1234, 2.5555 2.99999)",
                " <LineString><coordinates>1,1 3,3</coordinates></LineString>");
        }

        [TestAttribute]
        public void negative_precision_means_floating_precision()
        {
            KMLWriter writer = new KMLWriter { Precision = -1 };
            CheckEqual(writer, "LINESTRING (1.0001 1.1234, 2.5555 2.99999)",
                " <LineString><coordinates>1.0001,1.1234 2.5555,2.99999</coordinates></LineString>");
        }

        private void CheckEqual(string wkt, string expectedKML)
        {
            KMLWriter writer = new KMLWriter();
            CheckEqual(writer, wkt, expectedKML);
        }

        private void CheckEqual(KMLWriter writer, string wkt, string expected)
        {
            IGeometry geom = new WKTReader().Read(wkt);
            CheckEqual(writer, geom, expected);
        }

        private void CheckEqual(KMLWriter writer, IGeometry geom, string expected)
        {
            string actual = writer.Write(geom);
            string actualNorm = normalizeKML(actual);
            string expectedNorm = normalizeKML(expected);
            bool isEqual = String.Equals(actualNorm, expectedNorm, StringComparison.InvariantCultureIgnoreCase);
            Assert.IsTrue(isEqual, String.Format("\nGenerated KML:  {0}\n  Expected KML: {1}", actualNorm, expectedNorm));
        }

        /// <summary>
        /// Normalizes an XML string by converting all whitespace to a single blank char.
        /// </summary>
        private string normalizeKML(string kml)
        {
            string s = kml.Replace("\n", "").Trim();
            // I really need to learn regular expressions :(
            s = s.Replace(">      <", "><");
            s = s.Replace(">     <", "><");
            s = s.Replace(">    <", "><");
            s = s.Replace(">   <", "><");
            s = s.Replace(">  <", "><");
            s = s.Replace("> <", "><");
            return s;
        }
    }
}
#endif
