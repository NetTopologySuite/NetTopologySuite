using System;
using System.Globalization;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;

namespace NetTopologySuite.IO.Tests.GeoJSON
{
    [Category("GitHub")]
    [TestFixture]
    public class GitHubIssues
    {
        [Test(Description = "Testcase for Issue 83")]
        public void TestIssue83()
        {
            var geoJson = "{ \"type\": \"Feature\", " +
                            "\"geometry\": { \"type\": \"Point\", \"coordinates\": [10.0, 60.0] }, " +
                            "\"id\": 1, " +
                            "\"properties\": { \"Name\": \"test\" } }";

            var s = new GeoJsonSerializer();
            Feature f = null;
            Assert.DoesNotThrow(() =>
                f = s.Deserialize<Feature>(new JsonTextReader(new StringReader(geoJson)))
                );

            Assert.IsNotNull(f, "f != null");
            Assert.IsTrue(FeatureExtensions.HasID(f), "f.HasID()");
            Assert.AreEqual(1, FeatureExtensions.ID(f), "f.ID != 1");

            var sb = new StringBuilder();
            var tw = new JsonTextWriter(new StringWriter(sb));
            s.Serialize(tw, f);
            var geoJsonRes = sb.ToString();

            CompareJson(geoJson, geoJsonRes);
        }

        public void CompareJson(string expectedJson, string json)
        {
            JToken e = JToken.Parse(expectedJson);
            JToken j = JToken.Parse(json);
            if (!JToken.DeepEquals(e, j))
                Assert.Fail("The json's do not match:\n1: {0}\n2:{1}", expectedJson, json);
        }


        [Test(Description = "Testcase for Issue 88")]
        public void TestIssue88WithoutAdditionalProperties()
        {
            string expectedJson = @"
{
	""id"" : ""00000000-0000-0000-0000-000000000000"",
	""type"" : ""Feature"",
	""geometry"" : null,
	""properties"" : {
		""yesNo 1"" : false,
		""date 1"" : ""2016-02-16T00:00:00""
	}
}
";

            GeoJsonSerializer serializer = new GeoJsonSerializer();
            Feature feat = null;
            Assert.DoesNotThrow(() =>
            {
                using (JsonReader reader = new JsonTextReader(new StringReader(expectedJson)))
                    feat = serializer.Deserialize<Feature>(reader);
            });

            Assert.IsNotNull(feat);
            Assert.IsTrue(FeatureExtensions.HasID(feat));
            Assert.AreEqual("00000000-0000-0000-0000-000000000000", FeatureExtensions.ID(feat));
            IGeometry geometry = feat.Geometry;
            Assert.IsNull(geometry);
            IAttributesTable attributes = feat.Attributes;
            Assert.IsNotNull(attributes);
            Assert.AreEqual(3, attributes.Count);
            Assert.AreEqual(FeatureExtensions.ID(feat), attributes["id"]);
            Assert.AreEqual(false, attributes["yesNo 1"]);
            Assert.AreEqual(DateTime.Parse("2016-02-16T00:00:00", CultureInfo.InvariantCulture), attributes["date 1"]);
        }

        [Test(Description = "Testcase for Issue 88")]
        public void TestIssue88WithFlatProperties()
        {
            string expectedJson = @"
{
	""id"" : ""00000000-0000-0000-0000-000000000000"",
	""type"" : ""Feature"",
	""geometry"" : null,
	""properties"" : {
		""yesNo 1"" : false,
		""date 1"" : ""2016-02-16T00:00:00""
	},	
	""collection_userinfo"" : ""()"",
	""collection_timestamp"" : ""2016-02-25T14:38:01.9087672"",
	""collection_todoid"" : """",
	""collection_templateid"" : ""nj7Glv-AqV0"",
	""collection_layerid"" : ""nj7Glv-AqV0""	
}
";

            GeoJsonSerializer serializer = new GeoJsonSerializer();
            Feature feat = null;
            Assert.DoesNotThrow(() =>
            {
                using (JsonReader reader = new JsonTextReader(new StringReader(expectedJson)))
                    feat = serializer.Deserialize<Feature>(reader);
            });

            Assert.IsNotNull(feat);
            Assert.IsNull(feat.Geometry);
        }

        [Test(Description = "Testcase for Issue 88")]
        public void TestIssue88()
        {
            string expectedJson = @"
{
	""id"" : ""00000000-0000-0000-0000-000000000000"",
	""type"" : ""Feature"",
	""geometry"" : null,
	""properties"" : {
		""yesNo 1"" : false,
		""date 1"" : ""2016-02-16T00:00:00""
	},
	""metadata"" : {
		""collection_userinfo"" : ""()"",
		""collection_timestamp"" : ""2016-02-25T14:38:01.9087672"",
		""collection_todoid"" : """",
		""collection_templateid"" : ""nj7Glv-AqV0"",
		""collection_layerid"" : ""nj7Glv-AqV0""
	}
}
";

            GeoJsonSerializer serializer = new GeoJsonSerializer();
            Feature feat = null;
            Assert.DoesNotThrow(() =>
            {
                using (JsonReader reader = new JsonTextReader(new StringReader(expectedJson)))
                    feat = serializer.Deserialize<Feature>(reader);
            });

            Assert.IsNotNull(feat);
            Assert.IsNull(feat.Geometry);
        }

        private Feature _feature;
        private GeoJsonReader _reader;
        private GeoJsonWriter _writer;

