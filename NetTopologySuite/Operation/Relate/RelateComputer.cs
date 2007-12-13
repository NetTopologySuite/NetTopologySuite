using System;
using System.Collections.Generic;
using System.Diagnostics;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Algorithm;
using GisSharpBlog.NetTopologySuite.GeometriesGraph;
using GisSharpBlog.NetTopologySuite.GeometriesGraph.Index;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Relate
{
    /// <summary>
    /// Computes the topological relationship between two Geometries.
    /// RelateComputer does not need to build a complete graph structure to compute
    /// the IntersectionMatrix.  The relationship between the geometries can
    /// be computed by simply examining the labeling of edges incident on each node.
    /// RelateComputer does not currently support arbitrary GeometryCollections.
    /// This is because GeometryCollections can contain overlapping Polygons.
    /// In order to correct compute relate on overlapping Polygons, they
    /// would first need to be noded and merged (if not explicitly, at least
    /// implicitly).
    /// </summary>
    public class RelateComputer<TCoordinate>
         where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                             IComputable<TCoordinate>, IConvertible
    {
        private readonly LineIntersector<TCoordinate> _li = new RobustLineIntersector<TCoordinate>();
        private readonly PointLocator<TCoordinate> _ptLocator = new PointLocator<TCoordinate>();
        // the arg(s) of the operation
        private readonly GeometryGraph<TCoordinate> _g0;
        private readonly GeometryGraph<TCoordinate> _g1;
        private readonly NodeMap<TCoordinate> _nodes = new NodeMap<TCoordinate>(new RelateNodeFactory<TCoordinate>());
        private readonly List<Edge<TCoordinate>> _isolatedEdges = new List<Edge<TCoordinate>>();

        public RelateComputer(GeometryGraph<TCoordinate> graph1, GeometryGraph<TCoordinate> graph2)
        {
            _g0 = graph1;
            _g1 = graph2;
        }

        public IntersectionMatrix ComputeIntersectionMatrix()
        {
            IntersectionMatrix im = new IntersectionMatrix();
            // since Geometries are finite and embedded in a 2-D space, the EE element must always be 2
            im.Set(Locations.Exterior, Locations.Exterior, Dimensions.Surface);

            // if the Geometries don't overlap there is nothing to do
            if (!_g0.Geometry.Extents.Intersects(_g1.Geometry.Extents))
            {
                computeDisjointIM(im);
                return im;
            }

            _g0.ComputeSelfNodes(_li, false);
            _g1.ComputeSelfNodes(_li, false);

            // compute intersections between edges of the two input geometries
            SegmentIntersector<TCoordinate> intersector = _g0.ComputeEdgeIntersections(_g1, _li, false);
            computeIntersectionNodes(0);
            computeIntersectionNodes(1);

            /*
             * Copy the labeling for the nodes in the parent Geometries.  These override
             * any labels determined by intersections between the geometries.
             */
            copyNodesAndLabels(0);
            copyNodesAndLabels(1);

            // complete the labeling for any nodes which only have a label for a single point
            labelIsolatedNodes();

            // If a proper intersection was found, we can set a lower bound on the IM.
            computeProperIntersectionIntersectionMatrix(intersector, im);

            /*
             * Now process improper intersections
             * (eg where one or other of the geometries has a vertex at the intersection point)
             * We need to compute the edge graph at all nodes to determine the IM.
             */

            // build EdgeEnds for all intersections
            EdgeEndBuilder<TCoordinate> eeBuilder = new EdgeEndBuilder<TCoordinate>();
            IEnumerable<EdgeEnd<TCoordinate>> ee0 = eeBuilder.ComputeEdgeEnds(_g0.Edges);
            insertEdgeEnds(ee0);
            IEnumerable<EdgeEnd<TCoordinate>> ee1 = eeBuilder.ComputeEdgeEnds(_g1.Edges);
            insertEdgeEnds(ee1);

            labelNodeEdges();

            /*
             * Compute the labeling for isolated components
             * <br>
             * Isolated components are components that do not touch any other components in the graph.
             * They can be identified by the fact that they will
             * contain labels containing ONLY a single element, the one for their parent point.
             * We only need to check components contained in the input graphs, since
             * isolated components will not have been replaced by new components formed by intersections.
             */
            labelIsolatedEdges(0, 1);
            labelIsolatedEdges(1, 0);

            // update the IM from all components
            updateIntersectionMatrix(im);
            return im;
        }

        private void insertEdgeEnds(IEnumerable<EdgeEnd<TCoordinate>> ee)
        {
            foreach (EdgeEnd<TCoordinate> end in ee)
            {
                _nodes.Add(end);
            }
        }

        private void computeProperIntersectionIntersectionMatrix(SegmentIntersector<TCoordinate> intersector, IntersectionMatrix im)
        {
            // If a proper intersection is found, we can set a lower bound on the IM.
            Dimensions dimA = _g0.Geometry.Dimension;
            Dimensions dimB = _g1.Geometry.Dimension;
            Boolean hasProper = intersector.HasProperIntersection;
            Boolean hasProperInterior = intersector.HasProperInteriorIntersection;

            // For Geometry's of dim 0 there can never be proper intersections.
            /*
             * If edge segments of Areas properly intersect, the areas must properly overlap.
             */
            if (dimA == Dimensions.Surface && dimB == Dimensions.Surface)
            {
                if (hasProper)
                {
                    im.SetAtLeast("212101212");
                }
            }

                /*
             * If an Line segment properly intersects an edge segment of an Area,
             * it follows that the Interior of the Line intersects the Boundary of the Area.
             * If the intersection is a proper <i>interior</i> intersection, then
             * there is an Interior-Interior intersection too.
             * Note that it does not follow that the Interior of the Line intersects the Exterior
             * of the Area, since there may be another Area component which contains the rest of the Line.
             */
            else if (dimA == Dimensions.Surface && dimB == Dimensions.Curve)
            {
                if (hasProper)
                {
                    im.SetAtLeast("FFF0FFFF2");
                }
                if (hasProperInterior)
                {
                    im.SetAtLeast("1FFFFF1FF");
                }
            }

            else if (dimA == Dimensions.Curve && dimB == Dimensions.Surface)
            {
                if (hasProper)
                {
                    im.SetAtLeast("F0FFFFFF2");
                }
                if (hasProperInterior)
                {
                    im.SetAtLeast("1F1FFFFFF");
                }
            }

                /* If edges of LineStrings properly intersect *in an interior point*, all
               we can deduce is that
               the interiors intersect.  (We can NOT deduce that the exteriors intersect,
               since some other segments in the geometries might cover the points in the
               neighbourhood of the intersection.)
               It is important that the point be known to be an interior point of
               both Geometries, since it is possible in a self-intersecting point to
               have a proper intersection on one segment that is also a boundary point of another segment.
            */
            else if (dimA == Dimensions.Curve && dimB == Dimensions.Curve)
            {
                if (hasProperInterior)
                {
                    im.SetAtLeast("0FFFFFFFF");
                }
            }
        }

        /// <summary>
        /// Copy all nodes from an arg point into this graph.
        /// The node label in the arg point overrides any previously computed
        /// label for that argIndex.
        /// (E.g. a node may be an intersection node with
        /// a computed label of Boundary,
        /// but in the original arg Geometry it is actually
        /// in the interior due to the Boundary Determination Rule)
        /// </summary>
        private void copyNodesAndLabels(Int32 argIndex)
        {
            GeometryGraph<TCoordinate> graph = getGraph(argIndex);

            foreach (Node<TCoordinate> node in graph.Nodes)
            {
                Node<TCoordinate> newNode = _nodes.AddNode(node.Coordinate);
                newNode.SetLabel(argIndex, node.Label.Value[argIndex].On);
            }
        }

        /// <summary>
        /// Insert nodes for all intersections on the edges of a Geometry.
        /// Label the created nodes the same as the edge label if they do not already have a label.
        /// This allows nodes created by either self-intersections or
        /// mutual intersections to be labeled.
        /// Endpoint nodes will already be labeled from when they were inserted.
        /// </summary>
        private void computeIntersectionNodes(Int32 argIndex)
        {
            GeometryGraph<TCoordinate> graph = getGraph(argIndex);

            foreach (Edge<TCoordinate> e in graph.Edges)
            {
                Debug.Assert(e.Label != null);
                Locations eLoc = e.Label.Value[argIndex].On;

                foreach (EdgeIntersection<TCoordinate> intersection in e.EdgeIntersectionList)
                {
                    RelateNode<TCoordinate> n = _nodes.AddNode(intersection.Coordinate) as RelateNode<TCoordinate>;
                    Debug.Assert(n != null);

                    if (eLoc == Locations.Boundary)
                    {
                        n.SetLabelBoundary(argIndex);
                    }
                    else
                    {
                        Debug.Assert(n.Label != null);
                        if (n.Label.Value.IsNull(argIndex))
                        {
                            n.SetLabel(argIndex, Locations.Interior);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// For all intersections on the edges of a Geometry,
        /// label the corresponding node IF it doesn't already have a label.
        /// This allows nodes created by either self-intersections or
        /// mutual intersections to be labeled.
        /// Endpoint nodes will already be labeled from when they were inserted.
        /// </summary>
        private void labelIntersectionNodes(Int32 argIndex)
        {
            GeometryGraph<TCoordinate> graph = getGraph(argIndex);

            foreach (Edge<TCoordinate> e in graph.Edges)
            {
                Locations eLoc = e.Label.Value[argIndex].On;

                foreach (EdgeIntersection<TCoordinate> intersection in e.EdgeIntersectionList)
                {
                    RelateNode<TCoordinate> n = _nodes.Find(intersection.Coordinate) as RelateNode<TCoordinate>;
                    Debug.Assert(n != null);
                    Debug.Assert(n.Label != null);
                    if (n.Label.Value.IsNull(argIndex))
                    {
                        if (eLoc == Locations.Boundary)
                        {
                            n.SetLabelBoundary(argIndex);
                        }
                        else
                        {
                            n.SetLabel(argIndex, Locations.Interior);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If the Geometries are disjoint, we need to enter their dimension and
        /// boundary dimension in the Ext rows in the IM
        /// </summary>
        private void computeDisjointIM(IntersectionMatrix im)
        {
            IGeometry ga = _g0.Geometry;

            if (!ga.IsEmpty)
            {
                im.Set(Locations.Interior, Locations.Exterior, ga.Dimension);
                im.Set(Locations.Boundary, Locations.Exterior, ga.BoundaryDimension);
            }

            IGeometry gb = _g1.Geometry;

            if (!gb.IsEmpty)
            {
                im.Set(Locations.Exterior, Locations.Interior, gb.Dimension);
                im.Set(Locations.Exterior, Locations.Boundary, gb.BoundaryDimension);
            }
        }

        private void labelNodeEdges()
        {
            foreach (RelateNode<TCoordinate> node in _nodes)
            {
                node.Edges.ComputeLabeling(_g0, _g1);
            }
        }

        /// <summary>
        /// Update the IM with the sum of the IMs for each component.
        /// </summary>
        private void updateIntersectionMatrix(IntersectionMatrix im)
        {
            foreach (Edge<TCoordinate> e in _isolatedEdges)
            {
                e.UpdateIntersectionMatrix(im);
            }

            foreach (RelateNode<TCoordinate> node in _nodes)
            {
                node.UpdateIntersectionMatrix(im);
                node.UpdateIntersectionMatrixFromEdges(im);
            }
        }

        /// <summary> 
        /// Processes isolated edges by computing their labeling and adding them
        /// to the isolated edges list.
        /// Isolated edges are guaranteed not to touch the boundary of the target (since if they
        /// did, they would have caused an intersection to be computed and hence would
        /// not be isolated).
        /// </summary>
        private void labelIsolatedEdges(Int32 thisIndex, Int32 targetIndex)
        {
            GeometryGraph<TCoordinate> graph = getGraph(thisIndex);

            foreach (Edge<TCoordinate> e in graph.Edges)
            {
                if (e.IsIsolated)
                {
                    GeometryGraph<TCoordinate> target = getGraph(targetIndex);
                    labelIsolatedEdge(e, targetIndex, target.Geometry);
                    _isolatedEdges.Add(e);
                }
            }
        }

        /// <summary>
        /// Label an isolated edge of a graph with its relationship to the target point.
        /// If the target has dim 2 or 1, the edge can either be in the interior or the exterior.
        /// If the target has dim 0, the edge must be in the exterior.
        /// </summary>
        private void labelIsolatedEdge(Edge<TCoordinate> e, Int32 targetIndex, IGeometry<TCoordinate> target)
        {
            // this won't work for GeometryCollections with both dim 2 and 1 geoms
            if (target.Dimension > 0)
            {
                // since edge is not in boundary, may not need the full generality of PointLocator?
                // Possibly should use ptInArea locator instead?  We probably know here
                // that the edge does not touch the bdy of the target Geometry
                Locations loc = _ptLocator.Locate(e.Coordinate, target);
                e.Label.SetAllLocations(targetIndex, loc);
            }
            else
            {
                e.Label.SetAllLocations(targetIndex, Locations.Exterior);
            }
        }

        /// <summary>
        /// Isolated nodes are nodes whose labels are incomplete
        /// (e.g. the location for one Geometry is null).
        /// This is the case because nodes in one graph which don't intersect
        /// nodes in the other are not completely labeled by the initial process
        /// of adding nodes to the nodeList.
        /// To complete the labeling we need to check for nodes that lie in the
        /// interior of edges, and in the interior of areas.
        /// </summary>
        private void labelIsolatedNodes()
        {
            foreach (Node<TCoordinate> n in _nodes)
            {
                Debug.Assert(n.Label != null);
                Label label = n.Label.Value;

                // isolated nodes should always have at least one point in their label
                Assert.IsTrue(label.GeometryCount > 0, "node with empty label found");

                if (n.IsIsolated)
                {
                    if (label.IsNull(0))
                    {
                        labelIsolatedNode(n, 0);
                    }
                    else
                    {
                        labelIsolatedNode(n, 1);
                    }
                }
            }
        }

        /// <summary>
        /// Label an isolated node with its relationship to the target point.
        /// </summary>
        private void labelIsolatedNode(Node<TCoordinate> n, Int32 targetIndex)
        {
            GeometryGraph<TCoordinate> graph = getGraph(targetIndex);
            Locations loc = _ptLocator.Locate(n.Coordinate, graph.Geometry);
            n.Label.SetAllLocations(targetIndex, loc);
        }

        private GeometryGraph<TCoordinate> getGraph(int argIndex)
        {
            return argIndex == 0 ? _g0 : _g1;
        }
    }
}