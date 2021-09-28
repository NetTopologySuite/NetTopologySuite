using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;

namespace NetTopologySuite.Operation.OverlayNG
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
    internal sealed class LineBuilder
    {

        private readonly GeometryFactory _geometryFactory;
        private readonly OverlayGraph _graph;
        private readonly SpatialFunction _opCode;
        private readonly int _inputAreaIndex;
        private readonly bool _hasResultArea;

        /// <summary>
        /// Indicates whether intersections are allowed to produce
        /// heterogeneous results including proper boundary touches.
        /// This does not control inclusion of touches along collapses.<br/>
        /// True provides the original JTS semantics.
        /// </summary>
        private bool _isAllowMixedResult = !OverlayNG.STRICT_MODE_DEFAULT;


        /// <summary>
        /// Allow lines created by area topology collapses
        /// to appear in the result.<br/>
        /// True provides the original JTS semantics.
        /// </summary>
        private bool _isAllowCollapseLines = !OverlayNG.STRICT_MODE_DEFAULT;


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

        public bool StrictMode
        {
            set { _isAllowCollapseLines = _isAllowMixedResult = !value; }
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
             * Omit edge which is a boundary of a single geometry
             * (i.e. not a collapse or line edge as well).
             * These are only included if part of a result area.
             * This is a short-circuit for the most common area edge case
             */
            if (lbl.IsBoundarySingleton) return false;

            /*
             * Omit edge which is a collapse along a boundary.
             * I.e a result line edge must be from a input line
             * OR two coincident area boundaries.
             * 
             * This logic is only used if not including collapse lines in result.
             */
            if (!_isAllowCollapseLines
                && lbl.IsBoundaryCollapse) return false;

            /*
             * Omit edge which is a collapse interior to its parent area.
             * (E.g. a narrow gore, or spike off a hole)
             */
            if (lbl.IsInteriorCollapse) return false;

            /*
             * For ops other than Intersection, omit a line edge
             * if it is interior to the other area.
             * 
             * For Intersection, a line edge interior to an area is included.
             */
            if (_opCode != OverlayNG.INTERSECTION)
            {
                /*
                 * Omit collapsed edge in other area interior.
                 */
                if (lbl.IsCollapseAndNotPartInterior) return false;

                /*
                 * If there is a result area, omit line edge inside it.
                 * It is sufficient to check against the input area rather 
                 * than the result area, 
                 * because if line edges are present then there is only one input area, 
                 * and the result area must be the same as the input area. 
                 */
                if (_hasResultArea && lbl.IsLineInArea(_inputAreaIndex))
                    return false;
            }

            /*
             * Include line edge formed by touching area boundaries,
             * if enabled.
             */
            if (_isAllowMixedResult
                && _opCode == OverlayNG.INTERSECTION && lbl.IsBoundaryTouch)
            {
                return true;
            }

            /*
             * Finally, determine included line edge
             * according to overlay op boolean logic.
             */
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

                /*
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
        /// This ensures the orientation of linework is faithful to the input
        /// in the case of polygon-line overlay.
        /// However, this does not provide a consistent orientation 
        /// in the case of line-line intersection(where A and B might have different orientations).
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
