using NetTopologySuite.Geometries;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// A noder which extracts chains of boundary segments
    /// as {@link SegmentString}s.
    /// Boundary segments are those which are not duplicated in the input.
    /// The segment strings are extracted in a way that maximises their length,
    /// and minimizes the total number of edges.
    /// This produces the most efficient topological graph structure.
    /// <para/>
    /// Segments which are not on the boundary are those which
    /// have an identical segment in another polygon ring.
    /// <para/>
    /// This enables fast overlay of polygonal coverages in {@link CoverageUnion}.
    /// This noder is faster than {@link SegmentExtractingNoder}
    /// and {@link BoundarySegmentNoder}.
    /// <para/>
    /// No precision reduction is carried out.
    /// If that is required, another noder must be used (such as a snap-rounding noder),
    /// or the input must be precision-reduced beforehand.
    /// </summary>
    /// <author>Martin Davis</author>
    public class BoundaryChainNoder : INoder
    {

        private List<ISegmentString> _chainList;

        /// <summary>
        /// Creates a new boundary-extracting noder.
        /// </summary>
        public BoundaryChainNoder()
        {

        }

        /// <inheritdoc/>
        public void ComputeNodes(IList<ISegmentString> segStrings)
        {
            var segSet = new HashSet<Segment>();
            var bdySections = new BoundarySegmentMap[segStrings.Count];
            AddSegments(segStrings, segSet, bdySections);
            MarkBoundarySegments(segSet);
            _chainList = ExtractChains(bdySections);
        }

        private static void AddSegments(ICollection<ISegmentString> segStrings, HashSet<Segment> segSet,
            BoundarySegmentMap[] includedSegs)
        {
            int i = 0;
            foreach (var ss in segStrings)
            {
                var segInclude = new BoundarySegmentMap(ss);
                includedSegs[i++] = segInclude;
                AddSegments(ss, segInclude, segSet);
            }
        }

        private static void AddSegments(ISegmentString segString, BoundarySegmentMap segInclude, HashSet<Segment> segSet)
        {
            for (int i = 0; i < segString.Count - 1; i++)
            {
                var p0 = segString.Coordinates[i];
                var p1 = segString.Coordinates[i + 1];
                var seg = new Segment(p0, p1, segInclude, i);
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

        private static void MarkBoundarySegments(HashSet<Segment> segSet)
        {
            foreach (var seg in segSet)
            {
                seg.MarkInBoundary();
            }
        }

        private static List<ISegmentString> ExtractChains(BoundarySegmentMap[] sections)
        {
            var sectionList = new List<ISegmentString>();
            foreach (var sect in sections)
            {
                sect.CreateChains(sectionList);
            }
            return sectionList;
        }

        /// <inheritdoc/>
        public IList<ISegmentString> GetNodedSubstrings()
        {
            return _chainList;
        }

        private class BoundarySegmentMap
        {
            private ISegmentString _segString;
            private bool[] _isBoundary;

            public BoundarySegmentMap(ISegmentString ss)
            {
                _segString = ss;
                _isBoundary = new bool[ss.Count - 1];
            }

            public void SetBoundarySegment(int index)
            {
                _isBoundary[index] = true;
            }

            public void CreateChains(List<ISegmentString> chainList)
            {
                int endIndex = 0;
                while (true)
                {
                    int startIndex = FindChainStart(endIndex);
                    if (startIndex >= _segString.Count - 1)
                        break;
                    endIndex = FindChainEnd(startIndex);
                    var ss = CreateChain(_segString, startIndex, endIndex);
                    chainList.Add(ss);
                }
            }

            private static ISegmentString CreateChain(ISegmentString segString, int startIndex, int endIndex)
            {
                var pts = new Coordinate[endIndex - startIndex + 1];
                int ipts = 0;
                for (int i = startIndex; i < endIndex + 1; i++)
                {
                    pts[ipts++] = segString.Coordinates[i].Copy();
                }
                return new BasicSegmentString(pts, segString.Context);
            }

            private int FindChainStart(int index)
            {
                while (index < _isBoundary.Length && !_isBoundary[index])
                {
                    index++;
                }
                return index;
            }

            private int FindChainEnd(int index)
            {
                index++;
                while (index < _isBoundary.Length && _isBoundary[index])
                {
                    index++;
                }
                return index;
            }
        }

        private class Segment : LineSegment
        {
            private readonly BoundarySegmentMap _segMap;
            private readonly int _index;

            public Segment(Coordinate p0, Coordinate p1,
                BoundarySegmentMap segMap, int index)
                        : base(p0, p1)
            {
                _segMap = segMap;
                _index = index;
                Normalize();
            }

            public void MarkInBoundary()
            {
                _segMap.SetBoundarySegment(_index);
            }
        }
    }
}
