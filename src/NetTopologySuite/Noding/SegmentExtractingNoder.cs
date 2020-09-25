using System.Collections.Generic;
using NetTopologySuite.Geometries;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// A noder which extracts all line segments 
    /// as <see cref="ISegmentString"/>s.
    /// This enables fast overlay of geometries which are known to be already fully noded.
    /// In particular, it provides fast union of polygonal and linear coverages.
    /// Unioning a noded set of lines is an effective way 
    /// to perform line merging and line dissolving.
    /// <para/>
    /// No precision reduction is carried out. 
    /// If that is required, another noder must be used (such as a snap-rounding noder),
    /// or the input must be precision-reduced beforehand.
    /// </summary>
    /// <author>Martin Davis</author>
    public sealed class SegmentExtractingNoder : INoder
    {
        private List<ISegmentString> _segList;

        public void ComputeNodes(IList<ISegmentString> segStrings)
        {
            _segList = ExtractSegments(segStrings);
        }

        private static List<ISegmentString> ExtractSegments(IEnumerable<ISegmentString> segStrings)
        {
            var segList = new List<ISegmentString>();
            foreach (var ss in segStrings)
            {
                ExtractSegments(ss, segList);
            }
            return segList;
        }

        private static void ExtractSegments(ISegmentString ss, List<ISegmentString> segList)
        {
            var coords = ss.Coordinates;
            object context = ss.Context;
            int cnt = ss.Count;
            for (int i = 0; i < cnt - 1; i++)
            {
                var p0 = coords[i];
                var p1 = coords[i + 1];
                var seg = new BasicSegmentString(new Coordinate[] { p0, p1 }, context);
                segList.Add(seg);
            }
        }

        public IList<ISegmentString> GetNodedSubstrings()
        {
            return _segList;
        }

    }
}
