using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// A noder which extracts boundary line segments
    /// as <see cref="ISegmentString"/>.
    /// Boundary segments are those which are not duplicated in the input.
    /// It is appropriate for use with valid polygonal coverages.
    /// <para/>
    /// No precision reduction is carried out.
    /// If that is required, another noder must be used (such as a snap-rounding noder),
    /// or the input must be precision-reduced beforehand.
    /// </summary>
    /// <author>Martin Davis</author>
    public class BoundarySegmentNoder : INoder
    {

        private List<ISegmentString> _segList;

        /// <summary>
        /// Creates a new segment-dissolving noder.
        /// </summary>
        public BoundarySegmentNoder()
        {

        }

        /// <inheritdoc/>
        public void ComputeNodes(IList<ISegmentString> segStrings)
        {
            var segSet = new HashSet<Segment>();
            AddSegments(segStrings, segSet);
            _segList = ExtractSegments(segSet);
        }

        private static void AddSegments(IEnumerable<ISegmentString> segStrings, HashSet<Segment> segSet)
        {
            foreach (var ss in segStrings)
            {
                AddSegments(ss, segSet);
            }
        }

        private static void AddSegments(ISegmentString segString, HashSet<Segment> segSet)
        {
            for (int i = 0; i < segString.Count - 1; i++)
            {
                var p0 = segString.Coordinates[i];
                var p1 = segString.Coordinates[i + 1];
                var seg = new Segment(p0, p1, segString, i);
                if (segSet.Contains(seg))
                {
                    segSet.Remove(seg);
                }
                else
                {
                    segSet.Add(seg);
                }
            }
        }

        private static List<ISegmentString> ExtractSegments(HashSet<Segment> segSet)
        {
            var segList = new List<ISegmentString>();
            foreach (var seg in segSet)
            {
                var ss = seg.SegmentString;
                int i = seg.Index;
                var p0 = ss.Coordinates[i];
                var p1 = ss.Coordinates[i + 1];
                var segStr = new BasicSegmentString(new Coordinate[] { p0, p1 }, ss.Context);
                segList.Add(segStr);
            }
            return segList;
        }
        /// <inheritdoc/>
        public IList<ISegmentString> GetNodedSubstrings()
        {
            return _segList;
        }

        private class Segment : LineSegment
        {
            private readonly ISegmentString _segStr;
            private readonly int _index;

            public Segment(Coordinate p0, Coordinate p1,
                ISegmentString segStr, int index)
                        : base(p0, p1)
            {
                _segStr = segStr;
                _index = index;
                Normalize();
            }

            public ISegmentString SegmentString
            {
                get => _segStr;
            }

            public int Index
            {
                get => _index;
            }
        }
    }
}
