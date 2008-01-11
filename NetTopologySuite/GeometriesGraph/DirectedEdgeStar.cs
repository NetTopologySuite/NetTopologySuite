using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using GeoAPI.Coordinates;
using GeoAPI.Geometries;
using GeoAPI.Utilities;
using GisSharpBlog.NetTopologySuite.Geometries;
using GisSharpBlog.NetTopologySuite.Utilities;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.GeometriesGraph
{
    /// <summary> 
    /// A DirectedEdgeStar is an ordered list of outgoing DirectedEdges around a node.
    /// It supports labeling the edges as well as linking the edges to form both
    /// MaximalEdgeRings and MinimalEdgeRings.
    /// </summary>
    public class DirectedEdgeStar<TCoordinate> : EdgeEndStar<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private const Int32 ScanningForIncoming = 1;
        private const Int32 LinkingToOutgoing = 2;

        private readonly List<DirectedEdge<TCoordinate>> _resultAreaEdgeList = new List<DirectedEdge<TCoordinate>>();
        private Label _label;

        /// <summary> 
        /// Insert a directed edge in the list.
        /// </summary>
        public override void Insert(EdgeEnd<TCoordinate> ee)
        {
            DirectedEdge<TCoordinate> de = ee as DirectedEdge<TCoordinate>;
            InsertEdgeEnd(de, de);
        }

        public Label Label
        {
            get { return _label; }
        }

        public Int32 GetOutgoingDegree()
        {
            Int32 degree = 0;

            foreach (DirectedEdge<TCoordinate> end in EdgesInternal)
            {
                if (end.IsInResult)
                {
                    degree++;
                }
            }

            return degree;
        }

        public Int32 GetOutgoingDegree(EdgeRing<TCoordinate> er)
        {
            Int32 degree = 0;

            foreach (DirectedEdge<TCoordinate> end in EdgesInternal)
            {
                if (end.EdgeRing.Equals(er))
                {
                    degree++;
                }
            }

            return degree;
        }

        public DirectedEdge<TCoordinate> GetRightmostEdge()
        {
            List<EdgeEnd<TCoordinate>> edges = EdgesInternal;
            Int32 size = edges.Count;

            if (size < 1)
            {
                return null;
            }

            DirectedEdge<TCoordinate> de0 = Slice.GetFirst(edges) as DirectedEdge<TCoordinate>;

            Debug.Assert(de0 != null);

            if (size == 1)
            {
                return de0;
            }

            DirectedEdge<TCoordinate> deLast = Slice.GetLast(edges) as DirectedEdge<TCoordinate>;

            Debug.Assert(deLast != null);

            Quadrants quad0 = de0.Quadrant;
            Quadrants quad1 = deLast.Quadrant;

            if (QuadrantOp<TCoordinate>.IsNorthern(quad0) && QuadrantOp<TCoordinate>.IsNorthern(quad1))
            {
                return de0;
            }
            else if (!QuadrantOp<TCoordinate>.IsNorthern(quad0) && !QuadrantOp<TCoordinate>.IsNorthern(quad1))
            {
                return deLast;
            }
            else
            {
                // edges are in different hemispheres - make sure we return one that is non-horizontal                        
                if (de0.Direction[Ordinates.Y] != 0)
                {
                    return de0;
                }
                else if (deLast.Direction[Ordinates.Y] != 0)
                {
                    return deLast;
                }
            }

            Assert.ShouldNeverReachHere("found two horizontal edges incident on node");
            return null;
        }

        /// <summary> 
        /// Compute the labeling for all dirEdges in this star, as well
        /// as the overall labeling.
        /// </summary>
        public override void ComputeLabeling(IEnumerable<GeometryGraph<TCoordinate>> geom)
        {
            base.ComputeLabeling(geom);

            // determine the overall labeling for this DirectedEdgeStar
            // (i.e. for the node it is based at)
            _label = new Label(Locations.None);

            foreach (EdgeEnd<TCoordinate> ee in this)
            {
                Edge<TCoordinate> e = ee.Edge;
                Debug.Assert(e.Label.HasValue);
                Label eLabel = e.Label.Value;

                for (Int32 i = 0; i < 2; i++)
                {
                    Locations eLoc = eLabel[i].On;

                    if (eLoc == Locations.Interior || eLoc == Locations.Boundary)
                    {
                        _label = new Label(_label, i, Locations.Interior);
                    }
                }
            }
        }

        /// <summary> 
        /// For each <see cref="DirectedEdge{TCoordinate}"/> 
        /// in the star, merge the label.
        /// </summary>
        public void MergeSymLabels()
        {
            foreach (EdgeEnd<TCoordinate> end in this)
            {
                DirectedEdge<TCoordinate> de = end as DirectedEdge<TCoordinate>;
                Debug.Assert(de != null);
                Debug.Assert(de.Label.HasValue);
                Debug.Assert(de.Sym.Label.HasValue);
                Label label = de.Label.Value;
                label.Merge(de.Sym.Label.Value);
                de.Label = label;
            }
        }

        /// <summary> 
        /// Update incomplete dirEdge labels from the labeling for the node.
        /// </summary>
        public void UpdateLabeling(Label nodeLabel)
        {
            foreach (EdgeEnd<TCoordinate> end in this)
            {
                DirectedEdge<TCoordinate> de = end as DirectedEdge<TCoordinate>;
                Debug.Assert(de != null);
                Debug.Assert(de.Label.HasValue);
                Label label = de.Label.Value;
                label = Label.SetAllLocationsIfNull(label, 0, nodeLabel[0].On);
                label = Label.SetAllLocationsIfNull(label, 1, nodeLabel[1].On);
                de.Label = label;
            }
        }

        /// <summary> 
        /// Traverse the star of DirectedEdges, linking the included edges together.
        /// </summary>
        /// <remarks>
        /// To link two dirEdges, the next pointer for an incoming dirEdge
        /// is set to the next outgoing edge.
        /// DirEdges are only linked if:
        /// they belong to an area (i.e. they have sides)
        /// they are marked as being in the result
        /// Edges are linked in CCW order (the order they are stored).
        /// This means that rings have their face on the Right
        /// (in other words, the topological location of the face is given by the RHS label of the DirectedEdge).
        /// PRECONDITION: No pair of dirEdges are both marked as being in the result.
        /// </remarks>
        public void LinkResultDirectedEdges()
        {
            // make sure edges are copied to resultAreaEdges list
            List<DirectedEdge<TCoordinate>> resultAreaEdges = getResultAreaEdges();

            // find first area edge (if any) to start linking at
            DirectedEdge<TCoordinate> firstOut = null;
            DirectedEdge<TCoordinate> incoming = null;
            Int32 state = ScanningForIncoming;

            // link edges in CCW order
            for (Int32 i = 0; i < resultAreaEdges.Count; i++)
            {
                DirectedEdge<TCoordinate> nextOut = resultAreaEdges[i];
                DirectedEdge<TCoordinate> nextIn = nextOut.Sym;
                Debug.Assert(nextOut.Label.HasValue);

                // skip de's that we're not interested in
                if (!nextOut.Label.Value.IsArea())
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
                        Debug.Assert(incoming != null);
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
                Debug.Assert(incoming != null);
                incoming.Next = firstOut;
            }
        }

        public void LinkMinimalDirectedEdges(EdgeRing<TCoordinate> er)
        {
            // make sure edges are copied to resultAreaEdges list
            List<DirectedEdge<TCoordinate>> resultAreaEdges = getResultAreaEdges();

            // find first area edge (if any) to start linking at
            DirectedEdge<TCoordinate> firstOut = null;
            DirectedEdge<TCoordinate> incoming = null;
            Int32 state = ScanningForIncoming;

            // link edges in CW order
            for (Int32 i = resultAreaEdges.Count - 1; i >= 0; i--)
            {
                DirectedEdge<TCoordinate> nextOut = resultAreaEdges[i];
                DirectedEdge<TCoordinate> nextIn = nextOut.Sym;

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
                        Debug.Assert(incoming != null);
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
                Debug.Assert(firstOut != null);
                Assert.IsTrue(firstOut.EdgeRing == er, "unable to link last incoming dirEdge");
                Debug.Assert(incoming != null);
                incoming.NextMin = firstOut;
            }
        }

        public void LinkAllDirectedEdges()
        {
            List<EdgeEnd<TCoordinate>> edges = EdgesInternal;

            // find first area edge (if any) to start linking at
            DirectedEdge<TCoordinate> prevOut = null;
            DirectedEdge<TCoordinate> firstIn = null;

            // link edges in CW order
            for (Int32 i = edges.Count - 1; i >= 0; i--)
            {
                DirectedEdge<TCoordinate> nextOut = edges[i] as DirectedEdge<TCoordinate>;
                Debug.Assert(nextOut != null);
                DirectedEdge<TCoordinate> nextIn = nextOut.Sym;

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

            Debug.Assert(firstIn != null);
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
            Locations startLoc = Locations.None;

            foreach (DirectedEdge<TCoordinate> nextOut in EdgesInternal)
            {
                Debug.Assert(nextOut != null);
                DirectedEdge<TCoordinate> nextIn = nextOut.Sym;

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
            if (startLoc == Locations.None)
            {
                return;
            }

            /*
            * move around ring, keeping track of the current location
            * (Interior or Exterior) for the result area.
            * If Curve edges are found, mark them as covered if they are in the interior
            */
            Locations currLoc = startLoc;

            foreach (DirectedEdge<TCoordinate> nextOut in EdgesInternal)
            {
                Debug.Assert(nextOut != null);
                DirectedEdge<TCoordinate> nextIn = nextOut.Sym;

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

        public void ComputeDepths(DirectedEdge<TCoordinate> de)
        {
            List<EdgeEnd<TCoordinate>> edges = EdgesInternal;
            Int32 edgeIndex = FindIndex(de);
            Int32 startDepth = de.GetDepth(Positions.Left);
            Int32 targetLastDepth = de.GetDepth(Positions.Right);
            // compute the depths from this edge up to the end of the edge array
            Int32 nextDepth = computeDepths(edgeIndex + 1, edges.Count, startDepth);
            // compute the depths for the initial part of the array
            Int32 lastDepth = computeDepths(0, edgeIndex, nextDepth);

            if (lastDepth != targetLastDepth)
            {
                throw new TopologyException("depth mismatch at " + de.Coordinate);
            }
        }

        public override void Write(StreamWriter outstream)
        {
            foreach (EdgeEnd<TCoordinate> end in this)
            {
                DirectedEdge<TCoordinate> de = end as DirectedEdge<TCoordinate>;
                Debug.Assert(de != null);
                outstream.Write("out ");
                de.Write(outstream);
                outstream.WriteLine();
                outstream.Write("in ");
                de.Sym.Write(outstream);
                outstream.WriteLine();
            }
        }

        private List<DirectedEdge<TCoordinate>> getResultAreaEdges()
        {
            if (_resultAreaEdgeList.Count == 0)
            {
                return _resultAreaEdgeList;
            }

            foreach (EdgeEnd<TCoordinate> end in this)
            {
                DirectedEdge<TCoordinate> de = end as DirectedEdge<TCoordinate>;
                Debug.Assert(de != null);

                if (de.IsInResult || de.Sym.IsInResult)
                {
                    _resultAreaEdgeList.Add(de);
                }
            }

            return _resultAreaEdgeList;
        }

        /// <summary> 
        /// Compute the DirectedEdge depths for a subsequence of the edge array.
        /// </summary>
        /// <returns>
        /// The last depth assigned (from the R side of the last edge visited).
        /// </returns>
        private Int32 computeDepths(Int32 startIndex, Int32 endIndex, Int32 startDepth)
        {
            Int32 currDepth = startDepth;
            List<EdgeEnd<TCoordinate>> edges = EdgesInternal;

            for (Int32 i = startIndex; i < endIndex; i++)
            {
                DirectedEdge<TCoordinate> nextDe = edges[i] as DirectedEdge<TCoordinate>;
                Debug.Assert(nextDe != null);
                nextDe.SetEdgeDepths(Positions.Right, currDepth);
                currDepth = nextDe.GetDepth(Positions.Left);
            }

            return currDepth;
        }
    }
}