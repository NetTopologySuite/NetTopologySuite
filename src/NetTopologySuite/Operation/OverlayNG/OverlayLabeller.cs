using System.Collections.Generic;
using System.Text;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Operation.Overlay;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.OverlayNG
{
    /// <summary>
    /// Implements the logic to compute the full labeling
    /// for the edges in an <see cref="OverlayGraph"/>.
    /// </summary>
    /// <author>Martin Davis</author>
    internal sealed class OverlayLabeller
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

        /// <summary>
        /// Computes the topological labeling for the edges in the graph.
        /// </summary>
        public void ComputeLabelling()
        {
            var nodes = _graph.NodeEdges;
            LabelAreaNodeEdges(nodes);
            LabelConnectedLinearEdges();

            //TODO: is there a way to avoid scanning all edges in these steps?
            /*
             * At this point collapsed edges labeled with location UNKNOWN
             * must be disconnected from the area edges of the parent.
             * This can occur with a collapsed hole or shell.
             * The edges can be labeled based on their parent ring role (shell or hole).
             */
            LabelCollapsedEdges();
            LabelConnectedLinearEdges();

            LabelDisconnectedEdges();
        }

        /// <summary>
        /// Labels edges around nodes based on the arrangement
        /// of incident area boundary edges.
        /// Also propagates the labeling to connected linear edges.
        /// </summary>
        /// <param name="nodes">The nodes to label</param>
        private void LabelAreaNodeEdges(IEnumerable<OverlayEdge> nodes)
        {
            foreach (var nodeEdge in nodes)
            {
                PropagateAreaLocations(nodeEdge, 0);
                if (_inputGeometry.HasEdges(1))
                {
                    PropagateAreaLocations(nodeEdge, 1);
                }
            }
        }

        /// <summary>
        /// Scans around a node CCW, propagating the side labels
        /// for a given area geometry to all edges (and their sym)
        /// with unknown locations for that geometry.
        /// </summary>
        /// <param name="nodeEdge"></param>
        /// <param name="geomIndex">The geometry to propagate locations for</param>
        public void PropagateAreaLocations(OverlayEdge nodeEdge, int geomIndex)
        {
            /*
             * Only propagate for area geometries
             */
            if (!_inputGeometry.IsArea(geomIndex)) return;
            /*
             * No need to propagate if node has only one edge.
             * This handles dangling edges created by overlap limiting.
             */
            if (nodeEdge.Degree() == 1) return;

            var eStart = FindPropagationStartEdge(nodeEdge, geomIndex);
            // no labelled edge found, so nothing to propagate
            if (eStart == null)
                return;

            // initialize currLoc to location of L side
            var currLoc = eStart.GetLocation(geomIndex, Position.Left);
            var e = eStart.ONextOE;

            //Debug.println("\npropagateSideLabels geomIndex = " + geomIndex + " : " + eStart);
            //Debug.print("BEFORE: " + toString(eStart));

            do
            {
                var label = e.Label;
                if (!label.IsBoundary(geomIndex))
                {
                    /*
                     * If this is not a Boundary edge for this input area, 
                     * its location is now known relative to this input area
                     */
                    label.SetLocationLine(geomIndex, currLoc);
                }
                else
                {
                    // must be a boundary edge
                    Assert.IsTrue(label.HasSides(geomIndex));
                    /*
                     *  This is a boundary edge for the input area geom.
                     *  Update the current location from its labels.
                     *  Also check for topological consistency.
                     */
                    var locRight = e.GetLocation(geomIndex, Position.Right);
                    if (locRight != currLoc)
                    {
                        /*
                        Debug.println("side location conflict: index= " + geomIndex + " R loc " 
                      + Location.toLocationSymbol(locRight) + " <>  curr loc " + Location.toLocationSymbol(currLoc) 
                      + " for " + e);
                      //*/
                        throw new TopologyException("side location conflict: arg " + geomIndex, e.Coordinate);
                    }
                    var locLeft = e.GetLocation(geomIndex, Position.Left);
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

        /// <summary>
        /// Finds a boundary edge for this geom originating at the given
        /// node, if one exists.
        /// A boundary edge should exist if this is a node on the boundary
        /// of the parent area geometry.
        /// </summary>
        /// <param name="nodeEdge">An edge for this node</param>
        /// <param name="geomIndex">The parent geometry index</param>
        /// <returns>A boundary edge, or <c>null</c> if no boundary edge exists</returns>
        private static OverlayEdge FindPropagationStartEdge(OverlayEdge nodeEdge, int geomIndex)
        {
            var eStart = nodeEdge;
            do
            {
                var label = eStart.Label;
                if (label.IsBoundary(geomIndex))
                {
                    Assert.IsTrue(label.HasSides(geomIndex));
                    return eStart;
                }
                eStart = eStart.ONextOE;
            } while (eStart != nodeEdge);
            return null;
        }

        /// <summary>
        /// At this point collapsed edges with unknown location
        /// must be disconnected from the boundary edges of the parent
        /// (because otherwise the location would have
        /// been propagated from them).<br/>
        /// They can be now located based on their parent ring role(shell or hole).
        /// (This cannot be done earlier, because the location
        /// based on the boundary edges must take precedence.<br/>
        /// There are situations where a collapsed edge has a location 
        /// which is different to its ring role - 
        /// e.g.a narrow gore in a polygon, which is in
        /// the interior of the reduced polygon, but whose
        /// ring role would imply the location EXTERIOR.)
        /// <para/> 
        /// Note that collapsed edges can NOT have location determined via a PIP location check,
        /// because that is done against the unreduced input geometry,
        /// which may give an invalid result due to topology collapse.
        /// <para/>
        /// The labeling is propagated to other connected linear edges,
        /// since there may be NOT_PART edges which are connected,
        /// and they can be labeled in the same way.
        /// (These would get labeled anyway during subsequent disconnected labeling pass,
        /// but may be more efficient and accurate to do it here.)
        /// </summary>
        private void LabelCollapsedEdges()
        {
            foreach (var edge in _edges)
            {
                if (edge.Label.IsLineLocationUnknown(0))
                {
                    LabelCollapsedEdge(edge, 0);
                }
                if (edge.Label.IsLineLocationUnknown(1))
                {
                    LabelCollapsedEdge(edge, 1);
                }
            }
        }

        private void LabelCollapsedEdge(OverlayEdge edge, int geomIndex)
        {
            //Debug.println("\n------  labelCollapsedEdge - geomIndex= " + geomIndex);
            //Debug.print("BEFORE: " + edge.toStringNode());
            var label = edge.Label;
            if (!label.IsCollapse(geomIndex)) return;
            /*
             * This must be a collapsed edge which is disconnected
             * from any area edges (e.g. a fully collapsed shell or hole).
             * It can be labeled according to its parent source ring role. 
             */
            label.SetLocationCollapse(geomIndex);
            //Debug.print("AFTER: " + edge.toStringNode());
        }

        /// <summary>
        /// There can be edges which have unknown location
        /// but are connected to a linear edge with known location.
        /// In this case linear location is propagated to the connected edges.
        /// </summary>
        private void LabelConnectedLinearEdges()
        {
            //TODO: can these be merged to avoid two scans?
            PropagateLinearLocations(0);
            if (_inputGeometry.HasEdges(1))
            {
                PropagateLinearLocations(1);
            }
        }

        /// <summary>
        /// Performs a breadth-first graph traversal to find and label
        /// connected linear edges.
        /// </summary>
        /// <param name="geomIndex">The index of the input geometry to label.</param>
        private void PropagateLinearLocations(int geomIndex)
        {
            // find located linear edges
            var linearEdges = FindLinearEdgesWithLocation(_edges, geomIndex);
            if (linearEdges.Count == 0) return;

            // Note: This is originally a ArrayDeque<T> in JTS. It is used as a Stack.
            var edgeStack = new Stack<OverlayEdge>(linearEdges);
            bool isInputLine = _inputGeometry.IsLine(geomIndex);

            // traverse line edges, labeling unknown ones that are connected
            while (edgeStack.Count > 0)
            {
                var lineEdge = edgeStack.Pop();
                // assert: lineEdge.getLabel().isLine(geomIndex);

                // for any edges around origin with unknown location for this geomIndex,
                // add those edges to stack to continue traversal
                PropagateLinearLocationAtNode(lineEdge, geomIndex, isInputLine, edgeStack);
            }
        }

        private static void PropagateLinearLocationAtNode(OverlayEdge eNode, int geomIndex,
            bool isInputLine, Stack<OverlayEdge> edgeStack)
        {
            // Note: edgeStack is a Deque<OverlayEdge> in JTS. It is used as a Stack.
            var lineLoc = eNode.Label.GetLineLocation(geomIndex);

            /*
             * If the parent geom is a Line
             * then only propagate EXTERIOR locations.
             */
            if (isInputLine && lineLoc != Location.Exterior) return;

            //Debug.println("check " + geomIndex + " from: " + eNode);
            var e = eNode.ONextOE;
            do
            {
                var label = e.Label;
                //Debug.println(""*** setting "+ geomIndex + ": " + e);
                if (label.IsLineLocationUnknown(geomIndex))
                {
                    /*
                     * If edge is not a boundary edge, 
                     * its location is now known for this area
                     */
                    label.SetLocationLine(geomIndex, lineLoc);
                    //Debug.println("propagateLineLocationAtNode - setting "+ index + ": " + e);

                    /*
                     * Add sym edge to stack for graph traversal
                     * (Don't add e itself, since e origin node has now been scanned)
                     */
                    edgeStack.Push(e.SymOE);
                }
                e = e.ONextOE;
            } while (e != eNode);
        }

        /// <summary>
        /// Finds all OverlayEdges which are linear
        /// (i.e.line or collapsed) and have a known location
        /// for the given input geometry.
        /// </summary>
        /// <param name="geomIndex">The index of the input geometry</param>
        /// <returns>A list of linear edges with known location</returns>
        private static List<OverlayEdge> FindLinearEdgesWithLocation(IEnumerable<OverlayEdge> edges, int geomIndex)
        {
            var lineEdges = new List<OverlayEdge>();
            foreach (var edge in edges)
            {
                var lbl = edge.Label;
                // keep if linear with known location
                if (lbl.IsLinear(geomIndex)
                    && !lbl.IsLineLocationUnknown(geomIndex))
                {
                    lineEdges.Add(edge);
                }
            }
            return lineEdges;
        }

        /// <summary>
        /// At this point there may still be edges which have unknown location
        /// relative to an input geometry.<br/>
        /// This must be because they are NOT_PART edges for that geometry,
        /// and are disconnected from any edges of that geometry.
        /// An example of this is rings of one geometry wholly contained
        /// in another geometry.<br/>
        /// The location must be fully determined to compute a
        /// correct result for all overlay operations.
        /// <para/>
        /// If the input geometry is an Area the edge location can
        /// be determined via a PIP test.
        /// If the input is not an Area the location is EXTERIOR.
        /// </summary>
        private void LabelDisconnectedEdges()
        {
            foreach (var edge in _edges)
            {
                //Debug.println("\n------  checking for Disconnected edge " + edge);
                if (edge.Label.IsLineLocationUnknown(0))
                {
                    LabelDisconnectedEdge(edge, 0);
                }
                if (edge.Label.IsLineLocationUnknown(1))
                {
                    LabelDisconnectedEdge(edge, 1);
                }
            }
        }

        /// <summary>
        /// Determines the location of an edge relative to a target input geometry.
        /// The edge has no location information
        /// because it is disconnected from other
        /// edges that would provide that information.
        /// The location is determined by checking
        /// if the edge lies inside the target geometry area(if any).
        /// </summary>
        /// <param name="edge">The edge to label</param>
        /// <param name="geomIndex">The input geometry to label against</param>
        private void LabelDisconnectedEdge(OverlayEdge edge, int geomIndex)
        {
            var label = edge.Label;
            //Assert.isTrue(label.isNotPart(geomIndex));

            /*
             * if target geom is not an area then 
             * edge must be EXTERIOR, since to be 
             * INTERIOR it would have been labelled
             * when it was created.
             */
            if (!_inputGeometry.IsArea(geomIndex))
            {
                label.SetLocationAll(geomIndex, Location.Exterior);
                return;
            };

            //Debug.println("\n------  labelDisconnectedEdge - geomIndex= " + geomIndex);
            //Debug.print("BEFORE: " + edge.toStringNode());
            /*
             * Locate edge in input area using a Point-In-Poly check.
             * This should be safe even with precision reduction, 
             * because since the edge has remained disconnected
             * its interior-exterior relationship 
             * can be determined relative to the original input geometry.
             */
            //int edgeLoc = locateEdge(geomIndex, edge);
            var edgeLoc = LocateEdgeBothEnds(geomIndex, edge);
            label.SetLocationAll(geomIndex, edgeLoc);
            //Debug.print("AFTER: " + edge.toStringNode());
        }

        /// <summary>
        /// Determines the <see cref="Location"/> for an edge within an Area geometry
        /// via point-in-polygon location.
        /// <para/>
        /// NOTE this is only safe to use for disconnected edges,
        /// since the test is carried out against the original input geometry,
        /// and precision reduction may cause incorrect results for edges
        /// which are close enough to a boundary to become connected.
        /// </summary>
        /// <param name="geomIndex">The parent geometry index</param>
        /// <param name="edge">The edge to locate</param>
        /// <returns>The location of the edge.</returns>
        private Location LocateEdge(int geomIndex, OverlayEdge edge)
        {
            var loc = _inputGeometry.LocatePointInArea(geomIndex, edge.Orig);
            var edgeLoc = loc != Location.Exterior ? Location.Interior : Location.Exterior;
            return edgeLoc;
        }

        /// <summary>
        /// Determines the {@link Location} for an edge within an Area geometry
        /// via point-in-polygon location,
        /// by checking that both endpoints are interior to the target geometry.
        /// Checking both endpoints ensures correct results in the presence of topology collapse.
        /// <para/>
        /// NOTE this is only safe to use for disconnected edges,
        /// since the test is carried out against the original input geometry,
        /// and precision reduction may cause incorrect results for edges
        /// which are close enough to a boundary to become connected. 
        /// </summary>
        /// <param name="geomIndex">The parent geometry index</param>
        /// <param name="edge">The edge to locate</param>
        /// <returns>The location of the edge</returns>
        private Location LocateEdgeBothEnds(int geomIndex, OverlayEdge edge)
        {
            /*
             * To improve the robustness of the point location,
             * check both ends of the edge.
             * Edge is only labelled INTERIOR if both ends are.
             */
            var locOrig = _inputGeometry.LocatePointInArea(geomIndex, edge.Orig);
            var locDest = _inputGeometry.LocatePointInArea(geomIndex, edge.Dest);
            bool isInt = locOrig != Location.Exterior && locDest != Location.Exterior;
            var edgeLoc = isInt ? Location.Interior : Location.Exterior;
            return edgeLoc;
        }

        public void MarkResultAreaEdges(SpatialFunction overlayOpCode)
        {
            foreach (var edge in _edges)
            {
                MarkInResultArea(edge, overlayOpCode);
            }
        }

        /// <summary>
        /// Marks an edge which forms part of the boundary of the result area.
        /// This is determined by the overlay operation being executed,
        /// and the location of the edge.
        /// The relevant location is either the right side of a boundary edge,
        /// or the line location of a non-boundary edge.
        /// </summary>
        /// <param name="e">The edge to mark</param>
        /// <param name="overlayOpCode">The overlay operation</param>
        public void MarkInResultArea(OverlayEdge e, SpatialFunction overlayOpCode)
        {
            var label = e.Label;
            if (label.IsBoundaryEither
                && OverlayNG.IsResultOfOp(
                    overlayOpCode,
                    label.GetLocationBoundaryOrLine(0, Position.Right, e.IsForward),
                    label.GetLocationBoundaryOrLine(1, Position.Right, e.IsForward)))
            {
                e.MarkInResultArea();
            }
            //Debug.println("markInResultArea: " + e);
        }

        /// <summary>
        /// Unmarks result area edges where the sym edge 
        /// is also marked as in the result.
        /// This has the effect of merging edge-adjacent result areas,
        /// as required by polygon validity rules.
        /// </summary>
        public void UnmarkDuplicateEdgesFromResultArea()
        {
            foreach (var edge in _edges)
            {
                if (edge.IsInResultAreaBoth)
                {
                    edge.UnmarkFromResultAreaBoth();
                }
            }
        }

        public static string ToString(OverlayEdge nodeEdge)
        {
            var orig = nodeEdge.Orig;
            var sb = new StringBuilder();
            sb.AppendFormat("Node( {0} )\n", WKTWriter.Format(orig));
            var e = nodeEdge;
            do
            {
                sb.Append($"  -> {e}");
                if (e.IsResultLinked)
                {
                    sb.Append(" Link: ");
                    sb.Append(e.NextResult);
                }
                sb.Append("\n");
                e = e.ONextOE;
            } while (e != nodeEdge);

            return sb.ToString();
        }
    }
}
