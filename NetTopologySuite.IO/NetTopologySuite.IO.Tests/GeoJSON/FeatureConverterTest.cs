using System;
using System.IO;
using System.Text;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Converters;
using Newtonsoft.Json;
using NUnit.Framework;

namespace NetTopologySuite.IO.Tests.GeoJSON
{
    ///<summary>
    ///    This is a test class for FeatureConverterTest and is intended
    ///    to contain all FeatureConverterTest Unit Tests
    ///</summary>
    [TestFixture]
    public class FeatureConverterTest
    {
        ///<summary>
        ///    A test for CanConvert
        ///</summary>
        [Test]
        public void CanConvertTest()
        {
            FeatureConverter target = new FeatureConverter();
            Type objectType = typeof(Feature);
            const bool expected = true;
            bool actual = target.CanConvert(objectType);
            Assert.AreEqual(expected, actual);
        }

        ///<summary>
        ///    A test for WriteJson
        ///</summary>
        [Test]
        public void WriteJsonTest()
        {
            FeatureConverter target = new FeatureConverter();
            StringBuilder sb = new StringBuilder();
            JsonTextWriter writer = new JsonTextWriter(new StringWriter(sb));
            AttributesTable attributes = new AttributesTable();
            attributes.AddAttribute("test1", "value1");
            IFeature value = new Feature(new Point(23, 56), attributes);
            GeoJsonSerializer serializer = new GeoJsonSerializer();
            target.WriteJson(writer, value, serializer);
            writer.Flush();
            Assert.AreEqual("{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[23.0,56.0]},\"properties\":{\"test1\":\"value1\"}}", sb.ToString());
        }

        ///<summary>
        ///    A test for WriteJson
        ///</summary>
        [Test]
        public void WriteJsonWithArrayTest()
        {
            FeatureConverter target = new FeatureConverter();
            StringBuilder sb = new StringBuilder();
            JsonTextWriter writer = new JsonTextWriter(new StringWriter(sb));
            AttributesTable attributes = new AttributesTable();
            attributes.AddAttribute("test1", new [] { "value1", "value2" });
            IFeature value = new Feature(new Point(23, 56), attributes);
            GeoJsonSerializer serializer = new GeoJsonSerializer();
            target.WriteJson(writer, value, serializer);
            writer.Flush();
            Assert.AreEqual("{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[23.0,56.0]},\"properties\":{\"test1\":[\"value1\",\"value2\"]}}", sb.ToString());
        }
    }
}