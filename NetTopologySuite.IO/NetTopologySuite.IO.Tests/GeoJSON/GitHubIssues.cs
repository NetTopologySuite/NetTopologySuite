using System.IO;
using System.Text;
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
        [Test(Description = "Testcase for Issue 83") ]
        public void TestIssue83()
        {
            var geoJson = "{ \"type\": \"Feature\", " + 
                            "\"geometry\": { \"type\": \"Point\", \"coordinates\": [10.0, 60.0] }, " +
                            "\"id\": 1, " +
                            "\"properties\": { \"Name\": \"test\" } }";

            var s = new GeoJsonSerializer();
            Feature f = null;
            Assert.DoesNotThrow( () =>
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
            var e = JToken.Parse(expectedJson);
            var j = JToken.Parse(json);

            if (!JToken.DeepEquals(e, j))
            {
                var res = string.Format("The json's do not match:\n1: {0}\n2:{1}",
                    expectedJson, json);
                Assert.IsTrue(false, res);
            }

        }
    }
}