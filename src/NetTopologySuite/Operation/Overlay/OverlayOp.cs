using System;
using System.Collections.Generic;
using NetTopologySuite.Algorithm;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Utilities;
using Position = NetTopologySuite.Geometries.Position;

namespace NetTopologySuite.Operation.Overlay
{
    /// <summary>
    /// The spatial functions supported by this class.
    /// These operations implement various bool combinations of the resultants of the overlay.
    /// </summary>
    public enum SpatialFunction
    {
        /// <summary>
        /// The code for the Intersection overlay operation
        /// </summary>
        Intersection  = 1,
        /// <summary>
        /// The code for the Union overlay operation
        /// </summary>
        Union = 2,
        /// <summary>
        /// The code for the Difference overlay operation
        /// </summary>
        Difference = 3,
        /// <summary>
        /// The code for the Symmetric Difference overlay operation
        /// </summary>
        SymDifference = 4,
    }

    /// <summary>
    /// Computes the geometric overlay of two <see cref="Geometry"/>s.  The overlay
    /// can be used to determine any bool combination of the geometries.
    /// </summary>
    public class OverlayOp : GeometryGraphOperation
    {
        /// <summary>
        /// Disable <see cref="EdgeNodingValidator"/>
        /// when an intersection is made (<see cref="ComputeOverlay"/>),
        /// so performances are dramatically improved but failures are not managed.
        /// </summary>
        /// <remarks>
        /// Use ay your own risk!
        /// </remarks>
        public static bool NodingValidatorDisabled { get; set; }

        /// <summary>
        /// Computes an overlay operation
        /// for the given geometry arguments.
        /// </summary>
        /// <param name="geom0">The first geometry argument</param>
        /// <param name="geom1">The second geometry argument</param>
        /// <param name="opCode">The code for the desired overlay operation</param>
        /// <returns>The result of the overlay operation</returns>
        /// <exception cref="TopologyException">Thrown if a robustness problem is encountered.</exception>
        public static Geometry Overlay(Geometry geom0, Geometry geom1, SpatialFunction opCode)
        {
            var gov = new OverlayOp(geom0, geom1);
            var geomOv = gov.GetResultGeometry(opCode);
            return geomOv;
        }

        /// <summary>
        /// Tests whether a point with a given topological <see cref="Label"/>
        /// relative to two geometries is contained in
        /// the result of overlaying the geometries using
        /// a given overlay operation.
        /// <para/>
        /// The method handles arguments of <see cref="Location.Null"/> correctly
        /// </summary>
        /// <param name="label">The topological label of the point</param>
        /// <param name="overlayOpCode">The code for the overlay operation to test</param>
        /// <returns><c>true</c> if the label locations correspond to the overlayOpCode</returns>
        public static bool IsResultOfOp(Label label, SpatialFunction overlayOpCode)
        {
            var loc0 = label.GetLocation(0);
            var loc1 = label.GetLocation(1);
            return IsResultOfOp(loc0, loc1, overlayOpCode);
        }

        /// <summary>
        /// Tests whether a point with given <see cref="Location"/>s
        /// relative to two geometries is contained in
        /// the result of overlaying the geometries using
        /// a given overlay operation.
        /// <para/>
        /// The method handles arguments of <see cref="Location.Null"/> correctly
        /// </summary>
        /// <param name="loc0">the code for the location in the first geometry </param>
        /// <param name="loc1">the code for the location in the second geometry</param>
        /// <param name="overlayOpCode">the code for the overlay operation to test</param>
        /// <returns><c>true</c> if the locations correspond to the overlayOpCode.</returns>
        public static bool IsResultOfOp(Location loc0, Location loc1, SpatialFunction overlayOpCode)
        {
            if (loc0 == Location.Boundary)
                loc0 = Location.Interior;
            if (loc1 == Location.Boundary)
                loc1 = Location.Interior;

            switch (overlayOpCode)
            {
                case SpatialFunction.Intersection:
                    return loc0 == Location.Interior && loc1 == Location.Interior;
                case SpatialFunction.Union:
                    return loc0 == Location.Interior || loc1 == Location.Interior;
                case SpatialFunction.Difference:
                    return loc0 == Location.Interior && loc1 != Location.Interior;
                case SpatialFunction.SymDifference:
                    return   (loc0 == Location.Interior &&  loc1 != Location.Interior)
                          || (loc0 != Location.Interior &&  loc1 == Location.Interior);
                default:
                    return false;
            }
        }

