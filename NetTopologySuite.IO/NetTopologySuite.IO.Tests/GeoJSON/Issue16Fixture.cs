﻿using NetTopologySuite.Features;
using NUnit.Framework;

namespace NetTopologySuite.IO.Tests.GeoJSON
{
    /// <summary>
    /// see: https://github.com/NetTopologySuite/NetTopologySuite/issues/16
    /// </summary>
    [TestFixture]
    public class Issue16Fixture
    {
        [Test]
        public void valid_geojson_feature_collection_should_be_serialized()
        {
            const string json = @"
{
	""type"" : ""FeatureCollection"",
	""features"" : [{
			""type"" : ""Feature"",
			""properties"" : {},
			""geometry"" : {
				""type"" : ""Polygon"",
				""coordinates"" : [[[-2.537841796875, 53.50111704294316], [-2.537841796875, 54.226707764386695], [-1.0986328125, 54.226707764386695], [-1.0986328125, 53.50111704294316], [-2.537841796875, 53.50111704294316]]]
			}
		}, {
			""type"" : ""Feature"",
			""properties"" : {},
			""geometry"" : {
				""type"" : ""Polygon"",
				""coordinates"" : [[[-2.724609375, 52.34205163638784], [-2.724609375, 53.08082737207479], [-0.9667968749999999, 53.08082737207479], [-0.9667968749999999, 52.34205163638784], [-2.724609375, 52.34205163638784]]]
			}
		}
	]
}
";
            GeoJsonReader reader = new GeoJsonReader();            
            FeatureCollection coll = reader.Read<FeatureCollection>(json);
            Assert.That(coll, Is.Not.Null);
            Assert.That(coll.Count, Is.EqualTo(2));
        }
    }
}