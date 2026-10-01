using System;
using System.Linq;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.KdTree;
using NetTopologySuite.Tests.NUnit.Utilities;
using NUnit.Framework;

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
        public void TestNearestNeighborEmptyTree()
        {
            var kd = new KdTree<string>();
            Assert.That(kd.NearestNeighbor(new Coordinate(0, 0)), Is.Null);
        }

        [Test]
        public void TestNearestNeighborsEmptyTree()
        {
            var kd = new KdTree<string>();
            var result = kd.NearestNeighbors(new Coordinate(0, 0), 5);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.Zero);
        }

        [Test]
        public void TestNearestNeighborsZeroK()
        {
            var kd = new KdTree<string>();
            kd.Insert(new Coordinate(1, 1), "A");
            kd.Insert(new Coordinate(2, 2), "B");
            var result = kd.NearestNeighbors(new Coordinate(0, 0), 0);
            Assert.That(result.Count, Is.Zero);
        }

        [Test]
        public void TestNearestNeighborsOrderedClosestFirst()
        {
            var kd = new KdTree<string>();
            kd.Insert(new Coordinate(0, 0), "Origin");
            kd.Insert(new Coordinate(1, 0), "East1");
            kd.Insert(new Coordinate(5, 0), "East5");
            kd.Insert(new Coordinate(10, 0), "East10");

            var result = kd.NearestNeighbors(new Coordinate(0, 0), 3);
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].Data, Is.EqualTo("Origin"));
            Assert.That(result[1].Data, Is.EqualTo("East1"));
            Assert.That(result[2].Data, Is.EqualTo("East5"));
        }

        [Test]
        public void TestNearestNeighborsRequestMoreThanAvailable()
        {
            var kd = new KdTree<string>();
            kd.Insert(new Coordinate(1, 1), "A");
            kd.Insert(new Coordinate(2, 2), "B");
            kd.Insert(new Coordinate(3, 3), "C");

            // k larger than tree size — should return all available, still sorted.
            var result = kd.NearestNeighbors(new Coordinate(0, 0), 10);
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result[0].Data, Is.EqualTo("A"));
            Assert.That(result[1].Data, Is.EqualTo("B"));
            Assert.That(result[2].Data, Is.EqualTo("C"));
        }

        [Test]
        public void TestNearestNeighborBruteForce()
        {
            // Compare KdTree.NearestNeighbor against a brute-force scan over 1000 random points,
            // running 500 queries against the same tree. Matches the JTS test pattern.
            const int n = 1000;
            const int queries = 500;
            var rand = new Random(1337);

            var tree = new KdTree<object>();
            var points = new System.Collections.Generic.List<Coordinate>(n);
            for (int i = 0; i < n; i++)
            {
                var p = new Coordinate(rand.NextDouble(), rand.NextDouble());
                tree.Insert(p);
                points.Add(p);
            }

            for (int i = 0; i < queries; i++)
            {
                var q = new Coordinate(rand.NextDouble(), rand.NextDouble());
                var nearestNode = tree.NearestNeighbor(q);
                var bruteForce = BruteForceNearestNeighbor(points, q);
                Assert.That(nearestNode.Coordinate, Is.EqualTo(bruteForce),
                    $"Query #{i} at ({q.X}, {q.Y}) mismatched brute force");
            }
        }

        [Test]
        public void TestNearestNeighborsBruteForce()
        {
            // Compare KdTree.NearestNeighbors(k) against a brute-force k-NN over 2500 random points,
            // running 50 trials with different seeds. Matches the JTS test pattern.
            const int n = 2500;
            const int numTrials = 50;
            var rand = new Random(0);

            for (int trial = 0; trial < numTrials; trial++)
            {
                var tree = new KdTree<object>();
                var points = new System.Collections.Generic.List<Coordinate>(n);
                for (int i = 0; i < n; i++)
                {
                    var p = new Coordinate(rand.NextDouble(), rand.NextDouble());
                    tree.Insert(p);
                    points.Add(p);
                }

                var query = new Coordinate(rand.NextDouble(), rand.NextDouble());
                int k = rand.Next(n / 10);

                var nearestNodes = tree.NearestNeighbors(query, k);
                var bruteForce = BruteForceNearestNeighbors(points, query, k);

                Assert.That(nearestNodes.Count, Is.EqualTo(k));
                for (int i = 0; i < k; i++)
                {
                    Assert.That(nearestNodes[i].Coordinate, Is.EqualTo(bruteForce[i]),
                        $"Trial {trial} position {i} mismatched brute force (k={k})");
                }
            }
        }

        private static Coordinate BruteForceNearestNeighbor(
            System.Collections.Generic.List<Coordinate> points, Coordinate query)
        {
            Coordinate nearest = null;
            double minDist = double.PositiveInfinity;
            foreach (var p in points)
            {
                double d = query.Distance(p);
                if (d < minDist)
                {
                    minDist = d;
                    nearest = p;
                }
            }
            return nearest;
        }

        private static System.Collections.Generic.List<Coordinate> BruteForceNearestNeighbors(
            System.Collections.Generic.List<Coordinate> points, Coordinate query, int k)
        {
            var sorted = new System.Collections.Generic.List<Coordinate>(points);
            sorted.Sort((a, b) => query.Distance(a).CompareTo(query.Distance(b)));
            return sorted.GetRange(0, System.Math.Min(k, sorted.Count));
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

        [Test]
        public void TestSizeDepth()
        {
            var index = Build("MULTIPOINT ( (10 60), (20 60), (16 60), (1 1), (23 400))", 0);
            int count = index.Count;
            Assert.AreEqual(5, count);
            int depth = index.Depth;
            // these are weak conditions, but depth varies depending on data and algorithm
            Assert.True(depth > 1);
            Assert.True(depth <= count);
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

        private void TestQueryRepeated(string wktInput, double tolerance,
            Envelope queryEnv, string wktExpected)
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

            bool isMatch = CoordinateArrays.Equals(result, expectedCoord);
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

            bool isMatch = CoordinateArrays.Equals(result, expectedCoord);
            Assert.IsTrue(isMatch, "Expected result coordinates not found");
        }

        private KdTree<object> Build(string wktInput, double tolerance)
        {
            var index = new KdTree<object>(tolerance);
            var coords = IOUtil.Read(wktInput).Coordinates;
            for (int i = 0; i < coords.Length; i++)
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
