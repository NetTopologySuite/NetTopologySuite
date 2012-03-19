using GeoAPI.Geometries;
using NetTopologySuite.Index.Quadtree;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Index
{
    public class QuadtreeTestCase //: SpatialIndexTestCase
    {
        /*
        protected override ISpatialIndex<object> CreateSpatialIndex()
        {
            return new Quadtree<object>();
        }
         */

        [Test]
        public void TestSpatialIndex()
        {
            var tester = new SpatialIndexTester {SpatialIndex = new Quadtree<object>()};
            tester.Init();
            tester.Run();
            Assert.IsTrue(tester.IsSuccess);
        }
    }
}
