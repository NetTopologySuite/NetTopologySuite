using System.Collections.Generic;
using System.Collections.ObjectModel;
using NetTopologySuite.Geometries;
using NetTopologySuite.GeometriesGraph;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.OverlayNg
{
    /// <summary>
    /// Implements the logic to compute the full labeling
    /// for the edges in an <see cref="OverlayGraph"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    class OverlayLabeller
    {
        private readonly OverlayGraph _graph;
        private readonly InputGeometry _inputGeometry;
        private readonly IList<OverlayEdge> _edges;

        public OverlayLabeller(OverlayGraph graph, InputGeometry inputGeometry)
        {
            _graph = graph;
            _inputGeometry = inputGeometry;
            _edges = graph.Edges;
        }

        /**
         * Computes the topological labelling for the edges in the graph.
         * 
         */
        public void ComputeLabelling()
        {
            var nodes = _graph.NodeEdges;
            LabelAreaNodeEdges(nodes);

            //TODO: is there a way to avoid scanning all edges in these steps?
            /**
             * At this point collapsed edges labeled with location UNKNOWN
             * must be disconnected from the area edges of the parent.
             * They can be located based on their parent ring role (shell or hole).
             */
            LabelCollapsedEdges();

            LabelDisconnectedEdges();
        }


        /**
         * Labels node edges based on the arrangement
         * of boundary edges incident on them.
         * Also propagates the labelling to connected linear edges.
         *  
         * @param nodes the nodes to label
         */
        private void LabelAreaNodeEdges(IEnumerable<OverlayEdge> nodes)
        {
            foreach (var nodeEdge in nodes)
            {
                PropagateAreaLocations(nodeEdge, 0);
                if (_inputGeometry.hasEdges(1))
                {
                    PropagateAreaLocations(nodeEdge, 1);
                }
            }
            labelConnectedLinearEdges();
        }

        /**
         * Scans around a node CCW, propagating the side labels
         * for a given area geometry to all edges (and their sym)
         * with unknown locations for that geometry.
         * @param e2 
         * 
         * @param geomIndex the geometry to propagate locations for
         */
        public static void PropagateAreaLocations(OverlayEdge nodeEdge, int geomIndex)
        {
            /**
             * This handles dangling edges created by overlap limiting
             */
            if (nodeEdge.Degree() == 1) return;

            var eStart = FindPropagationStartEdge(nodeEdge, geomIndex);
            // no labelled edge found, so nothing to propagate
            if (eStart == null)
                return;

            // initialize currLoc to location of L side
            var currLoc = eStart.GetLocation(geomIndex, Positions.Left);
            var e = eStart.ONextOE;

            //Debug.println("\npropagateSideLabels geomIndex = " + geomIndex + " : " + eStart);
            //Debug.print("BEFORE: " + toString(eStart));

            do
            {
                var label = e.Label;
                if (!label.isBoundary(geomIndex))
                {
                    /**
                     * If this is not a Boundary edge for this input area, 
                     * its location is now known relative to this input area
                     */
                    label.setLocationLine(geomIndex, currLoc);
                }
                else
                {
                    Assert.IsTrue(label.hasSides(geomIndex));
                    /**
                     *  This is a boundary edge for the input area geom.
                     *  Update the current location from its labels.
                     *  Also check for topological consistency.
                     */
                    var locRight = e.GetLocation(geomIndex, Positions.Right);
                    if (locRight != currLoc)
                    {
                        /*
                        Debug.println("side location conflict: index= " + geomIndex + " R loc " 
                      + Location.toLocationSymbol(locRight) + " <>  curr loc " + Location.toLocationSymbol(currLoc) 
                      + " for " + e);
                      //*/
                        throw new TopologyException("side location conflict: arg " + geomIndex, e.Coordinate);
                    }
                    var locLeft = e.GetLocation(geomIndex, Positions.Left);
                    if (locLeft == Location.Null)
                    {
                        Assert.ShouldNeverReachHere("found single null side at " + e);
                    }
                    currLoc = locLeft;
                }
                e = e.ONextOE;
            } while (e != eStart);
            //Debug.print("AFTER: " + toString(eStart));
        }

        /**
         * Finds a boundary edge for this geom, if one exists
         * 
         * @param nodeEdge an edge for this node
         * @param geomIndex the parent geometry index
         * @return a boundary edge, or null if no boundary edge exists
         */
        private static OverlayEdge FindPropagationStartEdge(OverlayEdge nodeEdge, int geomIndex)
        {
            var eStart = nodeEdge;
            do
            {
                var label = eStart.Label;
                if (label.isBoundary(geomIndex))
                {
                    Assert.IsTrue(label.hasSides(geomIndex));
                    return eStart;
                }
                eStart = eStart.ONextOE;
            } while (eStart != nodeEdge);
            return null;
        }

        /**
         * At this point collapsed edges with unknown location
         * must be disconnected from the boundary edges of the parent
         * (because otherwise the location would have
         * been propagated from them).
         * They can be now located based on their parent ring role (shell or hole).
         * (This cannot be done earlier, because the location
         * based on the boundary edges must take precedence.
         * There are situations where a collapsed edge has a location 
         * which is different to its ring role - 
         * e.g. a narrow gore in a polygon, which is in 
         * the interior of the reduced polygon, but whose
         * ring role would imply the location EXTERIOR.)
         * 
         * Note that collapsed edges can NOT have location determined via a PIP location check,
         * because that is done against the unreduced input geometry,
         * which may give an invalid result due to topology collapse.
         * 
         * The labeling is propagated to other connected edges, 
         * since there may be NOT_PART edges which are connected, 
         * and they need to be labeled in the same way.
         */
        private void LabelCollapsedEdges()
        {
            foreach (var edge in _edges)
            {
                if (edge.Label.isLineLocationUnknown(0))
                {
                    labelCollapsedEdge(edge, 0);
                }
                if (edge.Label.isLineLocationUnknown(1))
                {
                    labelCollapsedEdge(edge, 1);
                }
            }
            labelConnectedLinearEdges();
        }

        private void labelCollapsedEdge(OverlayEdge edge, int geomIndex)
        {
            //Debug.println("\n------  labelCollapsedEdge - geomIndex= " + geomIndex);
            //Debug.print("BEFORE: " + edge.toStringNode());
            var label = edge.Label;
            if (!label.isCollapse(geomIndex)) return;
            /**
             * This must be a collapsed edge which is disconnected
             * from any area edges (e.g. a fully collapsed shell or hole).
             * It can be labeled according to its parent source ring role. 
             */
            label.setLocationCollapse(geomIndex);
            //Debug.print("AFTER: " + edge.toStringNode());
        }

        /**
         * There can be edges which have unknown location
         * but are connected to a Line edge with known location.
         * In this case line location is propagated to the connected edges.
         */
        private void labelConnectedLinearEdges()
        {
            //TODO: can these be merged to avoid two scans?
            propagateLineLocations(0);
            if (_inputGeometry.hasEdges(1))
            {
                propagateLineLocations(1);
            }
        }

        private void propagateLineLocations(int geomIndex)
        {
            // find L edges
            var lineEdges = FindLinearEdgesWithLocation(geomIndex);
            // TODO: This is originally a ArrayDeque<T> in JTS.
            var edgeStack = new LinkedList<OverlayEdge>(lineEdges);

            PropagateLineLocations(geomIndex, edgeStack);
        }

        private void PropagateLineLocations(int geomIndex, LinkedList<OverlayEdge> edgeStack)
        {
            // traverse line edges, labelling unknown ones that are connected
            while (edgeStack.Count > 0)
            {
                var lineEdge = edgeStack.First;
                edgeStack.RemoveFirst();
                // assert: lineEdge.getLabel().isLine(geomIndex);

                // for any edges around origin with unknown location for this geomIndex,
                // add those edges to stack to continue traversal
                PropagateLineLocation(lineEdge.Value, geomIndex, edgeStack, _inputGeometry);
            }
        }

        private static void PropagateLineLocation(OverlayEdge eStart, int index,
            LinkedList<OverlayEdge> edgeStack, InputGeometry inputGeometry)
        {
            var e = eStart.ONextOE;
            var lineLoc = eStart.Label.getLineLocation(index);

            /**
             * If the parent geom is an L (dim 1) 
             * then only propagate EXTERIOR locations.
             */
            if (inputGeometry.isLine(index)
                && lineLoc != Location.Exterior) return;

            do
            {
                var label = e.Label;
                //Debug.println("propagateLineLocationAtNode - checking " + index + ": " + e);
                if (label.isLineLocationUnknown(index))
                {
                    /**
                     * If edge is not a boundary edge, 
                     * its location is now known for this area
                     */
                    label.setLocationLine(index, lineLoc);
                    //Debug.println("propagateLineLocationAtNode - setting "+ index + ": " + e);

                    /**
                     * Add sym edge to stack for graph traversal
                     * (Don't add e itself, since e origin node has now been scanned)
                     */
                    edgeStack.AddFirst(e.SymOE);
                }
                e = e.ONextOE;
            } while (e != eStart);
        }

        /**
         * Finds all OverlayEdges which are labelled as L dimension.
         * 
         * @param geomIndex
         * @return list of L edges
         */
        private List<OverlayEdge> FindLinearEdgesWithLocation(int geomIndex)
        {
            var lineEdges = new List<OverlayEdge>();
            foreach (var edge in _edges)
            {
                var lbl = edge.Label;
                if (lbl.isLinear(geomIndex)
                    && !lbl.isLineLocationUnknown(geomIndex))
                {
                    lineEdges.Add(edge);
                }
            }
            return lineEdges;
        }

        /**
         * At this point there may still be edges which have unknown location
         * relative to an input geometry.
         * This must be because they are NOT_PART edges for that geometry, 
         * and are disconnected from any edges of that geometry.
         * An example of this is rings of one geometry wholly contained
         * in another geometry.
         * The location must be fully determined to compute a 
         * correct result for all overlay operations.
         * 
         * If the input geometry is an Area the edge location can
         * be determined via a PIP test.
         * If the input is not an Area the location is EXTERIOR. 
         */
        private void LabelDisconnectedEdges()
        {
            foreach (var edge in _edges)
            {
                //Debug.println("\n------  checking for Disconnected edge " + edge);
                if (edge.Label.isLineLocationUnknown(0))
                {
                    LabelDisconnectedEdge(edge, 0);
                }
                if (edge.Label.isLineLocationUnknown(1))
                {
                    LabelDisconnectedEdge(edge, 1);
                }
            }
        }

        /**
         * Determines the location of an edge relative to a target input geometry.
         * The edge has no location information
         * because it is disconnected from other
         * edges that would provide that information.
         * The location is determined by checking 
         * if the edge lies inside the target geometry area (if any).
         * 
         * @param edge the edge to label
         * @param geomIndex the input geometry to label against
         */
        private void LabelDisconnectedEdge(OverlayEdge edge, int geomIndex)
        {
            var label = edge.Label;
            //Assert.isTrue(label.isNotPart(geomIndex));

            /**
             * if target geom is not an area then 
             * edge must be EXTERIOR, since to be 
             * INTERIOR it would have been labelled
             * when it was created.
             */
            if (!_inputGeometry.isArea(geomIndex))
            {
                label.setLocationAll(geomIndex, Location.Exterior);
                return;
            };

            //Debug.println("\n------  labelDisconnectedEdge - geomIndex= " + geomIndex);
            //Debug.print("BEFORE: " + edge.toStringNode());
            /**
             * Locate edge in input area using a Point-In-Poly check.
             * This should be safe even with precision reduction, 
             * because since the edge has remained disconnected
             * its interior-exterior relationship 
             * can be determined relative to the original input geometry.
             */
            //int edgeLoc = locateEdge(geomIndex, edge);
            var edgeLoc = LocateEdgeBothEnds(geomIndex, edge);
            label.setLocationAll(geomIndex, edgeLoc);
            //Debug.print("AFTER: " + edge.toStringNode());
        }

        /**
         * Determines the {@link Location} for an edge within an Area geometry
         * via point-in-polygon location.
         * <p>
         * NOTE this is only safe to use for disconnected edges,
         * since the test is carried out against the original input geometry,
         * and precision reduction may cause incorrect results for edges
         * which are close enough to a boundary to become connected. 
         * 
         * @param geomIndex the parent geometry index
         * @param edge the edge to locate
         * @return the location of the edge
         */
        private Location locateEdge(int geomIndex, OverlayEdge edge)
        {
            var loc = _inputGeometry.locatePointInArea(geomIndex, edge.Orig);
            var edgeLoc = loc != Location.Exterior ? Location.Interior : Location.Exterior;
            return edgeLoc;
        }

        /**
         * Determines the {@link Location} for an edge within an Area geometry
         * via point-in-polygon location,
         * by checking that both endpoints are interior to the target geometry.
         * Checking both endpoints ensures correct results in the presence of topology collapse.
         * <p>
         * NOTE this is only safe to use for disconnected edges,
         * since the test is carried out against the original input geometry,
         * and precision reduction may cause incorrect results for edges
         * which are close enough to a boundary to become connected. 
         * 
         * @param geomIndex the parent geometry index
         * @param edge the edge to locate
         * @return the location of the edge
         */
        private Location LocateEdgeBothEnds(int geomIndex, OverlayEdge edge)
        {
            /*
             * To improve the robustness of the point location,
             * check both ends of the edge.
             * Edge is only labelled INTERIOR if both ends are.
             */
            var locOrig = _inputGeometry.locatePointInArea(geomIndex, edge.Orig);
            var locDest = _inputGeometry.locatePointInArea(geomIndex, edge.Dest);
            bool isInt = locOrig != Location.Exterior && locDest != Location.Exterior;
            var edgeLoc = isInt ? Location.Interior : Location.Exterior;
            return edgeLoc;
        }

        public void markResultAreaEdges(SpatialFunction overlayOpCode)
        {
            foreach (var edge in _edges)
            {
                markInResultArea(edge, overlayOpCode);
            }
        }

        /**
         * Marks an edge which forms part of the boundary of the result area.
         * This is determined by the overlay operation being executed,
         * and the location of the edge.
         * The relevant location is either the right side of a boundary edge,
         * or the line location of a non-boundary edge.
         * 
         * @param e the edge to mark
         * @param overlayOpCode the overlay operation
         */
        public void markInResultArea(OverlayEdge e, SpatialFunction overlayOpCode)
        {
            var label = e.Label;
            if (label.isBoundaryEither()
                && OverlayNG.isResultOfOp(
                    overlayOpCode,
                    label.getLocationBoundaryOrLine(0, Positions.Right, e.IsForward),
                    label.getLocationBoundaryOrLine(1, Positions.Right, e.IsForward)))
            {
                e.markInResultArea();
            }
            //Debug.println("markInResultArea: " + e);
        }

        /**
         * Unmarks result area edges where the sym edge 
         * is also marked as in the result.
         * This has the effect of merging edge-adjacent result areas,
         * as required by polygon validity rules.
         */
        public void unmarkDuplicateEdgesFromResultArea()
        {
            foreach (var edge in _edges)
            {
                if (edge.IsInResultAreaBoth)
                {
                    edge.unmarkFromResultAreaBoth();
                }
            }
        }

    }
}
