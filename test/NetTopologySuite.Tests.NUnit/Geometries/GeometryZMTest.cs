using NetTopologySuite.Geometries;
using NetTopologySuite.Geometries.Implementation;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    internal class GeometryZMTest : GeometryTestCase
    {
        public GeometryZMTest()
            : base(PackedCoordinateSequenceFactory.DoubleFactory)
        { }

        [Test]
        public void TestArea()
        {
            var geom = (Polygon)Read("POLYGON ZM ((1 9 2 3, 9 9 2 3, 9 1 2 3, 1 1 2 3, 1 9 2 3))");
            double area = geom.Area;
            Assert.That(area, Is.EqualTo(64.0));
        }

        [Test]
        public void TestLength()
        {
            var geom = Read("POLYGON ZM ((1 9 2 3, 9 9 2 3, 9 1 2 3, 1 1 2 3, 1 9 2 3))");
            double len = geom.Length;
            Assert.That(len, Is.EqualTo(32.0));
        }
    }
}
