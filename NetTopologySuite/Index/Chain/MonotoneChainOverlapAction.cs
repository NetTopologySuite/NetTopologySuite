using NetTopologySuite.Geometries;

namespace NetTopologySuite.Index.Chain
{
    /// <summary>
    /// The action for the internal iterator for performing
    /// overlap queries on a MonotoneChain.
    /// </summary>
    public class MonotoneChainOverlapAction
    {
        /// <summary>
        ///
        /// </summary>
        protected LineSegment overlapSeg1 = new LineSegment();

        /// <summary>
        ///
        /// </summary>
        protected LineSegment overlapSeg2 = new LineSegment();

        /// <summary>
        /// This function can be overridden if the original chains are needed.
        /// </summary>
        /// <param name="mc1"></param>
        /// <param name="start1">The index of the start of the overlapping segment from mc1.</param>
        /// <param name="mc2"></param>
        /// <param name="start2">The index of the start of the overlapping segment from mc2.</param>
        public virtual void Overlap(MonotoneChain mc1, int start1, MonotoneChain mc2, int start2)
        {
            mc1.GetLineSegment(start1, ref overlapSeg1);
            mc2.GetLineSegment(start2, ref overlapSeg2);
            Overlap(overlapSeg1, overlapSeg2);
        }

        /// <summary>
        /// This is a convenience function which can be overridden to obtain the actual
        /// line segments which overlap.
        /// </summary>
        /// <param name="seg1"></param>
        /// <param name="seg2"></param>
        public virtual void Overlap(LineSegment seg1, LineSegment seg2) { }
    }
}
