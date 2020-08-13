using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.OverlayNg;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    public partial class OverlayGraphTest : GeometryTestCase
    {
        [Test]
        public void TestTriangle()
        {

            var line1 = CreateLine(0, 0, 10, 10);
            var line2 = CreateLine(10, 10, 0, 10);
            var line3 = CreateLine(0, 10, 0, 0);

            var graph = CreateGraph(line1, line2, line3);

            var e1 = FindEdge(graph, 0, 0, 10, 10);
            var e2 = FindEdge(graph, 10, 10, 0, 10);
            var e3 = FindEdge(graph, 0, 10, 0, 0);

            CheckNodeValid(e1);
            CheckNodeValid(e2);
            CheckNodeValid(e3);

            CheckNext(e1, e2);
            CheckNext(e2, e3);
            CheckNext(e3, e1);

            var e1sym = FindEdge(graph, 10, 10, 0, 0);
            var e2sym = FindEdge(graph, 0, 10, 10, 10);
            var e3sym = FindEdge(graph, 0, 0, 0, 10);

            Assert.AreEqual(e1sym, e1.Sym);
            Assert.AreEqual(e2sym, e2.Sym);
            Assert.AreEqual(e3sym, e3.SymOE);

            CheckNext(e1sym, e3sym);
            CheckNext(e2sym, e1sym);
            CheckNext(e3sym, e2sym);
        }

        [Test]
        public void TestStar()
        {

            var graph = new OverlayGraph();

            var e1 = AddEdge(graph, 5, 5, 0, 0);
            var e2 = AddEdge(graph, 5, 5, 0, 9);
            var e3 = AddEdge(graph, 5, 5, 9, 9);

            CheckNodeValid(e1);

            CheckNext(e1, e1.SymOE);
            CheckNext(e2, e2.SymOE);
            CheckNext(e3, e3.SymOE);

            CheckPrev(e1, e3.SymOE);
            CheckPrev(e2, e1.SymOE);
            CheckPrev(e3, e2.SymOE);
        }

        /**
         * This test produced an error using the old HalfEdge sorting algorithm
         * (in {@link HalfEdge#insert(HalfEdge)}).
         */
        [Test]
        public void TestCCWAfterInserts()
        {
            var e1 = CreateLine(50, 39, 35, 42, 37, 30);
            var e2 = CreateLine(50, 39, 50, 60, 20, 60);
            var e3 = CreateLine(50, 39, 68, 35);

            var graph = CreateGraph(e1, e2, e3);
            var node = graph.GetNodeEdge(new Coordinate(50, 39));
            CheckNodeValid(node);
        }


        [Test]
        public void TestCCWAfterInserts2()
        {
            var e1 = CreateLine(50, 200, 0, 200);
            var e2 = CreateLine(50, 200, 190, 50, 50, 50);
            var e3 = CreateLine(50, 200, 200, 200, 0, 200);

            var graph = CreateGraph(e1, e2, e3);
            var node = graph.GetNodeEdge(new Coordinate(50, 200));
            CheckNodeValid(node);
        }

        private void CheckNext(OverlayEdge e, OverlayEdge eNext)
        {
            Assert.AreEqual(eNext, e.Next);
        }

        private void CheckPrev(OverlayEdge e, OverlayEdge ePrev)
        {
            Assert.AreEqual(ePrev, e.Prev);
        }

        private void CheckNodeValid(OverlayEdge e)
        {
            bool isNodeValid = e.IsEdgesSorted;
            Assert.That(isNodeValid, Is.True, "Found non-sorted edges around node {0}", e.ToStringNode());
        }

        private static OverlayEdge FindEdge(OverlayGraph graph, double orgx, double orgy, double destx, double desty)
        {
            var edges = graph.Edges;
            foreach (var e in edges)
            {
                if (IsEdgeOrgDest(e, orgx, orgy, destx, desty))
                {
                    return e;
                }
                if (IsEdgeOrgDest(e.SymOE, orgx, orgy, destx, desty))
                {
                    return e.SymOE;
                }
            }
            return null;
        }

        private static bool IsEdgeOrgDest(OverlayEdge e, double orgx, double orgy, double destx, double desty)
        {
            if (!IsEqual(e.Orig, orgx, orgy)) return false;
            if (!IsEqual(e.Dest, destx, desty)) return false;
            return true;
        }

        private static bool IsEqual(Coordinate p, double x, double y)
        {
            return p.X == x && p.Y == y;
        }

        private OverlayGraph CreateGraph(params Coordinate[][] edges)
        {
            var graph = new OverlayGraph();
            foreach (var e in edges)
            {
                graph.AddEdge(e, new OverlayLabel());
            }
            return graph;
        }

        private OverlayEdge AddEdge(OverlayGraph graph, double x1, double y1, double x2, double y2)
        {
            var pts = new [] {
                new Coordinate(x1, y1), new Coordinate(x2, y2)
            };
            return graph.AddEdge(pts, new OverlayLabel());
        }

        private Coordinate[] CreateLine(params double[] ord)
        {
            var pts = ToCoordinates(ord);
            return pts;
        }

        private Coordinate[] ToCoordinates(double[] ord)
        {
            var pts = new Coordinate[ord.Length / 2];
            for (int i = 0; i < pts.Length; i++)
            {
                pts[i] = new Coordinate(ord[2 * i], ord[2 * i + 1]);
            }
            return pts;
        }
    }
}
