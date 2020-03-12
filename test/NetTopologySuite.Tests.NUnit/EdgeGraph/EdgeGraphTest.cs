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
            CheckEdge(graph, new Coordinate(0, 0), new Coordinate(1, 0));
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

        private EGraph Build(string wkt)
        {
            return Build(new[] { wkt });
        }

        private EGraph Build(string[] wkt)
        {
            var geoms = IOUtil.ReadWKT(wkt);
            return EdgeGraphBuilder.Build(geoms);
        }
    }
}
