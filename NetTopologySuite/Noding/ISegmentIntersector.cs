using System;
using System.Collections.Generic;
using System.Text;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Computes the intersections between two line segments in <see cref="SegmentString" />s
    /// and adds them to each string.
    /// The <see cref="SegmentIntersector" /> is passed to a <see cref="Noder" />.
    /// The <see cref="addIntersections" />  method is called whenever the <see cref="Noder" />
    /// detects that two <see cref="SegmentString" /> s might intersect.
    /// This class is an example of the Strategy pattern.
    /// </summary>
    public interface ISegmentIntersector
    {
        /// <summary>
        /// This method is called by clients
        /// of the <see cref="SegmentIntersector" /> interface to process
        /// intersections for two segments of the <see cref="SegmentString" />s being intersected.
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="segIndex0"></param>
        /// <param name="e1"></param>
        /// <param name="segIndex1"></param>
        void ProcessIntersections(SegmentString e0, int segIndex0, SegmentString e1, int segIndex1);
    }
}
