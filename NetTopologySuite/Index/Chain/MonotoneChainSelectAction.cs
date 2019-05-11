using NetTopologySuite.Geometries;

namespace NetTopologySuite.Index.Chain
{
    /// <summary>
    /// The action for the internal iterator for performing
    /// envelope select queries on a MonotoneChain.
    /// </summary>
    public class MonotoneChainSelectAction
    {
        /// <summary>
        ///
        /// </summary>
        public LineSegment SelectedSegment = new LineSegment();

        /// <summary>
        /// This method is overridden to process a segment
        /// in the context of the parent chain.
        /// </summary>
        /// <param name="mc">The parent chain</param>
        /// <param name="startIndex">The index of the start vertex of the segment being processed</param>
        public virtual void Select(MonotoneChain mc, int startIndex)
        {
            mc.GetLineSegment(startIndex, ref SelectedSegment);
            // call this routine in case select(segment) was overridden
            Select(SelectedSegment);
        }

        /// <summary>
        /// This is a convenience method which can be overridden to obtain the actual
        /// line segment which is selected.
        /// </summary>
        /// <param name="seg"></param>
        public virtual void Select(LineSegment seg) { }
    }
}
