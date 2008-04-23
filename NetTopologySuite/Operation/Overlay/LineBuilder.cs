using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Overlay
{
    /// <summary>
    /// Forms NTS LineStrings out of a the graph of <c>DirectedEdge</c>s
    /// created by an <c>OverlayOp</c>.
    /// </summary>
    public class LineBuilder<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, IComparable<TCoordinate>,
            IComputable<Double, TCoordinate>, IConvertible
    {
        private readonly OverlayOp<TCoordinate> _op;
        private readonly IGeometryFactory<TCoordinate> _geometryFactory;

        // [codekaizen 2008-01-06] field '_ptLocator' isn't used in JTS source LineBuilder.java rev. 1.15
        //private readonly PointLocator<TCoordinate> _ptLocator;

        public LineBuilder(OverlayOp<TCoordinate> op, IGeometryFactory<TCoordinate> geometryFactory)
        {
            _op = op;
            _geometryFactory = geometryFactory;
            //_ptLocator = ptLocator;
        }

        /// <summary>
        /// Returns a list of the <see cref="ILineString{TCoordinate}"/>s 
        /// in the result of the specified overlay operation.
        /// </summary>
        public IEnumerable<ILineString<TCoordinate>> Build(SpatialFunctions opCode)
        {
            findCoveredLineEdges();
            IEnumerable<Edge<TCoordinate>> edges = collectLines(opCode);
            return buildLines(opCode, edges);
        }

        /// <summary>
        /// Find and mark L edges which are "covered" by the result area (if any).
        /// L edges at nodes which also have A edges can be checked by checking
        /// their depth at that node.
        /// L edges at nodes which do not have A edges can be checked by doing a
        /// point-in-polygon test with the previously computed result areas.
        /// </summary>
        private void findCoveredLineEdges()
        {
            // first set covered for all L edges at nodes which have A edges too
            foreach (Node<TCoordinate> node in _op.Graph.Nodes)
            {
                DirectedEdgeStar<TCoordinate> edges = node.Edges as DirectedEdgeStar<TCoordinate>;
                Debug.Assert(edges != null);
                edges.FindCoveredLineEdges();   
            }

            /*
             * For all Curve edges which weren't handled by the above,
             * use a point-in-poly test to determine whether they are covered
             */
            foreach (DirectedEdge<TCoordinate> de in _op.Graph.EdgeEnds)
            {
                Edge<TCoordinate> e = de.Edge;

                if (de.IsLineEdge && !e.IsCoveredSet)
                {
                    Boolean isCovered = _op.IsCoveredByArea(de.Coordinate);
                    e.Covered = isCovered;
                }   
            }
        }

        private IEnumerable<Edge<TCoordinate>> collectLines(SpatialFunctions opCode)
        {
            foreach (DirectedEdge<TCoordinate> de in _op.Graph.EdgeEnds)
            {
                IEnumerable<Edge<TCoordinate>> edges = collectLineEdge(de, opCode);
                edges = Slice.Append(collectBoundaryTouchEdge(de, opCode), edges);

                foreach (Edge<TCoordinate> edge in edges)
                {
                    yield return edge;
                }
            }
        }

        private static IEnumerable<Edge<TCoordinate>> collectLineEdge(DirectedEdge<TCoordinate> de, SpatialFunctions opCode)
        {
            Debug.Assert(de.Label.HasValue);
            Label label = de.Label.Value;
            Edge<TCoordinate> e = de.Edge;

            // include Curve edges which are in the result
            if (de.IsLineEdge)
            {
                if (!de.IsVisited && OverlayOp<TCoordinate>.IsResultOfOp(label, opCode) && !e.IsCovered)
                {
                    yield return e;
                    de.VisitedEdge = true;
                }
            }
        }

        /// <summary>
        /// Collect edges from Area inputs which should be in the result but
        /// which have not been included in a result area.
        /// This happens ONLY:
        /// during an intersection when the boundaries of two
        /// areas touch in a line segment
        /// OR as a result of a dimensional collapse.
        /// </summary>
        private static IEnumerable<Edge<TCoordinate>> collectBoundaryTouchEdge(DirectedEdge<TCoordinate> de, SpatialFunctions opCode)
        {
            if (de.IsLineEdge)
            {
                yield break; // only interested in area edges         
            }

            if (de.IsVisited)
            {
                yield break; // already processed
            }

            if (de.IsInteriorAreaEdge)
            {
                yield break; // added to handle dimensional collapses            
            }

            if (de.Edge.IsInResult)
            {
                yield break; // if the edge linework is already included, don't include it again
            }

            // sanity check for labeling of result edgerings
            Assert.IsTrue(!(de.IsInResult || de.Sym.IsInResult) || !de.Edge.IsInResult);

            Debug.Assert(de.Label.HasValue);
            Label label = de.Label.Value;

            // include the linework if it's in the result of the operation
            if (OverlayOp<TCoordinate>.IsResultOfOp(label, opCode) 
                && opCode == SpatialFunctions.Intersection)
            {
                yield return de.Edge;
                de.VisitedEdge = true;
            }
        }

        // [codekaizen 2008-01-06] parameter 'opCode' isn't used in JTS source LineBuilder.java rev. 1.15
        private IEnumerable<ILineString<TCoordinate>> buildLines(SpatialFunctions opCode, IEnumerable<Edge<TCoordinate>> edges)
        {
            foreach (Edge<TCoordinate> edge in edges)
            {
                ILineString<TCoordinate> line = _geometryFactory.CreateLineString(edge.Coordinates);
                yield return line;
                edge.InResult = true;
            }
        }

        // [codekaizen 2008-01-06] method isn't used in JTS source LineBuilder.java rev. 1.15
        //private void labelIsolatedLines(IEnumerable<Edge<TCoordinate>> edgesList)
        //{
        //    foreach (Edge<TCoordinate> edge in edgesList)
        //    {
        //        Debug.Assert(edge.Label.HasValue);
        //        Label label = edge.Label.Value;
                
        //        if (edge.IsIsolated)
        //        {
        //            if (label.IsNull(0))
        //            {
        //                labelIsolatedLine(edge, 0);
        //            }
        //            else
        //            {
        //                labelIsolatedLine(edge, 1);
        //            }
        //        }   
        //    }
        //}

        ///// <summary>
        ///// Label an isolated node with its relationship to the target point.
        ///// </summary>
        //private void labelIsolatedLine(Edge<TCoordinate> e, Int32 targetIndex)
        //{
        //    Locations loc = _ptLocator.Locate(e.Coordinate, _op.GetArgGeometry(targetIndex));
        //    e.Label.SetLocation(targetIndex, loc);
        //}
    }
}