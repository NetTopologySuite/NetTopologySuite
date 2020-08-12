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

            var graph = createGraph(line1, line2, line3);

            var e1 = findEdge(graph, 0, 0, 10, 10);
            var e2 = findEdge(graph, 10, 10, 0, 10);
            var e3 = findEdge(graph, 0, 10, 0, 0);

            CheckNodeValid(e1);
            CheckNodeValid(e2);
            CheckNodeValid(e3);

            checkNext(e1, e2);
            checkNext(e2, e3);
            checkNext(e3, e1);

            var e1sym = findEdge(graph, 10, 10, 0, 0);
            var e2sym = findEdge(graph, 0, 10, 10, 10);
            var e3sym = findEdge(graph, 0, 0, 0, 10);

            Assert.AreEqual(e1sym, e1.Sym);
            Assert.AreEqual(e2sym, e2.Sym);
            Assert.AreEqual(e3sym, e3.SymOE);

            checkNext(e1sym, e3sym);
            checkNext(e2sym, e1sym);
            checkNext(e3sym, e2sym);
        }

        [Test]
        public void TestStar()
        {

            var graph = new OverlayGraph();

            var e1 = addEdge(graph, 5, 5, 0, 0);
            var e2 = addEdge(graph, 5, 5, 0, 9);
            var e3 = addEdge(graph, 5, 5, 9, 9);

            CheckNodeValid(e1);

            checkNext(e1, e1.SymOE);
            checkNext(e2, e2.SymOE);
            checkNext(e3, e3.SymOE);

            checkPrev(e1, e3.SymOE);
            checkPrev(e2, e1.SymOE);
            checkPrev(e3, e2.SymOE);
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

            var graph = createGraph(e1, e2, e3);
            var node = graph.GetNodeEdge(new Coordinate(50, 39));
            CheckNodeValid(node);
        }


        [Test]
        public void TestCCWAfterInserts2()
        {
            var e1 = CreateLine(50, 200, 0, 200);
            var e2 = CreateLine(50, 200, 190, 50, 50, 50);
            var e3 = CreateLine(50, 200, 200, 200, 0, 200);

            var graph = createGraph(e1, e2, e3);
            var node = graph.GetNodeEdge(new Coordinate(50, 200));
            CheckNodeValid(node);
        }

        private void checkNext(OverlayEdge e, OverlayEdge eNext)
        {
            Assert.AreEqual(eNext, e.Next);
        }

        private void checkPrev(OverlayEdge e, OverlayEdge ePrev)
        {
            Assert.AreEqual(ePrev, e.Prev);
        }

        private void CheckNodeValid(OverlayEdge e)
        {
            bool isNodeValid = e.IsEdgesSorted;
            Assert.That(isNodeValid, Is.True, "Found non-sorted edges around node {0}", e.ToStringNode());
        }

        private static OverlayEdge findEdge(OverlayGraph graph, double orgx, double orgy, double destx, double desty)
        {
            var edges = graph.Edges;
            foreach (var e in edges)
            {
                if (isEdgeOrgDest(e, orgx, orgy, destx, desty))
                {
                    return e;
                }
                if (isEdgeOrgDest(e.SymOE, orgx, orgy, destx, desty))
                {
                    return e.SymOE;
                }
            }
            return null;
        }

        private static bool isEdgeOrgDest(OverlayEdge e, double orgx, double orgy, double destx, double desty)
        {
            if (!isEqual(e.Orig, orgx, orgy)) return false;
            if (!isEqual(e.Dest, destx, desty)) return false;
            return true;
        }

        private static bool isEqual(Coordinate p, double x, double y)
        {
            return p.X == x && p.Y == y;
        }

        private OverlayGraph createGraph(params Coordinate[][] edges)
        {
            var graph = new OverlayGraph();
            foreach (var e in edges)
            {
                graph.AddEdge(e, new OverlayLabel());
            }
            return graph;
        }

        private OverlayEdge addEdge(OverlayGraph graph, double x1, double y1, double x2, double y2)
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
