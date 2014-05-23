using System.IO;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using Newtonsoft.Json;
using NUnit.Framework;

namespace NetTopologySuite.Tests.Various
{
    [TestFixture]
    public class Issue186TestFixture
    {
        [Test, Category("Issue186")]
        public void feature_collection_is_serialized_as_geojson_when_type_is_placed_as_last_property()
        {
            const string data = @"{ ""features"": [], ""type"": ""FeatureCollection"" }";
            JsonSerializer serializer = new GeoJsonSerializer();
            JsonTextReader reader = new JsonTextReader(new StringReader(data));
            FeatureCollection fc = serializer.Deserialize<FeatureCollection>(reader);
            Assert.That(fc, Is.Not.Null);
            Assert.That(fc.Count, Is.EqualTo(0));
        }

        [Test, Category("Issue186")]
        public void feature_collection_is_serialized_as_geojson_when_type_is_placed_as_first_property()
        {
            const string data = @"{ ""type"": ""FeatureCollection"", ""features"": [] }";
            JsonSerializer serializer = new GeoJsonSerializer();
            JsonTextReader reader = new JsonTextReader(new StringReader(data));
            FeatureCollection fc = serializer.Deserialize<FeatureCollection>(reader);
            Assert.That(fc, Is.Not.Null);
            Assert.That(fc.Count, Is.EqualTo(0));
        }

        [Test, Category("Issue186")]
        public void feature_collection_with_crs_is_serialized_as_geojson()
        {
            const string data = @"
{ 
    ""type"": ""FeatureCollection"", 
    ""crs"": {""type"": ""name"", ""properties"": {""name"": ""urn:ogc:def:crs:OGC:1.3:CRS84""}}, 
    ""features"": [] 
}";
            JsonSerializer serializer = new GeoJsonSerializer();
            JsonTextReader reader = new JsonTextReader(new StringReader(data));
            FeatureCollection fc = serializer.Deserialize<FeatureCollection>(reader);
            Assert.That(fc, Is.Not.Null);
            Assert.That(fc.Count, Is.EqualTo(0));
        }

        [Test, Category("Issue186")]
        public void feature_collection_with_arbitrary_properties_is_serialized_as_geojson()
        {
            const string data = @"
{ 
    ""foo1"": ""bar1"",
    ""type"": ""FeatureCollection"",     
    ""foo2"": ""bar2"",
    ""features"": [],
    ""foo3"": ""bar3""
}";
            JsonSerializer serializer = new GeoJsonSerializer();
            JsonTextReader reader = new JsonTextReader(new StringReader(data));
            FeatureCollection fc = serializer.Deserialize<FeatureCollection>(reader);
            Assert.That(fc, Is.Not.Null);
            Assert.That(fc.Count, Is.EqualTo(0));
        }
    }
}