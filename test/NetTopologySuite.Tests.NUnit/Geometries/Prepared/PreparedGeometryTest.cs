using NetTopologySuite.Geometries.Prepared;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries.Prepared
{
    public class PreparedGeometryTest : GeometryTestCase
    {

        [Test]
        public void TestEmptyElement()
        {
            var geomA = Read("MULTIPOLYGON (((9 9, 9 1, 1 1, 2 4, 7 7, 9 9)), EMPTY)");
            var geomB = Read("MULTIPOLYGON (((7 6, 7 3, 4 3, 7 6)), EMPTY)");
            var prepA = PreparedGeometryFactory.Prepare(geomA);
            Assert.That(prepA.Covers(geomB), Is.True);
            Assert.That(prepA.Contains(geomB), Is.True);
            Assert.That(prepA.Intersects(geomB), Is.True);
        }
    }
}
