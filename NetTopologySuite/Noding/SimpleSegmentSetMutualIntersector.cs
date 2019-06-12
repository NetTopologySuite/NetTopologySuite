using System.Collections.Generic;
using System.Linq;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// Intersects two sets of <see cref="ISegmentString"/>s using
    /// brute-force comparison.
    /// </summary>
    public class SimpleSegmentSetMutualIntersector : ISegmentSetMutualIntersector
    {
        private readonly ISegmentString[] _baseBaseSegStrings;

        /// <summary>
        /// Constructs a new intersector for a given set of <see cref="ISegmentString"/>s.
        /// </summary>
        /// <param name="baseSegStrings">The base segment strings to intersect</param>
        public SimpleSegmentSetMutualIntersector(IEnumerable<ISegmentString> baseSegStrings)
        {
            _baseBaseSegStrings = baseSegStrings.ToArray();
        }

        /// <summary>
        /// Calls <see cref="ISegmentIntersector.ProcessIntersections(ISegmentString, int, ISegmentString, int)"/>
        /// for all <i>candidate</i> intersections between
        /// the given collection of SegmentStrings and the set of base segments.
        /// </summary>
        /// <param name="segmentStrings">A collection of <see cref="ISegmentString"/>s to node</param>
        /// <param name="segmentIntersector">The intersection detector to either record intersection occurences
        /// or add intersection nodes to the input segment strings.</param>
        public void Process(IEnumerable<ISegmentString> segmentStrings, ISegmentIntersector segmentIntersector)
        {
            // don't iterate over the input more than once.
            var segmentStringsArray = segmentStrings?.ToArray();
            foreach (var baseSegmentString in _baseBaseSegStrings)
            {
                foreach (var segmentString in segmentStringsArray)
                {
                    Intersect(baseSegmentString, segmentString, segmentIntersector);
                    if (segmentIntersector.IsDone)
                        return;
                }
            }
        }

        /// <summary>
        /// Processes all of the segment pairs in the given segment strings
        /// using the given <paramref name="segInt">SegmentIntersector</paramref>.
        /// </summary>
        /// <param name="ss0">A segment string</param>
        /// <param name="ss1">A segment string</param>
        /// <param name="segInt">The segment intersector to use</param>
        private static void Intersect(ISegmentString ss0, ISegmentString ss1, ISegmentIntersector segInt)
        {
            var pts0 = ss0.Coordinates;
            var pts1 = ss1.Coordinates;
            for (int i0 = 0; i0 < pts0.Length - 1; i0++)
            {
                for (int i1 = 0; i1 < pts1.Length - 1; i1++)
                {
                    segInt.ProcessIntersections(ss0, i0, ss1, i1);
                    if (segInt.IsDone)
                        return;
                }
            }

        }

    }
}
