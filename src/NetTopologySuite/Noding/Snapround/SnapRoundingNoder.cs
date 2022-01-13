using System;
using System.Collections.Generic;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.KdTree;
using NetTopologySuite.Noding.Snap;

namespace NetTopologySuite.Noding.Snapround
{
    /// <summary>
    /// Uses Snap Rounding to compute a rounded,
    /// fully noded arrangement from a set of <see cref="ISegmentString"/>s,
    /// in a performant way, and avoiding unnecessary noding.
    /// <para/>
    /// Implements the Snap Rounding technique described in 
    /// the papers by Hobby, Guibas &amp; Marimont, and Goodrich et al.
    /// Snap Rounding enforces that all output vertices lie on a uniform grid,
    /// which is determined by the provided <see cref="PrecisionModel"/>.
    /// <para/>
    /// Input vertices do not have to be rounded to the grid beforehand; 
    /// this is done during the snap-rounding process.
    /// In fact, rounding cannot be done a priori,
    /// since rounding vertices by themselves can distort the rounded topology
    /// of the arrangement (i.e. by moving segments away from hot pixels
    /// that would otherwise intersect them, or by moving vertices
    /// across segments).
    /// <para/>
    /// To minimize the number of introduced nodes,
    /// the Snap-Rounding Noder avoids creating nodes
    /// at edge vertices if there is no intersection or snap at that location.
    /// However, if two different input edges contain identical segments,
    /// each of the segment vertices will be noded.
    /// This still provides fully-noded output.
    /// This is the same behaviour provided by other noders,
    /// such as <see cref="MCIndexNoder"/>
    /// and <see cref="SnappingNoder"/>.
    /// </summary>
    /// <version>1.17</version>
    public sealed class SnapRoundingNoder : INoder
    {
        /// <summary>
        /// The division factor used to determine
        /// nearness distance tolerance for intersection detection.
        /// </summary>
        private const int NEARNESS_FACTOR = 100;


        private readonly PrecisionModel _pm;
        private readonly HotPixelIndex _pixelIndex;

        private List<NodedSegmentString> _snappedResult;

        public SnapRoundingNoder(PrecisionModel pm)
        {
            _pm = pm;
            _pixelIndex = new HotPixelIndex(pm);
        }

        /// <summary>
        /// Gets a Collection of NodedSegmentStrings representing the substrings
        /// </summary>
        public IList<ISegmentString> GetNodedSubstrings()
        {
            return NodedSegmentString.GetNodedSubstrings(_snappedResult);
        }

        /// <summary>
        /// Computes the nodes in the snap-rounding line arrangement.
        /// The nodes are added to the <see cref="NodedSegmentString"/>s provided as the input.
        /// </summary>
        /// <param name="inputSegmentStrings">A Collection of NodedSegmentStrings</param>
        public void ComputeNodes(IList<ISegmentString> inputSegmentStrings)
        {
            _snappedResult = SnapRound(inputSegmentStrings);
        }

        private List<NodedSegmentString> SnapRound(IList<ISegmentString> segStrings)
        {
            /*
             * Determine hot pixels for intersections and vertices.
             * This is done BEFORE the input lines are rounded,
             * to avoid distorting the line arrangement 
             * (rounding can cause vertices to move across edges).
             */
            AddIntersectionPixels(segStrings);

            AddVertexPixels(segStrings);

            var snapped = ComputeSnaps(segStrings);
            return snapped;
        }

        /// <summary>
        /// Detects interior intersections in the collection of {@link SegmentString}s,
        /// and adds nodes for them to the segment strings.
        /// Also creates HotPixel nodes for the intersection points.
        /// </summary>
        /// <param name="segStrings">The input NodedSegmentStrings</param>
        private void AddIntersectionPixels(IList<ISegmentString> segStrings)
        {
            /*
             * nearness tolerance is a small fraction of the grid size.
             */
            double snapGridSize = 1.0 / _pm.Scale;
            double nearnessTol = snapGridSize / NEARNESS_FACTOR;

            var intAdder = new SnapRoundingIntersectionAdder(nearnessTol);
            var noder = new MCIndexNoder(intAdder, nearnessTol);
            noder.ComputeNodes(segStrings);
            var intPts = intAdder.Intersections;
            _pixelIndex.AddNodes(intPts);
        }

