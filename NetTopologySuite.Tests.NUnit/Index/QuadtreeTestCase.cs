using System;
using GeoAPI.Geometries;
using NetTopologySuite.Index.Quadtree;
using NUnit.Framework;
using NetTopologySuite.Tests.NUnit.Utilities;

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

        [Test]
        public void TestSerialization()
        {
            var tester = new SpatialIndexTester { SpatialIndex = new Quadtree<object>() };
            tester.Init();

            Console.WriteLine("\n\nTest with original data\n");
            tester.Run();
            var tree1 = (Quadtree<object>)tester.SpatialIndex;
            var data = SerializationUtility.Serialize(tree1);
            var tree2 = (Quadtree<object>)SerializationUtility.Deserialize(data);
            tester.SpatialIndex = tree2;

            Console.WriteLine("\n\nTest with deserialized data\n");
            tester.Run();
            Assert.IsTrue(tester.IsSuccess);
        }

    }
}
