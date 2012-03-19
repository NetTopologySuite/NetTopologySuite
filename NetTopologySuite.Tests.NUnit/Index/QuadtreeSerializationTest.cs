using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Index.Quadtree;

namespace NetTopologySuite.Tests.NUnit.Index
{
    public class QuadtreeSerializationTest
    {
        [Test]
        public void TestSerialization()
        {
            var tester = new SpatialIndexTester {SpatialIndex = new Quadtree<Envelope>()};
            tester.Init();

            Console.WriteLine("\n\nTest with original data\n");
            tester.Run();
            var tree1 = (Quadtree<Envelope>)tester.SpatialIndex;
            var data = Serialize(tree1);
            var tree2 = (Quadtree<Envelope>) Deserialize(data);
            tester.SpatialIndex = tree2;
            
            Console.WriteLine("\n\nTest with deserialized data\n");
            tester.Run();
            Assert.IsTrue(tester.IsSuccess);
        }

        private static byte[] Serialize(Object obj)
        {
            using (var bos = new MemoryStream())
            {
                var bf = new BinaryFormatter();
                bf.Serialize(bos, obj);
                return bos.ToArray();
            }
        }

        private static Object Deserialize(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                var bf = new BinaryFormatter();
                return bf.Deserialize(ms);
            }
        }
    }
}