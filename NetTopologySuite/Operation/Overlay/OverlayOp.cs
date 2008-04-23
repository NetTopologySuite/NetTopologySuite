using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
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
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>, 
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        public static IGeometry<TCoordinate> Overlay(IGeometry<TCoordinate> geom0, 
                                                     IGeometry<TCoordinate> geom1, 
                                                     SpatialFunctions opCode)
        {
            OverlayOp<TCoordinate> gov = new OverlayOp<TCoordinate>(geom0, geom1);
            IGeometry<TCoordinate> geomOv = gov.GetResultGeometry(opCode);
            return geomOv;
        }

        public static Boolean IsResultOfOp(Label label, SpatialFunctions opCode)
        {
            Locations loc0 = label[0].On;
            Locations loc1 = label[1].On;
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

        private readonly PointLocator<TCoordinate> _pointtLocator 
            = new PointLocator<TCoordinate>();
        private readonly IGeometryFactory<TCoordinate> _geoFactory;
        private IGeometry<TCoordinate> _resultGeometry;

        private readonly PlanarGraph<TCoordinate> _graph;
        private readonly EdgeList<TCoordinate> _edgeList;

        private readonly List<IPolygon<TCoordinate>> _resultPolyList 
            = new List<IPolygon<TCoordinate>>();
        private readonly List<ILineString<TCoordinate>> _resultLineList 
            = new List<ILineString<TCoordinate>>();
        private readonly List<IPoint<TCoordinate>> _resultPointList 
            = new List<IPoint<TCoordinate>>();

        public OverlayOp(IGeometry<TCoordinate> g0, IGeometry<TCoordinate> g1)
            : base(g0, g1)
        {
            _graph = new PlanarGraph<TCoordinate>(new OverlayNodeFactory<TCoordinate>());

            /*
            * Use factory of primary point.
            * Note that this does NOT handle mixed-precision arguments
            * where the second arg has greater precision than the first.
            */
            _geoFactory = g0.Factory;
            _edgeList = new EdgeList<TCoordinate>(_geoFactory);
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
        /// This method is used to decide if a point node should be 
        /// included in the result or not.
        /// </summary>
        /// <returns>
        /// <see langword="true"/> if the coord point is covered by a result 
        /// Line or Area point.
        /// </returns>
        public Boolean IsCoveredByLineOrArea(TCoordinate coord)
        {
            IEnumerable<IGeometry<TCoordinate>> geometries;

            geometries = Enumerable.Upcast<IGeometry<TCoordinate>, ILineString<TCoordinate>>(_resultLineList);

            if (isCovered(coord, geometries))
            {
                return true;
            }

            geometries = Enumerable.Upcast<IGeometry<TCoordinate>, IPolygon<TCoordinate>>(_resultPolyList);

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
                = Enumerable.Upcast<IGeometry<TCoordinate>, IPolygon<TCoordinate>>(_resultPolyList);

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
            copyPoints(0);
            copyPoints(1);

            // node the input Geometries
            Argument1.ComputeSelfNodes(LineIntersector, false);
            Argument2.ComputeSelfNodes(LineIntersector, false);

            // compute intersections between edges of the two input geometries
            Argument1.ComputeEdgeIntersections(Argument2, LineIntersector, true);

            List<Edge<TCoordinate>> baseSplitEdges = new List<Edge<TCoordinate>>();
            baseSplitEdges.AddRange(Argument1.ComputeSplitEdges());
            baseSplitEdges.AddRange(Argument2.ComputeSplitEdges());

            // add the noded edges to this result graph
            insertUniqueEdges(baseSplitEdges);

            computeLabelsFromDepths();
            replaceCollapsedEdges();

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
            PolygonBuilder<TCoordinate> polyBuilder = new PolygonBuilder<TCoordinate>(_geoFactory);
            polyBuilder.Add(_graph);
            _resultPolyList.AddRange(polyBuilder.Polygons);

            LineBuilder<TCoordinate> lineBuilder = new LineBuilder<TCoordinate>(this, _geoFactory);
            _resultLineList.AddRange(lineBuilder.Build(opCode));

            PointBuilder<TCoordinate> pointBuilder = new PointBuilder<TCoordinate>(this, _geoFactory);
            _resultPointList.AddRange(pointBuilder.Build(opCode));

            // gather the results from all calculations into a single Geometry for the result set
            _resultGeometry = computeGeometry(_resultPointList, _resultLineList, _resultPolyList);
        }

        private void insertUniqueEdges(IEnumerable<Edge<TCoordinate>> edges)
        {
            foreach (Edge<TCoordinate> edge in edges)
            {
                insertUniqueEdge(edge);
            }
        }

        /// <summary>
        /// Insert an edge from one of the noded input graphs.
        /// Checks edges that are inserted to see if an
        /// identical edge already exists.
        /// If so, the edge is not inserted, but its label is merged
        /// with the existing edge.
        /// </summary>
        protected void insertUniqueEdge(Edge<TCoordinate> e)
        {
            Int32 foundIndex = _edgeList.FindEdgeIndex(e);

            // If an identical edge already exists, simply update its label
            if (foundIndex >= 0)
            {
                Edge<TCoordinate> existingEdge = _edgeList[foundIndex];
                Label existingLabel = existingEdge.Label.Value;
                Label labelToMerge = e.Label.Value;

                // check if new edge is in reverse direction to existing edge
                // if so, must flip the label before merging it
                if (!existingEdge.IsPointwiseEqual(e))
                {
                    labelToMerge = e.Label.Value.Flip();
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
        private void computeLabelsFromDepths()
        {
            foreach (Edge<TCoordinate> e in _edgeList)
            {
                Label lbl = e.Label.Value;
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
                        if (!lbl.IsNone(i) && lbl.IsArea() && !depth.IsNull(i))
                        {
                            /*
                             * if the depths are equal, this edge is the result of
                             * the dimensional collapse of two or more edges.
                             * It has the same location on both sides of the edge,
                             * so it has collapsed to a line.
                             */
                            if (depth.GetDelta(i) == 0)
                            {
                                lbl = lbl.ToLine(i);
                            }
                            else
                            {
                                /*
                                * This edge may be the result of a dimensional collapse,
                                * but it still has different locations on both sides.  The
                                * label of the edge must be updated to reflect the resultant
                                * side locations indicated by the depth values.
                                */
                                Debug.Assert(!depth.IsNull(i, Positions.Left),
                                              "Depth of left side has not been initialized.");
                                Debug.Assert(!depth.IsNull(i, Positions.Right),
                                              "Depth of right side has not been initialized");

                                Locations left = depth.GetLocation(i, Positions.Left);
                                Locations right = depth.GetLocation(i, Positions.Right);

                                lbl = new Label(lbl, i, lbl[i, Positions.On], left, right);
                            }
                        }
                    }
                }

                e.Label = lbl;
            }
        }

        /// <summary>
        /// If edges which have undergone dimensional collapse are found,
        /// replace them with a new edge which is a L edge
        /// </summary>
        private void replaceCollapsedEdges()
        {
            List<Edge<TCoordinate>> newEdges = new List<Edge<TCoordinate>>();
            List<Edge<TCoordinate>> edgesToRemove = new List<Edge<TCoordinate>>();

            foreach (Edge<TCoordinate> e in _edgeList)
            {
                if (e.IsCollapsed)
                {
                    edgesToRemove.Add(e);
                    newEdges.Add(e.CollapsedEdge);
                }
            }

            _edgeList.RemoveRange(edgesToRemove);
            _edgeList.AddRange(newEdges);
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
        private void copyPoints(Int32 argIndex)
        {
            GeometryGraph<TCoordinate> graph = argIndex == 0 ? Argument1 : Argument2;

            foreach (Node<TCoordinate> graphNode in graph.Nodes)
            {
                Node<TCoordinate> newNode = _graph.AddNode(graphNode.Coordinate);
                newNode.SetLabel(argIndex, graphNode.Label.Value[argIndex].On);
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
            foreach (Node<TCoordinate> node in _graph.Nodes)
            {
                node.Edges.ComputeLabeling(Argument1, Argument2);
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
            foreach (Node<TCoordinate> node in _graph.Nodes)
            {
                DirectedEdgeStar<TCoordinate> edgeStar = node.Edges as DirectedEdgeStar<TCoordinate>;
                Debug.Assert(edgeStar != null);
                edgeStar.MergeSymLabels();
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
                    if (label.IsNone(0))
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
            GeometryGraph<TCoordinate> graph = getArgument(targetIndex);
            Locations loc = _pointtLocator.Locate(n.Coordinate, graph.Geometry);
            n.Label = new Label(n.Label.Value, targetIndex, loc);
        }

        private GeometryGraph<TCoordinate> getArgument(Int32 targetIndex) 
        {
            return targetIndex == 0 ? Argument1 : Argument2;
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

        private IGeometry<TCoordinate> computeGeometry(IEnumerable<IPoint<TCoordinate>> points,
            IEnumerable<ILineString<TCoordinate>> lines, IEnumerable<IPolygon<TCoordinate>> polys)
        {
            List<IGeometry<TCoordinate>> geomList = new List<IGeometry<TCoordinate>>();

            // element geometries of the result are always in the order Point, Curve, Surface
            geomList.AddRange(Enumerable.Upcast<IGeometry<TCoordinate>, IPoint<TCoordinate>>(points));
            geomList.AddRange(Enumerable.Upcast<IGeometry<TCoordinate>, ILineString<TCoordinate>>(lines));
            geomList.AddRange(Enumerable.Upcast<IGeometry<TCoordinate>, IPolygon<TCoordinate>>(polys));

            // build the most specific point possible
            return _geoFactory.BuildGeometry(geomList);
        }
    }
}