//using System.Collections;
using System.Collections.Generic;

namespace NetTopologySuite.Noding
{

    /// <summary>
    /// Nodes a set of <see cref="ISegmentString" />s by
    /// performing a brute-force comparison of every segment to every other one.
    /// This has n^2 performance, so is too slow for use on large numbers of segments.
    /// </summary>
    public class SimpleNoder : SinglePassNoder
    {

        private IList<ISegmentString> _nodedSegStrings;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleNoder"/> class.
        /// </summary>
        public SimpleNoder() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleNoder"/> class.
        /// </summary>
        /// <param name="segInt"></param>
        public SimpleNoder(ISegmentIntersector segInt)
            : base(segInt) { }

        /// <summary>
        /// Returns a <see cref="IList{ISegmentString}"/> of fully noded <see cref="NodedSegmentString"/>s.
        /// The <see cref="NodedSegmentString"/>s have the same context as their parent.
        /// </summary>
        /// <returns></returns>
        public override IList<ISegmentString> GetNodedSubstrings()
        {
            return NodedSegmentString.GetNodedSubstrings(_nodedSegStrings);
        }

        /// <summary>
        /// Computes the noding for a collection of <see cref="ISegmentString" />s.
        /// Some Noders may add all these nodes to the input <see cref="ISegmentString" />s;
        /// others may only add some or none at all.
        /// </summary>
        /// <param name="inputSegStrings"></param>
        public override void ComputeNodes(IList<ISegmentString> inputSegStrings)
        {
            _nodedSegStrings = inputSegStrings;
            foreach (var edge0 in inputSegStrings)
            {
                foreach (var edge1 in inputSegStrings)
                {
                    ComputeIntersects(edge0, edge1);
                }
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="e1"></param>
        private void ComputeIntersects(ISegmentString e0, ISegmentString e1)
        {
            var pts0 = e0.Coordinates;
            var pts1 = e1.Coordinates;
            for (int i0 = 0; i0 < pts0.Length - 1; i0++)
                for (int i1 = 0; i1 < pts1.Length - 1; i1++)
                    SegmentIntersector.ProcessIntersections(e0, i0, e1, i1);
        }
    }
}
