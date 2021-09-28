using System.Collections.Generic;
using System.IO;
using NetTopologySuite.Geometries;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.GeometriesGraph
{
    /// <summary>
    /// A DirectedEdgeStar is an ordered list of outgoing DirectedEdges around a node.
    /// It supports labelling the edges as well as linking the edges to form both
    /// MaximalEdgeRings and MinimalEdgeRings.
    /// </summary>
    public class DirectedEdgeStar : EdgeEndStar
    {
        /// <summary>
        /// A list of all outgoing edges in the result, in CCW order.
        /// </summary>
        private IList<DirectedEdge> _resultAreaEdgeList;

        private Label _label;
        /*
        /// <summary>
        ///
        /// </summary>
        public DirectedEdgeStar() { }
        */

        /// <summary>
        /// Insert a directed edge in the list.
        /// </summary>
        /// <param name="ee"></param>
        public override void Insert(EdgeEnd ee)
        {
            var de = (DirectedEdge) ee;
            InsertEdgeEnd(de, de);
        }

        /// <summary>
        ///
        /// </summary>
        public Label Label => _label;

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public int GetOutgoingDegree()
        {
            int degree = 0;
            foreach (DirectedEdge de in Edges)
                if (de.IsInResult)
                    degree++;
            return degree;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="er"></param>
        /// <returns></returns>
        public int GetOutgoingDegree(EdgeRing er)
        {
            int degree = 0;
            foreach (DirectedEdge de in Edges)
                if (de.EdgeRing == er)
                    degree++;
            return degree;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public DirectedEdge GetRightmostEdge()
        {
            var edges = Edges;
            int size = edges.Count;
            if (size < 1)
                return null;
            var de0 = (DirectedEdge)edges[0];
            if (size == 1)
                return de0;
            var deLast = (DirectedEdge)edges[size - 1];

            var quad0 = de0.QuadrantEx;
            var quad1 = deLast.QuadrantEx;
            if (quad0.IsNorthern && quad1.IsNorthern)
                return de0;
            else if (!quad0.IsNorthern && !quad1.IsNorthern)
                return deLast;
            else
            {
                // edges are in different hemispheres - make sure we return one that is non-horizontal
                if (de0.Dy != 0)
                    return de0;
                else if (deLast.Dy != 0)
                    return deLast;
            }
            Assert.ShouldNeverReachHere("found two horizontal edges incident on node");
            return null;
        }

        /// <summary>
        /// Compute the labelling for all dirEdges in this star, as well
        /// as the overall labelling.
        /// </summary>
        /// <param name="geom"></param>
        public override void ComputeLabelling(GeometryGraph[] geom)
        {
            base.ComputeLabelling(geom);

            // determine the overall labelling for this DirectedEdgeStar
            // (i.e. for the node it is based at)
            _label = new Label(Location.Null);
            var it = GetEnumerator();
            while(it.MoveNext())
            {
                var ee = it.Current;
                var e = ee.Edge;
                var eLabel = e.Label;
                for (int i = 0; i < 2; i++)
                {
                    var eLoc = eLabel.GetLocation(i);
                    if (eLoc == Location.Interior || eLoc == Location.Boundary)
                        _label.SetLocation(i, Location.Interior);
                }
            }
        }

        /// <summary>
        /// For each dirEdge in the star, merge the label .
        /// </summary>
        public void MergeSymLabels()
        {
            foreach (DirectedEdge de in Edges)
            {
                var label = de.Label;
                label.Merge(de.Sym.Label);
            }
        }

        /// <summary>
        /// Update incomplete dirEdge labels from the labeling for the node.
        /// </summary>
        /// <param name="nodeLabel">The label to apply</param>
        public void UpdateLabelling(Label nodeLabel)
        {
            foreach (DirectedEdge de in Edges)
            {
                var label = de.Label;
                label.SetAllLocationsIfNull(0, nodeLabel.GetLocation(0));
                label.SetAllLocationsIfNull(1, nodeLabel.GetLocation(1));
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        private IList<DirectedEdge> GetResultAreaEdges()
        {
            if (_resultAreaEdgeList != null)
                return _resultAreaEdgeList;
            _resultAreaEdgeList = new List<DirectedEdge>();
            foreach (DirectedEdge de in Edges)
            {
                if (de.IsInResult || de.Sym.IsInResult)
                    _resultAreaEdgeList.Add(de);
            }
            return _resultAreaEdgeList;
        }

        private const int ScanningForIncoming = 1;
        private const int LinkingToOutgoing = 2;

        /// <summary>
        /// Traverse the star of DirectedEdges, linking the included edges together.
        /// To link two dirEdges, the next pointer for an incoming dirEdge
        /// is set to the next outgoing edge.
        /// DirEdges are only linked if:
        /// they belong to an area (i.e. they have sides)
        /// they are marked as being in the result
        /// Edges are linked in CCW order (the order they are stored).
        /// This means that rings have their face on the Right
        /// (in other words, the topological location of the face is given by the RHS label of the DirectedEdge).
        /// PRECONDITION: No pair of dirEdges are both marked as being in the result.
        /// </summary>
        public void LinkResultDirectedEdges()
        {
            // make sure edges are copied to resultAreaEdges list
            GetResultAreaEdges();
            // find first area edge (if any) to start linking at
            DirectedEdge firstOut = null;
            DirectedEdge incoming = null;
            int state = ScanningForIncoming;
            // link edges in CCW order
            for (int i = 0; i < _resultAreaEdgeList.Count; i++)
            {
                var nextOut = _resultAreaEdgeList[i];
                var nextIn = nextOut.Sym;

                // skip de's that we're not interested in
                if (! nextOut.Label.IsArea())
                    continue;

                // record first outgoing edge, in order to link the last incoming edge
                if (firstOut == null && nextOut.IsInResult)
                    firstOut = nextOut;

                switch (state)
                {
                    case ScanningForIncoming:
                        if (!nextIn.IsInResult)
                            continue;
                        incoming = nextIn;
                        state = LinkingToOutgoing;
                        break;
                    case LinkingToOutgoing:
                        if (!nextOut.IsInResult)
                            continue;
                        incoming.Next = nextOut;
                        state = ScanningForIncoming;
                        break;
                    default:
                        break;
                }
            }
            if (state == LinkingToOutgoing)
            {
                if (firstOut == null)
                    throw new TopologyException("no outgoing dirEdge found", Coordinate);
                Assert.IsTrue(firstOut.IsInResult, "unable to link last incoming dirEdge");
                incoming.Next = firstOut;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="er"></param>
        public void LinkMinimalDirectedEdges(EdgeRing er)
        {
            // find first area edge (if any) to start linking at
            DirectedEdge firstOut = null;
            DirectedEdge incoming = null;
            int state = ScanningForIncoming;
            // link edges in CW order
            for (int i = _resultAreaEdgeList.Count - 1; i >= 0; i--)
            {
                var nextOut = this._resultAreaEdgeList[i];
                var nextIn = nextOut.Sym;

                // record first outgoing edge, in order to link the last incoming edge
                if (firstOut == null && nextOut.EdgeRing == er)
                    firstOut = nextOut;

                switch (state)
                {
                    case ScanningForIncoming:
                        if (nextIn.EdgeRing != er)
                            continue;
                        incoming = nextIn;
                        state = LinkingToOutgoing;
                        break;
                    case LinkingToOutgoing:
                        if (nextOut.EdgeRing != er)
                            continue;
                        incoming.NextMin = nextOut;
                        state = ScanningForIncoming;
                        break;
                    default:
                        break;
                }
            }
            if (state == LinkingToOutgoing)
            {
                Assert.IsTrue(firstOut != null, "found null for first outgoing dirEdge");
                Assert.IsTrue(firstOut.EdgeRing == er, "unable to link last incoming dirEdge");
                incoming.NextMin = firstOut;
            }
        }

        /// <summary>
        ///
        /// </summary>
        public void LinkAllDirectedEdges()
        {
            var temp = Edges;
            temp = null;    //Hack
            // find first area edge (if any) to start linking at
            DirectedEdge prevOut = null;
            DirectedEdge firstIn = null;
            // link edges in CW order
            for (int i = edgeList.Count - 1; i >= 0; i--)
            {
                var nextOut = (DirectedEdge)edgeList[i];
                var nextIn = nextOut.Sym;
                if (firstIn == null)
                    firstIn = nextIn;
                if (prevOut != null)
                    nextIn.Next = prevOut;
                // record outgoing edge, in order to link the last incoming edge
                prevOut = nextOut;
            }
            firstIn.Next = prevOut;
        }

        /// <summary>
        /// Traverse the star of edges, maintaining the current location in the result
        /// area at this node (if any).
        /// If any L edges are found in the interior of the result, mark them as covered.
        /// </summary>
        public void FindCoveredLineEdges()
        {
            // Since edges are stored in CCW order around the node,
            // as we move around the ring we move from the right to the left side of the edge

            /*
            * Find first DirectedEdge of result area (if any).
            * The interior of the result is on the RHS of the edge,
            * so the start location will be:
            * - Interior if the edge is outgoing
            * - Exterior if the edge is incoming
            */
            var startLoc = Location.Null;
            foreach (DirectedEdge nextOut in Edges)
            {
                var nextIn   = nextOut.Sym;
                if (!nextOut.IsLineEdge)
                {
                    if (nextOut.IsInResult)
                    {
                        startLoc = Location.Interior;
                        break;
                    }
                    if (nextIn.IsInResult)
                    {
                        startLoc = Location.Exterior;
                        break;
                    }
                }
            }
            // no A edges found, so can't determine if Curve edges are covered or not
            if (startLoc == Location.Null)
                return;

            /*
            * move around ring, keeping track of the current location
            * (Interior or Exterior) for the result area.
            * If Curve edges are found, mark them as covered if they are in the interior
            */
            var currLoc = startLoc;
            foreach (DirectedEdge nextOut in Edges)
            {
                var nextIn   = nextOut.Sym;
                if (nextOut.IsLineEdge)
                    nextOut.Edge.Covered = (currLoc == Location.Interior);
                else {
                    // edge is an Area edge
                    if (nextOut.IsInResult)
                        currLoc = Location.Exterior;
                    if (nextIn.IsInResult)
                        currLoc = Location.Interior;
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="de"></param>
        public void ComputeDepths(DirectedEdge de)
        {
            int edgeIndex = FindIndex(de);
            int startDepth = de.GetDepth(Geometries.Position.Left);
            int targetLastDepth = de.GetDepth(Geometries.Position.Right);
            // compute the depths from this edge up to the end of the edge array
            int nextDepth = ComputeDepths(edgeIndex + 1, edgeList.Count, startDepth);
            // compute the depths for the initial part of the array
            int lastDepth = ComputeDepths(0, edgeIndex, nextDepth);
            if (lastDepth != targetLastDepth)
                throw new TopologyException("depth mismatch at " + de.Coordinate);
        }

        /// <summary>
        /// Compute the DirectedEdge depths for a subsequence of the edge array.
        /// </summary>
        /// <returns>The last depth assigned (from the R side of the last edge visited).</returns>
        private int ComputeDepths(int startIndex, int endIndex, int startDepth)
        {
            int currDepth = startDepth;
            for (int i = startIndex; i < endIndex ; i++)
            {
                var nextDe = (DirectedEdge)edgeList[i];
                nextDe.SetEdgeDepths(Geometries.Position.Right, currDepth);
                currDepth = nextDe.GetDepth(Geometries.Position.Left);
            }
            return currDepth;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="outstream"></param>
        public override void Write(StreamWriter outstream)
        {
            foreach (DirectedEdge de in Edges)
            {
                outstream.Write("out ");
                de.Write(outstream);
                outstream.WriteLine();
                outstream.Write("in ");
                de.Sym.Write(outstream);
                outstream.WriteLine();
            }
        }
    }
}
