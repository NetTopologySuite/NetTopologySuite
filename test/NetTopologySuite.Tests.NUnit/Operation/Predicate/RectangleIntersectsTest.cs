using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Predicate;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.Predicate
{
    public class RectangleIntersectsTest : GeometryTestCase
    {
        [Test]
        public void TestXYZM()
        {
            var rect = (Polygon)Read("POLYGON ZM ((1 9 2 3, 9 9 2 3, 9 1 2 3, 1 1 2 3, 1 9 2 3))");
            var line = Read("LINESTRING ZM (5 15 5 5, 15 5 5 5)");
            bool rectIntersects = RectangleIntersects.Intersects(rect, line);
            Assert.That(rectIntersects, Is.False);
        }
    }
}
