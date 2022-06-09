using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.IO.KML;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.IO.KML
{
    public class KMLReaderTest
    {
        private readonly KMLReader _kmlReader = new KMLReader(new[] {"altitudeMode", "tesselate", "extrude"});
        private readonly KMLReader _kmlReaderNoAtt = new KMLReader();


        [Test]
        public void TestPoint()
        {
            CheckParsingResult(_kmlReader, "<Point><altitudeMode>absolute</altitudeMode><coordinates>1.0,1.0</coordinates></Point>",
                "POINT (1 1)",
                new IDictionary<string, string>[] {new Dictionary<string, string>(new[] {new KeyValuePair<string, string>("altitudeMode", "absolute")})});
            CheckParsingResult(_kmlReaderNoAtt, "<Point><altitudeMode>absolute</altitudeMode><coordinates>1.0,1.0</coordinates></Point>",
                "POINT (1 1)",
                new IDictionary<string, string>[] { (Dictionary<string, string>)null });
        }

        [Test]
        public void TestLineString()
        {
            CheckParsingResult(_kmlReader,
                "<LineString><tesselate>1</tesselate><coordinates>1.0,1.0 2.0,2.0</coordinates></LineString>",
                "LINESTRING (1 1, 2 2)",
                new IDictionary<string, string>[] {new Dictionary<string, string>(new[] {new KeyValuePair<string, string>("tesselate", "1")})});
            CheckParsingResult(_kmlReaderNoAtt,
                "<LineString><tesselate>1</tesselate><coordinates>1.0,1.0 2.0,2.0</coordinates></LineString>",
                "LINESTRING (1 1, 2 2)",
                new IDictionary<string, string>[] { null });
        }

        [Test]
        public void TestPolygon()
        {
            CheckParsingResult(_kmlReader,
                "<Polygon>" +
                "   <altitudeMode>relativeToGround</altitudeMode>" +
                "   <outerBoundaryIs>" +
                "       <LinearRing>" +
                "           <coordinates>1.0,1.0 1.0,10.0 10.0,10.0 10.0,1.0 1.0,1.0</coordinates>" +
                "       </LinearRing>" +
                "   </outerBoundaryIs>" +
                "   <innerBoundaryIs>" +
                "       <LinearRing>" +
                "           <coordinates>2.0,2.0 2.0,3.0 3.0,3.0 3.0,2.0 2.0,2.0</coordinates>" +
                "       </LinearRing>" +
                "   </innerBoundaryIs>" +
                "   <innerBoundaryIs>" +
                "       <LinearRing>" +
                "           <coordinates>6.0,6.0 6.0,7.0 7.0,7.0 7.0,6.0 6.0,6.0</coordinates>" +
                "       </LinearRing>" +
                "   </innerBoundaryIs>" +
                "</Polygon>",
                "POLYGON ((1 1, 1 10, 10 10, 10 1, 1 1), (2 2, 2 3, 3 3, 3 2, 2 2), (6 6, 6 7, 7 7, 7 6, 6 6))",
                new IDictionary<string, string>[] {new Dictionary<string, string>(new[]
                    {new KeyValuePair<string, string>("altitudeMode", "relativeToGround")})});
            CheckParsingResult(_kmlReaderNoAtt,
                "<Polygon>" +
                "   <altitudeMode>relativeToGround</altitudeMode>" +
                "   <outerBoundaryIs>" +
                "       <LinearRing>" +
                "           <coordinates>1.0,1.0 1.0,10.0 10.0,10.0 10.0,1.0 1.0,1.0</coordinates>" +
                "       </LinearRing>" +
                "   </outerBoundaryIs>" +
                "   <innerBoundaryIs>" +
                "       <LinearRing>" +
                "           <coordinates>2.0,2.0 2.0,3.0 3.0,3.0 3.0,2.0 2.0,2.0</coordinates>" +
                "       </LinearRing>" +
                "   </innerBoundaryIs>" +
                "   <innerBoundaryIs>" +
                "       <LinearRing>" +
                "           <coordinates>6.0,6.0 6.0,7.0 7.0,7.0 7.0,6.0 6.0,6.0</coordinates>" +
                "       </LinearRing>" +
                "   </innerBoundaryIs>" +
                "</Polygon>",
                "POLYGON ((1 1, 1 10, 10 10, 10 1, 1 1), (2 2, 2 3, 3 3, 3 2, 2 2), (6 6, 6 7, 7 7, 7 6, 6 6))",
                new IDictionary<string, string>[] { null });
        }

        [Test]
        public void TestMultiGeometry()
        {
            CheckParsingResult(_kmlReader,
                "<MultiGeometry>" +
                "   <Point>" +
                "       <altitudeMode>absolute</altitudeMode>" +
                "       <coordinates>1.0,1.0</coordinates>" +
                "   </Point>" +
                "   <LineString>" +
                "       <tesselate>1</tesselate>" +
                "       <coordinates>1.0,1.0 2.0,2.0</coordinates>" +
                "   </LineString>" +
                "   <Polygon>" +
                "       <altitudeMode>relativeToGround</altitudeMode>" +
                "       <outerBoundaryIs>" +
                "           <LinearRing>" +
                "               <coordinates>1.0,1.0 1.0,10.0 10.0,10.0 10.0,1.0 1.0,1.0</coordinates>" +
                "           </LinearRing>" +
                "       </outerBoundaryIs>" +
                "   </Polygon>" +
                "</MultiGeometry>",
                "GEOMETRYCOLLECTION (POINT (1 1), LINESTRING (1 1, 2 2), POLYGON ((1 1, 1 10, 10 10, 10 1, 1 1)))",
                new IDictionary<string, string>[] {
                    new Dictionary<string, string>(new[] { new KeyValuePair<string, string>("altitudeMode", "absolute") }),
                    new Dictionary<string, string>(new[] { new KeyValuePair<string, string>("tesselate", "1") }),
                    new Dictionary<string, string>(new[] { new KeyValuePair<string, string>("altitudeMode", "relativeToGround") })
                });
            CheckParsingResult(_kmlReaderNoAtt,
                "<MultiGeometry>" +
                "   <Point>" +
                "       <altitudeMode>absolute</altitudeMode>" +
                "       <coordinates>1.0,1.0</coordinates>" +
                "   </Point>" +
                "   <LineString>" +
                "       <tesselate>1</tesselate>" +
                "       <coordinates>1.0,1.0 2.0,2.0</coordinates>" +
                "   </LineString>" +
                "   <Polygon>" +
                "       <altitudeMode>relativeToGround</altitudeMode>" +
                "       <outerBoundaryIs>" +
                "           <LinearRing>" +
                "               <coordinates>1.0,1.0 1.0,10.0 10.0,10.0 10.0,1.0 1.0,1.0</coordinates>" +
                "           </LinearRing>" +
                "       </outerBoundaryIs>" +
                "   </Polygon>" +
                "</MultiGeometry>",
                "GEOMETRYCOLLECTION (POINT (1 1), LINESTRING (1 1, 2 2), POLYGON ((1 1, 1 10, 10 10, 10 1, 1 1)))",
                new IDictionary<string, string>[] { null, null, null });
        }

        [Test]
        public void TestMultiGeometryWithAllPoints()
        {
            CheckParsingResult(_kmlReader,
                "<MultiGeometry>" +
                "   <Point><coordinates>1.0,1.0</coordinates></Point>" +
                "   <Point><coordinates>2.0,2.0</coordinates></Point>" +
                "</MultiGeometry>",
                "MULTIPOINT ((1 1), (2 2))",
                new IDictionary<string, string>[] { null, null }
            );
        }

        [Test]
        public void TestMultiGeometryWithAllLines()
        {
            CheckParsingResult(_kmlReader,
                "<MultiGeometry>" +
                "   <LineString><coordinates>1.0,1.0 2.0,2.0</coordinates></LineString>" +
                "   <LineString><coordinates>5.0,5.0 6.0,6.0</coordinates></LineString>" +
                "</MultiGeometry>",
                "MULTILINESTRING ((1 1, 2 2), (5 5, 6 6))",
                new IDictionary<string, string>[] { null, null }
            );
        }

        [Test]
        public void TestMultiGeometryWithAllPolygons()
        {
            CheckParsingResult(_kmlReader,
                "<MultiGeometry>" +
                "   <Polygon><outerBoundaryIs><LinearRing><coordinates>2.0,2.0 2.0,3.0 3.0,3.0 3.0,2.0 2.0,2.0</coordinates></LinearRing></outerBoundaryIs></Polygon>" +
                "   <Polygon><outerBoundaryIs><LinearRing><coordinates>6.0,6.0 6.0,7.0 7.0,7.0 7.0,6.0 6.0,6.0</coordinates></LinearRing></outerBoundaryIs></Polygon>" +
                "</MultiGeometry>",
                "MULTIPOLYGON (((2 2, 2 3, 3 3, 3 2, 2 2)), ((6 6, 6 7, 7 7, 7 6, 6 6)))",
                new IDictionary<string, string>[]{ null, null }
            );
        }

        [Test]
        public void TestZ()
        {
            string kml = "<Point><coordinates>1.0,1.0,50.0</coordinates></Point>";
            var kmlReader = new KMLReader();
            try
            {
                var parsedGeometry = kmlReader.Read(kml);
                Assert.AreEqual(50.0, parsedGeometry.Coordinate.Z, "Wrong Z");
            }
            catch (ParseException e)
            {
                throw new AssertionException($"ParseException: {e.Message}", e);
            }
        }

        [Test]
        public void TestPrecisionAndSRID()
        {
            string kml = "<Point><altitudeMode>absolute</altitudeMode><coordinates>1.385093,1.436456</coordinates></Point>";
            var geometryFactory = new GeometryFactory(new PrecisionModel(1000.0), 4326);
            var kmlReader = new KMLReader(geometryFactory);
            try
            {
                var parsedGeometry = kmlReader.Read(kml);
                Assert.AreEqual(geometryFactory.SRID, parsedGeometry.SRID, "Wrong SRID");
                Assert.AreEqual("POINT (1.385 1.436)", parsedGeometry.AsText(), "Wrong precision");
            }
            catch (ParseException e)
            {
                throw new AssertionException($"ParseException: {e.Message}", e);
            }
        }

        [Test]
        public void TestCoordinatesErrors()
        {
            CheckExceptionThrown("<Point></Point>", "No element coordinates found in Point");
            CheckExceptionThrown("<Point><coordinates></coordinates></Point>", "Empty coordinates");
            CheckExceptionThrown("<Point><coordinates>1.0</coordinates></Point>", "Invalid coordinate format");
            CheckExceptionThrown("<Point><coordinates>,1.0</coordinates></Point>", "Invalid coordinate format");
            CheckExceptionThrown("<Point><coordinates>1.0,</coordinates></Point>", "Invalid coordinate format");

            CheckExceptionThrown("<Polygon></Polygon>", "No outer boundary for Polygon");
            CheckExceptionThrown("<Polygon><outerBoundaryIs><LinearRing></LinearRing></outerBoundaryIs></Polygon>",
                "No element coordinates found in outerBoundaryIs");
            CheckExceptionThrown("<Polygon><innerBoundaryIs><LinearRing></LinearRing></innerBoundaryIs></Polygon>",
                "No element coordinates found in innerBoundaryIs");
        }

        [Test]
        public void TestUnknownGeometryType()
        {
            CheckExceptionThrown("<StrangePoint></StrangePoint>", "Unknown KML geometry type StrangePoint");
        }

        [Test]
        public void TestPolygonIssue594()
        {
            CheckParsingResult(_kmlReaderNoAtt,
                @"<Polygon>
      <extrude>1</extrude>
      <altitudeMode>relativeToGround</altitudeMode>
      <outerBoundaryIs>
        <LinearRing>
          <coordinates>-122.366278,37.818844,30 -122.365248,37.819267,30 -122.365640,37.819861,30 -122.366669,37.819429,30 -122.366278,37.818844,30</coordinates>
        </LinearRing>
      </outerBoundaryIs>
    </Polygon>",
           "POLYGON Z((-122.366278 37.818844 30, -122.365248 37.819267 30, -122.36564 37.819861 30, -122.366669 37.819429 30, -122.366278 37.818844 30))",
           new IDictionary<string, string>[] { null }
           );
        }
       
        [Test]
        public void TestCoordinatesWithWhitespace()
        {
            CheckParsingResult(new KMLReader(),
                "<LineString>" +
                "   <coordinates> 1.0,2.0" +
                "   -1.0,-2.0 </coordinates>" +
                "</LineString>",
                "LINESTRING (1 2, -1 -2)",
                new IDictionary<string, string>[]{ null, null }
            );
        }

        private void CheckExceptionThrown(string kmlString, string expectedError)
        {
            try
            {
                _kmlReader.Read(kmlString);
                Assert.Fail("Exception must be thrown");
            }
            catch (ParseException e)
            {
                Assert.AreEqual(expectedError, e.Message, "Exception text differs");
            }
        }

        private void CheckParsingResult(KMLReader kmlReader, string kmlString, string expectedWKT,
            IDictionary<string, string>[] expectedAttributes)
        {
            try
            {
                var parsedGeometry = kmlReader.Read(kmlString);
                string wkt = parsedGeometry.AsText();

                Assert.AreEqual(expectedWKT, wkt, "WKTs are not equal");

                for (int i = 0; i < parsedGeometry.NumGeometries; i++)
                {
                    var geometryN = parsedGeometry.GetGeometryN(i);
                    Assert.IsTrue(geometryN.UserData != null || expectedAttributes[i] == null, "User data is not filled");

                    if (geometryN.UserData != null)
                    {
                        var actualUserData = (IDictionary<string, string>) geometryN.UserData;
                        Assert.AreEqual(expectedAttributes[i].Count,
                            actualUserData.Count, "Number of attributes differs in user data");

                        foreach (var entry in expectedAttributes[i])
                        {
                            Assert.IsTrue(actualUserData.ContainsKey(entry.Key), $"User data has not attribute {entry.Key}");
                            Assert.AreEqual(entry.Value, actualUserData[entry.Key], "Attribute value differs");
                        }
                    }
                }
            }
            catch (ParseException e)
            {
                throw new AssertionException($"ParseException: {e.Message}", e);
            }
        }
    }
}
