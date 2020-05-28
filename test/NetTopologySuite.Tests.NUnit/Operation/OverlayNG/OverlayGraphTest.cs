using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.OverlayNg;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Tests.NUnit.Operation.OverlayNG
{
    public partial class OverlayGraphTest : GeometryTestCase
    {

        /**
         * This test produced an error using the old HalfEdge sorting algorithm
         * (in {@link HalfEdge#insert(HalfEdge)}).
         */
        [Test]
        public void TestCCWAfterInserts()
        {
            var e1 = CreateEdge(50, 39, 35, 42, 37, 30);
            var e2 = CreateEdge(50, 39, 50, 60, 20, 60);
            var e3 = CreateEdge(50, 39, 68, 35);

            var graph = new OverlayGraph(CreateEdgeList(e1, e2, e3));
            var node = graph.GetNodeEdge(new Coordinate(50, 39));
            CheckNodeValid(node);
        }


        [Test]
        public void TestCCWAfterInserts2()
        {
            var e1 = CreateEdge(50, 200, 0, 200);
            var e2 = CreateEdge(50, 200, 190, 50, 50, 50);
            var e3 = CreateEdge(50, 200, 200, 200, 0, 200);

            var graph = new OverlayGraph(CreateEdgeList(e1, e2, e3));
            var node = graph.GetNodeEdge(new Coordinate(50, 200));
            CheckNodeValid(node);
        }

        private void CheckNodeValid(OverlayEdge e)
        {
            bool isNodeValid = e.IsEdgesSorted;
            Assert.That(isNodeValid, Is.True, "Found non-sorted edges around node {0}", e.ToStringNode());
        }

        private ICollection<Edge> CreateEdgeList(params Edge[] edges)
        {
            var edgeList = new List<Edge>();
            foreach (var e in edges)
            {
                edgeList.Add(e);
            }
            return edgeList;
        }

        private Edge CreateEdge(params double[] ord)
        {
            var pts = ToCoordinates(ord);
            return new Edge(pts, new EdgeSourceInfo(0));
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
