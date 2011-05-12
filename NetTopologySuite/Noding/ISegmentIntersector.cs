namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Computes the intersections between two line segments in <see cref="ISegmentString" />s
    /// and adds them to each string.
    /// The <see cref="ISegmentIntersector" /> is passed to a <see cref="INoder" />.
    /// The <see cref="INodableSegmentString.AddIntersection" />  method is called whenever the <see cref="INoder" />
    /// detects that two <see cref="ISegmentString" />s might intersect.
    /// This class is an example of the Strategy pattern.
    /// </summary>
    public interface ISegmentIntersector
    {
        /// <summary>
        /// This method is called by clients
        /// of the <see cref="ISegmentIntersector" /> interface to process
        /// intersections for two segments of the <see cref="ISegmentString" />s being intersected.
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="segIndex0"></param>
        /// <param name="e1"></param>
        /// <param name="segIndex1"></param>
        void ProcessIntersections(ISegmentString e0, int segIndex0, ISegmentString e1, int segIndex1);
    }
}
