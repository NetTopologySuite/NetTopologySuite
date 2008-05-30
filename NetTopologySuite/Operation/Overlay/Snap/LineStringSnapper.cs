using System;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Operation.Overlay.Snap
{
    /// <summary>
    /// Snaps the vertices and segments of a LineString to a set of target snap vertices.
    /// A snapping distance tolerance is used to control where snapping is performed.
    /// </summary>
    public class LineStringSnapper
    {
        private double snapTolerance = 0.0;

        private ICoordinate[] srcPts;
        private LineSegment seg = new LineSegment(); // for reuse during snapping
        private bool isClosed = false;

        /// <summary>
        /// Creates a new snapper using the points in the given {@link LineString}
        /// as target snap points.
        /// </summary>
        /// <param name="line"></param>
        /// <param name="snapTolerance"></param>
        public LineStringSnapper(ILineString line, double snapTolerance) : 
            this(line.Coordinates, snapTolerance) { }

        /// <summary>
        /// Creates a new snapper using the given points
        /// as target snap points.
        /// </summary>
        /// <param name="srcPts"></param>
        /// <param name="snapTolerance"></param>
        public LineStringSnapper(ICoordinate[] srcPts, double snapTolerance)
        {
            this.srcPts = srcPts;
            isClosed = srcPts[0].Equals2D(srcPts[srcPts.Length - 1]);
            this.snapTolerance = snapTolerance;
        }

        /// <summary>
        /// Snaps the vertices and segments of the source LineString 
        /// to the given set of target snap points.
        /// </summary>
        /// <param name="snapPts"></param>
        /// <returns></returns>
        public ICoordinate[] SnapTo(ICoordinate[] snapPts)
        {
            CoordinateList coordList = new CoordinateList(srcPts);
            SnapVertices(coordList, snapPts);
            SnapSegments(coordList, snapPts);
            ICoordinate[] newPts = coordList.ToCoordinateArray();
            return newPts;
        }

        /// <summary>
        /// Snap source vertices to vertices in the target.
        /// </summary>
        /// <param name="srcCoords"></param>
        /// <param name="snapPts"></param>
        private void SnapVertices(CoordinateList srcCoords, ICoordinate[] snapPts)
        {
            // try snapping vertices
            // assume src list has a closing point (is a ring)
            for (int i = 0; i < srcCoords.Count - 1; i++)
            {
                ICoordinate srcPt = srcCoords[i];
                ICoordinate snapVert = FindSnapForVertex(srcPt, snapPts);
                if (snapVert != null)
                {
                    // update src with snap pt
                    srcCoords[i] = new Coordinate(snapVert);
                    // keep final closing point in synch (rings only)
                    if (i == 0 && isClosed)
                        srcCoords[srcCoords.Count - 1] = new Coordinate(snapVert);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pt"></param>
        /// <param name="snapPts"></param>
        /// <returns></returns>
        private ICoordinate FindSnapForVertex(ICoordinate pt, ICoordinate[] snapPts)
        {
            foreach (ICoordinate coord in snapPts)
            {
                // if point is already equal to a src pt, don't snap
                if (pt.Equals2D(coord))
                    return null;
                if (pt.Distance(coord) < snapTolerance)
                    return coord;
            }
            return null;
        }

        /// <summary>
        /// Snap segments of the source to nearby snap vertices.
        /// Source segments are "cracked" at a snap vertex, and further
        /// snapping takes place on the modified list of segments.
        /// For each distinct snap vertex, at most one source segment
        /// is snapped to.  This prevents "cracking" multiple segments 
        /// at the same point, which would almost certainly cause the result to be invalid.
        /// </summary>
        /// <param name="srcCoords"></param>
        /// <param name="snapPts"></param>
        private void SnapSegments(CoordinateList srcCoords, ICoordinate[] snapPts)
        {
            int distinctPtCount = snapPts.Length;

            // check for duplicate snap pts.  
            // Need to do this better - need to check all points for dups (using a Set?)
            if (snapPts[0].Equals2D(snapPts[snapPts.Length - 1]))
                distinctPtCount = snapPts.Length - 1;

            for (int i = 0; i < distinctPtCount; i++)
            {
                ICoordinate snapPt = snapPts[i];
                int index = FindSegmentIndexToSnap(snapPt, srcCoords);
                /**
                 * If a segment to snap to was found, "crack" it at the snap pt.
                 * The new pt is inserted immediately into the src segment list,
                 * so that subsequent snapping will take place on the latest segments.
                 * Duplicate points are not added.
                 */
                if (index >= 0)
                    srcCoords.Add(index + 1, new Coordinate(snapPt), false);
            }
        }

        /// <summary>
        /// Finds a src segment which snaps to (is close to) the given snap point
        /// Only one segment is determined - this is to prevent
        /// snapping to multiple segments, which would almost certainly cause invalid geometry
        /// to be created.
        /// (The heuristic approach of snapping is really only appropriate when
        /// snap pts snap to a unique spot on the src geometry)
        /// </summary>
        /// <param name="snapPt"></param>
        /// <param name="srcCoords"></param>
        /// <returns>-1 if no segment snaps.</returns>
        private int FindSegmentIndexToSnap(ICoordinate snapPt, CoordinateList srcCoords)
        {
            double minDist = Double.MaxValue;
            int snapIndex = -1;
            for (int i = 0; i < srcCoords.Count - 1; i++)
            {
                seg.P0 = srcCoords[i];
                seg.P1 = srcCoords[i + 1];

                /**
                 * If the snap pt is already in the src list, don't snap
                 */
                if (seg.P0.Equals2D(snapPt) || seg.P1.Equals2D(snapPt))
                    return -1;

                double dist = seg.Distance(snapPt);
                if (dist < snapTolerance && dist < minDist)
                {
                    minDist = dist;
                    snapIndex = i;
                }
            }
            return snapIndex;
        }
    }
}