        private readonly PointLocator _ptLocator = new PointLocator();
        private readonly GeometryFactory _geomFact;
        private Geometry _resultGeom;

        private readonly PlanarGraph _graph;
        private readonly EdgeList _edgeList      = new EdgeList();

        private IList<Geometry> _resultPolyList = new List<Geometry>();
        private IList<Geometry> _resultLineList = new List<Geometry>();
        private IList<Geometry> _resultPointList = new List<Geometry>();

        /// <summary>
        /// Constructs an instance to compute a single overlay operation
        /// for the given geometries.
        /// </summary>
        /// <param name="g0">The first geometry argument</param>
        /// <param name="g1">The second geometry argument</param>
        public OverlayOp(Geometry g0, Geometry g1)
            : base(g0, g1)
        {
            _graph = new PlanarGraph(new OverlayNodeFactory());

            /*
            * Use factory of primary point.
            * Note that this does NOT handle mixed-precision arguments
            * where the second arg has greater precision than the first.
            */
            _geomFact = g0.Factory;
        }

        /// <summary>
        /// Gets the result of the overlay for a given overlay operation.
        /// <para/>
        /// Note: this method can be called once only.
        /// </summary>
        /// <param name="overlayOpCode">The code of the overlay operation to perform</param>
        /// <returns>The computed result geometry</returns>
        /// <exception cref="TopologyException">Thrown if a robustness problem is encountered</exception>
        public Geometry GetResultGeometry(SpatialFunction overlayOpCode)
        {
            ComputeOverlay(overlayOpCode);
            return _resultGeom;
        }

        /// <summary>
        /// Gets the graph constructed to compute the overlay.
        /// </summary>
        public PlanarGraph Graph => _graph;

        private void ComputeOverlay(SpatialFunction opCode)
        {
            // copy points from input Geometries.
            // This ensures that any Point geometries
            // in the input are considered for inclusion in the result set
            CopyPoints(0);
            CopyPoints(1);

            // node the input Geometries
            arg[0].ComputeSelfNodes(lineIntersector, false);
            arg[1].ComputeSelfNodes(lineIntersector, false);

            // compute intersections between edges of the two input geometries
            arg[0].ComputeEdgeIntersections(arg[1], lineIntersector, true);

            var baseSplitEdges = new List<Edge>();
            arg[0].ComputeSplitEdges(baseSplitEdges);
            arg[1].ComputeSplitEdges(baseSplitEdges);
            // add the noded edges to this result graph
            InsertUniqueEdges(baseSplitEdges);

            ComputeLabelsFromDepths();
            ReplaceCollapsedEdges();

            if (!NodingValidatorDisabled)
            {
                /*
                 * Check that the noding completed correctly.
                 *
                 * This test is slow, but necessary in order to catch robustness failure
                 * situations.
                 * If an exception is thrown because of a noding failure,
                 * then snapping will be performed, which will hopefully avoid the problem.
                 * In the future hopefully a faster check can be developed.
                 *
                 */
                var nv = new EdgeNodingValidator(_edgeList.Edges);
                nv.CheckValid();
            }

            _graph.AddEdges(_edgeList.Edges);
            ComputeLabelling();
            LabelIncompleteNodes();

            /*
            * The ordering of building the result Geometries is important.
            * Areas must be built before lines, which must be built before points.
            * This is so that lines which are covered by areas are not included
            * explicitly, and similarly for points.
            */
            FindResultAreaEdges(opCode);
            CancelDuplicateResultEdges();
            var polyBuilder = new PolygonBuilder(_geomFact);
            polyBuilder.Add(_graph);
            _resultPolyList = polyBuilder.Polygons;

            var lineBuilder = new LineBuilder(this, _geomFact, _ptLocator);
            _resultLineList = lineBuilder.Build(opCode);

            var pointBuilder = new PointBuilder(this, _geomFact);//, _ptLocator);
            _resultPointList = pointBuilder.Build(opCode);

            // gather the results from all calculations into a single Geometry for the result set
            _resultGeom = ComputeGeometry(_resultPointList, _resultLineList, _resultPolyList, opCode);
        }

