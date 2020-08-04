using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Overlay.Snap
{
    /// <summary>
    /// Snaps the vertices and segments of a <see cref="LineString"/>
    ///  to a set of target snap vertices.
    /// A snap distance tolerance is used to control where snapping is performed.
    /// <para/>The implementation handles empty geometry and empty snap vertex sets.
    /// </summary>
    public class LineStringSnapper
    {
        private readonly double _snapTolerance;

        private readonly Coordinate[] _srcPts;
        private readonly LineSegment _seg = new LineSegment(); // for reuse during snapping
        private bool _allowSnappingToSourceVertices;
        private readonly bool _isClosed;

        /// <summary>
        /// Creates a new snapper using the points in the given <see cref="LineString"/>
        /// as target snap points.
        /// </summary>
        /// <param name="srcLine">A LineString to snap (may be empty)</param>
        /// <param name="snapTolerance">the snap tolerance to use</param>
        public LineStringSnapper(LineString srcLine, double snapTolerance) :
            this(srcLine.Coordinates, snapTolerance) { }

        /// <summary>
        /// Creates a new snapper using the given points
        /// as source points to be snapped.
        /// </summary>
        /// <param name="srcPts"></param>
        /// <param name="snapTolerance"></param>
        public LineStringSnapper(Coordinate[] srcPts, double snapTolerance)
        {
            _srcPts = srcPts;
            _isClosed = IsClosed(_srcPts);// srcPts[0].Equals2D(srcPts[srcPts.Length - 1]);
            _snapTolerance = snapTolerance;
        }

        public bool AllowSnappingToSourceVertices
        {
            get => _allowSnappingToSourceVertices;
            set => _allowSnappingToSourceVertices = value;
        }

        private static bool IsClosed(Coordinate[] pts)
        {
            if (pts.Length <= 1) return false;
            return pts[0].Equals2D(pts[pts.Length - 1]);
        }

        /// <summary>
        /// Snaps the vertices and segments of the source LineString
        /// to the given set of snap points.
        /// </summary>
        /// <param name="snapPts">the vertices to snap to</param>
        /// <returns>list of the snapped points</returns>
        public Coordinate[] SnapTo(Coordinate[] snapPts)
        {
            var coordList = new CoordinateList(_srcPts);
            SnapVertices(coordList, snapPts);
            SnapSegments(coordList, snapPts);
            var newPts = coordList.ToCoordinateArray();
            return newPts;
        }

        /// <summary>
        /// Snap source vertices to vertices in the target.
        /// </summary>
        /// <param name="srcCoords">the points to snap</param>
        /// <param name="snapPts">the points to snap to</param>
        private void SnapVertices(CoordinateList srcCoords, Coordinate[] snapPts)
        {
            // try snapping vertices
            // if src is a ring then don't snap final vertex
            int end = _isClosed ? srcCoords.Count - 1 : srcCoords.Count;
            for (int i = 0; i < end; i++)
            {
                var srcPt = srcCoords[i];
                var snapVert = FindSnapForVertex(srcPt, snapPts);
                if (snapVert != null)
                {
                    // update src with snap pt
                    srcCoords[i] = snapVert.Copy();
                    // keep final closing point in synch (rings only)
                    if (i == 0 && _isClosed)
                        srcCoords[srcCoords.Count - 1] = snapVert.Copy();
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="snapPts"></param>
        /// <returns></returns>
        private Coordinate FindSnapForVertex(Coordinate pt, Coordinate[] snapPts)
        {
            foreach (var coord in snapPts)
            {
                // if point is already equal to a src pt, don't snap
                if (pt.Equals2D(coord))
                    return null;
                if (pt.Distance(coord) < _snapTolerance)
                    return coord;
            }
            return null;
        }

        /// <summary>
        /// Snap segments of the source to nearby snap vertices.<para/>
        /// Source segments are "cracked" at a snap vertex.
        /// A single input segment may be snapped several times
        /// to different snap vertices.<para/>
        /// For each distinct snap vertex, at most one source segment
        /// is snapped to.  This prevents "cracking" multiple segments
        /// at the same point, which would likely cause
        /// topology collapse when being used on polygonal linework.
        /// </summary>
        /// <param name="srcCoords">The coordinates of the source linestring to snap</param>
        /// <param name="snapPts">The target snap vertices</param>
        private void SnapSegments(CoordinateList srcCoords, Coordinate[] snapPts)
        {
            // guard against empty input
            if (snapPts.Length == 0) return;

            int distinctPtCount = snapPts.Length;

            // check for duplicate snap pts when they are sourced from a linear ring.
            // TODO: Need to do this better - need to check *all* points for dups (using a Set?)
            if (snapPts[0].Equals2D(snapPts[snapPts.Length - 1]))
                distinctPtCount = snapPts.Length - 1;

            for (int i = 0; i < distinctPtCount; i++)
            {
                var snapPt = snapPts[i];
                int index = FindSegmentIndexToSnap(snapPt, srcCoords);
                /*
                 * If a segment to snap to was found, "crack" it at the snap pt.
                 * The new pt is inserted immediately into the src segment list,
                 * so that subsequent snapping will take place on the modified segments.
                 * Duplicate points are not added.
                 */
                if (index >= 0)
                    srcCoords.Add(index + 1, snapPt.Copy(), false);
            }
        }

        /// <summary>
        /// Finds a src segment which snaps to (is close to) the given snap point<para/>
        /// Only a single segment is selected for snapping.
        /// This prevents multiple segments snapping to the same snap vertex,
        /// which would almost certainly cause invalid geometry
        /// to be created.
        /// (The heuristic approach of snapping used here
        /// is really only appropriate when
        /// snap pts snap to a unique spot on the src geometry)<para/>
        /// Also, if the snap vertex occurs as a vertex in the src coordinate list,
        /// no snapping is performed.
        /// </summary>
        /// <param name="snapPt">The point to snap to</param>
        /// <param name="srcCoords">The source segment coordinates</param>
        /// <returns>The index of the snapped segment <br/>
        /// or -1 if no segment snaps to the snap point.</returns>
        private int FindSegmentIndexToSnap(Coordinate snapPt, CoordinateList srcCoords)
        {
            double minDist = double.MaxValue;
            int snapIndex = -1;
            for (int i = 0; i < srcCoords.Count - 1; i++)
            {
                _seg.P0 = srcCoords[i];
                _seg.P1 = srcCoords[i + 1];

                /*
                 * Check if the snap pt is equal to one of the segment endpoints.
                 *
                 * If the snap pt is already in the src list, don't snap at all.
                 */
                if (_seg.P0.Equals2D(snapPt) || _seg.P1.Equals2D(snapPt))
                {
                    if (_allowSnappingToSourceVertices)
                        continue;
                    return -1;
                }

                double dist = _seg.Distance(snapPt);
                if (dist < _snapTolerance && dist < minDist)
                {
                    minDist = dist;
                    snapIndex = i;
                }
            }
            return snapIndex;
        }
    }
}
