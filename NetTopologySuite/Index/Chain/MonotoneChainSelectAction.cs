using GisSharpBlog.NetTopologySuite.Geometries;

namespace GisSharpBlog.NetTopologySuite.Index.Chain
{
    /// <summary>
    /// The action for the internal iterator for performing
    /// envelope select queries on a MonotoneChain.
    /// </summary>
    public class MonotoneChainSelectAction
    {
        /// <summary>
        /// These envelopes are used during the MonotoneChain search process.
        /// </summary>
        public Envelope TempEnv1 = new Envelope();

        /// <summary>
        /// 
        /// </summary>
        public LineSegment SelectedSegment = new LineSegment();

        /// <summary> 
        /// This function can be overridden if the original chain is needed.
        /// </summary>
        /// <param name="mc"></param>
        /// <param name="start"></param>
        public virtual void Select(MonotoneChain mc, int start)
        {
            mc.GetLineSegment(start, ref SelectedSegment);
            Select(SelectedSegment);
        }

        /// <summary>
        /// This is a convenience function which can be overridden to obtain the actual
        /// line segment which is selected.
        /// </summary>
        /// <param name="seg"></param>
        public virtual void Select(LineSegment seg) { }
    }
}