        private void InsertUniqueEdges(IEnumerable<Edge> edges)
        {
            for (var i = edges.GetEnumerator(); i.MoveNext(); )
            {
                var e = i.Current;
                InsertUniqueEdge(e);
            }
        }

        /// <summary>
        /// Insert an edge from one of the noded input graphs.
        /// Checks edges that are inserted to see if an
        /// identical edge already exists.
        /// If so, the edge is not inserted, but its label is merged
        /// with the existing edge.
        /// </summary>
        /// <param name="e">The edge to insert</param>
        protected void InsertUniqueEdge(Edge e)
        {
            var existingEdge = _edgeList.FindEqualEdge(e);
            // If an identical edge already exists, simply update its label
            if (existingEdge != null)
            {
                var existingLabel = existingEdge.Label;

                var labelToMerge = e.Label;
                // check if new edge is in reverse direction to existing edge
                // if so, must flip the label before merging it
                if (!existingEdge.IsPointwiseEqual(e))
                {
                    labelToMerge = new Label(e.Label);
                    labelToMerge.Flip();
                }
                var depth = existingEdge.Depth;
                // if this is the first duplicate found for this edge, initialize the depths
                if (depth.IsNull())
                    depth.Add(existingLabel);
                depth.Add(labelToMerge);
                existingLabel.Merge(labelToMerge);
            }
            else
            {
                // no matching existing edge was found
                // add this new edge to the list of edges in this graph
                _edgeList.Add(e);
            }
        }

        /// <summary>
        /// Update the labels for edges according to their depths.
        /// For each edge, the depths are first normalized.
        /// Then, if the depths for the edge are equal,
        /// this edge must have collapsed into a line edge.
        /// If the depths are not equal, update the label
        /// with the locations corresponding to the depths
        /// (i.e. a depth of 0 corresponds to a Location of Exterior,
        /// a depth of 1 corresponds to Interior)
        /// </summary>
        private void ComputeLabelsFromDepths()
        {
            for (var it = _edgeList.GetEnumerator(); it.MoveNext(); )
            {
                var e = it.Current;
                var lbl = e.Label;
                var depth = e.Depth;
                /*
                * Only check edges for which there were duplicates,
                * since these are the only ones which might
                * be the result of dimensional collapses.
                */
                if (depth.IsNull())
                    continue;

                depth.Normalize();
                for (int i = 0; i < 2; i++)
                {
                    if (lbl.IsNull(i) || !lbl.IsArea() || depth.IsNull(i))
                        continue;
                    /*
                     * if the depths are equal, this edge is the result of
                     * the dimensional collapse of two or more edges.
                     * It has the same location on both sides of the edge,
                     * so it has collapsed to a line.
                     */
                    if (depth.GetDelta(i) == 0)
                        lbl.ToLine(i);
                    else
                    {
                        /*
                         * This edge may be the result of a dimensional collapse,
                         * but it still has different locations on both sides.  The
                         * label of the edge must be updated to reflect the resultant
                         * side locations indicated by the depth values.
                         */
                        Assert.IsTrue(!depth.IsNull(i, Position.Left), "depth of Left side has not been initialized");
                        lbl.SetLocation(i, Position.Left, depth.GetLocation(i, Position.Left));
                        Assert.IsTrue(!depth.IsNull(i, Position.Right), "depth of Right side has not been initialized");
                        lbl.SetLocation(i, Position.Right, depth.GetLocation(i, Position.Right));
                    }
                }
            }
        }

