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
            var boundarySegSet = new HashSet<Segment>();
            var boundaryChains = new BoundaryChainMap[segStrings.Count];
            AddSegments(segStrings, boundarySegSet, boundaryChains);
            MarkBoundarySegments(boundarySegSet);
            _chainList = ExtractChains(boundaryChains);

            // check for self-touching nodes and split chains at those nodes
        var nodePts = FindNodePts(_chainList);
            if (nodePts.Count > 0) {
            _chainList = NodeChains(_chainList, nodePts);
            }
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
                if (!segSet.Add(seg))
                {
                    segSet.Remove(seg);
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

        private static HashSet<Coordinate> FindNodePts(ICollection<ISegmentString> segStrings)
        {
            var interorVertices = new HashSet<Coordinate>();
            var nodes = new HashSet<Coordinate>();
            foreach (var ss in segStrings)
            {
                // endpoints are nodes
                nodes.Add(ss.GetCoordinate(0));
                nodes.Add(ss.GetCoordinate(ss.Count - 1));

                // check for duplicate interior points
                for (int i = 1; i < ss.Count - 1; i++)
                {
                    var p = ss.GetCoordinate(i);
                    if (interorVertices.Contains(p))
                    {
                        nodes.Add(p);
                    }
                    interorVertices.Add(p);
                }
            }
            return nodes;
        }

        private static List<ISegmentString> NodeChains(List<ISegmentString> chains, HashSet<Coordinate> nodePts)
        {
            var nodedChains = new List<ISegmentString>();
            foreach (var chain in chains)
            {
                NodeChain(chain, nodePts, nodedChains);
            }
            return nodedChains;
        }

        private static void NodeChain(ISegmentString chain, HashSet<Coordinate> nodePts, ICollection<ISegmentString> nodedChains)
        {
            int start = 0;
            while (start < chain.Count - 1)
            {
                int end = FindNodeIndex(chain, start, nodePts);
                // if no interior nodes found, keep original chain
                if (start == 0 && end == chain.Count - 1)
                {
                    nodedChains.Add(chain);
                    return;
                }
                nodedChains.Add(BasicSegmentString.Substring(chain, start, end));
                start = end;
            }
        }

        private static int FindNodeIndex(ISegmentString chain, int start, HashSet<Coordinate> nodePts)
        {
            for (int i = start + 1; i < chain.Count; i++)
            {
                if (nodePts.Contains(chain.GetCoordinate(i)))
                    return i;
            }
            return chain.Count - 1;
        }

        /// <inheritdoc/>
        public IList<ISegmentString> GetNodedSubstrings()
        {
            return _chainList;
        }

        private class BoundaryChainMap
        {
            private readonly ISegmentString _segString;
            private readonly bool[] _isBoundary;

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
