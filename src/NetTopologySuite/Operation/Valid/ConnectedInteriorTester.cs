using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Utilities;
using Position = NetTopologySuite.Geometries.Position;

namespace NetTopologySuite.Operation.Valid
{
    /// <summary>
    /// This class tests that the interior of an area <see cref="Geometry" />
    /// (<see cref="Polygon" /> or <see cref="MultiPolygon" />)
    /// is connected.  An area Geometry is invalid if the interior is disconnected.
    /// This can happen if:
    /// - a shell self-intersects,
    /// - one or more holes form a connected chain touching a shell at two different points,
    /// - one or more holes form a ring around a subset of the interior.
    /// If a disconnected situation is found the location of the problem is recorded.
    /// </summary>
    [Obsolete]
    public class ConnectedInteriorTester
    {
        /// <summary>
        ///
        /// </summary>
        /// <param name="coord"></param>
        /// <param name="pt"></param>
        /// <returns></returns>
        public static Coordinate FindDifferentPoint(Coordinate[] coord, Coordinate pt)
        {
            foreach (var c in coord)
                if (!c.Equals(pt))
                    return c;
            return null;
        }

        private readonly GeometryFactory _geometryFactory = new GeometryFactory();

        private readonly GeometryGraph _geomGraph;

        // save a coordinate for any disconnected interior found
        // the coordinate will be somewhere on the ring surrounding the disconnected interior
        private Coordinate _disconnectedRingcoord;

        /// <summary>
        ///
        /// </summary>
        /// <param name="geomGraph"></param>
        public ConnectedInteriorTester(GeometryGraph geomGraph)
        {
            _geomGraph = geomGraph;
        }

        /// <summary>
        ///
        /// </summary>
        public Coordinate Coordinate => _disconnectedRingcoord;

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public bool IsInteriorsConnected()
        {
            // node the edges, in case holes touch the shell
            var splitEdges = new List<Edge>();
            _geomGraph.ComputeSplitEdges(splitEdges);

            // form the edges into rings
            var graph = new PlanarGraph(new OverlayNodeFactory());
            graph.AddEdges(splitEdges);
            SetInteriorEdgesInResult(graph);
            graph.LinkResultDirectedEdges();
            var edgeRings = BuildEdgeRings(graph.EdgeEnds);
            /*
             * Mark all the edges for the edgeRings corresponding to the shells
             * of the input polygons.  Note only ONE ring gets marked for each shell.
             */
            VisitShellInteriors(this._geomGraph.Geometry, graph);

            /*
             * If there are any unvisited shell edges
             * (i.e. a ring which is not a hole and which has the interior
             * of the parent area on the RHS)
             * this means that one or more holes must have split the interior of the
             * polygon into at least two pieces.  The polygon is thus invalid.
             */
            return !HasUnvisitedShellEdge(edgeRings);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="graph"></param>
        private static void SetInteriorEdgesInResult(PlanarGraph graph)
        {
            foreach (DirectedEdge de in graph.EdgeEnds)
                if (de.Label.GetLocation(0, Position.Right) == Location.Interior)
                    de.InResult = true;
        }

        /// <summary>
        /// Form <see cref="DirectedEdge" />s in graph into Minimal EdgeRings.
        /// (Minimal Edgerings must be used, because only they are guaranteed to provide
        /// a correct isHole computation).
        /// </summary>
        /// <param name="dirEdges"></param>
        /// <returns></returns>
        private IList<EdgeRing> BuildEdgeRings(IEnumerable<EdgeEnd> dirEdges)
        {
            var edgeRings = new List<EdgeRing>();
            foreach (DirectedEdge de in dirEdges)
            {
                // if this edge has not yet been processed
                if (de.IsInResult && de.EdgeRing == null)
                {
                    var er = new MaximalEdgeRing(de, _geometryFactory);

                    er.LinkDirectedEdgesForMinimalEdgeRings();
                    var minEdgeRings = er.BuildMinimalRings();
                    foreach(var o in minEdgeRings)
                        edgeRings.Add(o);
                }
            }
            return edgeRings;
        }

        /// <summary>
        /// Mark all the edges for the edgeRings corresponding to the shells of the input polygons.
        /// Only ONE ring gets marked for each shell - if there are others which remain unmarked
        /// this indicates a disconnected interior.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="graph"></param>
        private void VisitShellInteriors(Geometry g, PlanarGraph graph)
        {
            if (g is Polygon)
            {
                var p = (Polygon) g;
                VisitInteriorRing(p.Shell, graph);
            }
            if (g is MultiPolygon)
            {
                var mp = (MultiPolygon) g;
                foreach (Polygon p in mp.Geometries)
                    VisitInteriorRing(p.Shell, graph);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ring"></param>
        /// <param name="graph"></param>
        private void VisitInteriorRing(LineString ring, PlanarGraph graph)
        {
            if (ring.IsEmpty)
            {
                return;
            }

            var pts = ring.Coordinates;
            var pt0 = pts[0];
            /*
             * Find first point in coord list different to initial point.
             * Need special check since the first point may be repeated.
             */
            var pt1 = FindDifferentPoint(pts, pt0);
            var e = graph.FindEdgeInSameDirection(pt0, pt1);
            var de = (DirectedEdge) graph.FindEdgeEnd(e);
            DirectedEdge intDe = null;
            if (de.Label.GetLocation(0, Position.Right) == Location.Interior)
                intDe = de;
            else if (de.Sym.Label.GetLocation(0, Position.Right) == Location.Interior)
                intDe = de.Sym;
            Assert.IsTrue(intDe != null, "unable to find dirEdge with Interior on RHS");
            VisitLinkedDirectedEdges(intDe);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="start"></param>
        protected void VisitLinkedDirectedEdges(DirectedEdge start)
        {
            var startDe = start;
            var de = start;
            do
            {
                Assert.IsTrue(de != null, "found null Directed Edge");
                de.Visited = true;
                de = de.Next;
            }
            while (de != startDe);
        }

        /// <summary>
        /// Check if any shell ring has an unvisited edge.
        /// A shell ring is a ring which is not a hole and which has the interior
        /// of the parent area on the RHS.
        /// (Note that there may be non-hole rings with the interior on the LHS,
        /// since the interior of holes will also be polygonized into CW rings
        /// by the <c>LinkAllDirectedEdges()</c> step).
        /// </summary>
        /// <param name="edgeRings"></param>
        /// <returns><c>true</c> if there is an unvisited edge in a non-hole ring.</returns>
        private bool HasUnvisitedShellEdge(IList<EdgeRing> edgeRings)
        {
            for (int i = 0; i < edgeRings.Count; i++)
            {
                var er = edgeRings[i];
                if (er.IsHole) continue;
                var edges = er.Edges;
                var de = edges[0];
                // don't check CW rings which are holes
                if (de.Label.GetLocation(0, Position.Right) != Location.Interior) continue;

                // must have a CW ring which surrounds the INT of the area, so check all
                // edges have been visited
                for (int j = 0; j < edges.Count; j++)
                {
                    de = edges[j];
                    if (!de.IsVisited)
                    {
                        _disconnectedRingcoord = de.Coordinate;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
