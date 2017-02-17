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
        public void write_simple_point()
        {
            IPoint geometry = Factory.CreatePoint(new Coordinate(23.4, 56.7));
            AttributesTable attributes = new AttributesTable();
            attributes.Add("prop0", "value0");
            attributes.Add("prop1", "value1");
            IFeature feature = new Feature(geometry, attributes);

            TopoJsonWriter writer = new TopoJsonWriter();
            string actual = writer.Write(feature);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.Not.Empty);
            Console.WriteLine(actual);
            Assert.That(actual, Is.EqualTo(TopoWriterData.SimplePoint));
        }

        [Test]
        public void write_simple_linestring()
        {
            Coordinate c0 = new Coordinate(10.1, 10);
            Coordinate c1 = new Coordinate(20.2, 20);
            Coordinate c2 = new Coordinate(30.3, 30);
            ILineString geometry = Factory.CreateLineString(new[] { c0, c1, c2 });
            AttributesTable attributes = new AttributesTable();
            attributes.Add("prop0", "value0");
            attributes.Add("prop1", "value1");
            IFeature feature = new Feature(geometry, attributes);

            TopoJsonWriter writer = new TopoJsonWriter();
            string actual = writer.Write(feature);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.Not.Empty);
            Console.WriteLine(actual);
            Assert.That(actual, Is.EqualTo(TopoWriterData.SimpleLineString));
        }

        [Test]
        public void write_simple_polygon()
        {
            Coordinate c0 = new Coordinate(10.1, 10);
            Coordinate c1 = new Coordinate(20.2, 20);
            Coordinate c2 = new Coordinate(30.3, 30);
            IPolygon geometry = Factory.CreatePolygon(new[] { c0, c1, c2, c0 });
            AttributesTable attributes = new AttributesTable();
            attributes.Add("prop0", "value0");
            attributes.Add("prop1", "value1");
            IFeature feature = new Feature(geometry, attributes);

            TopoJsonWriter writer = new TopoJsonWriter();
            string actual = writer.Write(feature);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.Not.Empty);
            Console.WriteLine(actual);
            Assert.That(actual, Is.EqualTo(TopoWriterData.SimplePolygon));
        }

        [Test]
        public void write_polygon_with_hole()
        {
            Coordinate c0 = new Coordinate(10.1, 10);
            Coordinate c1 = new Coordinate(20.2, 20);
            Coordinate c2 = new Coordinate(30.3, 30);
            ILinearRing shell = Factory.CreateLinearRing(new[] { c0, c1, c2, c0 });
            Coordinate h0 = new Coordinate(15, 15);
            Coordinate h1 = new Coordinate(17, 15);
            Coordinate h2 = new Coordinate(15, 17);
            ILinearRing hole = Factory.CreateLinearRing(new[] { h0, h1, h2, h0 });
            IGeometry geometry = Factory.CreatePolygon(shell, new[] { hole });
            AttributesTable attributes = new AttributesTable();
            attributes.Add("prop0", "value0");
            attributes.Add("prop1", "value1");
            IFeature feature = new Feature(geometry, attributes);

            TopoJsonWriter writer = new TopoJsonWriter();
            string actual = writer.Write(feature);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.Not.Empty);
            Console.WriteLine(actual);
            Assert.That(actual, Is.EqualTo(TopoWriterData.PolygonWithHole));
        }
    }
}