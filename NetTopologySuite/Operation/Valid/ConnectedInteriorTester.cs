using System;
using System.Collections;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Operation.Overlay;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Operation.Valid
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
    public class ConnectedInteriorTester
    {
        public static ICoordinate FindDifferentPoint(ICoordinate[] coord, ICoordinate pt)
        {
            foreach (ICoordinate c in coord)
            {
                if (!c.Equals(pt))
                {
                    return c;
                }
            }
            return null;
        }

        private GeometryFactory geometryFactory = new GeometryFactory();

        private GeometryGraph geomGraph;

        // save a coordinate for any disconnected interior found
        // the coordinate will be somewhere on the ring surrounding the disconnected interior
        private ICoordinate disconnectedRingcoord;

        public ConnectedInteriorTester(GeometryGraph geomGraph)
        {
            this.geomGraph = geomGraph;
        }

        public ICoordinate Coordinate
        {
            get { return disconnectedRingcoord; }
        }

        public Boolean IsInteriorsConnected()
        {
            // node the edges, in case holes touch the shell
            IList splitEdges = new ArrayList();
            geomGraph.ComputeSplitEdges(splitEdges);

            // form the edges into rings
            PlanarGraph graph = new PlanarGraph(new OverlayNodeFactory());
            graph.AddEdges(splitEdges);
            SetInteriorEdgesInResult(graph);
            graph.LinkResultDirectedEdges();
            IList edgeRings = BuildEdgeRings(graph.EdgeEnds);
            /*
             * Mark all the edges for the edgeRings corresponding to the shells
             * of the input polygons.  Note only ONE ring gets marked for each shell.
             */
            VisitShellInteriors(geomGraph.Geometry, graph);

            /*
             * If there are any unvisited shell edges
             * (i.e. a ring which is not a hole and which has the interior
             * of the parent area on the RHS)
             * this means that one or more holes must have split the interior of the
             * polygon into at least two pieces.  The polygon is thus invalid.
             */
            return !HasUnvisitedShellEdge(edgeRings);
        }

        private void SetInteriorEdgesInResult(PlanarGraph graph)
        {
            foreach (DirectedEdge de in graph.EdgeEnds)
            {
                if (de.Label.GetLocation(0, Positions.Right) == Locations.Interior)
                {
                    de.InResult = true;
                }
            }
        }

        /// <summary>
        /// Form <see cref="DirectedEdge" />s in graph into Minimal EdgeRings.
        /// (Minimal Edgerings must be used, because only they are guaranteed to provide
        /// a correct isHole computation).
        /// </summary>
        private IList BuildEdgeRings(IList dirEdges)
        {
            IList edgeRings = new ArrayList();
            foreach (DirectedEdge de in dirEdges)
            {
                // if this edge has not yet been processed
                if (de.IsInResult && de.EdgeRing == null)
                {
                    MaximalEdgeRing er = new MaximalEdgeRing(de, geometryFactory);

                    er.LinkDirectedEdgesForMinimalEdgeRings();
                    IList minEdgeRings = er.BuildMinimalRings();
                    foreach (object o in minEdgeRings)
                    {
                        edgeRings.Add(o);
                    }
                }
            }
            return edgeRings;
        }

        /// <summary>
        /// Mark all the edges for the edgeRings corresponding to the shells of the input polygons.  
        /// Only ONE ring gets marked for each shell - if there are others which remain unmarked
        /// this indicates a disconnected interior.
        /// </summary>
        private void VisitShellInteriors(IGeometry g, PlanarGraph graph)
        {
            if (g is IPolygon)
            {
                IPolygon p = (IPolygon) g;
                VisitInteriorRing(p.Shell, graph);
            }
            if (g is IMultiPolygon)
            {
                IMultiPolygon mp = (IMultiPolygon) g;
                foreach (IPolygon p in mp.Geometries)
                {
                    VisitInteriorRing(p.Shell, graph);
                }
            }
        }

        private void VisitInteriorRing(ILineString ring, PlanarGraph graph)
        {
            ICoordinate[] pts = ring.Coordinates;
            ICoordinate pt0 = pts[0];
            /*
             * Find first point in coord list different to initial point.
             * Need special check since the first point may be repeated.
             */
            ICoordinate pt1 = FindDifferentPoint(pts, pt0);
            Edge e = graph.FindEdgeInSameDirection(pt0, pt1);
            DirectedEdge de = (DirectedEdge) graph.FindEdgeEnd(e);
            DirectedEdge intDe = null;
            if (de.Label.GetLocation(0, Positions.Right) == Locations.Interior)
            {
                intDe = de;
            }
            else if (de.Sym.Label.GetLocation(0, Positions.Right) == Locations.Interior)
            {
                intDe = de.Sym;
            }
            Assert.IsTrue(intDe != null, "unable to find dirEdge with Interior on RHS");
            VisitLinkedDirectedEdges(intDe);
        }

        protected void VisitLinkedDirectedEdges(DirectedEdge start)
        {
            DirectedEdge startDe = start;
            DirectedEdge de = start;
            do
            {
                Assert.IsTrue(de != null, "found null Directed Edge");
                de.Visited = true;
                de = de.Next;
            } while (de != startDe);
        }

        /// <summary>
        /// Check if any shell ring has an unvisited edge.
        /// A shell ring is a ring which is not a hole and which has the interior
        /// of the parent area on the RHS.
        /// (Note that there may be non-hole rings with the interior on the LHS,
        /// since the interior of holes will also be polygonized into CW rings
        /// by the <c>LinkAllDirectedEdges()</c> step).
        /// </summary>
        /// <returns><c>true</c> if there is an unvisited edge in a non-hole ring.</returns>
        private Boolean HasUnvisitedShellEdge(IList edgeRings)
        {
            for (Int32 i = 0; i < edgeRings.Count; i++)
            {
                EdgeRing er = (EdgeRing) edgeRings[i];
                if (er.IsHole)
                {
                    continue;
                }
                IList edges = er.Edges;
                DirectedEdge de = (DirectedEdge) edges[0];
                // don't check CW rings which are holes
                if (de.Label.GetLocation(0, Positions.Right) != Locations.Interior)
                {
                    continue;
                }

                // must have a CW ring which surrounds the INT of the area, so check all
                // edges have been visited
                for (Int32 j = 0; j < edges.Count; j++)
                {
                    de = (DirectedEdge) edges[j];
                    if (!de.IsVisited)
                    {
                        disconnectedRingcoord = de.Coordinate;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}