        /// <summary>
        /// Creates HotPixels for each vertex in the input segStrings.
        /// The HotPixels are not marked as nodes, since they will
        /// only be nodes in the final line arrangement
        /// if they interact with other segments(or they are already
        /// created as intersection nodes).
        /// </summary>
        /// <param name="segStrings">The input NodedSegmentStrings</param>
        private void AddVertexPixels(IEnumerable<ISegmentString> segStrings)
        {
            foreach (var nss in segStrings)
            {
                var pts = nss.Coordinates;
                _pixelIndex.Add(pts);
            }
        }

        private Coordinate Round(Coordinate pt)
        {
            var p2 = pt.Copy();
            _pm.MakePrecise(p2);
            return p2;
        }

        /// <summary>
        /// Gets a list of the rounded coordinates.
        /// Duplicate (collapsed) coordinates are removed.
        /// </summary>
        /// <param name="pts">The coordinates to round</param>
        /// <returns>Array of rounded coordinates</returns>
        private Coordinate[] Round(Coordinate[] pts)
        {
            var roundPts = new CoordinateList();

            for (int i = 0; i < pts.Length; i++)
            {
                roundPts.Add(Round(pts[i]), false);
            }

            return roundPts.ToCoordinateArray();
        }

        /// <summary>
        /// Computes new segment strings which are rounded and contain
        /// intersections added as a result of snapping segments to snap points (hot pixels).
        /// </summary>
        /// <param name="segStrings">Segments to snap</param>
        /// <returns>The snapped segment strings</returns>
        private List<NodedSegmentString> ComputeSnaps(IEnumerable<ISegmentString> segStrings)
        {
            var snapped = new List<NodedSegmentString>();
            foreach (var ss in segStrings)
            {
                var snappedSS = ComputeSegmentSnaps((NodedSegmentString) ss);
                if (snappedSS != null)
                    snapped.Add(snappedSS);
            }

            /*
             * Some intersection hot pixels may have been marked as nodes in the previous
             * loop, so add nodes for them.
             */
            foreach (var ss in snapped)
            {
                AddVertexNodeSnaps(ss);
            }

            return snapped;
        }

        /// <summary>
        /// Add snapped vertices to a segment string.
        /// If the segment string collapses completely due to rounding,
        /// null is returned.
        /// </summary>
        /// <param name="ss">The segment string to snap</param>
        /// <returns>
        /// The snapped segment string, or null if it collapses completely
        /// </returns>
        private NodedSegmentString ComputeSegmentSnaps(NodedSegmentString ss)
        {
            //Coordinate[] pts = ss.getCoordinates();
            /*
             * Get edge coordinates, including added intersection nodes.
             * The coordinates are now rounded to the grid,
             * in preparation for snapping to the Hot Pixels
             */
            var pts = ss.NodedCoordinates;
            var ptsRound = Round(pts);

            // if complete collapse this edge can be eliminated
            if (ptsRound.Length <= 1)
                return null;

            // Create new nodedSS to allow adding any hot pixel nodes
            var snapSS = new NodedSegmentString(ptsRound, ss.Context);

            int snapSSindex = 0;
            for (int i = 0; i < pts.Length - 1; i++)
            {
                var currSnap = snapSS.GetCoordinate(snapSSindex);

                /*
                 * If the segment has collapsed completely, skip it
                 */
                var p1 = pts[i + 1];
                var p1Round = Round(p1);
                if (p1Round.Equals2D(currSnap))
                    continue;

                var p0 = pts[i];

                /*
                 * Add any Hot Pixel intersections with *original* segment to rounded segment.
                 * (It is important to check original segment because rounding can
                 * move it enough to intersect other hot pixels not intersecting original segment)
                 */
                SnapSegment(p0, p1, snapSS, snapSSindex);
                snapSSindex++;
            }

            return snapSS;
        }


