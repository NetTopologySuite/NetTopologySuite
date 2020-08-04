using System.Collections.Generic;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// An intersector for the red-blue intersection problem.
    /// In this class of line arrangement problem,
    /// two disjoint sets of linestrings are intersected.
    /// <para/>
    /// Implementing classes must provide a way
    /// of supplying the base set of segment strings to
    /// test against (e.g. in the constructor,
    /// for straightforward thread-safety).
    /// <para/>
    /// In order to allow optimizing processing,
    /// the following condition is assumed to hold for each set:
    /// <list Type="Bullet">
    /// <item><description>the only intersection between any two linestrings occurs at their endpoints.</description></item>
    /// </list>
    /// Implementations can take advantage of this fact to optimize processing
    /// (i.e. by avoiding testing for intersections between linestrings
    /// belonging to the same set).
    /// </summary>
    public interface ISegmentSetMutualIntersector
    {
        /// <summary>
        /// Computes the intersections with a given set of <see cref="ISegmentString"/>s,
        /// using the supplied <see cref="ISegmentIntersector"/>.
        /// </summary>
        /// <param name="segmentStrings">A collection of <see cref="ISegmentString"/>s to node</param>
        /// <param name="segmentIntersector">The intersection detector to either record intersection occurrences
        /// or add intersection nodes to the input segment strings.</param>
        void Process(IEnumerable<ISegmentString> segmentStrings, ISegmentIntersector segmentIntersector);
    }
}
