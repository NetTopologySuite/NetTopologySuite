using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using NetTopologySuite.CoordinateSystems;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
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
            GeoJsonSerializer serializer = new GeoJsonSerializer();
            serializer.Serialize(writer, featureCollection);
            writer.Flush();
            Assert.AreEqual("{\"features\":[{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[23.0,56.0]},\"properties\":{\"test1\":\"value1\"}}],\"type\":\"FeatureCollection\",\"crs\":{\"type\":\"name\",\"properties\":{\"name\":\"name1\"}}}", sb.ToString());
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
            GeoJsonSerializer serializer = new GeoJsonSerializer();
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
            GeoJsonSerializer serializer = new GeoJsonSerializer();
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
            GeoJsonSerializer serializer = new GeoJsonSerializer();
            serializer.Serialize(writer, attributes);
            writer.Flush();
            Assert.AreEqual("\"properties\":{\"test1\":\"value1\"}", sb.ToString());
        }
    }
}