using GeoAPI.Geometries;
using NetTopologySuite.Index.Quadtree;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index.Quadtree
{
    public class IsEmptyTest
    {
        [Test]
        public void TestSpatialIndex()
        {
            var index = new Quadtree<string>();
            Assert.AreEqual(0, index.Count);
            Assert.IsTrue(index.IsEmpty);

            index.Insert(new Envelope(0, 0, 1, 1), "test");
            Assert.AreEqual(1, index.Count);
            Assert.IsFalse(index.IsEmpty);

            index.Remove(new Envelope(0, 0, 1, 1), "test");
            Assert.AreEqual(0, index.Count);
            Assert.IsTrue(index.IsEmpty);
        }
    }
}