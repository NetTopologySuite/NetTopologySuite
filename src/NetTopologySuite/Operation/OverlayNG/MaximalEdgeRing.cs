using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Utilities;

namespace NetTopologySuite.Operation.OverlayNG
{
    internal sealed class MaximalEdgeRing
    {

        private const int STATE_FIND_INCOMING = 1;
        private const int STATE_LINK_OUTGOING = 2;

        /// <summary>
        /// Traverses the star of edges originating at a node
        /// and links consecutive result edges together
        /// into <b>maximal</b> edge rings.
        /// To link two edges the <c>resultNextMax</c> pointer
        /// for an <b>incoming</b> result edge
        /// is set to the next <b>outgoing</b> result edge.
        /// <para/>
        /// Edges are linked when:
        /// <list type="bullet">
        /// <item><description>they belong to an area (i.e.they have sides)</description></item>
        /// <item><description>they are marked as being in the result</description></item>
        /// </list>
        /// <para/>
        /// Edges are linked in CCW order 
        /// (which is the order they are linked in the underlying graph).
        /// This means that rings have their face on the Right
        /// (in other words,
        /// the topological location of the face is given by the RHS label of the DirectedEdge).
        /// This produces rings with CW orientation.
        /// <para/>
        /// PRECONDITIONS: <br/>
        /// - This edge is in the result<br/>
        /// - This edge is not yet linked<br/>
        /// - The edge and its sym are NOT both marked as being in the result
        /// </summary>
        public static void LinkResultAreaMaxRingAtNode(OverlayEdge nodeEdge)
        {
            Assert.IsTrue(nodeEdge.IsInResultArea, "Attempt to link non-result edge");
            // assertion is only valid if building a polygonal geometry (ie not a coverage)
            //Assert.isTrue(! nodeEdge.symOE().isInResultArea(), "Found both half-edges in result");

            /*
             * Since the node edge is an out-edge, 
             * make it the last edge to be linked
             * by starting at the next edge.
             * The node edge cannot be an in-edge as well, 
             * but the next one may be the first in-edge.
             */
            var endOut = nodeEdge.ONextOE;
            var currOut = endOut;
            //Debug.println("\n------  Linking node MAX edges");
            //Debug.println("BEFORE: " + toString(nodeEdge));
            int state = STATE_FIND_INCOMING;
            OverlayEdge currResultIn = null;
            do
            {
                /*
                 * If an edge is linked this node has already been processed
                 * so can skip further processing
                 */
                if (currResultIn != null && currResultIn.IsResultMaxLinked)
                    return;

                switch (state)
                {
                    case STATE_FIND_INCOMING:
                        var currIn = currOut.SymOE;
                        if (!currIn.IsInResultArea) break;
                        currResultIn = currIn;
                        state = STATE_LINK_OUTGOING;
                        //Debug.println("Found result in-edge:  " + currResultIn);
                        break;
                    case STATE_LINK_OUTGOING:
                        if (!currOut.IsInResultArea) break;
                        // link the in edge to the out edge
                        currResultIn.NextResultMax = currOut;
                        state = STATE_FIND_INCOMING;
                        //Debug.println("Linked Max edge:  " + currResultIn + " -> " + currOut);
                        break;
                }
                currOut = currOut.ONextOE;
            } while (currOut != endOut);
            //Debug.println("AFTER: " + toString(nodeEdge));
            if (state == STATE_LINK_OUTGOING)
            {
                //Debug.print(firstOut == null, this);
                throw new TopologyException("no outgoing edge found", nodeEdge.Coordinate);
            }
        }

        private readonly OverlayEdge _startEdge;

        public MaximalEdgeRing(OverlayEdge e)
        {
            _startEdge = e;
            AttachEdges(e);
        }

        private void AttachEdges(OverlayEdge startEdge)
        {
            var edge = startEdge;
            do
            {
                if (edge == null)
                    throw new TopologyException("Ring edge is null");
                if (edge.MaxEdgeRing == this)
                    throw new TopologyException($"Ring edge visited twice at {edge.Coordinate}", edge.Coordinate);
                if (edge.NextResultMax == null)
                {
                    throw new TopologyException($"Ring edge missing at {edge.Dest}", edge.Dest);
                }
                edge.MaxEdgeRing = this;
                edge = edge.NextResultMax;
            } while (edge != startEdge);
        }

        public List<OverlayEdgeRing> BuildMinimalRings(GeometryFactory geometryFactory)
        {
            LinkMinimalRings();

            var minEdgeRings = new List<OverlayEdgeRing>();
            var e = _startEdge;
            do
            {
                if (e.EdgeRing == null)
                {
                    var minEr = new OverlayEdgeRing(e, geometryFactory);
                    minEdgeRings.Add(minEr);
                }
                e = e.NextResultMax;
            } while (e != _startEdge);
            return minEdgeRings;
        }

