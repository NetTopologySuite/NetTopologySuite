using NetTopologySuite.Features;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    [TestFixture]
    public class Issue58Fixture
    {
        [Test]
        public void GeoJsonDeserializeFeatureCollectionTest()
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
        ""foo"": {
          ""bar"": ""xyz""
        }
      }
    }
  ]
}
";
            GeoJsonReader reader = new GeoJsonReader();
            FeatureCollection coll = reader.Read<FeatureCollection>(json);
            Assert.IsNotNull(coll);
            Assert.AreEqual(1, coll.Count);
            IFeature feature = coll[0];

            Assert.IsNotNull(feature);
            Assert.AreEqual(1, feature.Attributes.Count);
        }
    }
}