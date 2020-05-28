using System.Collections.Generic;
using System.Collections.ObjectModel;
using NetTopologySuite.Geometries;
using NetTopologySuite.Noding;

namespace NetTopologySuite.Operation.OverlayNg
{
    /// <summary>
    /// A noder which extracts all line segments 
    /// as {@link SegmentString}s.
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
    internal class SegmentExtractingNoder : INoder
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

        private static void ExtractSegments(ISegmentString ss, ICollection<ISegmentString> segList)
        {
            for (int i = 0; i < ss.Count - 1; i++)
            {
                var p0 = ss.Coordinates[i];
                var p1 = ss.Coordinates[i + 1];
                var seg = new BasicSegmentString(new Coordinate[] { p0, p1 }, ss.Context);
                segList.Add(seg);
            }
        }

        public IList<ISegmentString> GetNodedSubstrings()
        {
            return _segList;
        }

    }
}
