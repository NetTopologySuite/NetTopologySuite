using System;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    [TestFixture]
    public class Issue150Fixture
    {
        [Test]
        public void buildgeometry_creates_unnecessary_geometrycollection()
        {
            const string wkt = "MULTIPOLYGON (((0 0, 10 0, 10 10, 0 10, 0 0)), ((20 0, 30 0, 30 10, 20 10, 20 0)))";
            IGeometryFactory factory = GeometryFactory.Default;
            IGeometry read = new WKTReader().Read(wkt);
            Assert.IsNotNull(read);
            Assert.IsInstanceOf<IMultiPolygon>(read);

            IGeometry built = factory.BuildGeometry(new[] { read });
            Assert.IsNotNull(built);
            Assert.IsInstanceOf<IGeometryCollection>(built);
            Assert.AreEqual(1, built.NumGeometries);
        }
    }
}