using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;

namespace NetTopologySuite.Operation.OverlayNg
{

    /// <summary>
    /// Finds and builds overlay result lines from the overlay graph.
    /// Output linework has the following semantics:
    /// <list type="number">
    /// <item><description>Linework is fully noded</description></item>
    /// <item><description>Lines are as long as possible between nodes</description></item>
    /// </list>
    /// Various strategies are possible for how to
    /// merge graph edges into lines.
    /// This implementation uses the approach
    /// of having output lines run contiguously from node to node.
    /// For rings a node point is chosen arbitrarily.
    /// <para/>
    /// Another possible strategy would be to preserve input linework
    /// as far as possible (i.e.any sections of input lines which are not
    /// coincident with other linework would be preserved).
    /// <para/>
    /// It would also be possible to output LinearRings,
    /// if the input is a LinearRing and is unchanged.
    /// This will require additional info from the input linework.
    /// </summary>
    /// <author>Martin Davis</author>
    class LineBuilder
    {

        private readonly GeometryFactory _geometryFactory;
        private readonly OverlayGraph _graph;
        private readonly SpatialFunction _opCode;
        private readonly int _inputAreaIndex;
        private readonly bool _hasResultArea;
        private readonly List<LineString> _lines = new List<LineString>();

        /// <summary>
        /// Creates a builder for linear elements which may be present
        /// in the overlay result.
        /// </summary>
        /// <param name="inputGeom">The input geometries</param>
        /// <param name="graph">The topology graph</param>
        /// <param name="hasResultArea"><c>true</c> if an area has been generated for the result</param>
        /// <param name="opCode">The overlay operation code</param>
        /// <param name="geomFact">The output geometry factory</param>
        public LineBuilder(InputGeometry inputGeom, OverlayGraph graph, bool hasResultArea, SpatialFunction opCode, GeometryFactory geomFact)
        {
            _graph = graph;
            _opCode = opCode;
            _geometryFactory = geomFact;
            _hasResultArea = hasResultArea;
            _inputAreaIndex = inputGeom.GetAreaIndex();
        }

        public List<LineString> GetLines()
        {
            MarkResultLines();
            AddResultLines();
            return _lines;
        }

        private void MarkResultLines()
        {
            var edges = _graph.Edges;
            foreach (var edge in edges)
            {
                /*
                 * If the edge linework is already marked as in the result,
                 * it is not included as a line.
                 * This occurs when an edge either is in a result area
                 * or has already been included as a line.
                 */
                if (edge.IsInResultEither)
                    continue;
                if (IsResultLine(edge.Label))
                {
                    edge.MarkInResultLine();
                    //Debug.println(edge);
                }
            }
        }

        /// <summary>
        /// Checks if the topology indicated by an edge label
        /// determines that this edge should be part of a result line.
        /// <para/>
        /// Note that the logic here relies on the semantic
        /// that for intersection lines are only returned if
        /// there is no result area components.
        /// </summary>
        /// <param name="lbl">The label for an edge</param>
        /// <returns><c>true</c> if the edge should be included in the result</returns>
        private bool IsResultLine(OverlayLabel lbl)
        {
            /*
             * Edges which are collapses along boundaries are not output.
             * I.e a result line edge must be from a input line
             * or two coincident area boundaries.
             */
            if (lbl.IsBoundaryCollapse) return false;

            if (OverlayNG.ALLOW_INT_MIXED_INT_RESULT
                && _opCode == OverlayNG.INTERSECTION && lbl.IsBoundaryTouch)
            {
                return true;
            }


            /*
             * Skip edges that are inside result area, if there is one.
             * It is sufficient to check against an input area rather 
             * than the result area, since 
             * if lines are being included then the result area
             * must be the same as the input area. 
             * This logic relies on the semantic that if both inputs 
             * are areas, lines are only output if there is no 
             * result area.
             */
            if (_hasResultArea && lbl.IsLineInArea(_inputAreaIndex))
                return false;

            var aLoc = EffectiveLocation(lbl, 0);
            var bLoc = EffectiveLocation(lbl, 1);

            bool isInResult = OverlayNG.IsResultOfOp(_opCode, aLoc, bLoc);
            return isInResult;
        }

        /// <summary>
        /// Determines the effective location for a line,
        /// for the purpose of overlay operation evaluation.
        /// Line edges and Collapses are reported as INTERIOR
        /// so they may be included in the result
        /// if warranted by the effect of the operation on the two edges.
        /// (For instance, the intersection of a line edge and a collapsed boundary
        /// is included in the result).
        /// </summary>
        /// <param name="lbl">The label of line</param>
        /// <param name="geomIndex">The index of parent geometry</param>
        /// <returns>The effective location of the line</returns>
        private static Location EffectiveLocation(OverlayLabel lbl, int geomIndex)
        {
            if (lbl.IsCollapse(geomIndex))
                return Location.Interior;
            if (lbl.IsLineAt(geomIndex))
                return Location.Interior;
            return lbl.GetLineLocation(geomIndex);
        }

