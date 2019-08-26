using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.Overlay
{
    /// <summary>
    /// Forms NTS LineStrings out of a the graph of <c>DirectedEdge</c>s
    /// created by an <c>OverlayOp</c>.
    /// </summary>
    public class LineBuilder
    {
        private readonly OverlayOp _op;
        private readonly GeometryFactory _geometryFactory;
        private readonly PointLocator _ptLocator;

        private readonly List<Edge> _lineEdgesList = new List<Edge>();
        private readonly List<Geometry> _resultLineList = new List<Geometry>();

        /// <summary>
        ///
        /// </summary>
        /// <param name="op"></param>
        /// <param name="geometryFactory"></param>
        /// <param name="ptLocator"></param>
        public LineBuilder(OverlayOp op, GeometryFactory geometryFactory, PointLocator ptLocator)
        {
            _op = op;
            _geometryFactory = geometryFactory;
            _ptLocator = ptLocator;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="opCode"></param>
        /// <returns>
        /// A list of the LineStrings in the result of the specified overlay operation.
        /// </returns>
        public IList<Geometry> Build(SpatialFunction opCode)
        {
            FindCoveredLineEdges();
            CollectLines(opCode);
            BuildLines(opCode);
            return _resultLineList;
        }

        /// <summary>
        /// Find and mark L edges which are "covered" by the result area (if any).
        /// L edges at nodes which also have A edges can be checked by checking
        /// their depth at that node.
        /// L edges at nodes which do not have A edges can be checked by doing a
        /// point-in-polygon test with the previously computed result areas.
        /// </summary>
        private void FindCoveredLineEdges()
        {
            // first set covered for all L edges at nodes which have A edges too
            foreach (var node in _op.Graph.Nodes)
            {
                ((DirectedEdgeStar) node.Edges).FindCoveredLineEdges();
            }

            /*
             * For all Curve edges which weren't handled by the above,
             * use a point-in-poly test to determine whether they are covered
             */
            foreach (DirectedEdge de in _op.Graph.EdgeEnds)
            {
                var e = de.Edge;
                if (de.IsLineEdge && !e.IsCoveredSet)
                {
                    bool isCovered = _op.IsCoveredByA(de.Coordinate);
                    e.Covered = isCovered;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="opCode"></param>
        private void CollectLines(SpatialFunction opCode)
        {
            foreach (DirectedEdge de in _op.Graph.EdgeEnds)
            {
                CollectLineEdge(de, opCode, _lineEdgesList);
                CollectBoundaryTouchEdge(de, opCode, _lineEdgesList);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="de"></param>
        /// <param name="opCode"></param>
        /// <param name="edges"></param>
        public void CollectLineEdge(DirectedEdge de, SpatialFunction opCode, IList<Edge> edges)
        {
            var label = de.Label;
            var e = de.Edge;
            // include Curve edges which are in the result
            if (de.IsLineEdge)
            {
                if (!de.IsVisited && OverlayOp.IsResultOfOp(label, opCode) && !e.IsCovered)
                {
                    edges.Add(e);
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
        /// <param name="de"></param>
        /// <param name="opCode"></param>
        /// <param name="edges"></param>
        public void CollectBoundaryTouchEdge(DirectedEdge de, SpatialFunction opCode, IList<Edge> edges)
        {
            var label = de.Label;
            if (de.IsLineEdge)
                return;         // only interested in area edges
            if (de.IsVisited)
                return;         // already processed
            if (de.IsInteriorAreaEdge)
                return; // added to handle dimensional collapses
            if (de.Edge.IsInResult)
                return;     // if the edge linework is already included, don't include it again

            // sanity check for labelling of result edgerings
            Assert.IsTrue(!(de.IsInResult || de.Sym.IsInResult) || !de.Edge.IsInResult);
            // include the linework if it's in the result of the operation
            if (OverlayOp.IsResultOfOp(label, opCode) && opCode == SpatialFunction.Intersection)
            {
                edges.Add(de.Edge);
                de.VisitedEdge = true;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="opCode"></param>
        private void BuildLines(SpatialFunction opCode)
        {
            foreach (var e in _lineEdgesList)
            {
                // var label = e.Label;
                var line = _geometryFactory.CreateLineString(e.Coordinates);
                _resultLineList.Add(line);
                e.InResult = true;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="edgesList"></param>
        private void LabelIsolatedLines(IEnumerable<Edge> edgesList)
        {
            foreach (var e in edgesList)
            {
                var label = e.Label;
                if (e.IsIsolated)
                {
                    if (label.IsNull(0))
                         LabelIsolatedLine(e, 0);
                    else LabelIsolatedLine(e, 1);
                }
            }
        }

        /// <summary>
        /// Label an isolated node with its relationship to the target point.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="targetIndex"></param>
        private void LabelIsolatedLine(Edge e, int targetIndex)
        {
            var loc = _ptLocator.Locate(e.Coordinate, _op.GetArgGeometry(targetIndex));
            e.Label.SetLocation(targetIndex, loc);
        }
    }
}
