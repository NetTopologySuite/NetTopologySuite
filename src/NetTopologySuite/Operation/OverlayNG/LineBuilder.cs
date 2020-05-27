using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Operation.Overlay;

namespace NetTopologySuite.Operation.OverlayNg
{
    /**
     * Finds and builds overlay result lines from the overlay graph.
     * Output linework has the following semantics:
     * <ol>
     * <li>Linework is fully noded
     * <li>Lines are as long as possible between nodes
     * </ol>
     * 
     * Various strategies are possible for how to 
     * merge graph edges into lines.
     * This implementation uses the approach
     * of having output lines run contiguously from node to node.
     * For rings a node point is chosen arbitrarily.
     * <p>
     * Another possible strategy would be to preserve input linework 
     * as far as possible (i.e. any sections of input lines which are not 
     * coincident with other linework would be preserved).
     * <p>
     * It would also be possible to output LinearRings, 
     * if the input is a LinearRing and is unchanged.
     * This will require additional info from the input linework.
     * 
     * @author Martin Davis
     *
     */
    class LineBuilder
    {

        private readonly GeometryFactory _geometryFactory;
        private readonly OverlayGraph _graph;
        private readonly SpatialFunction _opCode;
        private readonly int _inputAreaIndex;
        private readonly bool _hasResultArea;
        private readonly List<LineString> _lines = new List<LineString>();

        /**
         * Creates a builder for linear elements which may be present 
         * in the overlay result.
         * 
         * @param inputGeom the input geometries
         * @param graph the topology graph
         * @param hasResultArea true if an area has been generated for the result
         * @param opCode the overlay operation code
         * @param geomFact the output geometry factory
         */
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
                if (IsInResult(edge))
                    continue;
                if (IsResultLine(edge.Label))
                {
                    edge.MarkInResultLine();
                    //Debug.println(edge);
                }
            }
        }

        /**
         * If the edge linework is already in the result, 
         * this edge does not need to be included as a line.
         * 
         * @param edge an edge of the topology graph
         * @return true if the edge linework is already in the result
         */
        private static bool IsInResult(OverlayEdge edge)
        {
            return edge.IsInResult || edge.SymOE.IsInResult;
        }

        /**
         * Checks if the topology indicated by an edge label
         * determines that this edge should be part of a result line.
         * <p>
         * Note that the logic here relies on the semantic
         * that for intersection lines are only returned if
         * there is no result area components.
         * 
         * @param lbl the label for an edge
         * @return true if the edge should be included in the result
         */
        private bool IsResultLine(OverlayLabel lbl)
        {
            /**
             * Edges which are just collapses along boundaries
             * are not output.
             * In other words, an edge must be from a source line
             * or two (coincident) area boundaries.
             */
            if (lbl.IsBoundaryCollapse) return false;

            /**
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

            var aLoc = EffectiveLocation(0, lbl);
            var bLoc = EffectiveLocation(1, lbl);

            bool isInResult = OverlayNG.isResultOfOp(_opCode, aLoc, bLoc);
            return isInResult;
        }

        /**
         * Determines the effective location for a line,
         * for the purpose of overlay operation evaluation.
         * Line edges and Collapses are reported as INTERIOR
         * so they may be included in the result
         * if warranted by the effect of the operation
         * on the two edges.
         * (For instance, the intersection of line edge and a collapsed boundary
         * is included in the result).
         * 
         * @param geomIndex index of parent geometry
         * @param lbl label of line
         * @return the effective location of the line
         */
        private static Location EffectiveLocation(int geomIndex, OverlayLabel lbl)
        {
            if (lbl.IsCollapse(geomIndex))
                return Location.Interior;
            if (lbl.IsLineAt(geomIndex))
                return Location.Interior;
            return lbl.GetLineLocation(geomIndex);
        }

        //----  Maximal line extraction methods

        private void AddResultLines()
        {
            AddResultLinesForNodes();
            AddResultLinesRings();
        }

        /**
         * FUTURE: To implement a strategy preserving input lines,
         * the label must carry an id for each input LineString.
         * The ids are zeroed out whenever two input edges are merged.
         * Additional result nodes are created where there are changes in id
         * at degree-2 nodes.
         * (degree>=3 nodes must be kept as nodes to ensure 
         * output linework is fully noded.
         */

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

        /**
         * Adds lines which form rings (i.e. have only degree-2 vertices).
         */
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

        /**
         * Traverses edges from edgeStart which
         * lie in a single line (have degree = 2).
         * 
         * The direction of the linework is preserved as far as possible.
         * Specifically, the direction of the line is determined 
         * by the start edge direction. This implies
         * that if all edges are reversed, the created line
         * will be reversed to match.
         * (Other more complex strategies would be possible.
         * E.g. using the direction of the majority of segments,
         * or preferring the direction of the A edges.)
         * 
         * @param node
         * @return 
         */
        private LineString BuildLine(OverlayEdge node)
        {
            // assert: edgeStart degree = 1
            // assert: edgeStart direction = forward
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

        /**
         * Finds the next edge around a node which forms
         * part of a result line.
         * 
         * @param node a line edge originating at the node to be scanned
         * @return the next line edge, or null if there is none
         */
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

        /**
         * Computes the degree of the line edges incident on a node
         * @param node node to compute degree for
         * @return degree of the node line edges
         */
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
