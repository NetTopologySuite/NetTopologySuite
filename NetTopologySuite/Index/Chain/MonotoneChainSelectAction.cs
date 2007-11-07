using System;
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
        public Extents TempEnv1 = new Extents();

        public LineSegment SelectedSegment = new LineSegment();

        /// <summary> 
        /// This function can be overridden if the original chain is needed.
        /// </summary>
        public virtual void Select(MonotoneChain mc, Int32 start)
        {
            mc.GetLineSegment(start, ref SelectedSegment);
            Select(SelectedSegment);
        }

        /// <summary>
        /// This is a convenience function which can be overridden to obtain the actual
        /// line segment which is selected.
        /// </summary>
        public virtual void Select(LineSegment seg) {}
    }
}