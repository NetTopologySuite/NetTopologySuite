using NetTopologySuite.Geometries;

namespace NetTopologySuite.Operation.Distance
{
    /// <summary>
    /// A location on a <see cref="FacetSequence"/> (JTS <c>CoordinateSequenceLocation</c>).
    /// </summary>
    /// <remarks>
    /// Location indexes are always the index of a sequence segment,
    /// so they are always less than the number of vertices.
    /// The endpoint of a sequence uses the index of the final segment.
    /// On a ring the index of the final endpoint is normalized to 0.
    /// Used by <c>DirectedHausdorffDistance</c> to skip bisection of
    /// identical or collinear zero-distance segments.
    /// </remarks>
    /// <author>Martin Davis</author>
    public sealed class CoordinateSequenceLocation
    {
        private readonly CoordinateSequence _seq;
        private readonly int _index;
        private readonly Coordinate _pt;

        /// <summary>
        /// Creates a location on <paramref name="seq"/> at segment <paramref name="index"/>.
        /// </summary>
        public CoordinateSequenceLocation(CoordinateSequence seq, int index, Coordinate pt)
        {
            _seq = seq;
            _pt = pt;
            _index = index >= seq.Count ? seq.Count - 1 : index;
        }

        /// <summary>The coordinate of this location.</summary>
        public Coordinate Coordinate => _pt;

        /// <summary>The segment index of this location.</summary>
        public int Index => _index;

        /// <summary>
        /// Tests whether two locations lie on the same target segment,
        /// including the shared vertex of consecutive segments.
        /// </summary>
        public bool IsSameSegment(CoordinateSequenceLocation other)
        {
            if (!ReferenceEquals(_seq, other._seq))
                return false;
            if (_index == other._index)
                return true;
            if (IsNext(_index, other._index))
            {
                var endPt = _seq.GetCoordinate(_index + 1);
                return other._pt.Equals2D(endPt);
            }
            if (IsNext(other._index, _index))
            {
                var endPt = _seq.GetCoordinate(_index + 1);
                return _pt.Equals2D(endPt);
            }
            return false;
        }

        /// <summary>
        /// JTS <c>isNext</c> writes <c>index1 == 0 &amp;&amp; isRing &amp;&amp; index1 == size-1</c>,
        /// which is unreachable. This is the documented ring intent
        /// (last segment is adjacent to segment 0).
        /// </summary>
        private bool IsNext(int index, int index1)
        {
            if (index1 == index + 1)
                return true;
            if (CoordinateSequences.IsRing(_seq) && index1 == 0 && index + 1 == _seq.Count - 1)
                return true;
            return false;
        }
    }
}
