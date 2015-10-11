using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    [TestFixture]
    public class Issue62Fixture
    {
        [Test]
        public void geojson_should_deserialize_a_geometry_with_geometrycollection()
        {
            const string json = @"
{
    ""geometry"": {
        ""type"": ""GeometryCollection"",
        ""geometries"": [ 
            {
                ""type"":""Polygon"",
                ""coordinates"":[[[1.0,1.0],[1.0,2.0],[2.0,2.0],[1.0,1.0]]]
            },
            {
                ""type"":""Point"",
                ""coordinates"":[100.0,100.0]
            },
            {
                ""type"":""Polygon"",
                ""coordinates"":[[[201.0,201.0],[201.0,202.0],[202.0,202.0],[201.0,201.0]]]
            }
        ]
    }
}
";
            GeoJsonReader reader = new GeoJsonReader();
            IGeometry geometry = reader.Read<IGeometry>(json);
            Assert.IsNotNull(geometry);
        }

        [Test]
        public void geojson_should_deserialize_a_feature_with_geometrycollection()
        {
            const string json = @"
{
    ""type"": ""Feature"",
    ""geometry"": {
        ""type"": ""GeometryCollection"",
        ""geometries"": [ 
            {
                ""type"":""Polygon"",
                ""coordinates"":[[[1.0,1.0],[1.0,2.0],[2.0,2.0],[1.0,1.0]]]
            },
            {
                ""type"":""Point"",
                ""coordinates"":[100.0,100.0]
            },
            {
                ""type"":""Polygon"",
                ""coordinates"":[[[201.0,201.0],[201.0,202.0],[202.0,202.0],[201.0,201.0]]]
            }
        ]
    },
    ""properties"": {
    }
}
";
            GeoJsonReader reader = new GeoJsonReader();
            Feature geometry = reader.Read<Feature>(json);
            Assert.IsNotNull(geometry);
        }
    }
}