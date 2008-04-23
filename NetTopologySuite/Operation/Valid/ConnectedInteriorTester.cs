using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Operation.Overlay;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Valid
{
    /// <summary> 
    /// This class tests that the interior of an area <see cref="Geometry{TCoordinate}" />
    /// (a descendent of <see cref="ISurface{TCoordinate}"/> such as 
    /// <see cref="IPolygon{TCoordinate}" /> or <see cref="IMultiPolygon{TCoordinate}" />)
    /// is connected.  An area Geometry is invalid if the interior is disconnected.
    /// This can happen if:
    /// - a shell self-intersects,
    /// - one or more holes form a connected chain touching a shell at two different points,
    /// - one or more holes form a ring around a subset of the interior.
    /// If a disconnected situation is found the location of the problem is recorded.
    /// </summary>
    public class ConnectedInteriorTester<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<Double, TCoordinate>, IConvertible
    {
        public static TCoordinate FindDifferentPoint(IEnumerable<TCoordinate> coord, TCoordinate pt)
        {
            foreach (TCoordinate c in coord)
            {
                if (!c.Equals(pt))
                {
                    return c;
                }
            }

            return default(TCoordinate);
        }

        private readonly IGeometryFactory<TCoordinate> _geometryFactory;
        private readonly GeometryGraph<TCoordinate> _geometryGraph;

        // save a coordinate for any disconnected interior found
        // the coordinate will be somewhere on the ring surrounding the disconnected interior
        private TCoordinate _disconnectedRingCoord;

        public ConnectedInteriorTester(GeometryGraph<TCoordinate> geometryGraph, IGeometryFactory<TCoordinate> geoFactory)
        {
            _geometryFactory = geoFactory;
            _geometryGraph = geometryGraph;
        }

        public TCoordinate Coordinate
        {
            get { return _disconnectedRingCoord; }
        }

        public Boolean AreInteriorsConnected
        {
            get
            {
                // node the edges, in case holes touch the shell
                IEnumerable<Edge<TCoordinate>> splitEdges = _geometryGraph.ComputeSplitEdges();

                // form the edges into rings
                PlanarGraph<TCoordinate> graph = new PlanarGraph<TCoordinate>(new OverlayNodeFactory<TCoordinate>());
                graph.AddEdges(splitEdges);
                setInteriorEdgesInResult(graph);
                graph.LinkResultDirectedEdges();
                IEnumerable<EdgeRing<TCoordinate>> edgeRings = buildEdgeRings(graph.EdgeEnds);

                /*
                 * Mark all the edges for the edgeRings corresponding to the shells
                 * of the input polygons.  Note only ONE ring gets marked for each shell.
                 */
                visitShellInteriors(_geometryGraph.Geometry, graph);

                /*
                 * If there are any unvisited shell edges
                 * (i.e. a ring which is not a hole and which has the interior
                 * of the parent area on the RHS)
                 * this means that one or more holes must have split the interior of the
                 * polygon into at least two pieces.  The polygon is thus invalid.
                 */
                return !hasUnvisitedShellEdge(edgeRings);
            }
        }

        protected static void VisitLinkedDirectedEdges(DirectedEdge<TCoordinate> start)
        {
            DirectedEdge<TCoordinate> startDe = start;
            DirectedEdge<TCoordinate> de = start;

            do
            {
                Debug.Assert(de != null);
                de.Visited = true;
                de = de.Next;
            } while (de != startDe);
        }

        private static void setInteriorEdgesInResult(PlanarGraph<TCoordinate> graph)
        {
            foreach (DirectedEdge<TCoordinate> de in graph.EdgeEnds)
            {
                Debug.Assert(de.Label.HasValue);

                if (de.Label.Value[0, Positions.Right] == Locations.Interior)
                {
                    de.InResult = true;
                }
            }
        }

        /// <summary>
        /// Form <see cref="DirectedEdge{TCoordinate}" />s in graph into Minimal EdgeRings.
        /// (Minimal Edgerings must be used, because only they are guaranteed to provide
        /// a correct isHole computation).
        /// </summary>
        private IEnumerable<EdgeRing<TCoordinate>> buildEdgeRings(IEnumerable<EdgeEnd<TCoordinate>> dirEdges)
        {
            foreach (DirectedEdge<TCoordinate> de in dirEdges)
            {
                Debug.Assert(de != null);

                // if this edge has not yet been processed
                if (de.IsInResult && de.EdgeRing == null)
                {
                    MaximalEdgeRing<TCoordinate> er = new MaximalEdgeRing<TCoordinate>(de, _geometryFactory);

                    er.LinkDirectedEdgesForMinimalEdgeRings();
                    IEnumerable<MinimalEdgeRing<TCoordinate>> minEdgeRings = er.BuildMinimalRings();

                    foreach (MinimalEdgeRing<TCoordinate> minimalEdgeRing in minEdgeRings)
                    {
                        yield return minimalEdgeRing;
                    }
                }
            }
        }

        /// <summary>
        /// Mark all the edges for the edgeRings corresponding to the shells of the input polygons.  
        /// Only ONE ring gets marked for each shell - if there are others which remain unmarked
        /// this indicates a disconnected interior.
        /// </summary>
        private void visitShellInteriors(IGeometry<TCoordinate> g, PlanarGraph<TCoordinate> graph)
        {
            if (g is IPolygon<TCoordinate>)
            {
                IPolygon<TCoordinate> p = g as IPolygon<TCoordinate>;
                visitInteriorRing(p.ExteriorRing, graph);
            }

            if (g is IMultiPolygon<TCoordinate>)
            {
                IMultiPolygon<TCoordinate> mp = g as IMultiPolygon<TCoordinate>;

                foreach (IPolygon<TCoordinate> p in mp)
                {
                    visitInteriorRing(p.ExteriorRing, graph);
                }
            }
        }

        private void visitInteriorRing(ILineString<TCoordinate> ring, PlanarGraph<TCoordinate> graph)
        {
            IEnumerable<TCoordinate> pts = ring.Coordinates;
            TCoordinate pt0 = Slice.GetFirst(pts);

            /*
             * Find first point in coord list different to initial point.
             * Need special check since the first point may be repeated.
             */
            TCoordinate pt1 = FindDifferentPoint(pts, pt0);

            Edge<TCoordinate> e = graph.FindEdgeInSameDirection(pt0, pt1);
            DirectedEdge<TCoordinate> de = (DirectedEdge<TCoordinate>)graph.FindEdgeEnd(e);
            DirectedEdge<TCoordinate> intDe = null;

            if (de.Label.Value[0, Positions.Right] == Locations.Interior)
            {
                intDe = de;
            }
            else if (de.Sym.Label.Value[0, Positions.Right] == Locations.Interior)
            {
                intDe = de.Sym;
            }

            Assert.IsTrue(intDe != null, "unable to find dirEdge with Interior on RHS");
            VisitLinkedDirectedEdges(intDe);
        }

        /// <summary>
        /// Check if any shell ring has an unvisited edge.
        /// A shell ring is a ring which is not a hole and which has the interior
        /// of the parent area on the RHS.
        /// (Note that there may be non-hole rings with the interior on the LHS,
        /// since the interior of holes will also be polygonized into CW rings
        /// by the <c>LinkAllDirectedEdges()</c> step).
        /// </summary>
        /// <returns><see langword="true"/> if there is an unvisited edge in a non-hole ring.</returns>
        private Boolean hasUnvisitedShellEdge(IEnumerable<EdgeRing<TCoordinate>> edgeRings)
        {
            foreach (EdgeRing<TCoordinate> er in edgeRings)
            {
                if (er.IsHole)
                {
                    continue;
                }

                IEnumerable<DirectedEdge<TCoordinate>> edges = er.Edges;
                DirectedEdge<TCoordinate> de = Slice.GetFirst(edges);

                Debug.Assert(de.Label.HasValue);

                // don't check CW rings which are holes
                if (de.Label.Value[0, Positions.Right] != Locations.Interior)
                {
                    continue;
                }

                // must have a CW ring which surrounds the INT of the area, so check all
                // edges have been visited
                foreach (DirectedEdge<TCoordinate> de2 in edges)
                {
                    if (!de2.IsVisited)
                    {
                        _disconnectedRingCoord = de2.Coordinate;
                        return true;
                    }
                }
            }

            return false;
        }
    }
}