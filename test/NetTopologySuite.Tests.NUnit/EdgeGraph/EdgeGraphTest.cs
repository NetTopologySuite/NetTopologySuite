using System.Collections.ObjectModel;
using NetTopologySuite.EdgeGraph;
using NetTopologySuite.Geometries;
using NetTopologySuite.Tests.NUnit.Utilities;
using NUnit.Framework;
using EGraph = NetTopologySuite.EdgeGraph.EdgeGraph;

namespace NetTopologySuite.Tests.NUnit.EdgeGraph
{
    [TestFixture]
    public class EdgeGraphTest
    {
        [Test]
        public void TestNode()
        {
            var graph = Build("MULTILINESTRING((0 0, 1 0), (0 0, 0 1), (0 0, -1 0))");
            CheckEdgeRing(graph, new Coordinate(0, 0), new[]
            {
                new Coordinate(1, 0),
                new Coordinate(0, 1),
                new Coordinate(-1, 0)
            });
            CheckNodeValid(graph, new Coordinate(0, 0), new Coordinate(1, 0));
            CheckEdge(graph, new Coordinate(0, 0), new Coordinate(1, 0));

            CheckNextPrev(graph);

            CheckNext(graph, 1, 0, 0, 0, 0, 1);
            CheckNext(graph, 0, 1, 0, 0, -1, 0);
            CheckNext(graph, -1, 0, 0, 0, 1, 0);

            CheckNextPrev(graph, 1, 0, 0, 0);
            CheckNextPrev(graph, 0, 1, 0, 0);
            CheckNextPrev(graph, -1, 0, 0, 0);

            Assert.That(FindEdge(graph, 0, 0, 1, 0).Degree() == 3);

        }

        [Test]
        public void TestRingGraph()
        {
            var graph = Build("MULTILINESTRING ((10 10, 10 90), (10 90, 90 90), (90 90, 90 10), (90 10, 10 10))");
            var e = FindEdge(graph, 10, 10, 10, 90);
            var eNext = FindEdge(graph, 10, 90, 90, 90);
            Assert.That(ReferenceEquals(e.Next, eNext), Is.True);
            Assert.That(ReferenceEquals(eNext.Prev, e), Is.True);

            var eSym = FindEdge(graph, 10, 90, 10, 10);
            Assert.That(ReferenceEquals(e.Sym, eSym), Is.True);
            Assert.That(e.Orig.Equals2D(new Coordinate(10, 10)));
            Assert.That(e.Dest.Equals2D(new Coordinate(10, 90)));

            CheckNextPrev(graph);
        }

        [Test]
        public void TestSingleEdgeGraph()
        {
            var graph = Build("LINESTRING (10 10, 20 20)");
            CheckNextPrev(graph);
        }


        /**
     * This test produced an error using the original buggy sorting algorithm
     * (in {@link HalfEdge#insert(HalfEdge)}).
     */
        [Test]
        public void TestCCWAfterInserts()
        {
            var graph = new NetTopologySuite.EdgeGraph.EdgeGraph();
            var e1 = AddEdge(graph, 50, 39, 35, 42);
            AddEdge(graph, 50, 39, 50, 60);
            AddEdge(graph, 50, 39, 68, 35);
            CheckNodeValid(e1);
        }

        [Test]
        public void TestCCWAfterInserts2()
        {
            var graph = new NetTopologySuite.EdgeGraph.EdgeGraph();
            var e1 = AddEdge(graph, 50, 200, 0, 200);
            AddEdge(graph, 50, 200, 190, 50);
            AddEdge(graph, 50, 200, 200, 200);
            CheckNodeValid(e1);
        }

        //==================================================

        private static void CheckEdgeRing(EGraph graph, Coordinate p, Coordinate[] dest)
        {
            var e = graph.FindEdge(p, dest[0]);
            var onext = e;
            int i = 0;
            do
            {
                Assert.IsTrue(onext.Dest.Equals2D(dest[i++]));
                onext = onext.ONext;
            } while (onext != e);

        }

        private static void CheckEdge(EGraph graph, Coordinate p0, Coordinate p1)
        {
            var e = graph.FindEdge(p0, p1);
            Assert.IsNotNull(e);
        }

        private void CheckNodeValid(EGraph graph, Coordinate p0, Coordinate p1)
        {
            var e = graph.FindEdge(p0, p1);
            Assert.That(e.IsEdgesSorted, $"Found non-sorted edges around node {e}.");
        }

        private void CheckNextPrev(EGraph graph)
        {
            var edges = graph.GetVertexEdges();
            foreach (var e in edges)
                Assert.That(ReferenceEquals(e.Next.Prev, e), Is.True);
        }



        private void CheckNext(EGraph graph, double x1, double y1, double x2, double y2, double x3, double y3)
        {
            var e1 = FindEdge(graph, x1, y1, x2, y2);
            var e2 = FindEdge(graph, x2, y2, x3, y3);
            Assert.That(ReferenceEquals(e1.Next, e2), Is.True);
            Assert.That(ReferenceEquals(e2.Prev, e1), Is.True);
        }

        private void CheckNextPrev(EGraph graph, double x1, double y1, double x2, double y2)
        {
            var e = FindEdge(graph, x1, y1, x2, y2);
            Assert.That(ReferenceEquals(e.Next.Prev, e), Is.True);
        }

        private HalfEdge FindEdge(EGraph graph, double x1, double y1, double x2, double y2)
        {
            return graph.FindEdge(new Coordinate(x1, y1), new Coordinate(x2, y2));
        }


        private void CheckNodeValid(HalfEdge e)
        {
            Assert.That(e.IsEdgesSorted, $"Found non-sorted edges around node {e}.");
        }

        private EGraph Build(string wkt)
        {
            return Build(new[] {wkt});
        }

        private EGraph Build(string[] wkt)
        {
            var geoms = IOUtil.ReadWKT(wkt);
            return EdgeGraphBuilder.Build(geoms);
        }

        private HalfEdge AddEdge(NetTopologySuite.EdgeGraph.EdgeGraph graph, double p0x, double p0y, double p1x,
            double p1y)
        {
            return graph.AddEdge(new Coordinate(p0x, p0y), new Coordinate(p1x, p1y));
        }

    }
}
