using System.Linq;
using GeoAPI.Geometries;
using NUnit.Framework;
using NetTopologySuite.Index.KdTree;
using NetTopologySuite.Tests.NUnit.Utilities;

namespace NetTopologySuite.Tests.NUnit.Index.KdTree
{
    public class KdTreeTest
    {
        [TestAttribute]
        public void TestSinglePoint()
        {
            var index = new KdTree<object>(.001);

            var node1 = index.Insert(new Coordinate(1, 1));

            var node2 = index.Insert(new Coordinate(1, 1));

            Assert.IsTrue(node1 == node2, "Inserting 2 identical points should create one node");

            var queryEnv = new Envelope(0, 10, 0, 10);

            var result = index.Query(queryEnv);
            Assert.IsTrue(result.Count == 1);

            var node = Enumerable.First(result);
            Assert.IsTrue(node.Count == 2);
            Assert.IsTrue(node.IsRepeated);
        }

        [TestAttribute]
        public void TestEndlessLoop()
        {
            var kd = new KdTree<string>();
            kd.Insert(new Coordinate(383, 381), "A");
            kd.Insert(new Coordinate(349, 168), "B");
            kd.Insert(new Coordinate(473, 223), "C");
            kd.Insert(new Coordinate(227, 44), "D");
            kd.Insert(new Coordinate(273, 214), "E");
            kd.Insert(new Coordinate(493, 87), "F");
            kd.Insert(new Coordinate(502, 290), "G");


            var res = kd.NearestNeighbor(new Coordinate(297, 133)); //Should be B
            Assert.AreEqual("B", res.Data);
            res = kd.NearestNeighbor(new Coordinate(272, 216)); //Should be E        }
            Assert.AreEqual("E", res.Data);
            res = kd.NearestNeighbor(new Coordinate(635, 377)); //Should be G
            Assert.AreEqual("G", res.Data);
        }

        [Test]
        public void TestMultiplePoint()
        {
            var index = Build("MULTIPOINT ((1 1), (2 2))");

            var queryEnv = new Envelope(0, 10, 0, 10);

            var result = index.Query(queryEnv);
            Assert.AreEqual(2, result.Count, "2 != result.Count");
            var node = result.First();
            Assert.IsTrue(node.Coordinate.Equals2D(new Coordinate(1, 1)));
            node = result.Last();
            Assert.IsTrue(node.Coordinate.Equals2D(new Coordinate(2, 2)));
        }

        private static KdTree<object> Build(string wkt)
        {
            var geom = IOUtil.Read(wkt);
            var index = new KdTree<object>(.001);
            geom.Apply(new TestCoordinateFilter<object>(index));
            return index;
        }

        private class TestCoordinateFilter<T> : ICoordinateFilter where T : class
        {
            private readonly KdTree<T> _index;

            public TestCoordinateFilter(KdTree<T> index)
            {
                _index = index;
            }

            public void Filter(Coordinate coord)
            {
                _index.Insert(coord);
            }
        }

    }

}