        /// <summary>
        /// If edges which have undergone dimensional collapse are found,
        /// replace them with a new edge which is a L edge
        /// </summary>
        private void ReplaceCollapsedEdges()
        {
            var newEdges = new List<Edge>();
            var edgesToRemove = new List<Edge>();
            var it = _edgeList.GetEnumerator();
            while (it.MoveNext())
            {
                var e = it.Current;
                if (!e.IsCollapsed)
                    continue;
                // edgeList.Remove(it.Current as Edge);
                // Diego Guidi says:
                // This instruction throws a "System.InvalidOperationException: Collection was modified; enumeration operation may not execute".
                // i try to not modify edgeList here, and remove all elements at the end of iteration.
                edgesToRemove.Add(it.Current);
                newEdges.Add(e.CollapsedEdge);
            }
            // Removing all collapsed edges at the end of iteration.
            foreach (var obj in edgesToRemove)
                _edgeList.Remove(obj);
            foreach (var obj in newEdges)
                _edgeList.Add(obj);
        }

        /// <summary>
        /// Copy all nodes from an arg point into this graph.
        /// The node label in the arg point overrides any previously computed
        /// label for that argIndex.
        /// (E.g. a node may be an intersection node with
        /// a previously computed label of Boundary,
        /// but in the original arg Geometry it is actually
        /// in the interior due to the Boundary Determination Rule)
        /// </summary>
        /// <param name="argIndex"></param>
        private void CopyPoints(int argIndex)
        {
            var i = arg[argIndex].GetNodeEnumerator();
            while (i.MoveNext())
            {
                var graphNode = i.Current;
                var newNode = _graph.AddNode(graphNode.Coordinate);
                newNode.SetLabel(argIndex, graphNode.Label.GetLocation(argIndex));
            }
        }

        /// <summary>
        /// Compute initial labelling for all DirectedEdges at each node.
        /// In this step, DirectedEdges will acquire a complete labelling
        /// (i.e. one with labels for both Geometries)
        /// only if they
        /// are incident on a node which has edges for both Geometries
        /// </summary>
        private void ComputeLabelling()
        {
            var nodeit = _graph.Nodes.GetEnumerator();
            while (nodeit.MoveNext())
            {
                var node = nodeit.Current;
                node.Edges.ComputeLabelling(arg);
            }
            MergeSymLabels();
            UpdateNodeLabelling();
        }

        /// <summary>
        /// For nodes which have edges from only one Geometry incident on them,
        /// the previous step will have left their dirEdges with no labelling for the other
        /// Geometry.  However, the sym dirEdge may have a labelling for the other
        /// Geometry, so merge the two labels.
        /// </summary>
        private void MergeSymLabels()
        {
            var nodeit = _graph.Nodes.GetEnumerator();
            while (nodeit.MoveNext())
            {
                var node = nodeit.Current;
                ((DirectedEdgeStar) node.Edges).MergeSymLabels();
            }
        }

        private void UpdateNodeLabelling()
        {
            // update the labels for nodes
            // The label for a node is updated from the edges incident on it
            // (Note that a node may have already been labelled
            // because it is a point in one of the input geometries)
            var nodeit = _graph.Nodes.GetEnumerator();
            while (nodeit.MoveNext())
            {
                var node = nodeit.Current;
                var lbl = ((DirectedEdgeStar) node.Edges).Label;
                node.Label.Merge(lbl);
            }
        }

