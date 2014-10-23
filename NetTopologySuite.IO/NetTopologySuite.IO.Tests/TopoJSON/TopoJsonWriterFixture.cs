using System;
using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.IO.TopoJSON.Fixtures
{
    [TestFixture]
    public class TopoJsonWriterFixture
    {
        private static readonly IGeometryFactory Factory = GeometryFactory.Fixed;

        [Test]
        public void write_simple_point_data()
        {
            IGeometry geometry = Factory.CreatePoint(new Coordinate(23.4, 56.7));
            AttributesTable attributes = new AttributesTable();
            attributes.AddAttribute("prop0", "value0");
            attributes.AddAttribute("prop1", "value1");
            IFeature feature = new Feature(geometry, attributes);
            FeatureCollection features = new FeatureCollection();
            features.Add(feature);

            TopoJsonWriter writer = new TopoJsonWriter();
            string actual = writer.Write(features);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.Not.Empty);
            Console.WriteLine(actual);
            Assert.That(actual, Is.EqualTo(TopoWriterData.SimplePoint));
        }
    }
}