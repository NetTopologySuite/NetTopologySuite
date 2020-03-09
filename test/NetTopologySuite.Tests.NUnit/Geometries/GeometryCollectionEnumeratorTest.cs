#nullable disable
using NetTopologySuite.Geometries;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Geometries
{
    [TestFixture]
    public class GeometryCollectionEnumeratorTest : GeometryTestCase
    {
        [Test]
        public void TestGeometryCollection()
        {
            var g = (GeometryCollection)Read(
                  "GEOMETRYCOLLECTION (GEOMETRYCOLLECTION (POINT (10 10)))");
            var i = new GeometryCollectionEnumerator(g);
            Assert.IsTrue(i.MoveNext());
            Assert.IsTrue(i.Current is GeometryCollection);
            Assert.IsTrue(i.MoveNext());
            Assert.IsTrue(i.Current is GeometryCollection);
            Assert.IsTrue(i.MoveNext());
            Assert.IsTrue(i.Current is Point);
            Assert.IsTrue(!i.MoveNext());
        }

        [TestCase("GEOMETRYCOLLECTION (GEOMETRYCOLLECTION (POINT (10 10)))")]
        [TestCase("MULTILINESTRING ((10 10, 20 20), (10 20, 20 30), (20 10, 30 20))")]
        [TestCase("MULTIPOINT ((10 10), (10 20), (30 30))")]
        [TestCase("MULTIPOLYGON (((0 0, 0 10, 10 10, 10 0, 0 0)), ((11 0, 11 10, 21 10, 21 0, 11 0)), "+
                  "((10 10, 10 20, 20 20, 20 15, 10 10)), ((60 60, 70 70, 80 60, 60 60)))")]
        public void TestRepeatedAccess(string wkt)
        {
            var geom = Read(wkt);
            if (!(geom is GeometryCollection gc))
                return;

            using (var it = gc.GetEnumerator())
            {
                while (it.MoveNext())
                {
                    var g1 = it.Current;
                    var g2 = it.Current;
                    Assert.That(g1, Is.Not.Null);
                    Assert.That(g2, Is.Not.Null);
                    Assert.That(g1.EqualsExact(g2), Is.True);
                }
            }
        }

        [Test]
        public void TestAtomic()
        {
            var g = (Polygon)Read("POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))");
            var i = new GeometryCollectionEnumerator(g);
            Assert.IsTrue(i.MoveNext());
            var current = i.Current;
            Assert.IsTrue(i.Current is Polygon);
            Assert.DoesNotThrow(() => current = i.Current);
            Assert.IsTrue(i.Current is Polygon);

            Assert.IsTrue(!i.MoveNext());
        }
    }
}