        /// <summary>
        /// Incomplete nodes are nodes whose labels are incomplete.
        /// (e.g. the location for one Geometry is null).
        /// These are either isolated nodes,
        /// or nodes which have edges from only a single Geometry incident on them.
        /// Isolated nodes are found because nodes in one graph which don't intersect
        /// nodes in the other are not completely labelled by the initial process
        /// of adding nodes to the nodeList.
        /// To complete the labelling we need to check for nodes that lie in the
        /// interior of edges, and in the interior of areas.
        /// When each node labelling is completed, the labelling of the incident
        /// edges is updated, to complete their labelling as well.
        /// </summary>
        private void LabelIncompleteNodes()
        {
            // int nodeCount = 0;
            var ni = _graph.Nodes.GetEnumerator();
            while (ni.MoveNext())
            {
                var n = ni.Current;
                var label = n.Label;
                if (n.IsIsolated)
                {
                    // nodeCount++;
                    if (label.IsNull(0))
                         LabelIncompleteNode(n, 0);
                    else LabelIncompleteNode(n, 1);
                }
                // now update the labelling for the DirectedEdges incident on this node
                ((DirectedEdgeStar) n.Edges).UpdateLabelling(label);
            }
            /*
            int nPoly0 = arg[0].getGeometry().getNumGeometries();
            int nPoly1 = arg[1].getGeometry().getNumGeometries();
            Console.WriteLine("# isolated nodes= " + nodeCount
                    + "   # poly[0] = " + nPoly0
                    + "   # poly[1] = " + nPoly1);
            */

        }

        /// <summary>
        /// Label an isolated node with its relationship to the target point.
        /// </summary>
        private void LabelIncompleteNode(GraphComponent n, int targetIndex)
        {
            var loc = _ptLocator.Locate(n.Coordinate, arg[targetIndex].Geometry);
            // MD - 2008-10-24 - experimental for now
            //int loc = arg[targetIndex].Locate(n.Coordinate);
            n.Label.SetLocation(targetIndex, loc);
        }

        /// <summary>
        /// Find all edges whose label indicates that they are in the result area(s),
        /// according to the operation being performed.  Since we want polygon shells to be
        /// oriented CW, choose dirEdges with the interior of the result on the RHS.
        /// Mark them as being in the result.
        /// Interior Area edges are the result of dimensional collapses.
        /// They do not form part of the result area boundary.
        /// </summary>
        private void FindResultAreaEdges(SpatialFunction opCode)
        {
            var it = _graph.EdgeEnds.GetEnumerator();
            while (it.MoveNext())
            {
                var de = (DirectedEdge) it.Current;
                // mark all dirEdges with the appropriate label
                var label = de.Label;
                if (label.IsArea() && !de.IsInteriorAreaEdge &&
                    IsResultOfOp(label.GetLocation(0, Position.Right), label.GetLocation(1, Position.Right), opCode))
                        de.InResult = true;
            }
        }

        /// <summary>
        /// If both a dirEdge and its sym are marked as being in the result, cancel
        /// them out.
        /// </summary>
        private void CancelDuplicateResultEdges()
        {
            // remove any dirEdges whose sym is also included
            // (they "cancel each other out")
            var it = _graph.EdgeEnds.GetEnumerator();
            while (it.MoveNext())
            {
                var de = (DirectedEdge) it.Current;
                var sym = de.Sym;
                if (!de.IsInResult || !sym.IsInResult)
                    continue;

                de.InResult = false;
                sym.InResult = false;
            }
        }

        /// <summary>
        /// Tests if a point node should be included in the result or not.
        /// </summary>
        /// <param name="coord">The point coordinate</param>
        /// <returns><c>true</c> if the coordinate point is covered by a result Line or Area geometry.</returns>
        public bool IsCoveredByLA(Coordinate coord)
        {
            if (IsCovered(coord, _resultLineList))
                return true;
            return IsCovered(coord, _resultPolyList);
        }
        /// <summary>
        /// Tests if an L edge should be included in the result or not.
        /// </summary>
        /// <param name="coord">The point coordinate</param>
        /// <returns><c>true</c> if the coordinate point is covered by a result Area geometry.</returns>
        public bool IsCoveredByA(Coordinate coord)
        {
            return IsCovered(coord, _resultPolyList);
        }

