using System;
using GeoAPI.Coordinates;
using GeoAPI.DataStructures;
using GeoAPI.Geometries;
using GisSharpBlog.NetTopologySuite.Geometries;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Operation.Overlay.Snap
{
    /**
     * Snaps the vertices and segments of a {@link LineString} to a set of target snap vertices.
     * A snapping distance tolerance is used to control where snapping is performed.
     *
     * @author Martin Davis
     * @version 1.7
     */

    ///<summary>
    /// Snaps the vertices and segments of a {@link LineString} to a set of target snap vertices.
    /// A snapping distance tolerance is used to control where snapping is performed.
    /// 
    /// author Martin Davis
    /// version 1.7
    ///</summary>
    ///<typeparam name="TCoordinate"></typeparam>
    public class LineStringSnapper<TCoordinate>
        where TCoordinate : ICoordinate<TCoordinate>, IEquatable<TCoordinate>,
                            IComparable<TCoordinate>, IConvertible,
                            IComputable<Double, TCoordinate>
    {
        private readonly Double _snapTolerance = 0.0;

        private readonly ICoordinateSequence<TCoordinate> _srcPts;
        private LineSegment<TCoordinate> _seg;
        private readonly Boolean _isClosed = false;

        ///<summary>
        /// Creates a new snapper using the points in the given <see cref="ILineString{TCoordinate}"/>
        ///</summary>
        ///<param name="srcLline">a LineString to snap</param>
        ///<param name="snapTolerance">the snap tolerance to use</param>
        public LineStringSnapper(ILineString<TCoordinate> srcLline, Double snapTolerance)
            : this(srcLline.Coordinates, snapTolerance)
        {
        }

        ///<summary>
        /// Creates a new snapper using the given points as source points to be snapped.
        ///</summary>
        ///<param name="srcPts">the points to snap</param>
        ///<param name="snapTolerance">the snap tolerance to use</param>
        public LineStringSnapper(ICoordinateSequence<TCoordinate> srcPts, Double snapTolerance)
        {
            _srcPts = srcPts;
            _isClosed = srcPts.First.Equals(srcPts.Last);
            _snapTolerance = snapTolerance;
        }

        ///<summary>
        /// Snaps the vertices and segments of the source LineString to the given set of target snap points.
        ///</summary>
        ///<param name="snapPts">the vertices to snap to</param>
        ///<returns>the snapped points</returns>
        public ICoordinateSequence<TCoordinate> SnapTo(ICoordinateSequence<TCoordinate> snapPts)
        {
            CoordinateList<TCoordinate> coordList = new CoordinateList<TCoordinate>(_srcPts);;

            SnapVertices(ref coordList, snapPts);
            SnapSegments(coordList, snapPts);

            return snapPts.CoordinateSequenceFactory.Create(coordList);
        }

        /**
         * Snap source vertices to vertices in the target.
         * 
         * @param srcCoords the points to snap
         * @param snapPts the points to snap to
         */
        private void SnapVertices(ref CoordinateList<TCoordinate> srcCoords, ICoordinateSequence<TCoordinate> snapPts)
        {
            CoordinateList<TCoordinate>  retVal = srcCoords.Clone(); 
            // try snapping vertices
            // assume src list has a closing point (is a ring)
            Int32 index = 0;
            foreach (TCoordinate srcPt in srcCoords)
            {
                TCoordinate snapVert = FindSnapForVertex(srcPt, snapPts);
                Boolean snapPt;
                if( typeof(TCoordinate).IsValueType)
                    snapPt = snapVert.Equals(default(TCoordinate));
                else
                    snapPt = snapVert == null;
                if (!snapPt)
                {
                    // update src with snap pt
                    retVal[index] = snapVert;
                    // keep final closing point in synch (rings only)
                    if (index == 0 && _isClosed)
                        retVal[srcCoords.Count - 1] = snapVert;
                }
                index++;
            }
            srcCoords = retVal;
        }

        private TCoordinate FindSnapForVertex(TCoordinate pt, ICoordinateSequence<TCoordinate> snapPts)
        {
            foreach (TCoordinate snapPt in snapPts)
            {
                if (pt.Equals(snapPt))
                    return default(TCoordinate);
                if (pt.Distance(snapPt) < _snapTolerance)
                    return snapPt;
            }
            return default(TCoordinate);
        }

        /**
         * Snap segments of the source to nearby snap vertices.
         * Source segments are "cracked" at a snap vertex, and further
         * snapping takes place on the modified list of segments.
         * For each distinct snap vertex, at most one source segment
         * is snapped to.  This prevents "cracking" multiple segments 
         * at the same point, which would almost certainly cause the result to be invalid.
         * 
         * @param srcCoords
         * @param snapPts
         */
        private void SnapSegments(CoordinateList<TCoordinate> srcCoords, ICoordinateSequence<TCoordinate> snapPts)
        {
            Int32 distinctPtCount = snapPts.Count;

            // check for duplicate snap pts.  
            // Need to do this better - need to check all points for dups (using a Set?)
            if (snapPts.First.Equals(snapPts.Last))
                distinctPtCount--;

            Int32 count = 0;
            foreach (TCoordinate snapPt in snapPts)
            {
                Int32 index = FindSegmentIndexToSnap(snapPt, srcCoords);
                /**
                 * If a segment to snap to was found, "crack" it at the snap pt.
                 * The new pt is inserted immediately into the src segment list,
                 * so that subsequent snapping will take place on the latest segments.
                 * Duplicate points are not added.
                 */
                if (index >= 0)
                {
                    srcCoords.Insert(index + 1, snapPt.Clone(), false);
                }
                count++;
                if (count >= distinctPtCount)
                    break;
            }
        }


        /**
         * Finds a src segment which snaps to (is close to) the given snap point
         * Only one segment is determined - this is to prevent
         * snapping to multiple segments, which would almost certainly cause invalid geometry
         * to be created.
         * (The heuristic approach of snapping is really only appropriate when
         * snap pts snap to a unique spot on the src geometry.)
         *
         * @param snapPt the point to snap to
         * @param srcCoords the source segment coordinates
         * @return the index of the snapped segment
         * @return -1 if no segment snaps
         */
        private int FindSegmentIndexToSnap(TCoordinate snapPt, CoordinateList<TCoordinate> srcCoords)
        {
            Double minDist = Double.MaxValue;
            Int32 snapIndex = -1, index = 0;

            foreach (Pair<TCoordinate> pair in Slice.GetOverlappingPairs(srcCoords))
            {
                if (pair.First.Equals(snapPt) || pair.Second.Equals(snapPt))
                    return -1;

                _seg = new LineSegment<TCoordinate>(pair);
                Double dist = _seg.Distance(snapPt);
                if ( dist < _snapTolerance && dist < minDist )
                {
                    minDist = dist;
                    snapIndex = index;
                }
                index++;
            }

            return snapIndex;
        }
    }
}