        [TestFixtureSetUp]
        public void GivenAGeoJsonReaderAndWriter()
        {
            _reader = new GeoJsonReader();
            _writer = new GeoJsonWriter();
            var geometry = new Point(1, 2);
            _feature = new Feature(geometry, new AttributesTable());
        }

        [Category("GitHub Issue")]
        [Test(Description = "Testcase for GitHub Issue 92")]
        public void WhenArrayOfJsonObjectArraysPropertyInGeoJsonThenReadable()
        {
            const string geojsonString = @"
{
  ""type"":""FeatureCollection"",
  ""features"":[
    {
      ""type"":""Feature"",
      ""geometry"":{
        ""type"":""Point"",
        ""coordinates"":[1.0,2.0]
      },
      ""properties"":{
        ""foo"":[
          {
            ""xyz"":[
                {""zee"":""xyz""},
                {""hay"":""zus""}
            ]
          }
        ]
      }
    }
  ]
}
";
            var featureCollection = _reader.Read<FeatureCollection>(geojsonString);
            Assert.AreEqual(1, featureCollection.Count);
        }

        [Category("GitHub Issue")]
        [Test(Description = "Testcase for GitHub Issue 92")]
        public void WhenArrayOfJsonObjectArraysPropertyInGeoJsonThenWriteable()
        {
            const string geojsonString = @"
{
  ""type"":""FeatureCollection"",
  ""features"":[
    {
      ""type"":""Feature"",
      ""geometry"":{
        ""type"":""Point"",
        ""coordinates"":[1.0,2.0]
      },
      ""properties"":{
        ""foo"":[
          {
            ""xyz"":[
                {""zee"":""xyz""},
                {""hay"":""zus""}
            ]
          }
        ]
      }
    }
  ]
}
";
            var featureCollection = _reader.Read<FeatureCollection>(geojsonString);
            var written = _writer.Write(featureCollection);
            Assert.IsTrue(written.Contains("FeatureCollection"));
        }

        [Category("GitHub Issue")]
        [Test(Description = "Testcase for GitHub Issue 92")]
        public void WhenArrayOfIntArraysPropertyInFeatureThenWritable()
        {
            var arrayOfInts = new[] { 1, 2, 3 };
            var anotherArrayOfInts = new[] { 4, 5, 6 };
            var arrayOfArrays = new[] { arrayOfInts, anotherArrayOfInts };
            _feature.Attributes.AddAttribute("foo", arrayOfArrays);
            var written = _writer.Write(_feature);
            Assert.IsTrue(written.Contains("Feature"));
        }

        [Category("GitHub Issue")]
        [Test(Description = "Testcase for GitHub Issue 92")]
        public void WhenArrayOfIntArraysPropertyInGeoJsonThenReadable()
        {
            const string geojsonString = @"
{
    ""type"":""Feature"",
    ""geometry"":{
        ""type"":""Point"",""coordinates"":[1.0,2.0]},
        ""properties"":{
            ""foo"":[[1,2,3],[4,5,6]]
        }
}
";
            var featureCollection = _reader.Read<Feature>(geojsonString);
            CompareJson(geojsonString, _writer.Write(featureCollection));
        }

        [Category("GitHub Issue")]
        [Test(Description = "Testcase for GitHub Issue 92")]
        public void WhenPopulatedIntArrayPropertyInGeoJsonThenReadable()
        {
            const string geojsonString = @"
{
  ""type"": ""FeatureCollection"",
  ""features"": [
    {
      ""type"": ""Feature"",
      ""geometry"": {
        ""type"": ""Point"",
        ""coordinates"": [1.0,2.0]
      },
      ""properties"": {
        ""foo"": [1, 2]
      }
    }
  ]
}
";
            var featureCollection = _reader.Read<FeatureCollection>(geojsonString);
            Assert.AreEqual(1, featureCollection.Count);
        }

        [Category("GitHub Issue")]
        [Test(Description = "Testcase for GitHub Issue 93")]
        public void WhenEmptyArrayPropertyInGeoJsonThenReadable()
        {
            const string geojsonString = @"
{
  ""type"":""FeatureCollection"",
  ""features"":[
    {
      ""type"":""Feature"",
      ""geometry"":{
        ""type"":""Point"",
        ""coordinates"":[1.0,2.0]
      },
      ""properties"":{
        ""foo"":[]
      }
    }
  ]
}
";
            var featureCollection = new GeoJsonReader().Read<FeatureCollection>(geojsonString);
            Assert.AreEqual(1, featureCollection.Count);
        }

        [Category("GitHub Issue")]
        [Test(Description = "Testcase for GitHub Issue 95, FeatureCollection having \"bbox\" property")]
        public void Test()
        {
            const string geoJsonString = "{ \"type\":\"FeatureCollection\", \"features\":[ { \"geometry\":{ \"type\":\"Point\", \"coordinates\":[ -6.091861724853516, 4.991835117340088 ] }, \"type\":\"Feature\", \"properties\":{ \"prop1\":[ \"a\", \"b\" ] }, \"id\":1 } ], \"bbox\":[ -8.599302291870117, 4.357067108154297, -2.494896650314331, 10.736641883850098 ] }";
            var featureCollection = new GeoJsonReader().Read<FeatureCollection>(geoJsonString);
            Assert.AreEqual(1, featureCollection.Count);
        }
    }
}