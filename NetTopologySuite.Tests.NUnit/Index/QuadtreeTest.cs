using System;
using NetTopologySuite.Index.Quadtree;
using NUnit.Framework;
using NetTopologySuite.Tests.NUnit.Utilities;

namespace NetTopologySuite.Tests.NUnit.Index
{
    public class QuadtreeTest
    {
        [TestAttribute]
        public void TestSpatialIndex()
        {
            var tester = new SpatialIndexTester { SpatialIndex = new Quadtree<object>() };
            tester.Init();
            tester.Run();
            Assert.IsTrue(tester.IsSuccess);
        }

#if !PCL
        [TestAttribute]
        public void TestSerialization()
        {
            var tester = new SpatialIndexTester { SpatialIndex = new Quadtree<object>() };
            tester.Init();

            Console.WriteLine("\n\nTest with original data\n");
            tester.Run();
            var tree1 = (Quadtree<object>)tester.SpatialIndex;
            var data = SerializationUtility.Serialize(tree1);
            var tree2 = SerializationUtility.Deserialize<Quadtree<object>>(data);
            tester.SpatialIndex = tree2;

            Console.WriteLine("\n\nTest with deserialized data\n");
            tester.Run();
            Assert.IsTrue(tester.IsSuccess);
        }
#endif
    }
}
