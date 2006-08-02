using System;
using System.Collections;
using System.Text;

using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.Operation.Overlay;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.Operation.Valid
{
    /// <summary> 
    /// This class tests that the interior of an area <c>Geometry</c>
    /// (<c>Polygon</c>  or <c>MultiPolygon</c> )
    /// is connected.  An area Geometry is invalid if the interior is disconnected.
    /// This can happen if:
    /// One or more holes either form a chain touching the shell at two places.
    /// One or more holes form a ring around a portion of the interior.
    /// If an inconsistency if found the location of the problem is recorded.
    /// </summary>
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
            for (int i = 0; i < coord.Length; i++)
                if (!coord[i].Equals(pt))
                    return coord[i];            
            return null;
        }

        private GeometryFactory geometryFactory = new GeometryFactory();

        private GeometryGraph geomGraph;

        // save a coordinate for any disconnected interior found
        // the coordinate will be somewhere on the ring surrounding the disconnected interior
        private Coordinate disconnectedRingcoord;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="geomGraph"></param>
        public ConnectedInteriorTester(GeometryGraph geomGraph)
        {
            this.geomGraph = geomGraph;
        }

        /// <summary>
        /// 
        /// </summary>
        public virtual Coordinate Coordinate
        {
            get
            {
                return disconnectedRingcoord;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public virtual bool IsInteriorsConnected()
        {
            // node the edges, in case holes touch the shell
            IList splitEdges = new ArrayList();
            geomGraph.ComputeSplitEdges(splitEdges);

            // polygonize the edges
            PlanarGraph graph = new PlanarGraph(new OverlayNodeFactory());
            graph.AddEdges(splitEdges);
            SetAllEdgesInResult(graph);
            graph.LinkAllDirectedEdges();
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="graph"></param>
        private void SetAllEdgesInResult(PlanarGraph graph)
        {
            for (IEnumerator it = graph.EdgeEnds.GetEnumerator(); it.MoveNext(); )
            {
                DirectedEdge de = (DirectedEdge)it.Current;
                de.InResult = true;
            }
        }

        /// <summary>
        /// For all DirectedEdges in result, form them into EdgeRings.
        /// </summary>
        /// <param name="dirEdges"></param>
        /// <returns></returns>
        private IList BuildEdgeRings(IList dirEdges)
        {
            IList edgeRings = new ArrayList();
            for (IEnumerator it = dirEdges.GetEnumerator(); it.MoveNext(); )
            {
                DirectedEdge de = (DirectedEdge)it.Current;
                // if this edge has not yet been processed
                if (de.EdgeRing == null)
                {
                    EdgeRing er = new MaximalEdgeRing(de, geometryFactory);
                    edgeRings.Add(er);
                }
            }
            return edgeRings;
        }

        /// <summary>
        /// Mark all the edges for the edgeRings corresponding to the shells
        /// of the input polygons.  Note only ONE ring gets marked for each shell.
        /// </summary>
        /// <param name="g"></param>
        /// <param name="graph"></param>
        private void VisitShellInteriors(Geometry g, PlanarGraph graph)
        {
            if (g is Polygon) 
            {
                Polygon p = (Polygon) g;
                VisitInteriorRing(p.ExteriorRing, graph);
            }
            if (g is MultiPolygon) 
            {
                MultiPolygon mp = (MultiPolygon) g;
                for (int i = 0; i < mp.NumGeometries; i++) 
                {
                    Polygon p = (Polygon) mp.GetGeometryN(i);
                    VisitInteriorRing(p.ExteriorRing, graph);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ring"></param>
        /// <param name="graph"></param>
        private void VisitInteriorRing(LineString ring, PlanarGraph graph)
        {
            Coordinate[] pts = ring.Coordinates;
            Coordinate pt0 = pts[0];
            /*
             * Find first point in coord list different to initial point.
             * Need special check since the first point may be repeated.
             */
            Coordinate pt1 = FindDifferentPoint(pts, pt0);
            Edge e = graph.FindEdgeInSameDirection(pt0, pt1);
            DirectedEdge de = (DirectedEdge)graph.FindEdgeEnd(e);
            DirectedEdge intDe = null;
            if (de.Label.GetLocation(0, Positions.Right) == Locations.Interior)
                intDe = de;            
            else if (de.Sym.Label.GetLocation(0, Positions.Right) == Locations.Interior)            
                intDe = de.Sym;            
            Assert.IsTrue(intDe != null, "unable to find dirEdge with Interior on RHS");
            VisitLinkedDirectedEdges(intDe);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="start"></param>
        protected virtual void VisitLinkedDirectedEdges(DirectedEdge start)
        {
            DirectedEdge startDe = start;
            DirectedEdge de = start;
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
        private bool HasUnvisitedShellEdge(IList edgeRings)
        {
            for (int i = 0; i < edgeRings.Count; i++)
            {
                EdgeRing er = (EdgeRing)edgeRings[i];
                if (er.IsHole) continue;
                IList edges = er.Edges;
                DirectedEdge de = (DirectedEdge)edges[0];
                // don't check CW rings which are holes
                if (de.Label.GetLocation(0, Positions.Right) != Locations.Interior) continue;

                // must have a CW ring which surrounds the INT of the area, so check all
                // edges have been visited
                for (int j = 0; j < edges.Count; j++)
                {
                    de = (DirectedEdge)edges[j];
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
