using System;
using System.Collections;
using System.IO;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
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
        private IList resultAreaEdgeList;

        private Label label;

        public DirectedEdgeStar() {}

        /// <summary> 
        /// Insert a directed edge in the list.
        /// </summary>
        public override void Insert(EdgeEnd ee)
        {
            DirectedEdge de = (DirectedEdge) ee;
            InsertEdgeEnd(de, de);
        }

        public Label Label
        {
            get { return label; }
        }

        public Int32 GetOutgoingDegree()
        {
            Int32 degree = 0;

            for (IEnumerator it = GetEnumerator(); it.MoveNext();)
            {
                DirectedEdge de = (DirectedEdge) it.Current;
                if (de.IsInResult)
                {
                    degree++;
                }
            }

            return degree;
        }

        public Int32 GetOutgoingDegree(EdgeRing er)
        {
            Int32 degree = 0;

            for (IEnumerator it = GetEnumerator(); it.MoveNext();)
            {
                DirectedEdge de = (DirectedEdge) it.Current;
                if (de.EdgeRing == er)
                {
                    degree++;
                }
            }

            return degree;
        }

        public DirectedEdge GetRightmostEdge()
        {
            IList edges = Edges;
            Int32 size = edges.Count;

            if (size < 1)
            {
                return null;
            }

            DirectedEdge de0 = (DirectedEdge) edges[0];
            
            if (size == 1)
            {
                return de0;
            }

            DirectedEdge deLast = (DirectedEdge) edges[size - 1];

            Int32 quad0 = de0.Quadrant;
            Int32 quad1 = deLast.Quadrant;
            
            if (QuadrantOp.IsNorthern(quad0) && QuadrantOp.IsNorthern(quad1))
            {
                return de0;
            }
            else if (!QuadrantOp.IsNorthern(quad0) && !QuadrantOp.IsNorthern(quad1))
            {
                return deLast;
            }
            else
            {
                // edges are in different hemispheres - make sure we return one that is non-horizontal                        
                if (de0.Dy != 0)
                {
                    return de0;
                }
                else if (deLast.Dy != 0)
                {
                    return deLast;
                }
            }

            Assert.ShouldNeverReachHere("found two horizontal edges incident on node");
            return null;
        }

        /// <summary> 
        /// Compute the labelling for all dirEdges in this star, as well
        /// as the overall labelling.
        /// </summary>
        public override void ComputeLabelling(GeometryGraph[] geom)
        {
            base.ComputeLabelling(geom);

            // determine the overall labelling for this DirectedEdgeStar
            // (i.e. for the node it is based at)
            label = new Label(Locations.Null);
            IEnumerator it = GetEnumerator();
           
            while (it.MoveNext())
            {
                EdgeEnd ee = (EdgeEnd) it.Current;
                Edge e = ee.Edge;
                Label eLabel = e.Label;

                for (Int32 i = 0; i < 2; i++)
                {
                    Locations eLoc = eLabel.GetLocation(i);
                    if (eLoc == Locations.Interior || eLoc == Locations.Boundary)
                    {
                        label.SetLocation(i, Locations.Interior);
                    }
                }
            }
        }

        /// <summary> 
        /// For each dirEdge in the star, merge the label .
        /// </summary>
        public void MergeSymLabels()
        {
            for (IEnumerator it = GetEnumerator(); it.MoveNext();)
            {
                DirectedEdge de = (DirectedEdge) it.Current;
                Label label = de.Label;
                label.Merge(de.Sym.Label);
            }
        }

        /// <summary> 
        /// Update incomplete dirEdge labels from the labelling for the node.
        /// </summary>
        public void UpdateLabelling(Label nodeLabel)
        {
            for (IEnumerator it = GetEnumerator(); it.MoveNext();)
            {
                DirectedEdge de = (DirectedEdge) it.Current;
                Label label = de.Label;
                label.SetAllLocationsIfNull(0, nodeLabel.GetLocation(0));
                label.SetAllLocationsIfNull(1, nodeLabel.GetLocation(1));
            }
        }

        private IList GetResultAreaEdges()
        {
            if (resultAreaEdgeList != null)
            {
                return resultAreaEdgeList;
            }
            resultAreaEdgeList = new ArrayList();
            for (IEnumerator it = GetEnumerator(); it.MoveNext();)
            {
                DirectedEdge de = (DirectedEdge) it.Current;
                if (de.IsInResult || de.Sym.IsInResult)
                {
                    resultAreaEdgeList.Add(de);
                }
            }
            return resultAreaEdgeList;
        }

        private const Int32 ScanningForIncoming = 1;
        private const Int32 LinkingToOutgoing = 2;

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
            Int32 state = ScanningForIncoming;
            
            // link edges in CCW order
            for (Int32 i = 0; i < resultAreaEdgeList.Count; i++)
            {
                DirectedEdge nextOut = (DirectedEdge) resultAreaEdgeList[i];
                DirectedEdge nextIn = nextOut.Sym;

                // skip de's that we're not interested in
                if (! nextOut.Label.IsArea())
                {
                    continue;
                }

                // record first outgoing edge, in order to link the last incoming edge
                if (firstOut == null && nextOut.IsInResult)
                {
                    firstOut = nextOut;
                }

                switch (state)
                {
                    case ScanningForIncoming:
                        if (!nextIn.IsInResult)
                        {
                            continue;
                        }
                        incoming = nextIn;
                        state = LinkingToOutgoing;
                        break;
                    case LinkingToOutgoing:
                        if (!nextOut.IsInResult)
                        {
                            continue;
                        }
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
                {
                    throw new TopologyException("no outgoing dirEdge found", Coordinate);
                }

                Assert.IsTrue(firstOut.IsInResult, "unable to link last incoming dirEdge");
                incoming.Next = firstOut;
            }
        }

        public void LinkMinimalDirectedEdges(EdgeRing er)
        {
            // find first area edge (if any) to start linking at
            DirectedEdge firstOut = null;
            DirectedEdge incoming = null;
            Int32 state = ScanningForIncoming;
            
            // link edges in CW order
            for (Int32 i = resultAreaEdgeList.Count - 1; i >= 0; i--)
            {
                DirectedEdge nextOut = (DirectedEdge) resultAreaEdgeList[i];
                DirectedEdge nextIn = nextOut.Sym;

                // record first outgoing edge, in order to link the last incoming edge
                if (firstOut == null && nextOut.EdgeRing == er)
                {
                    firstOut = nextOut;
                }

                switch (state)
                {
                    case ScanningForIncoming:
                        if (nextIn.EdgeRing != er)
                        {
                            continue;
                        }
                        incoming = nextIn;
                        state = LinkingToOutgoing;
                        break;
                    case LinkingToOutgoing:
                        if (nextOut.EdgeRing != er)
                        {
                            continue;
                        }
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

        public void LinkAllDirectedEdges()
        {
            IList temp = Edges;
            temp = null; //Hack

            // find first area edge (if any) to start linking at
            DirectedEdge prevOut = null;
            DirectedEdge firstIn = null;

            // link edges in CW order
            for (Int32 i = edgeList.Count - 1; i >= 0; i--)
            {
                DirectedEdge nextOut = (DirectedEdge) edgeList[i];
                DirectedEdge nextIn = nextOut.Sym;

                if (firstIn == null)
                {
                    firstIn = nextIn;
                }

                if (prevOut != null)
                {
                    nextIn.Next = prevOut;
                }
                // record outgoing edge, in order to link the last incoming edge
                prevOut = nextOut;
            }

            firstIn.Next = prevOut;
        }

        /// <summary> 
        /// Traverse the star of edges, maintaing the current location in the result
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
            Locations startLoc = Locations.Null;
            for (IEnumerator it = GetEnumerator(); it.MoveNext();)
            {
                DirectedEdge nextOut = (DirectedEdge) it.Current;
                DirectedEdge nextIn = nextOut.Sym;
                if (!nextOut.IsLineEdge)
                {
                    if (nextOut.IsInResult)
                    {
                        startLoc = Locations.Interior;
                        break;
                    }
                    if (nextIn.IsInResult)
                    {
                        startLoc = Locations.Exterior;
                        break;
                    }
                }
            }
            // no A edges found, so can't determine if Curve edges are covered or not
            if (startLoc == Locations.Null)
            {
                return;
            }

            /*
            * move around ring, keeping track of the current location
            * (Interior or Exterior) for the result area.
            * If Curve edges are found, mark them as covered if they are in the interior
            */
            Locations currLoc = startLoc;

            for (IEnumerator it = GetEnumerator(); it.MoveNext();)
            {
                DirectedEdge nextOut = (DirectedEdge) it.Current;
                DirectedEdge nextIn = nextOut.Sym;
                if (nextOut.IsLineEdge)
                {
                    nextOut.Edge.Covered = (currLoc == Locations.Interior);
                }
                else
                {
                    // edge is an Area edge
                    if (nextOut.IsInResult)
                    {
                        currLoc = Locations.Exterior;
                    }

                    if (nextIn.IsInResult)
                    {
                        currLoc = Locations.Interior;
                    }
                }
            }
        }

        public void ComputeDepths(DirectedEdge de)
        {
            Int32 edgeIndex = FindIndex(de);
            Int32 startDepth = de.GetDepth(Positions.Left);
            Int32 targetLastDepth = de.GetDepth(Positions.Right);
            // compute the depths from this edge up to the end of the edge array
            Int32 nextDepth = ComputeDepths(edgeIndex + 1, edgeList.Count, startDepth);
            // compute the depths for the initial part of the array
            Int32 lastDepth = ComputeDepths(0, edgeIndex, nextDepth);

            if (lastDepth != targetLastDepth)
            {
                throw new TopologyException("depth mismatch at " + de.Coordinate);
            }
        }

        /// <summary> 
        /// Compute the DirectedEdge depths for a subsequence of the edge array.
        /// </summary>
        /// <returns>The last depth assigned (from the R side of the last edge visited).</returns>
        private Int32 ComputeDepths(Int32 startIndex, Int32 endIndex, Int32 startDepth)
        {
            Int32 currDepth = startDepth;

            for (Int32 i = startIndex; i < endIndex; i++)
            {
                DirectedEdge nextDe = (DirectedEdge) edgeList[i];
                nextDe.SetEdgeDepths(Positions.Right, currDepth);
                currDepth = nextDe.GetDepth(Positions.Left);
            }
            return currDepth;
        }

        public override void Write(StreamWriter outstream)
        {
            for (IEnumerator it = GetEnumerator(); it.MoveNext();)
            {
                DirectedEdge de = (DirectedEdge) it.Current;
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