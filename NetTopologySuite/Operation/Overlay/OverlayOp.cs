using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Overlay
{
    /// <summary>
    /// The spatial functions supported by this class.
    /// These operations implement various Boolean combinations 
    /// of the resultants of the overlay.
    /// </summary>
    public enum SpatialFunctions
    {
        Intersection = 1,
        Union = 2,
        Difference = 3,
        SymDifference = 4,
    }

    /// <summary>
    /// Computes the overlay of two <see cref="IGeometry{TCoordinate}"/>s.  
    /// The overlay can be used to determine any Boolean combination of the geometries.
    /// </summary>
    public class OverlayOp<TCoordinate> : GeometryGraphOperation<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        public static IGeometry<TCoordinate> Overlay(IGeometry<TCoordinate> geom0, IGeometry<TCoordinate> geom1, SpatialFunctions opCode)
        {
            OverlayOp<TCoordinate> gov = new OverlayOp<TCoordinate>(geom0, geom1);
            IGeometry<TCoordinate> geomOv = gov.GetResultGeometry(opCode);
            return geomOv;
        }

        public static Boolean IsResultOfOp(Label label, SpatialFunctions opCode)
        {
            Locations loc0 = label[0];
            Locations loc1 = label[1];
            return IsResultOfOp(loc0, loc1, opCode);
        }

        /// <summary>
        /// This method will handle arguments of Location.NULL correctly.
        /// </summary>
        /// <returns><see langword="true"/> if the locations correspond to the opCode.</returns>
        public static Boolean IsResultOfOp(Locations loc0, Locations loc1, SpatialFunctions opCode)
        {
            if (loc0 == Locations.Boundary)
            {
                loc0 = Locations.Interior;
            }

            if (loc1 == Locations.Boundary)
            {
                loc1 = Locations.Interior;
            }

            switch (opCode)
            {
                case SpatialFunctions.Intersection:
                    return loc0 == Locations.Interior && loc1 == Locations.Interior;
                case SpatialFunctions.Union:
                    return loc0 == Locations.Interior || loc1 == Locations.Interior;
                case SpatialFunctions.Difference:
                    return loc0 == Locations.Interior && loc1 != Locations.Interior;
                case SpatialFunctions.SymDifference:
                    return (loc0 == Locations.Interior && loc1 != Locations.Interior)
                           || (loc0 != Locations.Interior && loc1 == Locations.Interior);
                default:
                    return false;
            }
        }

        private readonly PointLocator<TCoordinate> _pointtLocator = new PointLocator<TCoordinate>();
        private readonly IGeometryFactory<TCoordinate> _geometryFactory;
        private IGeometry<TCoordinate> _resultGeometry;

        private readonly PlanarGraph<TCoordinate> _graph;
        private readonly EdgeList<TCoordinate> _edgeList = new EdgeList<TCoordinate>();

        private readonly List<IPolygon<TCoordinate>> _resultPolyList = new List<IPolygon<TCoordinate>>();
        private readonly List<ILineString<TCoordinate>> _resultLineList = new List<ILineString<TCoordinate>>();
        private readonly List<IPoint<TCoordinate>> _resultPointList = new List<IPoint<TCoordinate>>();

        public OverlayOp(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
            : base(g0, g1)
        {
            _graph = new PlanarGraph<TCoordinate>(new OverlayNodeFactory<TCoordinate>());

            /*
            * Use factory of primary point.
            * Note that this does NOT handle mixed-precision arguments
            * where the second arg has greater precision than the first.
            */
            _geometryFactory = g0.Factory;
        }

        public IGeometry<TCoordinate> GetResultGeometry(SpatialFunctions funcCode)
        {
            computeOverlay(funcCode);
            return _resultGeometry;
        }

        public PlanarGraph<TCoordinate> Graph
        {
            get { return _graph; }
        }

        /// <summary>
        /// This method is used to decide if a point node should be included in the result or not.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the coord point is covered by a result Line or Area point.
        /// </returns>
        public Boolean IsCoveredByLineOrArea(TCoordinate coord)
        {
            IEnumerable<IGeometry<TCoordinate>> geometries;

            geometries = EnumerableConverter.Upcast<IGeometry<TCoordinate>, ILineString<TCoordinate>>(_resultLineList);

            if (isCovered(coord, geometries))
            {
                return true;
            }

            geometries = EnumerableConverter.Upcast<IGeometry<TCoordinate>, IPolygon<TCoordinate>>(_resultPolyList);

            if (isCovered(coord, geometries))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// This method is used to decide if an 
        /// L edge should be included in the result or not.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the coord point is covered by a result Area point.
        /// </returns>
        public Boolean IsCoveredByArea(TCoordinate coord)
        {
            IEnumerable<IGeometry<TCoordinate>> geometries 
                = EnumerableConverter.Upcast<IGeometry<TCoordinate>, IPolygon<TCoordinate>>(_resultPolyList);

            if (isCovered(coord, geometries))
            {
                return true;
            }

            return false;
        }

        private void computeOverlay(SpatialFunctions opCode)
        {
            // copy points from input Geometries.
            // This ensures that any Point geometries
            // in the input are considered for inclusion in the result set
            CopyPoints(0);
            CopyPoints(1);

            // node the input Geometries
            Argument1.ComputeSelfNodes(LineIntersector, false);
            Argument2.ComputeSelfNodes(LineIntersector, false);

            // compute intersections between edges of the two input geometries
            Argument1.ComputeEdgeIntersections(Argument2, LineIntersector, true);

            IList baseSplitEdges = new ArrayList();
            Argument1.ComputeSplitEdges(baseSplitEdges);
            Argument2.ComputeSplitEdges(baseSplitEdges);

            // add the noded edges to this result graph
            InsertUniqueEdges(baseSplitEdges);

            ComputeLabelsFromDepths();
            ReplaceCollapsedEdges();

            _graph.AddEdges(_edgeList);
            computeLabeling();
            labelIncompleteNodes();

            /*
            * The ordering of building the result Geometries is important.
            * Areas must be built before lines, which must be built before points.
            * This is so that lines which are covered by areas are not included
            * explicitly, and similarly for points.
            */
            findResultAreaEdges(opCode);
            cancelDuplicateResultEdges();
            PolygonBuilder<TCoordinate> polyBuilder = new PolygonBuilder<TCoordinate>(_geometryFactory);
            polyBuilder.Add(_graph);
            _resultPolyList = polyBuilder.Polygons;

            LineBuilder lineBuilder = new LineBuilder(this, _geometryFactory, _pointtLocator);
            _resultLineList = lineBuilder.Build(opCode);

            PointBuilder pointBuilder = new PointBuilder(this, _geometryFactory, _pointtLocator);
            _resultPointList = pointBuilder.Build(opCode);

            // gather the results from all calculations into a single Geometry for the result set
            _resultGeometry = computeGeometry(_resultPointList, _resultLineList, _resultPolyList);
        }

        private void InsertUniqueEdges(IList edges)
        {
            for (IEnumerator i = edges.GetEnumerator(); i.MoveNext();)
            {
                Edge e = (Edge) i.Current;
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
        protected void InsertUniqueEdge(Edge<TCoordinate> e)
        {
            Int32 foundIndex = _edgeList.FindEdgeIndex(e);
            // If an identical edge already exists, simply update its label
            if (foundIndex >= 0)
            {
                Edge<TCoordinate> existingEdge = _edgeList[foundIndex];
                Label existingLabel = existingEdge.Label;

                Label labelToMerge = e.Label;

                // check if new edge is in reverse direction to existing edge
                // if so, must flip the label before merging it
                if (!existingEdge.IsPointwiseEqual(e))
                {
                    labelToMerge = new Label(e.Label);
                    labelToMerge.Flip();
                }

                Depth depth = existingEdge.Depth;

                // if this is the first duplicate found for this edge, initialize the depths
                if (depth.IsNull())
                {
                    depth.Add(existingLabel);
                }

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
            for (IEnumerator it = _edgeList.GetEnumerator(); it.MoveNext();)
            {
                Edge e = (Edge) it.Current;
                Label lbl = e.Label;
                Depth depth = e.Depth;
                /*
                * Only check edges for which there were duplicates,
                * since these are the only ones which might
                * be the result of dimensional collapses.
                */
                if (!depth.IsNull())
                {
                    depth.Normalize();
                    for (Int32 i = 0; i < 2; i++)
                    {
                        if (!lbl.IsNull(i) && lbl.IsArea() && ! depth.IsNull(i))
                        {
                            /*
                             * if the depths are equal, this edge is the result of
                             * the dimensional collapse of two or more edges.
                             * It has the same location on both sides of the edge,
                             * so it has collapsed to a line.
                             */
                            if (depth.GetDelta(i) == 0)
                            {
                                lbl.ToLine(i);
                            }
                            else
                            {
                                /*
                                * This edge may be the result of a dimensional collapse,
                                * but it still has different locations on both sides.  The
                                * label of the edge must be updated to reflect the resultant
                                * side locations indicated by the depth values.
                                */
                                Assert.IsTrue(!depth.IsNull(i, Positions.Left),
                                              "depth of Left side has not been initialized");
                                lbl.SetLocation(i, Positions.Left, depth.GetLocation(i, Positions.Left));
                                Assert.IsTrue(!depth.IsNull(i, Positions.Right),
                                              "depth of Right side has not been initialized");
                                lbl.SetLocation(i, Positions.Right, depth.GetLocation(i, Positions.Right));
                            }
                        }
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
            IList newEdges = new ArrayList();
            IList edgesToRemove = new ArrayList();
            IEnumerator it = _edgeList.GetEnumerator();
            while (it.MoveNext())
            {
                Edge e = (Edge) it.Current;
                if (e.IsCollapsed)
                {
                    // edgeList.Remove(it.Current as Edge); 
                    // Diego Guidi says:
                    // This instruction throws a "System.InvalidOperationException: Collection was modified; enumeration operation may not execute".
                    // i try to not modify edgeList here, and remove all elements at the end of iteration.
                    edgesToRemove.Add((Edge) it.Current);
                    newEdges.Add(e.CollapsedEdge);
                }
            }

            // Removing all collapsed edges at the end of iteration.
            foreach (Edge obj in edgesToRemove)
            {
                _edgeList.Remove(obj);
            }
            foreach (object obj in newEdges)
            {
                _edgeList.Add((Edge) obj);
            }
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
        private void CopyPoints(Int32 argIndex)
        {
            IEnumerator i = arg[argIndex].GetNodeEnumerator();
            while (i.MoveNext())
            {
                Node graphNode = (Node) i.Current;
                Node newNode = _graph.AddNode(graphNode.Coordinate);
                newNode.SetLabel(argIndex, graphNode.Label.GetLocation(argIndex));
            }
        }

        /// <summary> 
        /// Compute initial labeling for all DirectedEdges at each node.
        /// In this step, DirectedEdges will acquire a complete labeling
        /// (i.e. one with labels for both Geometries)
        /// only if they
        /// are incident on a node which has edges for both Geometries
        /// </summary>
        private void computeLabeling()
        {
            IEnumerator nodeit = _graph.Nodes.GetEnumerator();
            while (nodeit.MoveNext())
            {
                Node node = (Node) nodeit.Current;
                node.Edges.ComputeLabeling(arg);
            }
            mergeSymLabels();
            updateNodeLabeling();
        }

        /// <summary> 
        /// For nodes which have edges from only one Geometry incident on them,
        /// the previous step will have left their dirEdges with no labeling for the other
        /// Geometry.  However, the sym dirEdge may have a labeling for the other
        /// Geometry, so merge the two labels.
        /// </summary>
        private void mergeSymLabels()
        {
            IEnumerator nodeit = _graph.Nodes.GetEnumerator();
            while (nodeit.MoveNext())
            {
                Node node = (Node) nodeit.Current;
                ((DirectedEdgeStar) node.Edges).MergeSymLabels();
            }
        }

        private void updateNodeLabeling()
        {
            // update the labels for nodes
            // The label for a node is updated from the edges incident on it
            // (Note that a node may have already been labeled
            // because it is a point in one of the input geometries)
            foreach (Node<TCoordinate> node in _graph.Nodes)
            {
                DirectedEdgeStar<TCoordinate> edges = node.Edges as DirectedEdgeStar<TCoordinate>;
                Debug.Assert(edges != null);
                Label lbl = edges.Label;
                Debug.Assert(node.Label.HasValue);
                node.Label = node.Label.Value.Merge(lbl);
            }
        }

        /// <summary>
        /// Incomplete nodes are nodes whose labels are incomplete.
        /// (e.g. the location for one Geometry is null).
        /// These are either isolated nodes,
        /// or nodes which have edges from only a single Geometry incident on them.
        /// Isolated nodes are found because nodes in one graph which don't intersect
        /// nodes in the other are not completely labeled by the initial process
        /// of adding nodes to the nodeList.
        /// To complete the labeling we need to check for nodes that lie in the
        /// interior of edges, and in the interior of areas.
        /// When each node labeling is completed, the labeling of the incident
        /// edges is updated, to complete their labeling as well.
        /// </summary>
        private void labelIncompleteNodes()
        {
            foreach (Node<TCoordinate> node in _graph.Nodes)
            {
                Debug.Assert(node.Label.HasValue);
                Label label = node.Label.Value;

                if (node.IsIsolated)
                {
                    if (label.IsNull(0))
                    {
                        labelIncompleteNode(node, 0);
                    }
                    else
                    {
                        labelIncompleteNode(node, 1);
                    }
                }

                // now update the labeling for the DirectedEdges incident on this node
                DirectedEdgeStar<TCoordinate> edges = node.Edges as DirectedEdgeStar<TCoordinate>;
                Debug.Assert(edges != null);
                edges.UpdateLabeling(label);
            }
        }

        /// <summary>
        /// Label an isolated node with its relationship to the target point.
        /// </summary>
        private void labelIncompleteNode(Node<TCoordinate> n, Int32 targetIndex)
        {
            Locations loc = _pointtLocator.Locate(n.Coordinate, arg[targetIndex].Geometry);
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
        private void findResultAreaEdges(SpatialFunctions opCode)
        {
            foreach (DirectedEdge<TCoordinate> de in _graph.EdgeEnds)
            {
                // mark all dirEdges with the appropriate label
                Debug.Assert(de.Label.HasValue);
                Label label = de.Label.Value;

                if (label.IsArea() && !de.IsInteriorAreaEdge &&
                    IsResultOfOp(label[0, Positions.Right], label[1, Positions.Right], opCode))
                {
                    de.InResult = true;
                }
            }
        }

        /// <summary>
        /// If both a <see cref="DirectedEdge{TCoordinate}"/> and its 
        /// sym are marked as being in the result, cancel them out.
        /// </summary>
        private void cancelDuplicateResultEdges()
        {
            // remove any dirEdges whose sym is also included
            // (they "cancel each other out")
            foreach (DirectedEdge<TCoordinate> de in _graph.EdgeEnds)
            {
                DirectedEdge<TCoordinate> sym = de.Sym;

                if (de.IsInResult && sym.IsInResult)
                {
                    de.InResult = false;
                    sym.InResult = false;
                }
            }
        }

        /// <returns>
        /// <see langword="true"/> if the coord is located in the interior or boundary of
        /// a point in the list.
        /// </returns>
        private Boolean isCovered(TCoordinate coord, IEnumerable<IGeometry<TCoordinate>> geometries)
        {
            foreach (IGeometry<TCoordinate> geometry in geometries)
            {
                Locations loc = _pointtLocator.Locate(coord, geometry);

                if (loc != Locations.Exterior)
                {
                    return true;
                }
            }

            return false;
        }

        private IGeometry computeGeometry(IEnumerable<IPoint<TCoordinate>> points,
            IEnumerable<ILineString<TCoordinate>> lines, IEnumerable<IPolygon<TCoordinate>> polys)
        {
            List<IGeometry<TCoordinate>> geomList = new List<IGeometry<TCoordinate>>();

            // element geometries of the result are always in the order Point, Curve, Surface
            geomList.AddRange(EnumerableConverter.Upcast<IGeometry<TCoordinate>, IPoint<TCoordinate>>(points));
            geomList.AddRange(EnumerableConverter.Upcast<IGeometry<TCoordinate>, ILineString<TCoordinate>>(lines));
            geomList.AddRange(EnumerableConverter.Upcast<IGeometry<TCoordinate>, IPolygon<TCoordinate>>(polys));

            // build the most specific point possible
            return _geometryFactory.BuildGeometry(geomList);
        }
    }
}