using System.Collections.Generic;
using NetTopologySuite.Index;
using NetTopologySuite.Index.Chain;
using NetTopologySuite.Index.Strtree;
using MonotoneChain = NetTopologySuite.Index.Chain.MonotoneChain;

namespace NetTopologySuite.Noding
{
    ///<summary>
    /// Intersects two sets of <see cref="ISegmentString"/>s using a index based
    /// on <see cref="MonotoneChain"/>s and a <see cref="ISpatialIndex"/>.
    ///</summary>
    public class MCIndexSegmentSetMutualIntersector : SegmentSetMutualIntersector
    {
        private readonly IList<MonotoneChain> _monoChains = new List<MonotoneChain>();
       /*
        * The SpatialIndex used should be something that supports envelope
        * (range) queries efficiently (such as a Quadtree or STRtree.
        */
        private readonly ISpatialIndex _index = new STRtree();
        private int _indexCounter;
        private int _processCounter;
        // statistics
        private int _nOverlaps;

        public IList<MonotoneChain> MonotoneChains
        {
            get { return _monoChains; }
        }

        public ISpatialIndex Index
        {
            get { return _index; }
        }


        public override void SetBaseSegments(IList<ISegmentString> segStrings)
        {
            foreach (ISegmentString segmentString in segStrings)
            {
                AddToIndex(segmentString);
            }
        }

        private void AddToIndex(ISegmentString segStr)
        {
            IList<MonotoneChain> segChains = MonotoneChainBuilder.GetChains(segStr.Coordinates, segStr);
            foreach (MonotoneChain mc in segChains)
            {
                mc.Id = _indexCounter++;
                _index.Insert(mc.Envelope, mc);
            }
        }

        public override void Process(IList<ISegmentString> segStrings)
        {
            _processCounter = _indexCounter + 1;
            _nOverlaps = 0;
            _monoChains.Clear();
            foreach (ISegmentString segStr in segStrings)
            {
                AddToMonoChains(segStr);
            }
            IntersectChains();
            //    System.out.println("MCIndexBichromaticIntersector: # chain overlaps = " + nOverlaps);
            //    System.out.println("MCIndexBichromaticIntersector: # oct chain overlaps = " + nOctOverlaps);
        }

        private void IntersectChains()
        {
            MonotoneChainOverlapAction overlapAction = new SegmentOverlapAction(SegmentIntersector);

            foreach (MonotoneChain queryChain in MonotoneChains)
            {
                var overlapChains = _index.Query(queryChain.Envelope);
                foreach (MonotoneChain testChain in overlapChains)
                {
                    queryChain.ComputeOverlaps(testChain, overlapAction);
                    _nOverlaps++;
                    if (SegmentIntersector.IsDone) return;
                }
            }
        }

        private void AddToMonoChains(ISegmentString segStr)
        {
            IList<MonotoneChain> segChains = MonotoneChainBuilder.GetChains(segStr.Coordinates, segStr);
            foreach (MonotoneChain mc in segChains)
            {
                mc.Id = _processCounter++;
                _monoChains.Add(mc);
            }
        }

        public class SegmentOverlapAction : MonotoneChainOverlapAction
        {
            private readonly ISegmentIntersector _si;

            public SegmentOverlapAction(ISegmentIntersector si)
            {
                _si = si;
            }

            public override void Overlap(MonotoneChain mc1, int start1, MonotoneChain mc2, int start2)
            {
                ISegmentString ss1 = (ISegmentString)mc1.Context;
                ISegmentString ss2 = (ISegmentString)mc2.Context;
                _si.ProcessIntersections(ss1, start1, ss2, start2);
            }

        }
    }
}
