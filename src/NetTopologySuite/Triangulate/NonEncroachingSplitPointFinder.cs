using NetTopologySuite.Geometries;

namespace NetTopologySuite.Triangulate
{
    /// <summary>
    /// A strategy for finding constraint split points which attempts to maximise the length of the split
    /// segments while preventing further encroachment. (This is not always possible for narrow angles).
    /// </summary>
    /// <author>Martin Davis</author>
    public class NonEncroachingSplitPointFinder : IConstraintSplitPointFinder
    {
        /// <summary>
        /// A basic strategy for finding split points when nothing extra is known about the geometry of
        /// the situation.
        /// </summary>
        /// <param name="seg">the encroached segment</param>
        /// <param name="encroachPt">the encroaching point</param>
        /// <returns>the point at which to split the encroached segment</returns>
        public Coordinate FindSplitPoint(Segment seg, Coordinate encroachPt)
        {
            var lineSeg = seg.LineSegment;
            double segLen = lineSeg.Length;
            double midPtLen = segLen / 2;
            var splitSeg = new SplitSegment(lineSeg);

            var projPt = ProjectedSplitPoint(seg, encroachPt);
            /*
             * Compute the largest diameter (length) that will produce a split segment which is not
             * still encroached upon by the encroaching point (The length is reduced slightly by a
             * safety factor)
             */
            double nonEncroachDiam = projPt.Distance(encroachPt) * 2 * 0.8; // .99;
            double maxSplitLen = nonEncroachDiam;
            if (maxSplitLen > midPtLen) {
                maxSplitLen = midPtLen;
            }
            splitSeg.MinimumLength = maxSplitLen;

            splitSeg.SplitAt(projPt);

            return splitSeg.SplitPoint;
        }

        /// <summary>
        /// Computes a split point which is the projection of the encroaching point on the segment
        /// </summary>
        /// <param name="seg">The segment</param>
        /// <param name="encroachPt">The enchroaching point</param>
        /// <returns>A split point on the segment</returns>
        public static Coordinate ProjectedSplitPoint(Segment seg, Coordinate encroachPt)
        {
            var lineSeg = seg.LineSegment;
            var projPt = lineSeg.Project(encroachPt);
            return projPt;
        }
    }
}