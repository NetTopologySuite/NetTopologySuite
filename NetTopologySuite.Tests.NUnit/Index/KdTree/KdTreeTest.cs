using System;
using System.Linq;
using GeoAPI.Geometries;
using NetTopologySuite.Geometries;
using NUnit.Framework;
using NetTopologySuite.Index.KdTree;
using NetTopologySuite.Tests.NUnit.Utilities;

namespace NetTopologySuite.Tests.NUnit.Index.KdTree
{
    public class KdTreeTest
    {
        [Test]
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

        [Test]
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
        public void TestNearestNeighbor()
        {
            var kd = new KdTree<string>();
            kd.Insert(new Coordinate(12, 16), "A");
            kd.Insert(new Coordinate(15, 8), "B");
            kd.Insert(new Coordinate(5, 18), "C");
            kd.Insert(new Coordinate(18, 5), "D");
            kd.Insert(new Coordinate(16, 15), "E");
            kd.Insert(new Coordinate(2, 5), "F");
            kd.Insert(new Coordinate(7, 10), "G");
            kd.Insert(new Coordinate(8, 7), "H");
            kd.Insert(new Coordinate(5, 5), "I");
            kd.Insert(new Coordinate(19, 12), "J");
            kd.Insert(new Coordinate(10, 2), "K");

            var res = kd.NearestNeighbor(new Coordinate(13, 2));

            Assert.AreEqual("K", res.Data);
        }

        [Test]
        public void TestMultiplePoint()
        {
            TestQuery("MULTIPOINT ( (1 1), (2 2) )", 0,
                new Envelope(0, 10, 0, 10),
                "MULTIPOINT ( (1 1), (2 2) )");
        }

        [Test]
        public void TestSubset()
        {
            TestQuery("MULTIPOINT ( (1 1), (2 2), (3 3), (4 4) )", 0,
                new Envelope(1.5, 3.4, 1.5, 3.5),
                "MULTIPOINT ( (2 2), (3 3) )");
        }

        [Test]
        public void TestTolerance()
        {
            TestQuery("MULTIPOINT ( (0 0), (-.1 1), (.1 1) )",
                1,
                new Envelope(-9, 9, -9, 9),
                "MULTIPOINT ( (0 0), (-.1 1) )");
        }

        [Test]
        public void TestTolerance2()
        {
            TestQuery("MULTIPOINT ((10 60), (20 60), (30 60), (30 63))",
                9,
                new Envelope(0, 99, 0, 99),
                "MULTIPOINT ((10 60), (20 60), (30 60))");
        }

        [Test]
        public void TestTolerance2_perturbedY()
        {
            TestQuery("MULTIPOINT ((10 60), (20 61), (30 60), (30 63))",
                9,
                new Envelope(0, 99, 0, 99),
                "MULTIPOINT ((10 60), (20 61), (30 60))");
        }

        [Test]
        public void TestSnapToNearest()
        {
            TestQueryRepeated("MULTIPOINT ( (10 60), (20 60), (16 60))",
                5,
                new Envelope(0, 99, 0, 99),
                "MULTIPOINT ( (10 60), (20 60), (20 60))");
        }


        private void TestQuery(string wktInput, double tolerance,
            Envelope queryEnv, string wktExpected)
        {
            var index = Build(wktInput, tolerance);
            TestQuery(
                index,
                queryEnv, false,
                IOUtil.Read(wktExpected).Coordinates);
        }

        private void TestQueryRepeated(String wktInput, double tolerance,
            Envelope queryEnv, String wktExpected)
        {
            var index = Build(wktInput, tolerance);
            TestQuery(
                index,
                queryEnv, true,
                IOUtil.Read(wktExpected).Coordinates);
        }

        private void TestQuery(KdTree<object> index, Envelope queryEnv,
            Coordinate[] expectedCoord)
        {
            var result = KdTree<object>.ToCoordinates(index.Query(queryEnv));

            Array.Sort(result);
            Array.Sort(expectedCoord);

            Assert.IsTrue(result.Length == expectedCoord.Length, 
                          "Result count = {0}, expected count = {1}", 
                          result.Length, expectedCoord.Length);

            var isMatch = CoordinateArrays.Equals(result, expectedCoord);
            Assert.IsTrue(isMatch, "Expected result coordinates not found");
        }

        private void TestQuery(KdTree<object> index, Envelope queryEnv, 
            bool includeRepeated, Coordinate[] expectedCoord)
        {
            var result = KdTree<object>.ToCoordinates(index.Query(queryEnv), includeRepeated);

            Array.Sort(result);
            Array.Sort(expectedCoord);

            Assert.IsTrue(result.Length == expectedCoord.Length,
                          "Result count = {0}, expected count = {1}",
                          result.Length, expectedCoord.Length);

            var isMatch = CoordinateArrays.Equals(result, expectedCoord);
            Assert.IsTrue(isMatch, "Expected result coordinates not found");
        }

        private KdTree<object> Build(string wktInput, double tolerance)
        {
            var index = new KdTree<object>(tolerance);
            var coords = IOUtil.Read(wktInput).Coordinates;
            for (var i = 0; i < coords.Length; i++)
                index.Insert(coords[i]);
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