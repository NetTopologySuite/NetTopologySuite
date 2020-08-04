using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;

namespace NetTopologySuite.Tests.NUnit.Utilities
{
    public class UniqueCoordinateArrayFilterTest : GeometryTestCase
    {
        [Test]
        public void TestFilter()
        {
            var g = Read("MULTIPOINT(10 10, 20 20, 30 30, 20 20, 10 10)");
            var f = new UniqueCoordinateArrayFilter();
            g.Apply(f);

            Assert.That(f.Coordinates.Length, Is.EqualTo(3));
            Assert.That(f.Coordinates[0], Is.EqualTo(new Coordinate(10, 10)));
            Assert.That(f.Coordinates[1], Is.EqualTo(new Coordinate(20, 20)));
            Assert.That(f.Coordinates[2], Is.EqualTo(new Coordinate(30, 30)));
        }
    }
}
