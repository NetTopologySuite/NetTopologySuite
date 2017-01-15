using System;
using System.IO;
using System.Reflection;
using NetTopologySuite.Features;
using Newtonsoft.Json;
using NUnit.Framework;

namespace NetTopologySuite.IO.Tests.GeoJSON
{
    /// <summary>
    /// https://github.com/NetTopologySuite/NetTopologySuite/issues/145
    /// </summary>
    [TestFixture]
    public class Issue145Fixture
    {
        [Test]
        public void deserialize_geojson_from_osm()
        {            
            const string resourceName = "NetTopologySuite.IO.Tests.GeoJSON.World_AL6.GeoJson";
            var assembly = Assembly.GetExecutingAssembly();
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                Assert.IsNotNull(stream);
                {
                    var serializer = GeoJsonSerializer.CreateDefault();
                    using (var reader = new StreamReader(stream))
                    using (var textReader = new JsonTextReader(reader))
                    {
                        var collection = serializer.Deserialize<FeatureCollection>(textReader);
                        Assert.IsNotNull(collection);
                        Assert.AreEqual(4, collection.Count);

                        foreach (var feature in collection.Features)
                        {
                            Assert.IsNotNull(feature.Geometry);
                            Assert.IsNull(feature.BoundingBox);
                            var attributes = feature.Attributes;
                            Assert.IsNotNull(attributes);
                            var names = attributes.GetNames();
                            foreach (var name in names)
                                Console.WriteLine("{0}: {1}", name, attributes[name]);
                        }
                    }
                }
            }
        }
	}
}