        private void AddResultLines()
        {
            var edges = _graph.Edges;
            foreach (var edge in edges)
            {
                if (!edge.IsInResultLine) continue;
                if (edge.IsVisited) continue;
      
                _lines.Add(ToLine(edge ));
                edge.MarkVisitedBoth();
            }
        }

        private LineString ToLine(OverlayEdge edge)
        {
            bool isForward = edge.IsForward;
            var pts = new CoordinateList();
            pts.Add(edge.Orig, false);
            edge.AddCoordinates(pts);

            var ptsOut = pts.ToCoordinateArray(isForward);
            var line = _geometryFactory.CreateLineString(ptsOut);
            return line;
        }

        //-----------------------------------------------
        //----  Maximal line extraction logic
        //-----------------------------------------------

        /*
         * NOT USED currently.
         * Instead the raw noded edges are output.
         * This matches the original overlay semantics.
         * It is also faster.
         */

        // FUTURE: enable merging via an option switch on OverlayNG
        private void AddResultLinesMerged()
        {
            AddResultLinesForNodes();
            AddResultLinesRings();
        }

        private void AddResultLinesForNodes()
        {
            var edges = _graph.Edges;
            foreach (var edge in edges)
            {
                if (!edge.IsInResultLine) continue;
                if (edge.IsVisited) continue;

                /**
                 * Choose line start point as a node.
                 * Nodes in the line graph are degree-1 or degree >= 3 edges.
                 * 
                 * This will find all lines originating at nodes
                 */
                if (DegreeOfLines(edge) != 2)
                {
                    _lines.Add(BuildLine(edge));
                    //Debug.println(edge);
                }
            }
        }

        /// <summary>
        /// Adds lines which form rings (i.e. have only degree-2 vertices).
        /// </summary>
        private void AddResultLinesRings()
        {
            // TODO: an ordering could be imposed on the endpoints to make this more repeatable

            // TODO: preserve input LinearRings if possible?  Would require marking them as such
            var edges = _graph.Edges;
            foreach (var edge in edges)
            {
                if (!edge.IsInResultLine) continue;
                if (edge.IsVisited) continue;

                _lines.Add(BuildLine(edge));
                //Debug.println(edge);
            }
        }

        /// <summary>
        /// Traverses edges from edgeStart which
        /// lie in a single line (have degree = 2).
        /// <para/>
        /// The direction of the linework is preserved as far as possible.
        /// Specifically, the direction of the line is determined
        /// by the start edge direction. This implies
        /// that if all edges are reversed, the created line
        /// will be reversed to match.
        /// (Other more complex strategies would be possible.
        /// E.g. using the direction of the majority of segments,
        /// or preferring the direction of the A edges.)
        /// </summary>
        private LineString BuildLine(OverlayEdge node)
        {
            var pts = new CoordinateList();
            pts.Add(node.Orig, false);

            bool isForward = node.IsForward;

            var e = node;
            do
            {
                e.MarkVisitedBoth();
                e.AddCoordinates(pts);

                // end line if next vertex is a node
                if (DegreeOfLines(e.SymOE) != 2)
                {
                    break;
                }
                e = NextLineEdgeUnvisited(e.SymOE);
                // e will be null if next edge has been visited, which indicates a ring
            }
            while (e != null);

            var ptsOut = pts.ToCoordinateArray(isForward);

            var line = _geometryFactory.CreateLineString(ptsOut);
            return line;
        }

        /// <summary>
        /// Finds the next edge around a node which forms
        /// part of a result line.
        /// </summary>
        /// <param name="node">A line edge originating at the node to be scanned</param>
        /// <returns>The next line edge, or null if there is none
        /// </returns>
        private static OverlayEdge NextLineEdgeUnvisited(OverlayEdge node)
        {
            var e = node;
            do
            {
                e = e.ONextOE;
                if (e.IsVisited) continue;
                if (e.IsInResultLine)
                {
                    return e;
                }
            } while (e != node);
            return null;
        }

        /// <summary>
        /// Computes the degree of the line edges incident on a node
        /// </summary>
        /// <param name="node">Node to compute degree for</param>
        /// <returns>Degree of the node line edges</returns>
        private static int DegreeOfLines(OverlayEdge node)
        {
            int degree = 0;
            var e = node;
            do
            {
                if (e.IsInResultLine)
                {
                    degree++;
                }
                e = e.ONextOE;
            } while (e != node);
            return degree;
        }
    }
}
