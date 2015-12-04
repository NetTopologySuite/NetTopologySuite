using System.IO;
using NetTopologySuite.Features;
using NetTopologySuite.Utilities;
using Newtonsoft.Json;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace NetTopologySuite.Samples.Tests.Github
{
    //[TestFixture("GitHub", "Various issues")]
    public class Issues
    {
        public Issues()
        { }

        [Test(Description =
                "Issue 69: GeoJsonReader.Read<FeatureCollection>(geojson) fails with certain order of properties")]
        public void Test69()
        {
            const string json1 = "{\"type\": \"FeatureCollection\", \"features\": [{\"geometry\": {\"type\": \"Point\", \"coordinates\": [-5.442235, 36.12328]}, \"type\": \"Feature\", \"properties\": {\"latitud\": 36.12328, \"categoria\": -1, \"mmsi\": \"2241001\", \"nudos\": 0.0, \"longitud\": -5.442235, \"rumbo\": 0.0, \"utc\": \"2015-12-03T15:30:00.757000Z\", \"estatica\": 1, \"nombre\": \"VTS 2241001\", \"aisShipType\": 90, \"aisTypeName\": -1}}, {\"geometry\": {\"type\": \"Point\", \"coordinates\": [-5.4421301, 36.127991]}, \"type\": \"Feature\", \"properties\": {\"latitud\": 36.127991, \"categoria\": -1, \"mmsi\": \"225970570\", \"nudos\": 0.0, \"longitud\": -5.4421301, \"rumbo\": 282.0, \"utc\": \"2015-12-03T15:30:00.757000Z\", \"estatica\": 1, \"nombre\": \"ALG.PILOTS GETARES\", \"aisShipType\": 30, \"aisTypeName\": -1}}]}";
            const string json2 = "{\"type\": \"FeatureCollection\", \"features\": [{\"type\": \"Feature\", \"properties\": {\"latitud\": 36.12328, \"categoria\": -1, \"mmsi\": \"2241001\", \"nudos\": 0.0, \"longitud\": -5.442235, \"rumbo\": 0.0, \"utc\": \"2015-12-03T15:30:00.757000Z\", \"estatica\": 1, \"nombre\": \"VTS 2241001\", \"aisShipType\": 90, \"aisTypeName\": -1},\"geometry\": {\"type\": \"Point\", \"coordinates\": [-5.442235, 36.12328]}}, {\"type\": \"Feature\", \"properties\": {\"latitud\": 36.127991, \"categoria\": -1, \"mmsi\": \"225970570\", \"nudos\": 0.0, \"longitud\": -5.4421301, \"rumbo\": 282.0, \"utc\": \"2015-12-03T15:30:00.757000Z\", \"estatica\": 1, \"nombre\": \"ALG.PILOTS GETARES\", \"aisShipType\": 30, \"aisTypeName\": -1},\"geometry\": {\"type\": \"Point\", \"coordinates\": [-5.4421301, 36.127991]}}]}";

            Assert.IsTrue(TestGeoJsonDeserialize<FeatureCollection>(json2), json2);
            Assert.IsTrue(TestGeoJsonDeserialize<FeatureCollection>(json1), json1);
        }

        private static bool TestGeoJsonDeserialize<T>(string json) where T:class
        {
            var jr = new JsonTextReader(new StringReader(json));
            var s = new NetTopologySuite.IO.GeoJsonSerializer();
            var f = default(T);
            Assert.DoesNotThrow(() => f = s.Deserialize<T>(jr));
            Assert.IsNotNull(f);

            return true;
        }
    }
}