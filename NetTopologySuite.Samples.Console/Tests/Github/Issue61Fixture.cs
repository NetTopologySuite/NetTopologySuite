using NetTopologySuite.Features;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    [TestFixture]
    public class Issue61Fixture
    {
        [Test]
        public void geojson_should_serialize_an_array_witn_a_single_item()
        {
            const string json = @"
{
  ""type"": ""FeatureCollection"",
  ""features"": [
    {
      ""type"": ""Feature"",
      ""geometry"": {
        ""type"": ""Point"",
        ""coordinates"": [
          1.0,
          2.0
        ]
      },
      ""properties"": {
        ""foo"": [
            {
              ""zee1"": ""xyz1""
            }
          ]
      }
    }
  ]
}
";
            GeoJsonReader reader = new GeoJsonReader();
            FeatureCollection coll = reader.Read<FeatureCollection>(json);
            Assert.IsNotNull(coll);

            GeoJsonWriter writer = new GeoJsonWriter();
            string s = writer.Write(coll);
            Assert.IsNotNull(s);
        }

        [Test]
        public void geojson_should_serialize_an_array_with_two_items()
        {
            const string json = @"
{
  ""type"": ""FeatureCollection"",
  ""features"": [
    {
      ""type"": ""Feature"",
      ""geometry"": {
        ""type"": ""Point"",
        ""coordinates"": [
          1.0,
          2.0
        ]
      },
      ""properties"": {
        ""foo"": [
            {
              ""zee1"": ""xyz1""
            },
            {
              ""zee2"": ""xyz2"",
              ""zee3"": ""xyz3""
            }
          ]
      }
    }
  ]
}
";
            GeoJsonReader reader = new GeoJsonReader();
            FeatureCollection coll = reader.Read<FeatureCollection>(json);
            Assert.IsNotNull(coll);

            GeoJsonWriter writer = new GeoJsonWriter();
            string s = writer.Write(coll);
            Assert.IsNotNull(s);
        }

        [Test]
        public void geojson_should_serialize_an_array_with_a_nested_array()
        {
            const string json = @"
{
  ""type"": ""FeatureCollection"",
  ""features"": [
    {
      ""type"": ""Feature"",
      ""geometry"": {
        ""type"": ""Point"",
        ""coordinates"": [
          1.0,
          2.0
        ]
      },
      ""properties"": {
        ""foo"": [
            {
              ""zee1"": ""xyz1""
            },
            {
              ""zee2"": ""xyz2"",
              ""zee3"": ""xyz3""
            },
            [
                {
                  ""zee11"": ""xyz11""
                },
                {
                  ""zee22"": ""xyz22"",
                  ""zee33"": ""xyz33""
                }
              ]
          ]
      }
    }
  ]
}
";
            GeoJsonReader reader = new GeoJsonReader();
            FeatureCollection coll = reader.Read<FeatureCollection>(json);
            Assert.IsNotNull(coll);

            GeoJsonWriter writer = new GeoJsonWriter();
            string s = writer.Write(coll);
            Assert.IsNotNull(s);
        }  
    }
}