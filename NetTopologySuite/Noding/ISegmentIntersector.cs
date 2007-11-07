using System;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Computes the intersections between two line segments in <see cref="SegmentString" />s
    /// and adds them to each string.
    /// The <see cref="ISegmentIntersector" /> is passed to a <see cref="INoder" />.
    /// The <see cref="SegmentString.AddIntersections" />  method is called whenever the <see cref="INoder" />
    /// detects that two <see cref="SegmentString" /> s might intersect.
    /// This class is an example of the Strategy pattern.
    /// </summary>
    public interface ISegmentIntersector
    {
        /// <summary>
        /// This method is called by clients
        /// of the <see cref="ISegmentIntersector" /> interface to process
        /// intersections for two segments of the <see cref="SegmentString" />s being intersected.
        /// </summary>
        void ProcessIntersections(SegmentString e0, Int32 segIndex0, SegmentString e1, Int32 segIndex1);
    }
}