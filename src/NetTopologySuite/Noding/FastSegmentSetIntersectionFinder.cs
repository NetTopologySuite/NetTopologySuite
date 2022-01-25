using System.Collections.Generic;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// Finds if two sets of <see cref="ISegmentString"/>s intersect.
    /// </summary>
    /// <remarks>
    /// Uses indexing for fast performance and to optimize repeated tests
    /// against a target set of lines.
    /// Short-circuited to return as soon an intersection is found.
    /// <para/>
    /// Immutable and thread-safe.
    /// </remarks>
    public class FastSegmentSetIntersectionFinder
    {
        private readonly ISegmentSetMutualIntersector _segSetMutInt;

        //for testing purposes
        //private SimpleSegmentSetMutualIntersector mci;

        /// <summary>
        /// Creates an intersection finder against a given set of segment strings.
        /// </summary>
        /// <param name="baseSegStrings">The segment strings to search for intersections</param>
        public FastSegmentSetIntersectionFinder(IEnumerable<ISegmentString> baseSegStrings)
        {
            _segSetMutInt = new MCIndexSegmentSetMutualIntersector(baseSegStrings);
        }

        /// <summary>Gets the segment set intersector used by this class.</summary>
        /// <remarks>This allows other uses of the same underlying indexed structure.</remarks>
        public ISegmentSetMutualIntersector SegmentSetIntersector => _segSetMutInt;

        /// <summary>
        /// Tests for intersections with a given set of target <see cref="ISegmentString"/>s.
        /// </summary>
        /// <param name="segStrings">The SegmentStrings to test</param>
        /// <returns><c>true</c> if an intersection was found</returns>
        public bool Intersects(IList<ISegmentString> segStrings)
        {
            var intFinder = new SegmentIntersectionDetector();
            return Intersects(segStrings, intFinder);
        }

        /// <summary>
        /// Tests for intersections with a given set of target <see cref="ISegmentString"/>s.
        /// using a given SegmentIntersectionDetector.
        /// </summary>
        /// <param name="segStrings">The SegmentStrings to test</param>
        /// <param name="intDetector">The intersection detector to use</param>
        /// <returns><c>true</c> if the detector reports intersections</returns>
        public bool Intersects(IList<ISegmentString> segStrings, SegmentIntersectionDetector intDetector)
        {
            _segSetMutInt.Process(segStrings, intDetector);
            return intDetector.HasIntersection;
        }
    }
}
