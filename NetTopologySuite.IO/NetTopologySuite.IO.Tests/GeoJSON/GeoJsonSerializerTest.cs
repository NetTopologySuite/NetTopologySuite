using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using NetTopologySuite.CoordinateSystems;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace NetTopologySuite.IO.Tests.GeoJSON
{
    ///<summary>
    ///    This is a test class for GeoJsonSerializerTest and is intended
    ///    to contain all GeoJsonSerializerTest Unit Tests
    ///</summary>
    [TestFixture]
    public class GeoJsonSerializerTest
    {        
        ///<summary>
        ///    A test for GeoJsonSerializer Serialize method
        ///</summary>
        [Test]
        public void GeoJsonSerializerFeatureCollectionTest()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            AttributesTable attributes = new AttributesTable();
            attributes.AddAttribute("test1", "value1");
            IFeature feature = new Feature(new Point(23, 56), attributes);
            FeatureCollection featureCollection = new FeatureCollection(new Collection<IFeature> {feature})
                                        {CRS = new NamedCRS("name1")};
            JsonSerializer serializer = new GeoJsonSerializer();
            serializer.Serialize(writer, featureCollection);
            writer.Flush();
            Assert.AreEqual("{\"type\":\"FeatureCollection\",\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[23.0,56.0]},\"properties\":{\"test1\":\"value1\"}}],\"crs\":{\"type\":\"name\",\"properties\":{\"name\":\"name1\"}}}", sb.ToString());
        }

        ///<summary>
        ///    A test for GeoJsonSerializer Serialize method
        ///</summary>
        [Test]
        public void GeoJsonSerializerFeatureTest()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            AttributesTable attributes = new AttributesTable();
            attributes.AddAttribute("test1", "value1");
            IFeature feature = new Feature(new Point(23, 56), attributes);
            JsonSerializer serializer = new GeoJsonSerializer();
            serializer.Serialize(writer, feature);
            writer.Flush();
            Assert.AreEqual("{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[23.0,56.0]},\"properties\":{\"test1\":\"value1\"}}", sb.ToString());
        }

        ///<summary>
        ///    A test for GeoJsonSerializer Serialize method
        ///</summary>
        [Test]
        public void GeoJsonSerializerGeometryTest()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            JsonSerializer serializer = new GeoJsonSerializer();
            serializer.Serialize(writer, new Point(23, 56));
            writer.Flush();
            Assert.AreEqual("{\"type\":\"Point\",\"coordinates\":[23.0,56.0]}", sb.ToString());
        }

        ///<summary>
        ///    A test for GeoJsonSerializer Serialize method
        ///</summary>
        [Test]
        public void GeoJsonSerializerAttributesTest()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);
            AttributesTable attributes = new AttributesTable();
            attributes.AddAttribute("test1", "value1");
            JsonSerializer serializer = new GeoJsonSerializer();
            serializer.Serialize(writer, attributes);
            writer.Flush();
            Assert.AreEqual("{\"test1\":\"value1\"}", sb.ToString());
        }

        /// <summary>
        /// test for a real life failure
        /// </summary>
        [Test]
        public void GeoJsonDeserializePolygonTest()
        {
            StringBuilder sb = new StringBuilder();
            JsonSerializer serializer = new GeoJsonSerializer();

            var s = "{\"type\": \"Polygon\",\"coordinates\": [[[-180,-67.8710140098964],[-180,87.270282879440586],[180,87.270282879440586],[180,-67.8710140098964],[-180,-67.8710140098964]]],\"rect\": {\"min\": [-180,-67.8710140098964],\"max\": [180,87.270282879440586]}}";
            var poly = serializer.Deserialize<Polygon>(JObject.Parse(s).CreateReader());


        }
    }
}