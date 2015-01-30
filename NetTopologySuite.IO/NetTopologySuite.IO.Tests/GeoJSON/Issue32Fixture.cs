using NetTopologySuite.Features;
using NUnit.Framework;

namespace NetTopologySuite.IO.Tests.GeoJSON
{
    /// <summary>
    /// see: https://github.com/NetTopologySuite/NetTopologySuite/issues/32
    /// </summary>
    [TestFixture]
    public class Issue32Fixture
    {
        [Test]
        public void geojson_feature_with_fid_should_be_serialized()
        {
            const string json = @"
{
	""type"" : ""FeatureCollection"",
	""totalFeatures"" : 5,
	""features"" : [{
			""type"" : ""Feature"",
			""id"" : ""vfs_kommun.256"",
			""geometry"" : null,
			""properties"" : {
				""kommunnamn"" : ""Karlskrona""
			}
		}
	]
}
";
            GeoJsonReader reader = new GeoJsonReader();
            FeatureCollection coll = reader.Read<FeatureCollection>(json);
            Assert.That(coll, Is.Not.Null);
            Assert.That(coll.Count, Is.EqualTo(1));

            IFeature feature = coll[0];
            Assert.That(feature.Geometry, Is.Null);
            Assert.That(feature.Attributes, Is.Not.Null);
            Assert.That(feature.Attributes.Exists("id"), Is.True);
            Assert.That(feature.Attributes["id"], Is.EqualTo("vfs_kommun.256"));
        }

        [Test]
        public void geojson_feature_with_null_crs_should_be_serialized()
        {
            const string json = @"
{
	""type"" : ""FeatureCollection"",
	""totalFeatures"" : 5,
	""features"" : [{
			""type"" : ""Feature"",
			""geometry"" : null,
			""properties"" : {
				""kommunnamn"" : ""Karlskrona""
			}
		}
	],
	""crs"" : null
}
";
            GeoJsonReader reader = new GeoJsonReader();
            FeatureCollection coll = reader.Read<FeatureCollection>(json);
            Assert.That(coll, Is.Not.Null);
            Assert.That(coll.CRS, Is.Null);
            Assert.That(coll.Count, Is.EqualTo(1));             
        }
        
        [Test]
        public void valid_geojson_feature_should_be_serialized()
        {
            const string json = @"
{
	""type"" : ""FeatureCollection"",
	""totalFeatures"" : 5,
	""features"" : [{
			""type"" : ""Feature"",
			""id"" : ""vfs_kommun.256"",
			""geometry"" : null,
			""properties"" : {
				""kommunnamn"" : ""Karlskrona""
			}
		}
	],
	""crs"" : null
}
";
            GeoJsonReader reader = new GeoJsonReader();            
            FeatureCollection coll = reader.Read<FeatureCollection>(json);
            Assert.That(coll, Is.Not.Null);
            Assert.That(coll.CRS, Is.Null);
            Assert.That(coll.Count, Is.EqualTo(1));

            IFeature feature = coll[0];
            Assert.That(feature.Geometry, Is.Null);
            Assert.That(feature.Attributes, Is.Not.Null);
            Assert.That(feature.Attributes.Exists("id"), Is.True);
            Assert.That(feature.Attributes["id"], Is.EqualTo("vfs_kommun.256"));
        }
    }
}