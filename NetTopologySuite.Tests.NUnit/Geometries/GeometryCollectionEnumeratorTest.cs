using GeoAPI.Geometries;
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
            IGeometryCollection g = (IGeometryCollection)Read(
                  "GEOMETRYCOLLECTION (GEOMETRYCOLLECTION (POINT (10 10)))");
            GeometryCollectionEnumerator i = new GeometryCollectionEnumerator(g);
            Assert.IsTrue(i.MoveNext());
            Assert.IsTrue(i.Current is IGeometryCollection);
            Assert.IsTrue(i.MoveNext());
            Assert.IsTrue(i.Current is IGeometryCollection);
            Assert.IsTrue(i.MoveNext());
            Assert.IsTrue(i.Current is IPoint);
            Assert.IsTrue(!i.MoveNext());
        }

        [Test]
        public void TestAtomic()
        {
            IPolygon g = (IPolygon)Read("POLYGON ((1 9, 9 9, 9 1, 1 1, 1 9))");
            GeometryCollectionEnumerator i = new GeometryCollectionEnumerator(g);
            Assert.IsTrue(i.MoveNext());
            Assert.IsTrue(i.Current is IPolygon);
            Assert.IsTrue(!i.MoveNext());
        }
    }
}
