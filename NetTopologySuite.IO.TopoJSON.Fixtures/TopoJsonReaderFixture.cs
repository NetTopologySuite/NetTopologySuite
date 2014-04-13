using System.Collections.Generic;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.IO.TopoJSON.Fixtures
{
    [TestFixture]
    public class TopoJsonReaderFixture
    {
        private static readonly IGeometryFactory Factory = GeometryFactory.Fixed;

        [Test]
        public void read_reference_data()
        {
            TopoJsonReader reader = new TopoJsonReader(Factory);
            IDictionary<string, FeatureCollection> coll = reader.Read<IDictionary<string, FeatureCollection>>(TopoData.ReferenceData);
            Assert.That(coll, Is.Not.Null);
        }
    }
}
