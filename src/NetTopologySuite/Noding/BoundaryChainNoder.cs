using NetTopologySuite.Coverage;
using NetTopologySuite.Geometries;
using System.Collections.Generic;

namespace NetTopologySuite.Noding
{
    /// <summary>
    /// A noder which extracts chains of boundary segments
    /// as {@link SegmentString}s from a polygonal coverage.
    /// Boundary segments are those which are not duplicated in the input polygonal coverage.
    /// Extracting chains of segments minimize the number of segment strings created,
    /// which produces a more efficient topological graph structure.
    /// <para/>
    /// This enables fast overlay of polygonal coverages in <see cref="CoverageUnion"/>.
    /// Using this noder is faster than <see cref="SegmentExtractingNoder"/>
    /// and <see cref="BoundarySegmentNoder"/>.
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
            var boundaryChains = new BoundaryChainMap[segStrings.Count];
            AddSegments(segStrings, segSet, boundaryChains);
            MarkBoundarySegments(segSet);
            _chainList = ExtractChains(boundaryChains);
        }

        private static void AddSegments(ICollection<ISegmentString> segStrings, HashSet<Segment> segSet,
            BoundaryChainMap[] includedSegs)
        {
            int i = 0;
            foreach (var ss in segStrings)
            {
                var chainMap = new BoundaryChainMap(ss);
                includedSegs[i++] = chainMap;
                AddSegments(ss, chainMap, segSet);
            }
        }

        private static void AddSegments(ISegmentString segString, BoundaryChainMap segInclude, HashSet<Segment> segSet)
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
                seg.MarkBoundary();
            }
        }

        private static List<ISegmentString> ExtractChains(BoundaryChainMap[] boundaryChains)
        {
            var sectionList = new List<ISegmentString>();
            foreach (var chainMap in boundaryChains)
            {
                chainMap.CreateChains(sectionList);
            }
            return sectionList;
        }

        /// <inheritdoc/>
        public IList<ISegmentString> GetNodedSubstrings()
        {
            return _chainList;
        }

        private class BoundaryChainMap
        {
            private ISegmentString _segString;
            private bool[] _isBoundary;

            public BoundaryChainMap(ISegmentString ss)
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
            private readonly BoundaryChainMap _segMap;
            private readonly int _index;

            public Segment(Coordinate p0, Coordinate p1,
                BoundaryChainMap segMap, int index)
                        : base(p0, p1)
            {
                _segMap = segMap;
                _index = index;
                Normalize();
            }

            public void MarkBoundary()
            {
                _segMap.SetBoundarySegment(_index);
            }
        }
    }
}
