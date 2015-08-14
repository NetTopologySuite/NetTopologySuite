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
            TestQuery(
                ReadCoords(new [] { new[] { 1.0, 1.0 }, new[] { 2.0, 2.0 } }),
                0,
                new Envelope(0, 10, 0, 10),
                ReadCoords(new [] { new[] { 1.0, 1.0 }, new[] { 2.0, 2.0 } }));
        }

        [Test]
        public void TestSubset()
        {
            TestQuery(
                ReadCoords(new [] { new []{ 1.0, 1.0 }, new[] { 2.0, 2.0 }, new[] { 3.0, 3.0 }, new[] { 4.0, 4.0 } }),
                0,
                new Envelope(1.5, 3.4, 1.5, 3.5),
                ReadCoords(new [] { new[] { 2.0, 2.0 }, new[] { 3.0, 3.0 } }));
        }

        [Test, Ignore("Known to fail")]
        public void TestToleranceFailure()
        {
            TestQuery("MULTIPOINT ( (0 0), (-.1 1), (.1 1) )",
                1,
                new Envelope(-9, 9, -9, 9),
                "MULTIPOINT ( (0 0), (-.1 1) )");
        }

        private void TestQuery(Coordinate[] input, double tolerance, Envelope queryEnv,
            Coordinate[] expectedCoord)
        {
            var index = Build(input, tolerance);
            var result = KdTree<object>.ToCoordinates(index.Query(queryEnv));

            Array.Sort(result);
            Array.Sort(expectedCoord);

            Assert.IsTrue(result.Length == expectedCoord.Length, 
                          "Result count = {0}, expected count = {1}", 
                          result.Length, expectedCoord.Length);

            var isMatch = CoordinateArrays.Equals(result, expectedCoord);
            Assert.IsTrue(isMatch, "Expected result coordinates not found");
        }

        private void TestQuery(string wktInput, double tolerance,
            Envelope queryEnv, string wktExpected)
        {
            TestQuery(
                IOUtil.Read(wktInput).Coordinates,
                tolerance,
                queryEnv,
                IOUtil.Read(wktExpected).Coordinates);
        }

        private KdTree<object> Build(Coordinate[] coords, double tolerance)
        {
            var index = new KdTree<object>(tolerance);
            for (var i = 0; i < coords.Length; i++)
                index.Insert(coords[i]);
            return index;
        }

        private Coordinate[] ReadCoords(double[][] ords)
        {
            var coords = new Coordinate[ords.Length];
            for (var i = 0; i < ords.Length; i++)
            {
                var c = new Coordinate(ords[i][0], ords[i][1]);
                coords[i] = c;
            }
            return coords;
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