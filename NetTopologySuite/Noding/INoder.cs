using System.Collections;

namespace GisSharpBlog.NetTopologySuite.Noding
{

    /// <summary>
    /// Computes all intersections between segments in a set of <see cref="SegmentString" />s.
    /// Intersections found are represented as <see cref="SegmentNode" />s and added to the
    /// <see cref="SegmentString" />s in which they occur.
    /// As a final step in the noding a new set of segment strings split at the nodes may be returned.
    /// </summary>
    public interface INoder
    {

        /// <summary>
        /// Computes the noding for a collection of <see cref="SegmentString" />s.
        /// Some Noders may add all these nodes to the input <see cref="SegmentString" />s;
        /// others may only add some or none at all.
        /// </summary>
        /// <param name="segStrings"></param>
        void ComputeNodes(IList segStrings);

        /// <summary>
        /// Returns a <see cref="IList" /> of fully noded <see cref="SegmentString" />s.
        /// The <see cref="SegmentString" />s have the same context as their parent.
        /// </summary>
        /// <returns></returns>
        IList GetNodedSubstrings();

    }
}
