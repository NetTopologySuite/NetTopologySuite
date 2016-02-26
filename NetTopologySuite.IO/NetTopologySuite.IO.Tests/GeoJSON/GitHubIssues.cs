using System;
using System.Globalization;
using System.IO;
using System.Text;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
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
            Assert.IsTrue(f.HasID(), "f.HasID()");
            Assert.AreEqual(1, f.ID(), "f.ID != 1");

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
            Assert.IsTrue(feat.HasID());
            Assert.AreEqual("00000000-0000-0000-0000-000000000000", feat.ID());
            IGeometry geometry = feat.Geometry;
            Assert.IsNull(geometry);
            IAttributesTable attributes = feat.Attributes;
            Assert.IsNotNull(attributes);
            Assert.AreEqual(3, attributes.Count);
            Assert.AreEqual(feat.ID(), attributes["id"]);
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
    }
}