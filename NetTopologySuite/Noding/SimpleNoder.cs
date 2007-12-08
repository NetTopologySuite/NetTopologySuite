using System;
using System.Collections.Generic;
using GeoAPI.Coordinates;
using NPack.Interfaces;

namespace GisSharpBlog.NetTopologySuite.Noding
{
    /// <summary>
    /// Nodes a set of <see cref="SegmentString{TCoordinate}" />s by
    /// performing a brute-force comparison of every segment to every other one.
    /// This has n^2 performance, so is too slow for use on large numbers of segments.
    /// </summary>
    public class SimpleNoder<TCoordinate> : SinglePassNoder<TCoordinate>
        where TCoordinate : ICoordinate, IEquatable<TCoordinate>, IComparable<TCoordinate>,
                            IComputable<TCoordinate>, IConvertible
    {
        private IEnumerable<SegmentString<TCoordinate>> _nodedSegStrings;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleNoder{TCoordinate}"/> class.
        /// </summary>
        public SimpleNoder() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleNoder{TCoordinate}"/> class.
        /// </summary>
        public SimpleNoder(ISegmentIntersector<TCoordinate> segInt)
            : base(segInt) {}

        /// <summary>
        /// Returns an <see cref="IEnumerable{T}"/> of fully noded 
        /// <see cref="SegmentString{TCoordinate}" />s.
        /// The <see cref="SegmentString{TCoordinate}" />s have the same context as their parent.
        /// </summary>
        public override IEnumerable<SegmentString<TCoordinate>> GetNodedSubstrings()
        {
            return SegmentString<TCoordinate>.GetNodedSubstrings(_nodedSegStrings);
        }

        /// <summary>
        /// Computes the noding for a collection of <see cref="SegmentString{TCoordinate}" />s.
        /// Some Noders may add all these nodes to the input <see cref="SegmentString{TCoordinate}" />s;
        /// others may only add some or none at all.
        /// </summary>
        public override void ComputeNodes(IEnumerable<SegmentString<TCoordinate>> inputSegStrings)
        {
            _nodedSegStrings = inputSegStrings;

            foreach (SegmentString<TCoordinate> edge0 in inputSegStrings)
            {
                foreach (SegmentString<TCoordinate> edge1 in inputSegStrings)
                {
                    computeIntersects(edge0, edge1);
                }
            }
        }

        private void computeIntersects(SegmentString<TCoordinate> e0, SegmentString<TCoordinate> e1)
        {
            IEnumerator<TCoordinate> pts0 = e0.Coordinates.GetEnumerator();
            IEnumerator<TCoordinate> pts1 = e1.Coordinates.GetEnumerator();

            Int32 i0 = 0, i1 = 0;

            while(pts0.MoveNext())
            {
                while(pts1.MoveNext())
                {
                    SegmentIntersector.ProcessIntersections(e0, i0, e1, i1);

                    i1 += 1;
                }

                i0 += 1;
            }
        }
    }
}