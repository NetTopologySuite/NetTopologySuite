using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using NetTopologySuite.Geometries;
using NetTopologySuite.Index.KdTree;

namespace NetTopologySuite.Noding.Snapround
{
    /**
     * Uses Snap Rounding to compute a rounded,
     * fully noded arrangement from a set of {@link SegmentString}s,
     * in a performant way.
     * <p>
     * Implements the Snap Rounding technique described in 
     * the papers by Hobby, Guibas &amp; Marimont, and Goodrich et al.
     * Snap Rounding enforces that all output vertices lie on a uniform grid,
     * which is determined by the provided {@link PrecisionModel}.
     * Input vertices do not have to be rounded to the grid; 
     * this is done during the snap-rounding process.
     * In fact, rounding cannot be done a priori,
     * since rounding vertices alone can distort the rounded topology
     * of the arrangement (by moving segments away from hot pixels
     * that would otherwise intersect them, or by moving vertices
     * across segments).
     * 
     * @version 1.7
     */
    public class FastSnapRounder : INoder
    {
        private readonly PrecisionModel _pm;
        private readonly HotPixelIndex _pixelIndex;

        private List<NodedSegmentString> _snappedResult;

        public FastSnapRounder(PrecisionModel pm)
        {
            this._pm = pm;
            _pixelIndex = new HotPixelIndex(pm);
        }

        /**
       * @return a Collection of NodedSegmentStrings representing the substrings
       * 
       */
        public IList<ISegmentString> GetNodedSubstrings()
        {
            return NodedSegmentString.GetNodedSubstrings(_snappedResult);
        }

        /**
     * @param inputSegmentStrings a Collection of NodedSegmentStrings
     */
        public void ComputeNodes(IList<ISegmentString> inputSegmentStrings)
        {
            /**
             * Determine intersections at full precision.  
             * Rounding happens during Hot Pixel creation.
             */
            _snappedResult = SnapRound(inputSegmentStrings);

            // testing purposes only - remove in final version
            //checkCorrectness(inputSegmentStrings);
            //if (Debug.isDebugging()) dumpNodedLines(inputSegmentStrings);
            //if (Debug.isDebugging()) dumpNodedLines(snappedResult);
        }

        /*
        private void dumpNodedLines(Collection<NodedSegmentString> segStrings) {
          for (NodedSegmentString nss : segStrings) {
            Debug.println( WKTWriter.toLineString(nss.getNodeList().getSplitCoordinates()));
          }
        }
    
        private void checkValidNoding(Collection inputSegmentStrings)
        {
          Collection resultSegStrings = NodedSegmentString.getNodedSubstrings(inputSegmentStrings);
          NodingValidator nv = new NodingValidator(resultSegStrings);
          try {
            nv.checkValid();
          } catch (Exception ex) {
            ex.printStackTrace();
          }
        }
        */

        private List<NodedSegmentString> SnapRound(IList<ISegmentString> segStrings)
        {
            var inputSS = CreateNodedStrings(segStrings);
            /**
             * Determine hot pixels for intersections and vertices.
             * This is done BEFORE the input lines are rounded,
             * to avoid distorting the line arrangement 
             * (rounding can cause vertices to move across edges).
             */
            var intersections = FindInteriorIntersections(inputSS);
            _pixelIndex.Add(intersections);
            AddVertexPixels(segStrings);

            var snapped = computeSnaps(inputSS);
            return snapped;
        }

        private static IList<ISegmentString> CreateNodedStrings(IEnumerable<ISegmentString> segStrings)
        {
            var nodedStrings = new List<ISegmentString>();
            foreach (var ss in segStrings)
            {
                nodedStrings.Add(new NodedSegmentString(ss));
            }

            return nodedStrings;
        }

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

        /**
     * Gets a list of the rounded coordinates.
     * Duplicate (collapsed) coordinates are removed.
     * 
     * @param pts the coordinates to round
     * @return array of rounded coordinates
     */
        private Coordinate[] Round(Coordinate[] pts)
        {
            var roundPts = new CoordinateList();

            for (int i = 0; i < pts.Length; i++)
            {
                roundPts.Add(Round(pts[i]), false);
            }

            return roundPts.ToCoordinateArray();
        }

        /**
     * Computes all interior intersections in the collection of {@link SegmentString}s,
     * and returns their {@link Coordinate}s.
     *
     * Also adds the intersection nodes to the segments.
     *
     * @return a list of Coordinates for the intersections
     */
        private List<Coordinate> FindInteriorIntersections(IList<ISegmentString> inputSS)
        {
            var intAdder = new SnapIntersectionAdder(_pm);
            var noder = new MCIndexNoder();
            noder.SegmentIntersector = intAdder;
            noder.ComputeNodes(inputSS);
            return intAdder.Intersections;
        }

        /**
     * Computes new segment strings which are rounded and contain
     * any intersections added as a result of snapping segments to snap points (hot pixels).
     * 
     * @param segStrings segments to snap
     * @return the snapped segment strings
     */
        private List<NodedSegmentString> computeSnaps(IEnumerable<ISegmentString> segStrings)
        {
            var snapped = new List<NodedSegmentString>();
            foreach (var ss in segStrings)
            {
                var snappedSS = ComputeSnaps((NodedSegmentString)ss);
                if (snappedSS != null)
                    snapped.Add(snappedSS);
            }

            return snapped;
        }

        /**
     * Add snapped vertices to a segemnt string.
     * If the segment string collapses completely due to rounding,
     * null is returned.
     * 
     * @param ss the segment string to snap
     * @return the snapped segment string, or null if it collapses completely
     */
        private NodedSegmentString ComputeSnaps(NodedSegmentString ss)
        {
            //Coordinate[] pts = ss.getCoordinates();
            /**
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

                /**
                 * If the segment has collapsed completely, skip it
                 */
                var p1 = pts[i + 1];
                var p1Round = Round(p1);
                if (p1Round.Equals2D(currSnap))
                    continue;

                var p0 = pts[i];

                /**
                 * Add any Hot Pixel intersections with *original* segment to rounded segment.
                 * (It is important to check original segment because rounding can
                 * move it enough to intersect other hot pixels not intersecting original segment)
                 */
                SnapSegment(p0, p1, snapSS, snapSSindex);
                snapSSindex++;
            }

            return snapSS;
        }


        /**
     * Snaps a segment in a segmentString to HotPixels that it intersects.
     * 
     * @param p0 the segment start coordinate
     * @param p1 the segment end coordinate
     * @param ss the segment string to add intersections to
     * @param segIndex the index of the segment
     */
        private void SnapSegment(Coordinate p0, Coordinate p1, NodedSegmentString ss, int segIndex)
        {
            _pixelIndex.Query(p0, p1, visitor: new KdNodeVisitor(p0, p1, ss, segIndex));
        }

        // TODO. HotPixelIndexVisitor ?
        private class KdNodeVisitor : IKdNodeVisitor<HotPixel>
        {
            private readonly Coordinate _p0, _p1;
            private readonly NodedSegmentString _ss;
            private readonly int _segIndex;

            public KdNodeVisitor(Coordinate p0, Coordinate p1, NodedSegmentString ss, int segIndex)
            {
                _p0 = p0;
                _p0 = p1;
                _ss = ss;
                _segIndex = segIndex;
            }

            public void Visit(KdNode<HotPixel> node)
            {
                // TODO Auto-generated method stub
                var hp = node.Data;
                if (hp.Intersects(_p0, _p1))
                {
                    //System.out.println("Added intersection: " + hp.getCoordinate());
                    _ss.AddIntersection(hp.Coordinate, _segIndex);
                }

            }
        }

    }
}
