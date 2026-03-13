using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NetTopologySuite.IO;
using NUnit.Framework;

namespace NetTopologySuite.Samples.Tests.Github
{
    public class Discussion798
    {
        [Test, Description("GeometryFactory.CreateGeometry copies the factory from empty polygon wrong"), Author("FObermaier, driekus77")]
        [TestCase("POINT EMPTY")]
        [TestCase("LINESTRING EMPTY")]
        [TestCase("POLYGON EMPTY")]
        [TestCase("MULTIPOINT EMPTY")]
        [TestCase("MULTILINESTRING EMPTY")]
        [TestCase("MULTIPOLYGON EMPTY")]
        [TestCase("GEOMETRYCOLLECTION EMPTY")]
        public void TestCreateGeometryChangesCoordinateFactoryForEmpty(string wkt)
        {
            var gsSrc = new NtsGeometryServices();
            var rdr = new WKTReader(gsSrc);
            var geom = rdr.Read(wkt);
            if (!geom.IsEmpty) Assert.Inconclusive($"Not an empty geometry: {wkt}");

            var gsTgt = new NtsGeometryServices(new DotSpatialAffineCoordinateSequenceFactory(Ordinates.XY));
            var test = gsTgt.CreateGeometryFactory().CreateGeometry(geom);

            Assert.That(test.Factory.CoordinateSequenceFactory, Is.TypeOf<DotSpatialAffineCoordinateSequenceFactory>());
        }
    }
}
