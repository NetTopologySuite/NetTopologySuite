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
            CheckEdgeRing(graph, new Coordinate(0, 0), new[] {
                                                                 new Coordinate(1, 0),
                                                                 new Coordinate(0, 1),
                                                                 new Coordinate(-1, 0)
                                                             });
            CheckNodeValid(graph, new Coordinate(0, 0), new Coordinate(1, 0));
            CheckEdge(graph, new Coordinate(0, 0), new Coordinate(1, 0));
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

        private static void CheckEdgeRing(EGraph graph, Coordinate p, Coordinate[] dest)
        {
            var e = graph.FindEdge(p, dest[0]);
            var onext = e;
            int i = 0;
            do
            {
                Assert.IsTrue(onext.Dest.Equals2D(dest[i++]));
                onext = onext.ONext;
            }
            while (onext != e);

        }
        private static void CheckEdge(EGraph graph, Coordinate p0, Coordinate p1)
        {
            var e = graph.FindEdge(p0, p1);
            Assert.IsNotNull(e);
        }

        private void CheckNodeValid(NetTopologySuite.EdgeGraph.EdgeGraph graph, Coordinate p0, Coordinate p1)
        {
            var e = graph.FindEdge(p0, p1);
            Assert.That(e.IsEdgesSorted, $"Found non-sorted edges around node {e}.");
        }


        private void CheckNodeValid(HalfEdge e)
        {
            Assert.That(e.IsEdgesSorted, $"Found non-sorted edges around node {e}.");
        }

        private EGraph Build(string wkt)
        {
            return Build(new[] { wkt });
        }

        private EGraph Build(string[] wkt)
        {
            var geoms = IOUtil.ReadWKT(wkt);
            return EdgeGraphBuilder.Build(geoms);
        }

        private HalfEdge AddEdge(NetTopologySuite.EdgeGraph.EdgeGraph graph, double p0x, double p0y, double p1x, double p1y)
        {
            return graph.AddEdge(new Coordinate(p0x, p0y), new Coordinate(p1x, p1y));
        }

    }
}
