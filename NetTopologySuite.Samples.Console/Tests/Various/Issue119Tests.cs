namespace NetTopologySuite.Tests.Various
{
    using System;

    using GeoAPI.Geometries;

    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NetTopologySuite.Operation.Union;

    using global::NUnit.Framework;
    
    [TestFixture]
    public class Issue119Tests
    {
        /// <summary>
        /// this test works with NTS 1.12, breaks with current trunk 08/06/2012
        /// </summary>
        [Test]
        public void TestMultiPolygonInvalidWithTopologyCollapse()
        {
            WKTReader reader = new WKTReader(GeometryFactory.Fixed);
            IGeometry expected = reader.Read("GEOMETRYCOLLECTION (LINESTRING (0 0, 20 0), POLYGON ((150 0, 20 0, 20 100, 180 100, 180 0, 150 0)))");
            IGeometry geom = reader.Read("MULTIPOLYGON (((0 0, 150 0, 150 1, 0 0)), ((180 0, 20 0, 20 100, 180 100, 180 0)))");
            IGeometry result = UnaryUnionOp.Union(geom);
            Assert.That(expected.EqualsExact(result), String.Format(" expected '{0}' but was '{1}'", expected, result));
        }
    }
}