        /// <summary>
        /// Snaps a segment in a segmentString to HotPixels that it intersects.
        /// </summary>
        /// <param name="p0">The segment start coordinate</param>
        /// <param name="p1">The segment end coordinate</param>
        /// <param name="ss">The segment string to add intersections to</param>
        /// <param name="segIndex">The index of the segment/</param>
        private void SnapSegment(Coordinate p0, Coordinate p1, NodedSegmentString ss, int segIndex)
        {
            _pixelIndex.Query(p0, p1, visitor: new SnapSegmentVisitor(p0, p1, ss, segIndex));
        }

        // TODO. HotPixelIndexVisitor ?
        private abstract class KdNodeVisitor : IKdNodeVisitor<HotPixel>
        {
            protected readonly Coordinate P0, P1;
            protected readonly NodedSegmentString SS;
            protected readonly int SegIndex;

            protected KdNodeVisitor(Coordinate p0, Coordinate p1, NodedSegmentString ss, int segIndex)
            {
                P0 = p0;
                P1 = p1;
                SS = ss;
                SegIndex = segIndex;
            }

            public abstract void Visit(KdNode<HotPixel> node);
        }

        private sealed class SnapSegmentVisitor : KdNodeVisitor
        {
            public SnapSegmentVisitor(Coordinate p0, Coordinate p1, NodedSegmentString ss, int segIndex)
                : base(p0, p1, ss, segIndex)
            {
            }

            public override void Visit(KdNode<HotPixel> node)
            {
                var hp = node.Data;
                /*
                 * If the hot pixel is not a node, and it contains one of the segment vertices,
                 * then that vertex is the source for the hot pixel.
                 * To avoid over-noding a node is not added at this point. 
                 * The hot pixel may be subsequently marked as a node,
                 * in which case the intersection will be added during the final vertex noding phase.
                 */
                if (!hp.IsNode)
                {
                    if (hp.Intersects(P0) || hp.Intersects(P1))
                        return;
                }
                /*
                 * Add a node if the segment intersects the pixel.
                 * Mark the HotPixel as a node (since it may not have been one before).
                 * This ensures the vertex for it is added as a node during the final vertex noding phase.
                 */

                if (hp.Intersects(P0, P1))
                {
                    //System.out.println("Added intersection: " + hp.getCoordinate());
                    SS.AddIntersection(hp.Coordinate, SegIndex);
                    hp.IsNode = true;
                }
            }
        }

        /// <summary>
        /// Add nodes for any vertices in hot pixels that were
        /// added as nodes during segment noding.
        /// </summary>
        /// <param name="ss">A noded segment string</param>
        private void AddVertexNodeSnaps(NodedSegmentString ss)
        {
            var pts = ss.Coordinates;
            for (int i = 1; i < pts.Length - 1; i++)
            {
                var p0 = pts[i];
                SnapVertexNode(p0, ss, i);
            }
        }

        private void SnapVertexNode(Coordinate p0, NodedSegmentString ss, int segIndex)
        {
            _pixelIndex.Query(p0, p0, new SnapVertexVisitor(p0, ss, segIndex));
        }

        private sealed class SnapVertexVisitor : KdNodeVisitor
        {
            public SnapVertexVisitor(Coordinate p0, NodedSegmentString ss, int segIndex) :
                base(p0, null, ss, segIndex)
            {
            }

            public override void Visit(KdNode<HotPixel> node)
            {
                var hp = node.Data;
                /*
                 * If vertex pixel is a node, add it.
                 */
                if (hp.IsNode && hp.Coordinate.Equals2D(P0))
                {
                    SS.AddIntersection(P0, SegIndex);
                }
            }
        }
    }
}