        /// <returns>
        /// <c>true</c> if the coord is located in the interior or boundary of
        /// a point in the list.
        /// </returns>
        private bool IsCovered(Coordinate coord, IEnumerable<Geometry> geomList)
        {
            var it = geomList.GetEnumerator();
            while (it.MoveNext())
            {
                var geom = it.Current;
                var loc = _ptLocator.Locate(coord, geom);
                if (loc != Location.Exterior)
                    return true;
            }
            return false;
        }

        private Geometry ComputeGeometry(IEnumerable<Geometry> resultPtList, IEnumerable<Geometry> resultLiList, IEnumerable<Geometry> resultPlList, SpatialFunction opCode)
        {
            var geomList = new List<Geometry>();

            // element geometries of the result are always in the order Point,Curve,A
            geomList.AddRange(resultPtList);
            geomList.AddRange(resultLiList);
            geomList.AddRange(resultPlList);

            if (geomList.Count == 0)
                return CreateEmptyResult(opCode, arg[0].Geometry, arg[1].Geometry, _geomFact);

            // build the most specific point possible
            return _geomFact.BuildGeometry(geomList);
        }

        /// <summary>
        /// Creates an empty result geometry of the appropriate dimension,
        /// based on the given overlay operation and the dimensions of the inputs.
        /// The created geometry is always an atomic geometry,
        /// not a collection.
        /// <para/>
        /// The empty result is constructed using the following rules:
        /// <list type="bullet">
        /// <item><description><see cref="SpatialFunction.Intersection"/> - result has the dimension of the lowest input dimension</description></item>
        /// <item><description><see cref="SpatialFunction.Union"/> - result has the dimension of the highest input dimension</description></item>
        /// <item><description><see cref="SpatialFunction.Difference"/> - result has the dimension of the left-hand input</description></item>
        /// <item><description><see cref="SpatialFunction.SymDifference"/> - result has the dimension of the highest input dimension
        /// (since symDifference is the union of the differences).</description></item>
        /// </list>
        /// </summary>
        /// <param name="overlayOpCode">The overlay operation being performed</param>
        /// <param name="a">An input geometry</param>
        /// <param name="b">An input geometry</param>
        /// <param name="geomFact">The geometry factory being used for the operation</param>
        /// <returns>An empty atomic geometry of the appropriate dimension</returns>
        public static Geometry CreateEmptyResult(SpatialFunction overlayOpCode, Geometry a, Geometry b, GeometryFactory geomFact)
        {
            var resultDim = ResultDimension(overlayOpCode, a, b);
            // Handles resultDim == Dimension.False, although it should not happen
            return geomFact.CreateEmpty(resultDim);
        }

        private static Dimension ResultDimension(SpatialFunction opCode, Geometry g0, Geometry g1)
        {
            int dim0 = (int)g0.Dimension;
            int dim1 = (int)g1.Dimension;

            int resultDimension = -1;
            switch (opCode)
            {
                case SpatialFunction.Intersection:
                    resultDimension = Math.Min(dim0, dim1);
                    break;
                case SpatialFunction.Union:
                    resultDimension = Math.Max(dim0, dim1);
                    break;
                case SpatialFunction.Difference:
                    resultDimension = dim0;
                    break;
                case SpatialFunction.SymDifference:
                    /*
                     * This result is chosen because
                     * <pre>
                     * SymDiff = Union(Diff(A, B), Diff(B, A)
                     * </pre>
                     * and Union has the dimension of the highest-dimension argument.
                     */
                    resultDimension = Math.Max(dim0, dim1);
                    break;
            }
            return (Dimension)resultDimension;
        }

    }
}
