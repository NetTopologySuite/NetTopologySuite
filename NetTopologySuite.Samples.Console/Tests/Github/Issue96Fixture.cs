using System;
using GeoAPI.Geometries;
using NetTopologySuite.Algorithm.Distance;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    [TestFixture]
    public class Issue96Fixture
    {
        [Test]
        public void linearring_should_be_written_as_wkb()
        {
            IGeometryFactory factory = GeometryFactory.Default;
            ILinearRing expected = factory.CreateLinearRing(new[]
            {
                new Coordinate(0, 0),
                new Coordinate(10, 0),
                new Coordinate(10, 10),
                new Coordinate(0, 10),
                new Coordinate(0, 0)
            });

            WKBWriter writer  = new WKBWriter();
            byte[] bytes =  writer.Write(expected);
            Assert.That(bytes, Is.Not.Null);
            Assert.That(bytes, Is.Not.Empty);

            WKBReader reader = new WKBReader();
            IGeometry actual = reader.Read(bytes);
            Assert.That(actual, Is.Not.Null);
            Assert.That(actual, Is.EqualTo(expected));
            Assert.That(actual.OgcGeometryType, Is.EqualTo(expected.OgcGeometryType));

            // WKBReader reads "ILinearRing" geometries as ILineString
            Assert.That(expected, Is.InstanceOf<ILinearRing>());
            Assert.That(actual, Is.InstanceOf<ILineString>());
            Assert.That(actual.GeometryType, Is.Not.EqualTo(expected.GeometryType));
        }
    }
}