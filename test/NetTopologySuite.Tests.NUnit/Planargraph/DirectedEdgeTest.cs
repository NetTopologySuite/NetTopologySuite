using System;
using NetTopologySuite.Geometries;
using NetTopologySuite.Planargraph;
using NUnit.Framework;

namespace NetTopologySuite.Tests.NUnit.Planargraph
{
    public class DirectedEdgeTest
    {
        [Test]
        public void TestDirectedEdgeComparator()
        {
            var d1 = new DirectedEdge(new Node(new Coordinate(0, 0)),
                new Node(new Coordinate(10, 10)), new Coordinate(10, 10), true);
            var d2 = new DirectedEdge(new Node(new Coordinate(0, 0)),
                new Node(new Coordinate(20, 20)), new Coordinate(20, 20), false);
            Assert.That(d2.CompareTo(d1), Is.EqualTo(0));
        }

        [Test]
        public void TestDirectedEdgeToEdges()
        {
            var d1 = new DirectedEdge(new Node(new Coordinate(0, 0)),
                new Node(new Coordinate(10, 10)), new Coordinate(10, 10), true);
            var d2 = new DirectedEdge(new Node(new Coordinate(20, 0)),
                new Node(new Coordinate(20, 10)), new Coordinate(20, 10), false);
            var edges = DirectedEdge.ToEdges(new [] { d1, d2 });

            Assert.That(edges.Count, Is.EqualTo(2));
            Assert.That(edges[0], Is.Null);
            Assert.That(edges[1], Is.Null);
        }
    }
}
