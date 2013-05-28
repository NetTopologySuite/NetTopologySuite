using System;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace NetTopologySuite.IO.Tests.GeoJSON
{
    [TestFixture]
    public class Issue148
    {
        readonly IGeometryFactory factory = GeometryFactory.Default;

        private static IGeometry[] geometries;
        private static IGeometryCollection collection;
        private static string serializedGeometries;
        private static string serializedCollection;

        [SetUp]
        public void SetUp()
        {
            IPoint point = factory.CreatePoint(new Coordinate(1, 1));
            ILineString linestring = factory.CreateLineString(new[] { new Coordinate(1, 1), new Coordinate(2, 2), new Coordinate(3, 3), });
            ILinearRing shell = factory.CreateLinearRing(new[] { new Coordinate(1, 1), new Coordinate(2, 2), new Coordinate(3, 3), new Coordinate(1, 1) });
            IPolygon polygon = factory.CreatePolygon(shell);
            geometries = new IGeometry[] { point, linestring, polygon };
            serializedGeometries = "\"geometries\":[{\"type\":\"Point\",\"coordinates\":[1.0,1.0]},{\"type\":\"LineString\",\"coordinates\":[[1.0,1.0],[2.0,2.0],[3.0,3.0]]},{\"type\":\"Polygon\",\"coordinates\":[[[1.0,1.0],[2.0,2.0],[3.0,3.0],[1.0,1.0]]]}]";

            collection = factory.CreateGeometryCollection(geometries);
            serializedCollection = "{\"type\":\"GeometryCollection\",\"geometries\":[{\"type\":\"Point\",\"coordinates\":[1.0,1.0]},{\"type\":\"LineString\",\"coordinates\":[[1.0,1.0],[2.0,2.0],[3.0,3.0]]},{\"type\":\"Polygon\",\"coordinates\":[[[1.0,1.0],[2.0,2.0],[3.0,3.0],[1.0,1.0]]]}]}";
        }
        
        [Test]
        public void serialize_an_array_of_geometries_should_return_a_json_fragment()
        {
            StringBuilder sb = new StringBuilder();
            JsonTextWriter writer = new JsonTextWriter(new StringWriter(sb));
            JsonSerializer serializer = new GeoJsonSerializer(factory);
            serializer.Serialize(writer, geometries);
            string actual = sb.ToString();
            Console.WriteLine(actual);
            Assert.That(actual, Is.EqualTo(serializedGeometries));            
        }

        [Test]
        public void serialize_a_geometrycollection_should_return_a_valid_json()
        {
            StringBuilder sb = new StringBuilder();
            JsonTextWriter writer = new JsonTextWriter(new StringWriter(sb));
            JsonSerializer serializer = new GeoJsonSerializer(factory);
            serializer.Serialize(writer, collection);
            string actual = sb.ToString();
            Console.WriteLine(actual);
            Assert.That(actual, Is.EqualTo(serializedCollection));
        }

        [Test, ExpectedException(typeof(JsonReaderException))]
        public void deserialize_a_json_fragment_should_throws_an_error()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(serializedGeometries));
            JsonSerializer serializer = new GeoJsonSerializer(factory);
            IGeometry[] actual = serializer.Deserialize<IGeometry[]>(reader);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(geometries));
        }

        [Test]
        public void deserialize_a_valid_json_should_return_a_geometrycollection()
        {
            JsonTextReader reader = new JsonTextReader(new StringReader(serializedCollection));
            JsonSerializer serializer = new GeoJsonSerializer(factory);
            IGeometryCollection actual = serializer.Deserialize<GeometryCollection>(reader);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual.EqualsExact(collection), Is.True);
        }

        [Test]
        public void howto_serialize_geometries()
        {
            GeoJsonWriter writer = new GeoJsonWriter();
            string actual = writer.Write(collection);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(serializedCollection));
        }

         [Test]
         public void howto_deserialize_geometries()
         {
             GeoJsonReader reader = new GeoJsonReader();
             IGeometry actual = reader.Read<GeometryCollection>(serializedCollection);
             Assert.That(actual, Is.Not.Null);
             Assert.That(actual.EqualsExact(collection), Is.True);
         }
    }
}