        private void LinkMinimalRings()
        {
            var e = _startEdge;
            do
            {
                LinkMinRingEdgesAtNode(e, this);
                e = e.NextResultMax;
            } while (e != _startEdge);
        }

        /// <summary>
        /// Links the edges of a <see cref="MaximalEdgeRing"/> around this node
        /// into minimal edge rings (<see cref="OverlayEdgeRing"/>s).
        /// Minimal ring edges are linked in the opposite orientation (CW)
        /// to the maximal ring.
        /// This changes self-touching rings into a two or more separate rings,
        /// as per the OGC SFS polygon topology semantics.
        /// This relinking must be done to each max ring separately,
        /// rather than all the node result edges, since there may be 
        /// more than one max ring incident at the node.
        /// </summary>
        /// <param name="maxRing">The maximal ring to link</param>
        /// <param name="nodeEdge">An edge originating at this node</param>
        private static void LinkMinRingEdgesAtNode(OverlayEdge nodeEdge, MaximalEdgeRing maxRing)
        {
            //Assert.isTrue(nodeEdge.isInResult(), "Attempt to link non-result edge");

            /*
             * The node edge is an out-edge, 
             * so it is the first edge linked
             * with the next CCW in-edge
             */
            var endOut = nodeEdge;
            var currMaxRingOut = endOut;
            var currOut = endOut.ONextOE;
            //Debug.println("\n------  Linking node MIN ring edges");
            //Debug.println("BEFORE: " + toString(nodeEdge));
            do
            {
                if (IsAlreadyLinked(currOut.SymOE, maxRing))
                    return;

                if (currMaxRingOut == null)
                {
                    currMaxRingOut = SelectMaxOutEdge(currOut, maxRing);
                }
                else
                {
                    currMaxRingOut = LinkMaxInEdge(currOut, currMaxRingOut, maxRing);
                }
                currOut = currOut.ONextOE;
            } while (currOut != endOut);
            //Debug.println("AFTER: " + toString(nodeEdge));
            if (currMaxRingOut != null)
            {
                throw new TopologyException("Unmatched edge found during min-ring linking", nodeEdge.Coordinate);
            }
        }

        /// <summary>
        /// Tests if an edge of the maximal edge ring is already linked into
        /// a minimal <see cref="OverlayEdgeRing"/>. If so, this node has already been processed
        /// earlier in the maximal edgering linking scan.
        /// </summary>
        /// <param name="edge">An edge of a maximal edgering</param>
        /// <param name="maxRing">The maximal edgering</param>
        /// <returns><c>true</c> if the edge has already been linked into a minimal edgering.</returns>
        private static bool IsAlreadyLinked(OverlayEdge edge, MaximalEdgeRing maxRing)
        {
            bool isLinked = edge.MaxEdgeRing == maxRing
                               && edge.IsResultLinked;
            return isLinked;
        }

        private static OverlayEdge SelectMaxOutEdge(OverlayEdge currOut, MaximalEdgeRing maxEdgeRing)
        {
            // select if currOut edge is part of this max ring
            if (currOut.MaxEdgeRing == maxEdgeRing)
                return currOut;
            // otherwise skip this edge
            return null;
        }

        private static OverlayEdge LinkMaxInEdge(OverlayEdge currOut,
            OverlayEdge currMaxRingOut,
            MaximalEdgeRing maxEdgeRing)
        {
            var currIn = currOut.SymOE;
            // currIn is not in this max-edgering, so keep looking
            if (currIn.MaxEdgeRing != maxEdgeRing)
                return currMaxRingOut;

            //Debug.println("Found result in-edge:  " + currIn);

            currIn.NextResult = currMaxRingOut;
            //Debug.println("Linked Min Edge:  " + currIn + " -> " + currMaxRingOut);
            // return null to indicate to scan for the next max-ring out-edge
            return null;
        }

        public override string ToString()
        {
            return WKTWriter.ToLineString(Coordinates);
        }

        private Coordinate[] Coordinates
        {
            get
            {
                var coords = new CoordinateList();
                var edge = _startEdge;
                do
                {
                    coords.Add(edge.Orig);
                    if (edge.NextResultMax == null)
                        break;

                    edge = edge.NextResultMax;
                } while (edge != _startEdge);

                // add last coordinate
                coords.Add(edge.Dest);
                return coords.ToCoordinateArray();
            }
        }
    }
}
