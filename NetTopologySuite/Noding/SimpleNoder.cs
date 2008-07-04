using System.Collections;
using GeoAPI.Geometries;

namespace GisSharpBlog.NetTopologySuite.Noding
{

    /// <summary>
    /// Nodes a set of <see cref="SegmentString" />s by
    /// performing a brute-force comparison of every segment to every other one.
    /// This has n^2 performance, so is too slow for use on large numbers of segments.
    /// </summary>
    public class SimpleNoder : SinglePassNoder
    {

        private IList nodedSegStrings;

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
        /// Returns a <see cref="IList"/> of fully noded <see cref="SegmentString"/>s.
        /// The <see cref="SegmentString"/>s have the same context as their parent.
        /// </summary>
        /// <returns></returns>
        public override IList GetNodedSubstrings()
        {
            return SegmentString.GetNodedSubstrings(nodedSegStrings);
        }

        /// <summary>
        /// Computes the noding for a collection of <see cref="SegmentString" />s.
        /// Some Noders may add all these nodes to the input <see cref="SegmentString" />s;
        /// others may only add some or none at all.
        /// </summary>
        /// <param name="inputSegStrings"></param>
        public override void ComputeNodes(IList inputSegStrings)
        {
            this.nodedSegStrings = inputSegStrings;
            foreach (object obj0 in inputSegStrings)
            {
                SegmentString edge0 = (SegmentString) obj0;
                foreach(object obj1 in inputSegStrings)
                {
                    SegmentString edge1 = (SegmentString) obj1;
                    ComputeIntersects(edge0, edge1);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="e0"></param>
        /// <param name="e1"></param>
        private void ComputeIntersects(SegmentString e0, SegmentString e1)
        {
            ICoordinate[] pts0 = e0.Coordinates;
            ICoordinate[] pts1 = e1.Coordinates;
            for (int i0 = 0; i0 < pts0.Length - 1; i0++)
                for (int i1 = 0; i1 < pts1.Length - 1; i1++)
                    SegmentIntersector.ProcessIntersections(e0, i0, e1, i1);
        }
    }
}
